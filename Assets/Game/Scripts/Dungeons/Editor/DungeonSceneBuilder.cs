using System;
using System.Collections.Generic;
using System.IO;
using Cainos.PixelArtTopDown_Basic;
using DungeonMerchant.Dungeons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace DungeonMerchant.Dungeons.Editor
{
    /// <summary>Creates three independent dungeon scenes while leaving the existing hub scene untouched.</summary>
    [InitializeOnLoad]
    public static class DungeonSceneBuilder
    {
        private const string SceneFolder = "Assets/Game/Scenes/Dungeons";
        private const string ArtFolder = "Assets/Game/Art/Dungeons/Generated";
        private const string TileFolder = "Assets/Game/Art/Dungeons/Tiles";
        private const string PrefabFolder = "Assets/Game/Prefabs/Dungeons";
        private const string PlayerSourceScene = "Assets/Game/Scenes/Gameplay/Gameplay.unity";
        private const string HomeSceneName = "Gameplay";
        private const string BuilderVersion = "3";
        private const string BuilderVersionAsset = "Assets/Game/Scenes/Dungeons/DungeonBuildVersion.txt";
        private static readonly Color Ink = Hex("242833");
        private static bool waitingForEditMode;

        static DungeonSceneBuilder()
        {
            EditorApplication.delayCall += BuildMissingScenes;
        }

        private sealed class DungeonSpec
        {
            public string name;
            public string sceneName;
            public string reward;
            public Color floor;
            public Color floorAccent;
            public Color wall;
            public Color wallAccent;
            public Color decor;
            public Color hazard;
            public Color enemy;
            public Color boss;
        }

        [MenuItem("Dungeon Merchant/Dungeons/Build 3 Dungeon Scenes")]
        public static void BuildAll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Stop Play Mode before building dungeon scenes.");
                return;
            }

            EnsureFolder("Assets/Game/Art/Dungeons");
            EnsureFolder(ArtFolder);
            EnsureFolder(TileFolder);
            EnsureFolder(SceneFolder);
            EnsureFolder(PrefabFolder);
            EnsureFolder("Assets/Game/Prefabs/Characters");

            GameObject playerPrefab = BuildDungeonPlayerPrefab();
            if (playerPrefab == null)
            {
                Debug.LogError("Could not find the existing Player in " + PlayerSourceScene + "; no dungeon scenes were built.");
                return;
            }

            DungeonSpec[] specs =
            {
                new DungeonSpec { name = "The Hollow Shrine", sceneName = "Dungeon_HollowShrine", reward = "Golem Heart", floor = Hex("56565B"), floorAccent = Hex("77727A"), wall = Hex("34343D"), wallAccent = Hex("56515E"), decor = Hex("9C78B4"), hazard = Hex("914A98"), enemy = Hex("7E6C88"), boss = Hex("A74EAE") },
                new DungeonSpec { name = "Rootbound Wilds", sceneName = "Dungeon_RootboundWilds", reward = "Ancient Seed", floor = Hex("62664A"), floorAccent = Hex("85835A"), wall = Hex("393F32"), wallAccent = Hex("586046"), decor = Hex("9A9A55"), hazard = Hex("6D8B43"), enemy = Hex("687B46"), boss = Hex("A1A54B") },
                new DungeonSpec { name = "Ember Vault", sceneName = "Dungeon_EmberVault", reward = "Forge Core", floor = Hex("514540"), floorAccent = Hex("76584B"), wall = Hex("322D31"), wallAccent = Hex("57423D"), decor = Hex("C56B3F"), hazard = Hex("E17637"), enemy = Hex("A24E3E"), boss = Hex("D75D3F") }
            };

            GameObject lootPrefab = BuildLootPrefab();
            GameObject enemyPrefab = BuildEnemyPrefab("Dungeon Grunt", "grunt", Hex("6C8096"), 3, 1.65f, 1, lootPrefab);
            GameObject brutePrefab = BuildEnemyPrefab("Dungeon Brute", "brute", Hex("A05E59"), 5, 1.1f, 2, lootPrefab);
            GameObject[] enemies = { enemyPrefab, brutePrefab };
            foreach (DungeonSpec spec in specs)
                BuildDungeonScene(spec, playerPrefab, enemies, lootPrefab);

            AssetDatabase.SaveAssets();
            AddScenesToBuildSettings(specs);
            WriteBuildSettingsFile(specs);
            File.WriteAllText(Path.Combine(Application.dataPath, "Game/Scenes/Dungeons/DungeonBuildVersion.txt"), BuilderVersion);
            AssetDatabase.ImportAsset(BuilderVersionAsset, ImportAssetOptions.ForceUpdate);
            Debug.Log("Built dungeon scenes: Hollow Shrine, Rootbound Wilds, and Ember Vault. Home scene code was not changed.");
        }

        private static void BuildMissingScenes()
        {
            string[] paths =
            {
                SceneFolder + "/Dungeon_HollowShrine.unity",
                SceneFolder + "/Dungeon_RootboundWilds.unity",
                SceneFolder + "/Dungeon_EmberVault.unity"
            };
            string versionPath = Path.Combine(Application.dataPath, "Game/Scenes/Dungeons/DungeonBuildVersion.txt");
            if (Array.TrueForAll(paths, File.Exists) && File.Exists(versionPath) && File.ReadAllText(versionPath).Trim() == BuilderVersion) return;
            if (EditorApplication.isCompiling)
            {
                EditorApplication.delayCall += BuildMissingScenes;
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!waitingForEditMode)
                {
                    waitingForEditMode = true;
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                }
                return;
            }

            try
            {
                BuildAll();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            waitingForEditMode = false;
            EditorApplication.delayCall += BuildMissingScenes;
        }

        private static GameObject BuildDungeonPlayerPrefab()
        {
            string prefabPath = "Assets/Game/Prefabs/Characters/Dungeon Player.prefab";
            Scene originalActiveScene = SceneManager.GetActiveScene();
            Scene source = SceneManager.GetSceneByPath(PlayerSourceScene);
            bool openedSource = !source.IsValid() || !source.isLoaded;
            if (openedSource) source = EditorSceneManager.OpenScene(PlayerSourceScene, OpenSceneMode.Additive);
            GameObject sourcePlayer = FindByName(source.GetRootGameObjects(), "Player");
            if (sourcePlayer == null)
            {
                if (openedSource) EditorSceneManager.CloseScene(source, true);
                if (originalActiveScene.IsValid() && originalActiveScene.isLoaded)
                    SceneManager.SetActiveScene(originalActiveScene);
                return null;
            }

            GameObject copy = UnityEngine.Object.Instantiate(sourcePlayer);
            copy.name = "Dungeon Player";
            copy.transform.position = Vector3.zero;
            if (copy.GetComponent<DungeonPlayerHealth>() == null) copy.AddComponent<DungeonPlayerHealth>();
            if (copy.GetComponent<DungeonPlayerCombat>() == null) copy.AddComponent<DungeonPlayerCombat>();
            DungeonSpawnPoint playerSpawnComponent = copy.GetComponent<DungeonSpawnPoint>();
            if (playerSpawnComponent != null) UnityEngine.Object.DestroyImmediate(playerSpawnComponent);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(copy, prefabPath);
            UnityEngine.Object.DestroyImmediate(copy);
            if (openedSource) EditorSceneManager.CloseScene(source, true);
            if (originalActiveScene.IsValid() && originalActiveScene.isLoaded)
                SceneManager.SetActiveScene(originalActiveScene);
            AssetDatabase.SaveAssets();
            return prefab;
        }

        private static GameObject BuildEnemyPrefab(string displayName, string assetName, Color color, int hp, float speed, int damage, GameObject lootPrefab)
        {
            Sprite sprite = MakeSprite("Enemy_" + assetName, color, Hex("D8C8A2"), "creature");
            GameObject enemy = new GameObject(displayName);
            SpriteRenderer renderer = enemy.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Layer 3";
            renderer.sortingOrder = 1;
            enemy.AddComponent<Rigidbody2D>().gravityScale = 0f;
            CircleCollider2D collider = enemy.AddComponent<CircleCollider2D>();
            collider.radius = 0.28f;
            DungeonEnemyBase logic = enemy.AddComponent<DungeonEnemyBase>();
            SetField(logic, "maxHealth", hp);
            SetField(logic, "moveSpeed", speed);
            SetField(logic, "attackDamage", damage);
            SetField(logic, "lootPrefab", lootPrefab);
            string path = PrefabFolder + "/Enemy_" + assetName + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
            UnityEngine.Object.DestroyImmediate(enemy);
            return prefab;
        }

        private static GameObject BuildLootPrefab()
        {
            string path = PrefabFolder + "/Dungeon Shard.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            GameObject loot = new GameObject("Dungeon Shard");
            SpriteRenderer renderer = loot.AddComponent<SpriteRenderer>();
            renderer.sprite = MakeSprite("Loot_DungeonShard", Hex("E3C56B"), Hex("FFF1B2"), "gem");
            renderer.sortingLayerName = "Layer 3";
            renderer.sortingOrder = 2;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(loot, path);
            UnityEngine.Object.DestroyImmediate(loot);
            return prefab;
        }

        private static void BuildDungeonScene(DungeonSpec spec, GameObject playerPrefab, GameObject[] enemyPrefabs, GameObject lootPrefab)
        {
            var tiles = BuildTileAssets(spec);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            scene.name = spec.sceneName;

            GameObject gridObject = new GameObject("Dungeon Tilemaps", typeof(Grid));
            SceneManager.MoveGameObjectToScene(gridObject, scene);
            Grid grid = gridObject.GetComponent<Grid>();
            grid.cellSize = Vector3.one;

            Tilemap ground = CreateTilemap(gridObject.transform, "Ground", "Layer 1", 0);
            Tilemap walls = CreateTilemap(gridObject.transform, "Walls", "Layer 2", 0);
            Tilemap decor = CreateTilemap(gridObject.transform, "Decor", "Layer 3", -2);
            Tilemap hazard = CreateTilemap(gridObject.transform, "Hazard", "Layer 1", 2);

            for (int y = -14; y <= 14; y++)
            for (int x = -12; x <= 12; x++)
            {
                int hash = Mathf.Abs(x * 37 + y * 71 + spec.sceneName.GetHashCode());
                ground.SetTile(new Vector3Int(x, y, 0), hash % 11 == 0 ? tiles.floorAlt : tiles.floor);
            }

            for (int x = -12; x <= 12; x++)
            {
                walls.SetTile(new Vector3Int(x, -14, 0), tiles.wall);
                walls.SetTile(new Vector3Int(x, 14, 0), tiles.wall);
            }
            for (int y = -13; y <= 13; y++)
            {
                walls.SetTile(new Vector3Int(-12, y, 0), tiles.wall);
                walls.SetTile(new Vector3Int(12, y, 0), tiles.wall);
            }
            // Short broken masonry alcoves frame the upper boss room without blocking the route.
            for (int i = 0; i < 3; i++)
            {
                walls.SetTile(new Vector3Int(-10 + i, 10, 0), tiles.wall);
                walls.SetTile(new Vector3Int(8 + i, 10, 0), tiles.wall);
            }
            AddStaticTilemapCollider(walls, false);

            var random = new System.Random(spec.sceneName.GetHashCode());
            for (int i = 0; i < 28; i++)
            {
                int x = random.Next(-10, 11);
                int y = random.Next(-12, 13);
                if (Mathf.Abs(x) < 5 && y < 6 && y > -9) continue;
                decor.SetTile(new Vector3Int(x, y, 0), tiles.decor);
            }

            foreach (Vector3Int cell in new[]
            {
                new Vector3Int(-4, 0, 0), new Vector3Int(-3, 0, 0), new Vector3Int(3, 1, 0),
                new Vector3Int(4, 1, 0), new Vector3Int(-5, 2, 0), new Vector3Int(5, -1, 0)
            })
                hazard.SetTile(cell, tiles.hazard);
            AddStaticTilemapCollider(hazard, true);
            hazard.gameObject.AddComponent<DungeonHazard>();

            Sprite portalSprite = MakeSprite("Portal_" + spec.sceneName, Hex("386C92"), Hex("7AD5DB"), "portal");
            Sprite gateSprite = MakeSprite("Gate_" + spec.sceneName, spec.wall, spec.decor, "masonry");
            Sprite chestSprite = MakeSprite("Chest_" + spec.sceneName, Hex("8C5D2B"), Hex("E3C56B"), "chest");

            GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab, scene) as GameObject;
            if (player == null) throw new InvalidOperationException("Could not instantiate Dungeon Player prefab.");
            player.name = "Player";
            player.tag = "Player";
            GameObject playerSpawnObject = new GameObject("Player Spawn Point");
            SceneManager.MoveGameObjectToScene(playerSpawnObject, scene);
            playerSpawnObject.transform.position = new Vector3(0f, -11.4f, 0f);
            playerSpawnObject.AddComponent<DungeonSpawnPoint>();
            player.transform.position = playerSpawnObject.transform.position;

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = player.transform.position + new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Ink;
            CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
            follow.target = player.transform;
            follow.lerpSpeed = 8f;
            GameObject lightObject = new GameObject("Global Light 2D", typeof(Light2D));
            SceneManager.MoveGameObjectToScene(lightObject, scene);
            Light2D light = lightObject.GetComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = new Color(0.88f, 0.86f, 0.78f);
            light.intensity = 0.9f;

            CreatePortal("Entrance Portal (Return Home)", portalSprite, new Vector2(0f, -12.8f), HomeSceneName, true, scene);

            GameObject gateObject = CreateSpriteObject("Wave Gate (Locked Until Clear)", gateSprite, new Vector3(0f, 5f, 0f), new Vector3(9f, 0.72f, 1f), "Layer 2", 2, scene);
            BoxCollider2D gateCollider = gateObject.AddComponent<BoxCollider2D>();
            gateCollider.size = new Vector2(1f, 0.55f);
            DungeonGate gate = gateObject.AddComponent<DungeonGate>();
            SetField(gate, "gateVisual", gateObject.GetComponent<SpriteRenderer>());
            gate.SetLocked(true);

            var spawnPoints = new Transform[6];
            Vector2[] spawnPositions = { new Vector2(-5f, -1f), new Vector2(0f, 2f), new Vector2(5f, -1f), new Vector2(-4f, 3f), new Vector2(4f, 3f), new Vector2(0f, -2f) };
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                GameObject point = new GameObject("Enemy Spawn " + (i + 1));
                SceneManager.MoveGameObjectToScene(point, scene);
                point.transform.position = spawnPositions[i];
                spawnPoints[i] = point.transform;
            }

            GameObject spawnerObject = new GameObject("EnemySpawner - Waves");
            SceneManager.MoveGameObjectToScene(spawnerObject, scene);
            DungeonEnemySpawner spawner = spawnerObject.AddComponent<DungeonEnemySpawner>();
            SetField(spawner, "enemyPrefabs", enemyPrefabs);
            SetField(spawner, "spawnPoints", spawnPoints);
            SetField(spawner, "waveSizes", new[] { 3, 4 });
            SetField(spawner, "gate", gate);

            GameObject waveTriggerObject = new GameObject("Wave Start Trigger");
            SceneManager.MoveGameObjectToScene(waveTriggerObject, scene);
            waveTriggerObject.transform.position = new Vector3(0f, 0f, 0f);
            BoxCollider2D waveTriggerCollider = waveTriggerObject.AddComponent<BoxCollider2D>();
            waveTriggerCollider.isTrigger = true;
            waveTriggerCollider.size = new Vector2(21f, 7f);
            DungeonWaveTrigger waveTrigger = waveTriggerObject.AddComponent<DungeonWaveTrigger>();
            SetField(waveTrigger, "spawner", spawner);

            GameObject bossRoomObject = new GameObject("Boss Room Trigger");
            SceneManager.MoveGameObjectToScene(bossRoomObject, scene);
            bossRoomObject.transform.position = new Vector3(0f, 6.4f, 0f);
            BoxCollider2D bossTriggerCollider = bossRoomObject.AddComponent<BoxCollider2D>();
            bossTriggerCollider.isTrigger = true;
            bossTriggerCollider.size = new Vector2(10f, 1.4f);
            DungeonBossRoomTrigger bossTrigger = bossRoomObject.AddComponent<DungeonBossRoomTrigger>();
            SetField(bossTrigger, "waveSpawner", spawner);

            GameObject bossPrefab = BuildBossPrefab(spec, lootPrefab);
            GameObject bossSpawn = new GameObject("Boss Spawn Point");
            SceneManager.MoveGameObjectToScene(bossSpawn, scene);
            bossSpawn.transform.position = new Vector3(0f, 10f, 0f);
            SetField(bossTrigger, "bossPrefab", bossPrefab);
            SetField(bossTrigger, "bossSpawnPoint", bossSpawn.transform);

            GameObject chest = CreateSpriteObject("Reward Chest (Boss Reward)", chestSprite, new Vector3(0f, 11.8f, 0f), new Vector3(0.8f, 0.75f, 1f), "Layer 3", 2, scene);
            BoxCollider2D chestCollider = chest.AddComponent<BoxCollider2D>();
            chestCollider.isTrigger = true;
            DungeonRewardChest chestLogic = chest.AddComponent<DungeonRewardChest>();
            SetField(chestLogic, "chestVisual", chest.GetComponent<SpriteRenderer>());
            SetField(chestLogic, "rewardName", spec.reward);
            chest.SetActive(false);

            GameObject exit = CreatePortal("Exit Portal (Boss Reward)", portalSprite, new Vector2(0f, 13f), HomeSceneName, false, scene);
            exit.SetActive(false);
            SetField(bossTrigger, "rewardChest", chest);
            SetField(bossTrigger, "exitPortal", exit);
            SetField(bossTrigger, "bossGate", null);

            AddSceneLabel(scene, spec.name);
            string scenePath = SceneFolder + "/" + spec.sceneName + ".unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.CloseScene(scene, true);
        }

        private static GameObject BuildBossPrefab(DungeonSpec spec, GameObject lootPrefab)
        {
            Sprite sprite = MakeSprite("Boss_" + spec.sceneName, spec.boss, Hex("E9D7B1"), "boss");
            GameObject boss = new GameObject(spec.name + " Boss");
            SpriteRenderer renderer = boss.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Layer 3";
            renderer.sortingOrder = 2;
            boss.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
            Rigidbody2D body = boss.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            CircleCollider2D collider = boss.AddComponent<CircleCollider2D>();
            collider.radius = 0.42f;
            DungeonEnemyBase logic = boss.AddComponent<DungeonEnemyBase>();
            SetField(logic, "maxHealth", 18);
            SetField(logic, "moveSpeed", 1.25f);
            SetField(logic, "attackDamage", 2);
            SetField(logic, "attackCooldown", 0.8f);
            SetField(logic, "lootPrefab", lootPrefab);
            SetField(logic, "isBoss", true);
            string path = PrefabFolder + "/Boss_" + spec.sceneName + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(boss, path);
            UnityEngine.Object.DestroyImmediate(boss);
            return prefab;
        }

        private static (Tile floor, Tile floorAlt, Tile wall, Tile decor, Tile hazard) BuildTileAssets(DungeonSpec spec)
        {
            return (
                MakeTile(spec.sceneName + "_Floor", MakeSprite(spec.sceneName + "_Floor", spec.floor, spec.floorAccent, "floor"), Tile.ColliderType.None),
                MakeTile(spec.sceneName + "_FloorAlt", MakeSprite(spec.sceneName + "_FloorAlt", Color.Lerp(spec.floor, spec.floorAccent, 0.3f), spec.floorAccent, "floor"), Tile.ColliderType.None),
                MakeTile(spec.sceneName + "_Wall", MakeSprite(spec.sceneName + "_Wall", spec.wall, spec.wallAccent, "wall"), Tile.ColliderType.Grid),
                MakeTile(spec.sceneName + "_Decor", MakeSprite(spec.sceneName + "_Decor", Color.clear, spec.decor, "decor"), Tile.ColliderType.None),
                MakeTile(spec.sceneName + "_Hazard", MakeSprite(spec.sceneName + "_Hazard", spec.hazard, Color.Lerp(spec.hazard, Color.white, 0.4f), "hazard"), Tile.ColliderType.Grid)
            );
        }

        private static Tile MakeTile(string name, Sprite sprite, Tile.ColliderType colliderType)
        {
            string path = TileFolder + "/" + name + ".asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.name = name;
            tile.sprite = sprite;
            tile.colliderType = colliderType;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static Sprite MakeSprite(string name, Color32 baseColor, Color32 accent, string style)
        {
            string path = ArtFolder + "/" + name + ".png";
            if (!File.Exists(path))
            {
                const int size = 100;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
                Color32[] pixels = new Color32[size * size];
                var random = new System.Random(name.GetHashCode());
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    bool useAccent = false;
                    switch (style)
                    {
                        case "wall": useAccent = x < 4 || y < 4 || (y % 25 < 3 && random.Next(3) == 0); break;
                        case "masonry": useAccent = x % 20 < 3 || y % 20 < 3; break;
                        case "creature": useAccent = (x > 22 && x < 78 && y > 20 && y < 82 && ((x / 12 + y / 13) % 4 == 0)); break;
                        case "boss": useAccent = (x < 9 || x > 90 || y < 9 || y > 90 || (x > 34 && x < 66 && y > 34 && y < 66)); break;
                        case "chest": useAccent = y < 22 || x < 8 || x > 91 || (y > 45 && y < 54); break;
                        case "portal": useAccent = x < 8 || x > 91 || y < 5 || y > 94 || (x > 18 && x < 82 && (x + y) % 17 < 2); break;
                        case "gem": useAccent = (x > 22 && x < 78 && y > 10 && y < 90 && (Mathf.Abs(x - 50) + Mathf.Abs(y - 50) < 42)); break;
                        case "decor": useAccent = (x > 26 && x < 74 && y > 12 && y < 90 && (x % 17 < 4 || y % 23 < 4)); break;
                        default: useAccent = (x % 19 == 0 && y % 17 < 6) || (random.Next(70) == 0); break;
                    }
                    Color32 pixel = useAccent ? accent : baseColor;
                    if (style == "decor" && !useAccent) pixel.a = 0;
                    pixels[y * size + x] = pixel;
                }
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 100f || importer.filterMode != FilterMode.Point || importer.textureCompression != TextureImporterCompression.Uncompressed))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException("Failed to import placeholder sprite " + path);
            return sprite;
        }

        private static Tilemap CreateTilemap(Transform parent, string name, string sortingLayer, int order)
        {
            GameObject go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(parent, false);
            TilemapRenderer renderer = go.GetComponent<TilemapRenderer>();
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = order;
            renderer.mode = TilemapRenderer.Mode.Chunk;
            return go.GetComponent<Tilemap>();
        }

        private static void AddStaticTilemapCollider(Tilemap tilemap, bool isTrigger)
        {
            Rigidbody2D body = tilemap.gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            TilemapCollider2D collider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
            collider.isTrigger = isTrigger;
        }

        private static GameObject CreatePortal(string name, Sprite sprite, Vector2 position, string destination, bool requireInteract, Scene scene)
        {
            GameObject portal = CreateSpriteObject(name, sprite, new Vector3(position.x, position.y, 0f), new Vector3(1.4f, 1.8f, 1f), "Layer 3", 4, scene);
            CircleCollider2D trigger = portal.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.85f;
            DungeonPortal portalLogic = portal.AddComponent<DungeonPortal>();
            SetField(portalLogic, "destinationScene", destination);
            SetField(portalLogic, "requireInteract", requireInteract);
            SetField(portalLogic, "portalVisual", portal.GetComponent<SpriteRenderer>());
            return portal;
        }

        private static GameObject CreateSpriteObject(string name, Sprite sprite, Vector3 position, Vector3 scale, string sortingLayer, int order, Scene scene)
        {
            GameObject go = new GameObject(name, typeof(SpriteRenderer));
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.position = position;
            go.transform.localScale = scale;
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = order;
            return go;
        }

        private static void AddSceneLabel(Scene scene, string label)
        {
            GameObject labelObject = new GameObject("Dungeon Info");
            SceneManager.MoveGameObjectToScene(labelObject, scene);
            labelObject.AddComponent<DungeonSceneInfo>().DungeonName = label;
        }

        private static GameObject FindByName(GameObject[] roots, string name)
        {
            foreach (GameObject root in roots)
            {
                if (root.name == name) return root;
                Transform[] children = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                    if (child.name == name) return child.gameObject;
            }
            return null;
        }

        private static void SetField(UnityEngine.Object target, string fieldName, object value)
        {
            if (target == null) return;
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null) throw new InvalidOperationException("Missing serialized field '" + fieldName + "' on " + target.GetType().Name);
            if (value is UnityEngine.Object unityObject) property.objectReferenceValue = unityObject;
            else if (value is int intValue) property.intValue = intValue;
            else if (value is float floatValue) property.floatValue = floatValue;
            else if (value is bool boolValue) property.boolValue = boolValue;
            else if (value is string stringValue) property.stringValue = stringValue;
            else if (value is GameObject[] objects)
            {
                property.arraySize = objects.Length;
                for (int i = 0; i < objects.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
            }
            else if (value is Transform[] transforms)
            {
                property.arraySize = transforms.Length;
                for (int i = 0; i < transforms.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = transforms[i];
            }
            else if (value is int[] ints)
            {
                property.arraySize = ints.Length;
                for (int i = 0; i < ints.Length; i++) property.GetArrayElementAtIndex(i).intValue = ints[i];
            }
            else if (value == null && property.propertyType == SerializedPropertyType.ObjectReference) property.objectReferenceValue = null;
            else throw new InvalidOperationException("Unsupported value type for " + fieldName);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddScenesToBuildSettings(DungeonSpec[] specs)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (DungeonSpec spec in specs)
            {
                string path = SceneFolder + "/" + spec.sceneName + ".unity";
                bool alreadyAdded = scenes.Exists(scene => scene.path == path);
                if (!alreadyAdded) scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void WriteBuildSettingsFile(DungeonSpec[] specs)
        {
            string settingsPath = Path.GetFullPath("ProjectSettings/EditorBuildSettings.asset");
            if (!File.Exists(settingsPath)) return;
            string contents = File.ReadAllText(settingsPath);
            string additions = string.Empty;
            foreach (DungeonSpec spec in specs)
            {
                string scenePath = SceneFolder + "/" + spec.sceneName + ".unity";
                if (contents.Contains("path: " + scenePath)) continue;
                string guid = AssetDatabase.AssetPathToGUID(scenePath);
                additions += "  - enabled: 1\n    path: " + scenePath + "\n    guid: " + guid + "\n";
            }
            if (additions.Length == 0) return;
            const string insertionPoint = "  m_configObjects:";
            if (!contents.Contains(insertionPoint)) throw new InvalidOperationException("Could not append dungeon scenes to EditorBuildSettings.asset.");
            contents = contents.Replace(insertionPoint, additions + insertionPoint);
            File.WriteAllText(settingsPath, contents);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folder = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, folder);
        }

        private static Color32 Hex(string html)
        {
            ColorUtility.TryParseHtmlString("#" + html, out Color color);
            return color;
        }
    }
}
