#!/usr/bin/env bash
# Rebuild and validate BobTraining scene via Unity batchmode CLI.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${REPO_ROOT}"

mkdir -p logs

echo "Rebuilding training scene..."
./scripts/unity.sh -batchmode -quit -nographics \
  -logFile logs/unity-scene-build.log \
  -executeMethod BobTrainingSceneBuilder.CreateTrainingSceneFromCli

echo "Validating training scene..."
./scripts/unity.sh -batchmode -quit -nographics \
  -logFile logs/unity-validate.log \
  -executeMethod BobSceneValidator.VerifyFromCli

if grep -q "VALIDATE_PASS" logs/unity-validate.log; then
  echo "VALIDATE_PASS: Bob training scene is ready for Play mode and training"
else
  echo "VALIDATE_FAIL: see logs/unity-validate.log"
  exit 1
fi
