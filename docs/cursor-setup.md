# Cursor IDE Setup for Bob

Workspace-local configuration is committed in `.vscode/`. This guide covers manual steps and recommended extensions.

## Workspace Settings (Already Configured)

[`.vscode/settings.json`](../.vscode/settings.json) sets:

- Python interpreter → `python/.venv/bin/python`
- Auto-activate venv in integrated terminal
- Format on Save for C# and Python

## Recommended Extensions

Cursor will prompt to install these from [`.vscode/extensions.json`](../.vscode/extensions.json):

| Extension | ID | Purpose |
|-----------|-----|---------|
| Python | `ms-python.python` | venv, linting, formatting |
| C# | `ms-dotnettools.csharp` | Unity scripts |
| Unity | `visualstudiotoolsforunity.vstuc` | Unity integration |
| Terraform | `hashicorp.terraform` | IaC syntax and validation |
| Markdown | `yzhang.markdown-all-in-one` | docs editing |

## Python Interpreter

The repo pins **Python 3.10.12** (required by `mlagents==1.1.0`). Homebrew's `python@3.10` (3.10.20) is too new and will be rejected by pip.

### One-command setup

```bash
./scripts/setup-python.sh
```

This uses [uv](https://github.com/astral-sh/uv) to install Python 3.10.12 and create `python/.venv`.

### Apple Silicon (M-series)

`grpcio==1.48.2` (required by mlagents) has **no macOS arm64 wheel**. On Apple Silicon:

- **IDE / linting:** local venv (numpy, matplotlib, tensorboard) — valid Cursor interpreter
- **Training:** use Docker — `./scripts/train.sh`

### Select interpreter in Cursor

1. **Cmd+Shift+P** → `Python: Select Interpreter`
2. Choose `./python/.venv/bin/python` (Python 3.10.12)

Or reload the window after running `setup-python.sh` — workspace settings point to the venv automatically.

## Terminal Workflow

```bash
# Training via Docker (recommended on Apple Silicon)
./scripts/train.sh

# TensorBoard (local venv)
cd python && source .venv/bin/activate
tensorboard --logdir ../results
```

## Unity External Tools

In Unity Editor:

1. **Edit → Preferences → External Tools**
2. Set **Python** to `python/.venv/bin/python3.10` (full path)

See [docs/unity-dev.md](unity-dev.md) for Unity CLI, batchmode builds, and AI assistant options.

## Agent Context Files

Cursor reads these for project context:

- [`AGENTS.md`](../AGENTS.md) — canonical agent instructions
- [`.cursor/rules/bob.mdc`](../.cursor/rules/bob.mdc) — always-on rules
- [`.cursor/project-rules.md`](../.cursor/project-rules.md) — DevOps emphasis

## Format on Save

Enabled for C# and Python via workspace settings. For C#, ensure the C# extension is installed and OmniSharp is running (check status bar).

## Optional: Global Cursor Settings

These are personal preferences (not committed to repo):

- Enable AI features for C# and Python
- Set default terminal shell to zsh (macOS default)
