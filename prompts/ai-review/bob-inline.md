# Bob inline review

Review only changed lines. Project: Unity 6 + ML-Agents free-throw agent **Bob**.

Flag:

- Behavior Name drift from `Bob` or obs/action count changes without validator + alignment tests
- Reward shaping that allows unbounded per-step penalties or OOB-only episode end
- Secrets, API keys, or `.tfstate` committed
- Scope creep unrelated to current milestone (`docs/planning/next-14-days.md`)

Be specific. Prefer actionable fixes. Skip nitpicks on docs formatting unless wrong North Star claims.
