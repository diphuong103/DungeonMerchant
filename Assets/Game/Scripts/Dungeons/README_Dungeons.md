# Dungeon scenes

The Unity Editor menu **Dungeon Merchant → Dungeons → Build 3 Dungeon Scenes** creates:

- `Assets/Game/Scenes/Dungeons/Dungeon_HollowShrine.unity`
- `Assets/Game/Scenes/Dungeons/Dungeon_RootboundWilds.unity`
- `Assets/Game/Scenes/Dungeons/Dungeon_EmberVault.unity`

It copies the existing `Player` from `Assets/Game/Scenes/Gameplay/Gameplay.unity` into a separate dungeon player prefab, preserving the supplied controller, animator, and sprite. Dungeon-only health and melee components are added to that prefab. The existing hub scene and its code are not edited.

The scenes use 1-unit Tilemap cells, 100-pixel-per-unit point-filtered placeholder tiles, and the existing `Layer 1`, `Layer 2`, and `Layer 3` sorting layers. Controls are WASD/arrow keys to move, J/Space to attack, E to use the entrance portal or open the reward chest. Enemies start in waves when the player enters the marked arena; the gate opens after both waves. Enter the upper boss room after the waves, defeat its boss, open the chest, and walk through the exit portal to return to `Gameplay`.

Generated scenes are also appended to Build Settings so Unity can load them by scene name at runtime.
