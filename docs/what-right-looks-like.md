# What Right Looks Like — Bob North Star

**Canonical visual spec** for [bigessfour/bob](https://github.com/Bigessfour/Bob). Every agent, contributor, and planning session must align with these diagrams before proposing work, opening PRs, or merging changes.

**Pinned in:** [PROJECT.md](../PROJECT.md) · [AGENTS.md](../AGENTS.md) · [project-plan.md](project-plan.md) · [`.cursor/rules/bob.mdc`](../.cursor/rules/bob.mdc)

---

## How to use this document

| When              | Action                                                                                                      |
| ----------------- | ----------------------------------------------------------------------------------------------------------- |
| **Planning**      | Read both diagrams; confirm the task maps to the current week milestone and permanent quality bars          |
| **Before coding** | Ask: does this change move us toward the diagram, or is it scope creep?                                     |
| **Before a PR**   | Verify feature branch → green CI → merge to `main`; never commit directly to `main`                         |
| **Agent turns**   | Reference this doc in [AGENTS.md](../AGENTS.md); query `bob-rag` with `what-right-looks-like` for alignment |

If a proposed change does not advance a milestone **and** satisfy at least one permanent quality bar (E–H below), defer it.

---

## 1. Milestones + permanent quality bars

```mermaid
flowchart TD
    A["Vision<br/>Bob = Fun PPO Free-Throw Champion<br/>Portfolio + DevOps Showcase"]
    --> B["Week 1 - Setup and Foundations"]
    B --> C["Week 2 - Core Agent + Training Loop"]
    C --> D["Week 3 - Polish + WebGL + AWS Deploy"]

    subgraph WRLL["What Right Looks Like - Always True"]
        E["Protected main + feature/* branches<br/>Every change via PR + auto-merge on green CI"]
        F["80%+ test coverage<br/>Reproducible Docker + Python venv"]
        G["Clean C#, cheerful comments, portfolio-ready docs"]
        H["Live demo URL in README + TensorBoard GIFs"]
    end

    B -->|Always| E
    C -->|Always| F
    D -->|Always| G
    D -->|Always| H

    style A fill:#FF6200,stroke:#fff,color:#fff
    style E fill:#22d3ee,stroke:#fff
    style F fill:#a855f7,stroke:#fff
    style G fill:#eab308,stroke:#fff
```

**Week 1** ✅ foundations (Unity project, CI, agent scaffold, DevOps scaffold).  
**Week 2** — training loop, reward shaping, progress captures.  
**Week 3** — polish, WebGL, Terraform apply, live demo URL.

---

## 2. Repo structure + dev workflow compass

```mermaid
flowchart LR
    subgraph RepoRoot["Repo Root"]
        direction TB
        A["Assets/<br/>Scenes/BobTraining.unity<br/>Scripts/BobAgent.cs<br/>Prefabs/<br/>Materials/BobOrange.mat"]
        B["config/<br/>bob_free_throw.yaml"]
        C["python/<br/>requirements.txt<br/>tests/<br/>scripts/plot_rewards.py"]
        D["terraform/<br/>bootstrap/<br/>environments/dev/"]
        E[".github/workflows/<br/>ci.yml - pytest + terraform + docker"]
        F["docs/<br/>testing-strategy.md<br/>setup-checklist.md<br/>reward-curves/"]
    end

    G["Dev Workflow"] --> H["Create feature/xxx branch"]
    H --> I["Work + test locally<br/>pytest + Play in Unity"]
    I --> J["git commit -m feat: ..."]
    J --> K["git push - open PR"]
    K --> L["CI green? Auto-merge to main"]

    L --> M["What Right Looks Like Achieved"]

    style G fill:#22d3ee
    style L fill:#86efac,stroke:#166534
    style M fill:#fbbf24,stroke:#854d0e
```

---

## Alignment checklist (agents)

Before suggesting or implementing changes, confirm:

- [ ] Task maps to **current week** in [PROJECT.md](../PROJECT.md) (see Current Milestone)
- [ ] Change respects **repo layout** (Assets, config, python, terraform, docs — not ad-hoc paths)
- [ ] Work happens on **`feature/*`** (or fix branch), not direct commits to `main`
- [ ] **CI must pass** before merge (pytest, Terraform, Docker build)
- [ ] **Hyperparameters** stay in `config/*.yaml`; **Behavior Name** stays `Bob`
- [ ] **Docs updated** when behavior, workflow, or milestones change
- [ ] **Portfolio artifacts** (GIFs, demo URL, progress gallery) tracked for Week 2–3

---

## Related

- [project-plan.md](project-plan.md) — week-by-week checklist
- [testing-strategy.md](testing-strategy.md) — coverage targets toward 80%+
- [PROJECT.md](../PROJECT.md) — living status and next actions
- [AGENTS.md](../AGENTS.md) — agent rules including RAG and Unity MCP
