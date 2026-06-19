#!/usr/bin/env bash
# Fix white HDRP blowout on BobTraining without using Unity menu items.
# Close the Unity Editor first — batchmode cannot run while the project is open.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "${REPO_ROOT}"

if pgrep -f "Unity.*${REPO_ROOT}" >/dev/null 2>&1; then
	echo "Unity Editor has this project open. Save your work, quit Unity, then re-run:"
	echo "  ./scripts/fix-hdrp-blowout.sh"
	exit 1
fi

mkdir -p logs
UNITY_BATCH_FLAGS="${UNITY_BATCH_FLAGS:--disableBurstCompilation}"

echo "Applying in-place lab render fix (lights + volume)..."
./scripts/unity.sh -batchmode -quit -nographics "${UNITY_BATCH_FLAGS}" \
	-logFile logs/unity-fix-blowout.log \
	-executeMethod ArcAcademyLabSceneFix.FixWhiteBlowoutFromCli

if grep -q "ARC_LAB_FIX_OK" logs/unity-fix-blowout.log; then
	echo "ARC_LAB_FIX_OK: Reopen Unity, open BobTraining, press Play, check Game tab."
else
	echo "Fix may have failed — see logs/unity-fix-blowout.log"
	exit 1
fi
