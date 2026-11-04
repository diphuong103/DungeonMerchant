# Copilot instructions for Dungeon Merchant

## Project overview
- This repository is a Unity 2D top-down game project using Unity 6000.3.25f1.
- The authoritative project configuration is the Unity project files under `Assets/`, `ProjectSettings/`, `Packages/`, and the generated `.slnx`/`.csproj` files.
- Most gameplay logic is implemented as `MonoBehaviour` scripts under `Assets/Game/Scripts`, organized by gameplay domain rather than by framework layer.
- The project is scene-driven and uses direct component references, serialized fields, and trigger-based interaction logic instead of a large service framework.

## Working conventions
- Prefer Unity idioms over generic C# patterns: use `MonoBehaviour`, `SerializeField`, `Update()`, `FixedUpdate()`, trigger callbacks (`OnTriggerEnter2D`, etc.), and Unity’s `Input` APIs.
- Preserve the existing naming and structure used in the project. Script namespaces follow `Cainos.PixelArtTopDown_Basic` and scripts are organized by gameplay behavior.
- Keep changes small and scene-aware. This project is a compact prototype-style game, so aim for direct, readable logic rather than abstraction-heavy refactors.
- When editing scripts, assume the code will be used by Unity’s Editor and runtime, not by a standalone .NET CLI app.

## Architecture notes
- Player movement and animation are centered in `Assets/Game/Scripts/TopDownCharacterController.cs`.
  - Reads raw input from `Input.GetAxisRaw("Horizontal")` / `Vertical`.
  - Clamps movement to a unit vector.
  - Resolves facing direction from the dominant axis.
  - Uses animator state hashes for directional walking states.
  - Applies velocity in `FixedUpdate()`.
- Camera behavior is simple follow logic in `Assets/Game/Scripts/CameraFollow.cs`.
  - Keeps a target offset and smooths camera movement toward the target transform.
- World interaction scripts such as `PropsAltar.cs`, `StairsLayerTrigger.cs`, and `SpriteColorAnimation.cs` demonstrate the project’s standard pattern:
  - trigger-based behavior,
  - runtime state changes on enter/exit,
  - component-to-component object interaction,
  - sprite color / layer switching through straightforward Unity APIs.
- Treat the scene graph and serialized references as first-class behavior. Many scripts rely on object references assigned in the Unity Editor rather than a dependency-injection container.

## Build and validation guidance
- This repo does not appear to include a custom CLI build script, linter, or project-specific automated test suite.
- The primary validation path is the Unity Editor workflow: open the project in Unity, run scene/play mode checks, and validate gameplay behavior in-editor.
- The Unity project includes `com.unity.test-framework`, so Unity Test Framework tests are possible when such tests are added, but no repo-local test files were found to execute automatically from the command line.
- If you need a local command-line check, prefer the solution/project files only as IDE compatibility scaffolding; do not assume they are the authoritative build system for the game.
- `dotnet test DungeonMerchant.slnx --nologo --verbosity minimal` is a conservative check that the generated solution loads and that the .NET/IDE layer does not immediately fail, but it is not a substitute for Unity runtime validation.

## File and code guidance
- Keep C# code compatible with Unity 6 style and the installed package set, especially URP and 2D packages.
- Favor direct, explicit logic over generalized abstractions unless the project already uses that pattern.
- When adding new gameplay systems, follow the existing style:
  - small scene objects with `MonoBehaviour`
  - serialized fields for tunable values
  - simple state machines driven by direction, triggers, or animator parameters
  - minimal ceremony around runtime setup
- Avoid introducing heavy framework patterns, DI containers, or non-Unity tooling unless the task clearly requires them.

## When making changes
- Check the relevant scene and script pairing before editing: most features are controlled by a small set of scripts attached to in-scene objects.
- If a feature alters movement, animation, camera behavior, collision triggers, or sprite layer ordering, verify the logic in Unity play mode because these systems are highly scene-dependent.
- Keep changes compatible with the project’s top-down 2D design and any existing asset-driven references.

## Useful repo facts
- Main scripts directory: `Assets/Game/Scripts`
- Package manifest: `Packages/manifest.json`
- Unity project version: `ProjectSettings/ProjectVersion.txt`
- Project solution: `DungeonMerchant.slnx`
- Current Unity version installed in the project: 6000.3.25f1

This file should remain concise and project-specific. Do not invent generic instructions that are not supported by the repo or the actual Unity workflow.
