# AI Review (Ollama)

Advisory PR reviews via [ai-review](https://github.com/Nikita-Filonov/ai-review) using **local Ollama** — no OpenAI/Claude API keys; code stays on the runner + your Ollama host.

## Recommended model

| Model                  | Use                                                         | Size (approx.) |
| ---------------------- | ----------------------------------------------------------- | -------------- |
| **`qwen2.5-coder:7b`** | **Default** — PR summary + inline on Bob (C#, Python, YAML) | ~4.7 GB        |
| `qwen2.5-coder:14b`    | Manual deep review (`workflow_dispatch` + `run-context`)    | ~9 GB          |

**Why qwen2.5-coder:7b:** Strong code understanding, fast on Apple Silicon, fits GitHub Actions cache limits, better than general chat models (e.g. llama3.2:3b) for diff review.

Pinned in [`.ai-review-models.txt`](../.ai-review-models.txt) — changing it invalidates the CI model cache.

## Local setup (Mac)

```bash
# One-time: install Ollama from https://ollama.com
./scripts/setup-ai-review-ollama.sh   # pulls qwen2.5-coder:7b if missing

# Local review (Python 3.11 venv — Bob mlagents venv is 3.10)
./scripts/ai-review-local.sh <PR_NUMBER> --dry-run   # test without posting
./scripts/ai-review-local.sh <PR_NUMBER>             # post summary to GitHub
```

**Note:** `xai-review` requires **Python 3.11+**. Use `python/.venv-ai-review` (created by `scripts/ai-review-local.sh`), not `python/.venv` (3.10 for mlagents).

Ollama stores models under `~/.ollama/models` — **no re-download** after the first pull.

## GitHub Actions

Workflow: [`.github/workflows/ai-review.yml`](../.github/workflows/ai-review.yml)

| Trigger                 | Mode                               | Blocks merge?                      |
| ----------------------- | ---------------------------------- | ---------------------------------- |
| `pull_request` → `main` | `run-summary`                      | **No** (`continue-on-error: true`) |
| `workflow_dispatch`     | choice: summary / inline / context | No                                 |

**Caching:** `actions/cache` on `~/.ollama` keyed by `.ai-review-models.txt`. First run downloads the model (~5 min); later runs restore cache (~15 s).

Required secrets: none for LLM (Ollama is local on runner). Uses `GITHUB_TOKEN` for PR comments.

## Self-hosted runner (fastest)

For M-series Mac as a self-hosted runner with Ollama already running:

1. Run `./scripts/setup-ai-review-ollama.sh` once on the machine
2. Change `runs-on: ubuntu-latest` → `runs-on: self-hosted` in `ai-review.yml`
3. Skip Ollama install steps; set `OLLAMA_HOST=127.0.0.1:11434`

Reviews then use **cached local models** with no CI download.

## Related

- [AGENTS.md](../AGENTS.md) — merge on green **pytest/Terraform/Docker** CI, not AI review alone
- [what-right-looks-like.md](what-right-looks-like.md) — quality bar E (PR + CI)
