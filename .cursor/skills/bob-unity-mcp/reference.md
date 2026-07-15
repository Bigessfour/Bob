# Bob Unity MCP — reference

## Server

- Cursor MCP: **`unity-mcp`** (stdio → `~/.unity/relay/` → Unity Editor)
- Setup: [docs/unity-mcp.md](../../../docs/unity-mcp.md)

## Common tools (project-0-Bob-unity-mcp)

| Task              | Tool                     | Notes                                |
| ----------------- | ------------------------ | ------------------------------------ |
| Play/Stop         | `Unity_ManageEditor`     | Action: Play, Stop, GetState         |
| Scene             | `Unity_ManageScene`      | GetActive, GetHierarchy, Save        |
| Find Bob/hoop     | `Unity_ManageGameObject` | action: find, search_method: by_name |
| Console           | `Unity_ReadConsole`      | Types: Error, Warning                |
| One-off Editor C# | `Unity_RunCommand`       | Stop Play first; wait compile idle   |
| Capture           | `Unity_Camera_Capture`   | Main Camera or Scene View            |

## Batchmode fallback (Editor closed)

```bash
./scripts/validate-scene.sh
./scripts/unity.sh -batchmode -executeMethod BobSceneValidator.VerifyFromCli -quit
```

## Key scene paths

- Scene: `Assets/Scenes/BobTraining.unity`
- Agent: `Assets/Scripts/BobAgent.cs`
- Arena builder: `Assets/Scripts/Editor/SimpleArcAcademyArenaBuilder.cs`
