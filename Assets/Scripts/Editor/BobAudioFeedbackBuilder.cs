#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ensures <see cref="BobAudioFeedback"/> exists in the active training scene with lab SFX clips.
/// </summary>
public static class BobAudioFeedbackBuilder
{
    private const string BouncePath = "Assets/Audio/sfx_bounce.wav";
    private const string SwishPath = "Assets/Audio/sfx_swish.wav";
    private const string ScorePath = "Assets/Audio/sfx_score.wav";
    private const string MissPath = "Assets/Audio/sfx_miss.wav";

    public static BobAudioFeedback EnsureInScene()
    {
        var existing = Object.FindAnyObjectByType<BobAudioFeedback>();
        if (existing == null)
        {
            var go = new GameObject(BobAudioFeedback.RootName);
            existing = go.AddComponent<BobAudioFeedback>();
            go.AddComponent<AudioSource>();
        }

        existing.AssignClips(
            AssetDatabase.LoadAssetAtPath<AudioClip>(BouncePath),
            AssetDatabase.LoadAssetAtPath<AudioClip>(SwishPath),
            AssetDatabase.LoadAssetAtPath<AudioClip>(ScorePath),
            AssetDatabase.LoadAssetAtPath<AudioClip>(MissPath));
        EditorUtility.SetDirty(existing);
        return existing;
    }
}
#endif
