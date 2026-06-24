using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 50;

        private static int _frameCount = 0;
        private static bool _screenshotCaptured = false;
        private static bool _done = false;
        private static string _screenshotPath;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            switch (state)
            {
                case "Idle":
                    break;
                case "WaitingForCompile":
                    Debug.Log("[PlayModeTest] Bootstrap compiled. Scheduling Play Mode entry.");
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;
                case "EnteringPlayMode":
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += Tick;
                    }
                    break;
                case "InPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.update += Tick;
                    }
                    break;
                case "Done":
                    Debug.Log(SentinelLog);
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += Tick;
            }
        }

        private static void Tick()
        {
            if (_done) return;
            _frameCount++;
            if (_frameCount < 45) return;

            if (!_screenshotCaptured)
            {
                _screenshotPath = "Assets/PlayModeTest_BobView.png";
                ScreenCapture.CaptureScreenshot(_screenshotPath);
                SessionState.SetString("PlayModeTest.ScreenshotPath", _screenshotPath);
                Debug.Log("[Test] Screenshot captured at: " + _screenshotPath);
                _screenshotCaptured = true;
                return;
            }

            _done = true;
            EditorApplication.update -= Tick;

            Application.logMessageReceived += OnLogMessage;
            string resultJson;
            try { resultJson = RunTestLogic(); }
            catch (System.Exception e)
            {
                Debug.LogError("[PlayModeTest] Test threw exception: " + e);
                resultJson = JsonUtility.ToJson(new TestResult { success = false, error = e.Message });
            }
            finally { Application.logMessageReceived -= OnLogMessage; }

            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath))
            {
                AssetDatabase.DeleteAsset(scriptPath);
            }
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count >= MaxCapturedLogs) return;
            if (type == LogType.Error || type == LogType.Exception ||
                message.Contains("[Test]") || message.Contains("TEST_RESULT"))
            {
                _capturedLogs.Add("[" + type + "] " + message);
            }
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public string[] logs;
            public string screenshotPath;
            public Vector3 camPos;
            public Vector3 camRot;
            public float camFov;
            public bool hitSomething;
            public string hitObjectName;
            public Vector3 hitPoint;
            public int netStrandCount;
            public int rimSegCount;
            public bool trainingBaysActive;
        }

        private static string RunTestLogic()
        {
            var r = new TestResult { success = true };
            r.screenshotPath = _screenshotPath;

            var camObj = GameObject.Find("Main Camera");
            if (camObj != null)
            {
                r.camPos = camObj.transform.position;
                r.camRot = camObj.transform.rotation.eulerAngles;
                var cam = camObj.GetComponent<Camera>();
                if (cam != null) r.camFov = cam.fieldOfView;
                if (Physics.Raycast(camObj.transform.position, camObj.transform.forward, out RaycastHit hit, 200f))
                {
                    r.hitSomething = true;
                    r.hitObjectName = hit.collider.gameObject.name;
                    r.hitPoint = hit.point;
                }
            }

            var bays = GameObject.Find("TrainingArena/TrainingBays");
            r.trainingBaysActive = bays != null && bays.activeInHierarchy;

            var hoop = GameObject.Find("TrainingArena/Hoop");
            if (hoop != null)
            {
                int net = 0, seg = 0;
                foreach (var t in hoop.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.StartsWith("NetStrand")) net++;
                    else if (t.name.StartsWith("RimSeg")) seg++;
                }
                r.netStrandCount = net;
                r.rimSegCount = seg;
            }

            Debug.Log("TEST_RESULT: camPos=" + r.camPos + " fov=" + r.camFov + " hit=" + r.hitObjectName +
                      " net=" + r.netStrandCount + " seg=" + r.rimSegCount + " baysActive=" + r.trainingBaysActive);
            r.logs = _capturedLogs.ToArray();
            return JsonUtility.ToJson(r);
        }
    }
}