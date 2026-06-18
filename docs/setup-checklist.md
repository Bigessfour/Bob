# Local Setup Checklist — M5 MacBook

Step-by-step guide to get Bob training locally on Apple Silicon.

## Prerequisites

- macOS with Apple Silicon (M-series)
- ~20 GB free disk space (Unity Editor + modules)
- Git (Xcode Command Line Tools or Homebrew)

---

## 1. Unity Hub + Unity 6 LTS

1. Download and install [Unity Hub](https://unity.com/download)
2. Sign in or create a Unity ID (Personal license is free)
3. **Installs** → **Install Editor** → select **Unity 6 LTS** (6000.x)
4. Add modules:
   - **WebGL Build Support** (required for Week 3 deploy)
   - **Mac Build Support** (IL2CPP) — usually included by default
5. Wait for download/install to complete

## 2. Create Unity Project at Repo Root

1. Open Unity Hub → **New project**
2. Template: **3D (URP)** or **3D** (Built-in Render Pipeline)
3. Project name: `Bob` (or match repo folder name)
4. Location: **repo root** — e.g. `/Users/you/Bob/Bob` (same folder as `config/`, `python/`, `docs/`)
5. Click **Create project**

Unity will create `Assets/`, `ProjectSettings/`, and `Packages/` alongside existing scaffold folders.

> **Important:** Do not create the project in a nested subfolder. ML-Agents and this repo layout expect Unity at the root.

## 3. Install ML-Agents in Unity

1. In Unity Editor: **Window** → **Package Manager**
2. Click **+** → **Add package by name**
3. Enter: `com.unity.ml-agents`
4. Install the version compatible with `mlagents==1.1.0` in `python/requirements.txt`
   - Check [ML-Agents releases](https://github.com/Unity-Technologies/ml-agents/releases) for version matrix
5. Verify: **Window** → **ML-Agents** menu appears

## 4. Install Python 3.10

Choose one method:

### Option A — Homebrew (recommended)

```bash
brew install python@3.10
```

### Option B — pyenv

```bash
brew install pyenv
pyenv install 3.10.14
pyenv local 3.10.14
```

Verify:

```bash
python3.10 --version
# Python 3.10.x
```

## 5. Python Virtual Environment

From the repo root:

```bash
cd python
python3.10 -m venv .venv
source .venv/bin/activate
pip install --upgrade pip
pip install -r requirements.txt
```

### Apple Silicon notes

- `torch` installs the MPS-enabled build automatically on arm64
- If `pip install` fails on torch, try:
  ```bash
  pip install torch --index-url https://download.pytorch.org/whl/cpu
  ```
  then re-run `pip install -r requirements.txt`

Verify ML-Agents CLI:

```bash
mlagents-learn --help
```

## 6. First Training Run (after scene exists)

1. Open your training scene in Unity
2. Ensure Bob's `Behavior Parameters` → Behavior Name = `Bob`
3. Set **Behavior Type** = `Default` (training mode)
4. In terminal (venv active):

   ```bash
   mlagents-learn ../config/bob_free_throw.yaml --run-id=bob-v0
   ```

5. When prompted, press **Play** in Unity Editor
6. Training output appears in `results/` (gitignored)

## 7. Recommended Cursor Extensions

- C# (Microsoft)
- Unity (Unity Technologies)
- Python (Microsoft)
- Terraform (HashiCorp)
- Markdown All in One

## Troubleshooting

| Issue | Fix |
|-------|-----|
| `mlagents-learn` not found | Activate venv: `source python/.venv/bin/activate` |
| Unity can't find Python | Set **Edit → Preferences → External Tools → Python** to `python/.venv/bin/python3.10` |
| Behavior name mismatch | Unity Behavior Name must match `behaviors: Bob:` in `config/bob_free_throw.yaml` |
| Training hangs at "Waiting for connection" | Press Play in Unity; check firewall isn't blocking localhost |

## Next Steps

- Build court scene and Bob agent (Week 1)
- See [project-plan.md](project-plan.md) for milestone checklist
