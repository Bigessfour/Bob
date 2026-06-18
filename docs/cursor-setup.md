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

After creating the venv:

1. **Cmd+Shift+P** → `Python: Select Interpreter`
2. Choose `./python/.venv/bin/python`

Or rely on workspace settings (automatic once venv exists).

## Terminal Workflow

```bash
cd python
source .venv/bin/activate
mlagents-learn ../config/bob_free_throw.yaml --run-id=bob-v0
```

With `python.terminal.activateEnvironment: true`, opening a terminal from the workspace root auto-activates the venv.

## Unity External Tools

In Unity Editor:

1. **Edit → Preferences → External Tools**
2. Set **Python** to `python/.venv/bin/python3.10` (full path)

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
