# Game asset layout

Keep game-specific content under `Assets/Game`. Unity settings, editor tools, and project-wide templates stay in their existing top-level folders.

- `Art/`: source sprites, animations, tilesets, shaders, and visual effects.
- `Audio/`: music and sound effects.
- `Data/`: authored game data, grouped by feature (characters, items, quests, abilities).
- `Materials/`: shared materials.
- `Prefabs/`: reusable characters, enemies, interactables, props, plants, and UI.
- `Scenes/Gameplay/`: scenes included in the player build. `Gameplay.unity` is the current starting scene.
- `Scenes/Samples/`: experiments, template scenes, and visual showcases; not included in the build by default.
- `Scripts/Core/`: shared systems used across features.
- `Scripts/Player/`: player control and player-specific behavior.
- `Scripts/World/`: world objects, triggers, and environment behavior.
- `Scripts/<Feature>/`: add a folder when that feature gains its first script (for example `Inventory`, `Quests`, or `UI`).
- `Settings/` and `UI/`: game-specific settings and interface assets.

Keep each C# MonoBehaviour in a same-named script file. Prefer small feature-specific components and add shared systems only when more than one feature needs them. Keep sample scenes out of Build Settings unless they are intentionally playable entry points.
