#!/usr/bin/env bash
# Rebuild and validate BobTraining scene via Unity batchmode CLI.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${REPO_ROOT}"

mkdir -p logs

UNITY_BATCH_FLAGS="${UNITY_BATCH_FLAGS:--disableBurstCompilation}"

echo "Ensuring HDRP pipeline..."
./scripts/unity.sh -batchmode -quit -nographics "${UNITY_BATCH_FLAGS}" \
	-logFile logs/unity-hdrp-setup.log \
	-executeMethod ArcAcademyHdrpSetup.EnsureHdrpFromCli

echo "Rebuilding training scene..."
./scripts/unity.sh -batchmode -quit -nographics "${UNITY_BATCH_FLAGS}" \
	-logFile logs/unity-scene-build.log \
	-executeMethod BobTrainingSceneBuilder.CreateTrainingSceneFromCli

echo "Validating training scene..."
./scripts/unity.sh -batchmode -quit -nographics "${UNITY_BATCH_FLAGS}" \
	-logFile logs/unity-validate.log \
	-executeMethod BobSceneValidator.VerifyFromCli

if grep -q "VALIDATE_PASS" logs/unity-validate.log; then
	echo "VALIDATE_PASS: Bob training scene is ready for Play mode and training"
else
	echo "VALIDATE_FAIL: see logs/unity-validate.log"
	exit 1
fi
