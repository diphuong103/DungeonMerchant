using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Game.EditorTools
{
    [InitializeOnLoad]
    internal static class CryptOfBonesAutoBuild
    {
        private const string ScenePath = "Assets/Game/Scenes/Dungeons/Dungeon_CryptOfBones.unity";
        private const string BuildKey = "Game.CryptOfBones.AutoBuild.v1";

        static CryptOfBonesAutoBuild()
        {
            if (EditorPrefs.GetBool(BuildKey) || File.Exists(ScenePath)) return;
            EditorApplication.delayCall += () =>
            {
                if (EditorPrefs.GetBool(BuildKey) || File.Exists(ScenePath)) return;
                CryptOfBonesSceneBuilder.Build();
                EditorPrefs.SetBool(BuildKey, true);
            };
        }
    }

    internal static class CryptOfBonesSceneBuilder
    {
        private const string ScenePath = "Assets/Game/Scenes/Dungeons/Dungeon_CryptOfBones.unity";
        private const string EnemyFolder = "Assets/Game/Prefabs/Enemies";
        private const string BossFolder = "Assets/Game/Prefabs/Bosses";
        private const string DungeonFolder = "Assets/Game/Prefabs/Dungeon";
        private const string ArtFolder = "Assets/Game/Art/Dungeons/CryptOfBones";
        private const string TileFolder = "Assets/Game/Art/Dungeons/Tiles";
        private static readonly Color32 Bone = new Color32(218, 222, 224, 255);
        private static readonly Color32 Stone = new Color32(93, 111, 132, 255);
        private static readonly Color32 DarkStone = new Color32(34, 43, 57, 255);
        private static readonly Color32 Purple = new Color32(173, 91, 224, 255);
        private static readonly Color32 TorchOrange = new Color32(255, 143, 55, 255);

        [MenuItem("Dungeon Merchant/Build Crypt of Bones")]
        public static void Build()
        {
            if (File.Exists(ScenePath))
            {
                Debug.LogWarning("Dungeon_CryptOfBones already exists; the builder left it untouched.");
                return;
            }

            EnsureFolders();
            Sprite warriorSprite = CreatePixelSprite("Crypt_SkeletonWarrior_Placeholder.png", MakeSkeletonPixels);
            Sprite archerSprite = CreatePixelSprite("Crypt_SkeletonArcher_Placeholder.png", MakeArcherPixels);
            Sprite boneLordSprite = CreatePixelSprite("Crypt_BoneLord_Placeholder.png", MakeBoneLordPixels);
            Sprite tombstoneSprite = CreatePixelSprite("Crypt_Tombstone_Placeholder.png", MakeTombstonePixels);
            Sprite pillarSprite = CreatePixelSprite("Crypt_Pillar_Placeholder.png", MakePillarPixels);
            Sprite torchSprite = CreatePixelSprite("Crypt_Torch_Placeholder.png", MakeTorchPixels);
            Sprite flameSprite = CreatePixelSprite("Crypt_Flame_Placeholder.png", MakeFlamePixels);
            Sprite portalSprite = CreatePixelSprite("Crypt_Portal_Placeholder.png", MakePortalPixels);
            Sprite sarcophagusSprite = CreatePixelSprite("Crypt_Sarcophagus_Placeholder.png", MakeSarcophagusPixels);
            Sprite purpleShotSprite = CreatePixelSprite("Crypt_PurpleProjectile_Placeholder.png", MakePurpleProjectilePixels);
            Sprite arrowSprite = CreatePixelSprite("Crypt_BoneArrow_Placeholder.png", MakeArrowPixels);
            Sprite chestSprite = CreatePixelSprite("Crypt_Chest_Placeholder.png", MakeChestPixels);

            TileBase floorTile = LoadPaletteTile("TP Stone Ground.prefab", new Vector3Int(0, -13, 0));
            TileBase wallTile = LoadPaletteTile("TP Wall.prefab", new Vector3Int(0, -6, 0));
            TileBase decorTile = LoadPaletteTile("TP Stone Ground.prefab", new Vector3Int(1, -13, 0));
            if (floorTile == null)
                floorTile = CreateTile("Crypt_Floor.asset", CreatePixelSprite("Crypt_Floor_Placeholder.png", MakeFloorPixels), Tile.ColliderType.None);
            if (wallTile == null)
                wallTile = CreateTile("Crypt_Wall.asset", CreatePixelSprite("Crypt_Wall_Placeholder.png", MakeWallPixels), Tile.ColliderType.Grid);
            if (decorTile == null) decorTile = floorTile;

            GameObject purpleProjectile = CreateProjectilePrefab("CryptPurpleProjectile", purpleShotSprite, Purple, 8f, 3);
            GameObject boneArrow = CreateProjectilePrefab("CryptBoneArrow", arrowSprite, Bone, 7f, 2);
            GameObject skeletonWarrior = CreateEnemyPrefab("SkeletonWarrior", warriorSprite, 30, 4, 1.8f);
            GameObject skeletonArcher = CreateArcherPrefab("SkeletonArcher", archerSprite, boneArrow);
            GameObject boneLord = CreateBoneLordPrefab("BoneLord", boneLordSprite, skeletonWarrior, purpleProjectile);
            GameObject chestPrefab = CreateChestPrefab("CryptRewardChest", chestSprite);
            GameObject exitPrefab = CreateExitPortalPrefab("CryptExitPortal", portalSprite);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            Scene previousActiveScene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            GameObject root = new GameObject("Dungeon_CryptOfBones");

            BuildRoom(root.transform, floorTile, wallTile, decorTile, pillarSprite, tombstoneSprite,
                torchSprite, flameSprite, sarcophagusSprite, portalSprite, skeletonWarrior,
                skeletonArcher, boneLord, chestPrefab, exitPrefab, purpleProjectile);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Selection.activeGameObject = root;
            if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                SceneManager.SetActiveScene(scene);

            Debug.Log("Built Dungeon_CryptOfBones with the project's stone tiles. Colored placeholders: skeletons, archer, Bone Lord, tombstones, pillars, torches, portal, sarcophagus, projectiles, and chest.");
        }

        private static void BuildRoom(Transform root, TileBase floorTile, TileBase wallTile, TileBase decorTile,
            Sprite pillarSprite, Sprite tombstoneSprite, Sprite torchSprite, Sprite flameSprite,
            Sprite sarcophagusSprite, Sprite portalSprite, GameObject warriorPrefab,
            GameObject archerPrefab, GameObject bossPrefab, GameObject chestPrefab,
            GameObject exitPrefab, GameObject projectilePrefab)
        {
            GameObject gridObject = new GameObject("Grid", typeof(Grid));
            gridObject.transform.SetParent(root, false);
            Grid grid = gridObject.GetComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            Tilemap ground = CreateTilemap(gridObject.transform, "Ground", "Ground");
            Fill(ground, floorTile, -15, -9, 14, 8);

            Tilemap walls = CreateTilemap(gridObject.transform, "Walls", "Walls");
            FillPerimeter(walls, wallTile, -15, -9, 14, 8);
            Rigidbody2D wallBody = walls.gameObject.AddComponent<Rigidbody2D>();
            wallBody.bodyType = RigidbodyType2D.Static;
            TilemapCollider2D wallTilesCollider = walls.gameObject.AddComponent<TilemapCollider2D>();
            wallTilesCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            CompositeCollider2D composite = walls.gameObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Outlines;

            Tilemap decor = CreateTilemap(gridObject.transform, "Decor", "Decor");
            PaintDecor(decor, decorTile);

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(root, false);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.backgroundColor = new Color(0.055f, 0.07f, 0.10f, 1f);
            cameraObject.AddComponent<Cainos.PixelArtTopDown_Basic.CameraFollow>();

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Characters/Dungeon Player.prefab");
            GameObject player = playerPrefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, SceneManager.GetActiveScene())
                : CreateFallbackPlayer(root);
            player.name = "Player - Crypt Entrance";
            player.tag = "Player";
            player.transform.SetParent(root, true);
            player.transform.position = new Vector3(0f, -7f, 0f);
            cameraObject.GetComponent<Cainos.PixelArtTopDown_Basic.CameraFollow>().target = player.transform;

            GameObject entrancePortal = InstantiateUnder(exitPrefab, root, "Entrance Portal - Return Home", new Vector3(0f, -8f, 0f));
            ExitPortal returnPortal = entrancePortal.GetComponent<ExitPortal>();
            returnPortal.sceneToLoad = "Home";
            entrancePortal.tag = "Portal";

            BuildTorch(root, torchSprite, flameSprite, -13.5f, -7.5f);
            BuildTorch(root, torchSprite, flameSprite, 13.5f, -7.5f);
            BuildTorch(root, torchSprite, flameSprite, -13.5f, 7.5f);
            BuildTorch(root, torchSprite, flameSprite, 13.5f, 7.5f);

            BuildPillar(root, pillarSprite, -9f, -3f);
            BuildPillar(root, pillarSprite, 9f, -3f);
            BuildPillar(root, pillarSprite, -9f, 3f);
            BuildPillar(root, pillarSprite, 9f, 3f);

            GameObject[] tombstones = new GameObject[6];
            Vector2[] tombstonePositions =
            {
                new Vector2(-5f, -2f), new Vector2(-3.5f, -2f),
                new Vector2(5f, -2f), new Vector2(3.5f, -2f),
                new Vector2(-11f, 2f), new Vector2(11f, 2f)
            };
            for (int i = 0; i < tombstones.Length; i++)
                tombstones[i] = BuildTombstone(root, tombstoneSprite, warriorPrefab, player.transform,
                    tombstonePositions[i]);

            GameObject sarcophagus = CreateProp(root, "Bone Lord Sarcophagus", sarcophagusSprite,
                new Vector3(0f, 7f, 0f), new Vector3(2.5f, 1.35f, 1f), "Decor", true, null);
            sarcophagus.layer = SafeLayer("Walls");

            GameObject gateVisual = CreateProp(root, "Locked Gate", pillarSprite,
                new Vector3(0f, 3.5f, 0f), new Vector3(4.5f, 0.5f, 1f), "Entities", false, null);
            gateVisual.GetComponent<SpriteRenderer>().color = DarkStone;
            BoxCollider2D gateCollider = gateVisual.AddComponent<BoxCollider2D>();
            gateCollider.size = new Vector2(1f, 1f);
            Gate gate = gateVisual.AddComponent<Gate>();
            gate.blockingCollider = gateCollider;
            gate.closedVisual = gateVisual;
            gate.startsLocked = true;
            gateVisual.layer = SafeLayer("Walls");

            GameObject waveOne = CreateWaveSpawner(root, "EnemySpawner - Wave 1", new Vector3(-7f, 0f, 0f),
                warriorPrefab, 4, null, 0, player.transform);
            GameObject waveTwo = CreateWaveSpawner(root, "EnemySpawner - Wave 2", new Vector3(7f, 0f, 0f),
                warriorPrefab, 6, archerPrefab, 2, player.transform);
            GameObject waveThree = CreateWaveSpawner(root, "EnemySpawner - Wave 3", new Vector3(0f, -4f, 0f),
                warriorPrefab, 8, archerPrefab, 4, player.transform);

            GameObject controllerObject = new GameObject("DungeonController");
            controllerObject.transform.SetParent(root, false);
            DungeonController controller = controllerObject.AddComponent<DungeonController>();
            controller.gate = gate;
            controller.playerTarget = player.transform;
            controller.waveSpawners.Add(waveOne.GetComponent<EnemySpawner>());
            controller.waveSpawners.Add(waveTwo.GetComponent<EnemySpawner>());
            controller.waveSpawners.Add(waveThree.GetComponent<EnemySpawner>());
            controller.bossPrefab = bossPrefab;
            controller.bossSpawnPoint = CreatePoint(root, "Bone Lord Rise Point", new Vector3(0f, 5.7f, 0f)).transform;
            controller.chestPrefab = chestPrefab;
            controller.chestSpawnPoint = CreatePoint(root, "Reward Chest Spawn Point", new Vector3(3f, 6f, 0f)).transform;
            controller.exitPortalPrefab = exitPrefab;
            controller.exitPortalSpawnPoint = CreatePoint(root, "Boss Exit Portal Spawn Point", new Vector3(0f, 4.8f, 0f)).transform;

            GameObject bossTriggerObject = new GameObject("Boss Room Trigger");
            bossTriggerObject.transform.SetParent(root, false);
            bossTriggerObject.transform.position = new Vector3(0f, 5.4f, 0f);
            BoxCollider2D bossTriggerCollider = bossTriggerObject.AddComponent<BoxCollider2D>();
            bossTriggerCollider.isTrigger = true;
            bossTriggerCollider.size = new Vector2(7f, 3f);
            BossRoomTrigger bossTrigger = bossTriggerObject.AddComponent<BossRoomTrigger>();
            bossTrigger.controller = controller;
            controller.bossRoomTrigger = bossTrigger;

        }

        private static GameObject CreateEnemyPrefab(string name, Sprite sprite, int hp, int damage, float speed)
        {
            string path = EnemyFolder + "/" + name + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            EnsureFolders();
            GameObject actor = new GameObject(name);
            actor.layer = SafeLayer("Enemy");
            actor.tag = "Enemy";
            SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Entities";
            Rigidbody2D body = actor.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            CapsuleCollider2D collider = actor.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.7f, 0.95f);
            EnemyBase enemy = actor.AddComponent<EnemyBase>();
            enemy.MaxHP = hp;
            enemy.MoveSpeed = speed;
            enemy.AttackDamage = damage;
            enemy.MeleeRange = 0.9f;
            enemy.ChaseRange = 9f;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(actor, path);
            UnityEngine.Object.DestroyImmediate(actor);
            return prefab;
        }

        private static GameObject CreateArcherPrefab(string name, Sprite sprite, GameObject projectile)
        {
            string path = EnemyFolder + "/" + name + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            GameObject actor = new GameObject(name);
            actor.layer = SafeLayer("Enemy");
            actor.tag = "Enemy";
            SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Entities";
            Rigidbody2D body = actor.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            CapsuleCollider2D collider = actor.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.7f, 0.95f);
            SkeletonArcher archer = actor.AddComponent<SkeletonArcher>();
            archer.MaxHP = 22;
            archer.MoveSpeed = 1.5f;
            archer.AttackRange = 6f;
            archer.ProjectileDamage = 2;
            archer.ProjectilePrefab = projectile;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(actor, path);
            UnityEngine.Object.DestroyImmediate(actor);
            return prefab;
        }

        private static GameObject CreateBoneLordPrefab(string name, Sprite sprite, GameObject skeleton, GameObject projectile)
        {
            string path = BossFolder + "/" + name + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            GameObject boss = new GameObject(name);
            boss.layer = SafeLayer("Enemy");
            boss.tag = "Boss";
            SpriteRenderer renderer = boss.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Entities";
            renderer.sortingOrder = 4;
            Rigidbody2D body = boss.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            CircleCollider2D collider = boss.AddComponent<CircleCollider2D>();
            collider.radius = 0.7f;
            BoneLord boneLord = boss.AddComponent<BoneLord>();
            boneLord.MaxHP = 180;
            boneLord.MoveSpeed = 1.1f;
            boneLord.AttackDamage = 5;
            boneLord.MeleeRange = 1.2f;
            boneLord.ChaseRange = 10f;
            boneLord.SkeletonPrefab = skeleton;
            boneLord.SkeletonsToSummon = 3;
            boneLord.PurpleProjectilePrefab = projectile;
            boneLord.ProjectileDamage = 4;
            boneLord.ProjectileCooldown = 1.7f;
            boneLord.RiseOffset = 1.4f;
            boneLord.RiseDuration = 1f;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(boss, path);
            UnityEngine.Object.DestroyImmediate(boss);
            return prefab;
        }

        private static GameObject CreateProjectilePrefab(string name, Sprite sprite, Color tint, float speed, int damage)
        {
            string path = EnemyFolder + "/" + name + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            GameObject shot = new GameObject(name);
            shot.layer = SafeLayer("Enemy");
            SpriteRenderer renderer = shot.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingLayerName = "Entities";
            Rigidbody2D body = shot.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            CircleCollider2D collider = shot.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            BossProjectile projectile = shot.AddComponent<BossProjectile>();
            projectile.Damage = damage;
            projectile.Speed = speed;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(shot, path);
            UnityEngine.Object.DestroyImmediate(shot);
            return prefab;
        }

        private static GameObject CreateChestPrefab(string name, Sprite sprite)
        {
            string path = DungeonFolder + "/" + name + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            GameObject chest = new GameObject(name);
            chest.tag = "Chest";
            SpriteRenderer renderer = chest.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Entities";
            BoxCollider2D collider = chest.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            chest.AddComponent<Chest>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(chest, path);
            UnityEngine.Object.DestroyImmediate(chest);
            return prefab;
        }

        private static GameObject CreateExitPortalPrefab(string name, Sprite sprite)
        {
            string path = DungeonFolder + "/" + name + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            GameObject portal = new GameObject(name);
            portal.tag = "Portal";
            SpriteRenderer renderer = portal.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "Entities";
            BoxCollider2D collider = portal.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            portal.AddComponent<ExitPortal>().sceneToLoad = "Home";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(portal, path);
            UnityEngine.Object.DestroyImmediate(portal);
            return prefab;
        }

        private static GameObject CreateFallbackPlayer(Transform root)
        {
            GameObject player = CreateProp(root, "Player", null, new Vector3(0f, -7f, 0f), Vector3.one,
                "Entities", false, null);
            player.tag = "Player";
            player.layer = SafeLayer("Player");
            player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            player.AddComponent<CircleCollider2D>();
            return player;
        }

        private static GameObject CreateWaveSpawner(Transform root, string name, Vector3 position,
            GameObject primary, int primaryCount, GameObject additional, int additionalCount, Transform player)
        {
            GameObject spawnerObject = new GameObject(name);
            spawnerObject.transform.SetParent(root, false);
            spawnerObject.transform.position = position;
            EnemySpawner spawner = spawnerObject.AddComponent<EnemySpawner>();
            spawner.playerTarget = player;
            spawner.waves.Add(new EnemySpawner.Wave
            {
                prefab = primary,
                count = primaryCount,
                delay = 0.45f,
                additionalPrefab = additional,
                additionalCount = additionalCount
            });
            for (int i = 0; i < 8; i++)
            {
                GameObject point = new GameObject("Spawn " + (i + 1));
                point.transform.SetParent(spawnerObject.transform, false);
                float angle = i * Mathf.PI * 0.25f;
                point.transform.localPosition = new Vector3(Mathf.Cos(angle) * 2.2f, Mathf.Sin(angle) * 1.5f, 0f);
                spawner.spawnPoints.Add(point.transform);
            }
            return spawnerObject;
        }

        private static GameObject BuildTombstone(Transform root, Sprite sprite, GameObject skeleton,
            Transform player, Vector2 position)
        {
            GameObject tombstone = CreateProp(root, "Tombstone - Cracked", sprite,
                new Vector3(position.x, position.y, 0f), new Vector3(0.8f, 1f, 1f), "Decor", true, null);
            tombstone.layer = SafeLayer("Walls");
            TombstoneSpawner spawner = tombstone.AddComponent<TombstoneSpawner>();
            spawner.skeletonPrefab = skeleton;
            spawner.playerTarget = player;
            spawner.spawnInterval = 18f;
            spawner.maxSkeletonsSpawned = 2;
            spawner.crackOpenDuration = 0.8f;
            spawner.tombstoneRenderer = tombstone.GetComponent<SpriteRenderer>();
            GameObject spawnPoint = new GameObject("Skeleton Emergence Point");
            spawnPoint.transform.SetParent(tombstone.transform, false);
            spawnPoint.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            spawner.spawnPoint = spawnPoint.transform;
            return tombstone;
        }

        private static void BuildPillar(Transform root, Sprite sprite, float x, float y)
        {
            GameObject pillar = CreateProp(root, "Stone Pillar", sprite, new Vector3(x, y, 0f),
                new Vector3(1.2f, 1.5f, 1f), "Decor", true, null);
            pillar.layer = SafeLayer("Walls");
            pillar.GetComponent<SpriteRenderer>().color = Stone;
        }

        private static void BuildTorch(Transform root, Sprite torch, Sprite flame, float x, float y)
        {
            GameObject torchObject = CreateProp(root, "Corner Torch", torch, new Vector3(x, y, 0f),
                new Vector3(0.7f, 1f, 1f), "Overhead", false, null);
            torchObject.GetComponent<SpriteRenderer>().color = Stone;
            GameObject flameObject = CreateProp(torchObject.transform, "Orange Flame", flame,
                new Vector3(0f, 0.32f, -0.1f), new Vector3(0.75f, 0.75f, 1f), "Overhead", false, null);
            flameObject.transform.localPosition = new Vector3(0f, 0.32f, -0.1f);
            flameObject.GetComponent<SpriteRenderer>().color = TorchOrange;
            AddPointLight(flameObject, new Color(1f, 0.42f, 0.12f, 1f), 1.6f, 4f);
        }

        private static void AddPointLight(GameObject target, Color color, float intensity, float radius)
        {
            Type lightType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
            if (lightType == null) return;
            Component light = target.AddComponent(lightType);
            SetProperty(lightType, light, "color", color);
            SetProperty(lightType, light, "intensity", intensity);
            SetProperty(lightType, light, "pointLightOuterRadius", radius);
            var typeProperty = lightType.GetProperty("lightType");
            if (typeProperty != null && typeProperty.PropertyType.IsEnum)
                typeProperty.SetValue(light, Enum.Parse(typeProperty.PropertyType, "Point"), null);
        }

        private static void SetProperty(Type type, Component component, string propertyName, object value)
        {
            var property = type.GetProperty(propertyName);
            if (property != null && property.CanWrite && property.PropertyType.IsInstanceOfType(value))
                property.SetValue(component, value, null);
        }

        private static GameObject CreateProp(Transform parent, string name, Sprite sprite, Vector3 position,
            Vector3 scale, string sortingLayer, bool obstacle, Color? tint)
        {
            GameObject prop = new GameObject(name);
            prop.transform.SetParent(parent, false);
            prop.transform.position = position;
            prop.transform.localScale = scale;
            if (sprite != null)
            {
                SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingLayerName = sortingLayer;
                renderer.sortingOrder = 1;
                if (tint.HasValue) renderer.color = tint.Value;
            }
            if (obstacle)
            {
                BoxCollider2D collider = prop.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
                collider.offset = new Vector2(0f, 0.05f);
            }
            return prop;
        }

        private static GameObject CreatePoint(Transform parent, string name, Vector3 position)
        {
            GameObject point = new GameObject(name);
            point.transform.SetParent(parent, false);
            point.transform.position = position;
            return point;
        }

        private static GameObject InstantiateUnder(GameObject prefab, Transform parent, string name, Vector3 position)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
            instance.name = name;
            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            instance.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
            return instance;
        }

        private static Tilemap CreateTilemap(Transform parent, string name, string sortingLayer)
        {
            GameObject tilemapObject = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            tilemapObject.transform.SetParent(parent, false);
            Tilemap tilemap = tilemapObject.GetComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObject.GetComponent<TilemapRenderer>();
            renderer.sortingLayerName = sortingLayer;
            renderer.mode = TilemapRenderer.Mode.Chunk;
            if (name == "Ground")
                tilemap.color = new Color(0.58f, 0.68f, 0.80f, 1f);
            else if (name == "Walls")
            {
                tilemapObject.layer = SafeLayer("Walls");
                tilemap.color = new Color(0.34f, 0.40f, 0.50f, 1f);
            }
            else
                tilemap.color = new Color(0.64f, 0.68f, 0.77f, 0.78f);
            return tilemap;
        }

        private static void Fill(Tilemap tilemap, TileBase tile, int minX, int minY, int maxX, int maxY)
        {
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }

        private static void FillPerimeter(Tilemap tilemap, TileBase tile, int minX, int minY, int maxX, int maxY)
        {
            for (int x = minX; x <= maxX; x++)
            {
                tilemap.SetTile(new Vector3Int(x, minY, 0), tile);
                tilemap.SetTile(new Vector3Int(x, maxY, 0), tile);
            }
            for (int y = minY; y <= maxY; y++)
            {
                tilemap.SetTile(new Vector3Int(minX, y, 0), tile);
                tilemap.SetTile(new Vector3Int(maxX, y, 0), tile);
            }
        }

        private static void PaintDecor(Tilemap tilemap, TileBase tile)
        {
            Vector3Int[] cells =
            {
                new Vector3Int(-12, -6, 0), new Vector3Int(-11, -6, 0),
                new Vector3Int(11, -6, 0), new Vector3Int(12, -6, 0),
                new Vector3Int(-12, 6, 0), new Vector3Int(-11, 6, 0),
                new Vector3Int(11, 6, 0), new Vector3Int(12, 6, 0),
                new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0),
                new Vector3Int(-1, 1, 0), new Vector3Int(1, 1, 0)
            };
            foreach (Vector3Int cell in cells) tilemap.SetTile(cell, tile);
        }

        private static TileBase LoadPaletteTile(string palettePrefabName, Vector3Int cell)
        {
            string path = "Assets/Game/Art/Tile Palette/" + palettePrefabName;
            GameObject palette = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Tilemap map = palette != null ? palette.GetComponentInChildren<Tilemap>(true) : null;
            return map != null ? map.GetTile(cell) : null;
        }

        private static Tile CreateTile(string fileName, Sprite sprite, Tile.ColliderType colliderType)
        {
            string path = TileFolder + "/" + fileName;
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile != null) return tile;
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.colliderType = colliderType;
            AssetDatabase.CreateAsset(tile, path);
            return tile;
        }

        private static Sprite CreatePixelSprite(string fileName, Action<Texture2D> draw)
        {
            string path = ArtFolder + "/" + fileName;
            if (!File.Exists(path))
            {
                Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                Color32[] clear = new Color32[32 * 32];
                for (int i = 0; i < clear.Length; i++) clear[i] = new Color32(0, 0, 0, 0);
                texture.SetPixels32(clear);
                draw(texture);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Game/Art/Dungeons", "CryptOfBones");
            EnsureFolder("Assets/Game/Art/Dungeons/Tiles", "");
            EnsureFolder("Assets/Game/Prefabs", "Enemies");
            EnsureFolder("Assets/Game/Prefabs", "Bosses");
            EnsureFolder("Assets/Game/Prefabs", "Dungeon");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (string.IsNullOrEmpty(child) || AssetDatabase.IsValidFolder(parent + "/" + child)) return;
            if (!AssetDatabase.IsValidFolder(parent)) Directory.CreateDirectory(parent);
            AssetDatabase.CreateFolder(parent, child);
        }

        private static int SafeLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            return layer < 0 ? 0 : layer;
        }

        private static void AddToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene scene in existing)
                if (scene.path == scenePath) return;
            Array.Resize(ref existing, existing.Length + 1);
            existing[existing.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = existing;
        }

        private static void MakeFloorPixels(Texture2D t)
        {
            FillRect(t, 0, 0, 32, 32, new Color32(84, 101, 122, 255));
            for (int y = 0; y < 32; y += 8)
                for (int x = (y / 8 % 2) * 8; x < 32; x += 16)
                    FillRect(t, x, y, 15, 7, new Color32(91, 109, 131, 255));
            FillRect(t, 0, 0, 32, 1, DarkStone);
            FillRect(t, 0, 16, 32, 1, new Color32(52, 64, 81, 255));
        }

        private static void MakeWallPixels(Texture2D t)
        {
            FillRect(t, 0, 0, 32, 32, new Color32(35, 42, 55, 255));
            FillRect(t, 1, 2, 30, 13, new Color32(48, 57, 72, 255));
            FillRect(t, 0, 16, 32, 2, new Color32(23, 29, 40, 255));
            FillRect(t, 2, 19, 28, 11, new Color32(39, 47, 62, 255));
            FillRect(t, 15, 2, 1, 13, DarkStone);
            FillRect(t, 7, 20, 1, 10, DarkStone);
            FillRect(t, 23, 20, 1, 10, DarkStone);
        }

        private static void MakeSkeletonPixels(Texture2D t)
        {
            DrawSkeletonBody(t, new Color32(205, 211, 213, 255), new Color32(69, 80, 95, 255));
            FillRect(t, 6, 12, 20, 3, new Color32(94, 65, 47, 255));
            FillRect(t, 24, 8, 2, 18, Bone);
            FillRect(t, 23, 26, 4, 2, Bone);
        }

        private static void MakeArcherPixels(Texture2D t)
        {
            DrawSkeletonBody(t, new Color32(215, 219, 218, 255), new Color32(81, 74, 92, 255));
            FillRect(t, 25, 5, 2, 21, new Color32(112, 71, 44, 255));
            FillRect(t, 27, 5, 1, 2, new Color32(170, 111, 58, 255));
            FillRect(t, 27, 24, 1, 2, new Color32(170, 111, 58, 255));
            FillRect(t, 23, 12, 6, 1, Bone);
        }

        private static void MakeBoneLordPixels(Texture2D t)
        {
            DrawSkeletonBody(t, new Color32(229, 224, 220, 255), new Color32(88, 62, 112, 255));
            FillRect(t, 8, 27, 3, 4, Purple);
            FillRect(t, 14, 27, 3, 4, Purple);
            FillRect(t, 20, 27, 3, 4, Purple);
            FillRect(t, 4, 17, 3, 8, new Color32(115, 71, 150, 255));
            FillRect(t, 25, 17, 3, 8, new Color32(115, 71, 150, 255));
            FillRect(t, 13, 20, 6, 2, new Color32(177, 98, 229, 255));
        }

        private static void DrawSkeletonBody(Texture2D t, Color32 bone, Color32 armor)
        {
            FillRect(t, 10, 22, 12, 9, bone);
            FillRect(t, 12, 18, 8, 4, bone);
            FillRect(t, 11, 10, 10, 9, armor);
            FillRect(t, 6, 10, 5, 8, bone);
            FillRect(t, 21, 10, 5, 8, bone);
            FillRect(t, 11, 2, 4, 9, bone);
            FillRect(t, 17, 2, 4, 9, bone);
            FillRect(t, 13, 25, 2, 2, DarkStone);
            FillRect(t, 18, 25, 2, 2, DarkStone);
            FillRect(t, 15, 22, 2, 2, new Color32(92, 100, 109, 255));
        }

        private static void MakeTombstonePixels(Texture2D t)
        {
            FillRect(t, 7, 4, 18, 20, new Color32(81, 96, 115, 255));
            FillRect(t, 9, 24, 14, 4, new Color32(81, 96, 115, 255));
            FillRect(t, 10, 26, 12, 3, new Color32(81, 96, 115, 255));
            FillRect(t, 9, 7, 2, 16, new Color32(125, 140, 158, 255));
            FillRect(t, 14, 15, 4, 8, new Color32(153, 157, 173, 255));
            FillRect(t, 12, 18, 8, 2, new Color32(153, 157, 173, 255));
            FillRect(t, 13, 8, 2, 7, new Color32(49, 59, 74, 255));
        }

        private static void MakePillarPixels(Texture2D t)
        {
            FillRect(t, 7, 3, 18, 27, new Color32(74, 88, 106, 255));
            FillRect(t, 4, 2, 24, 5, new Color32(123, 135, 150, 255));
            FillRect(t, 4, 26, 24, 5, new Color32(114, 126, 142, 255));
            FillRect(t, 11, 8, 2, 17, new Color32(132, 144, 158, 255));
            FillRect(t, 21, 8, 2, 17, new Color32(48, 58, 72, 255));
        }

        private static void MakeTorchPixels(Texture2D t)
        {
            FillRect(t, 14, 2, 5, 23, new Color32(99, 61, 42, 255));
            FillRect(t, 11, 22, 11, 4, new Color32(67, 47, 41, 255));
            FillRect(t, 12, 25, 9, 5, TorchOrange);
            FillRect(t, 15, 28, 4, 4, new Color32(255, 216, 103, 255));
        }

        private static void MakeFlamePixels(Texture2D t)
        {
            FillRect(t, 12, 3, 8, 14, new Color32(236, 85, 28, 255));
            FillRect(t, 14, 8, 5, 15, TorchOrange);
            FillRect(t, 15, 13, 3, 12, new Color32(255, 221, 110, 255));
            FillRect(t, 16, 22, 1, 6, new Color32(255, 246, 184, 255));
        }

        private static void MakePortalPixels(Texture2D t)
        {
            for (int y = 3; y < 29; y++)
                for (int x = 5; x < 27; x++)
                {
                    float dx = (x - 16f) / 10f;
                    float dy = (y - 16f) / 13f;
                    float d = dx * dx + dy * dy;
                    if (d < 1f && d > 0.52f) t.SetPixel(x, y, new Color32(149, 75, 206, 255));
                    else if (d < 0.52f) t.SetPixel(x, y, new Color32(60, 38, 91, 255));
                }
            FillRect(t, 13, 4, 2, 4, new Color32(209, 139, 255, 255));
            FillRect(t, 20, 22, 2, 4, new Color32(209, 139, 255, 255));
        }

        private static void MakeSarcophagusPixels(Texture2D t)
        {
            FillRect(t, 3, 5, 26, 20, new Color32(48, 58, 75, 255));
            FillRect(t, 5, 8, 22, 14, new Color32(82, 95, 115, 255));
            FillRect(t, 7, 10, 18, 10, new Color32(71, 80, 104, 255));
            FillRect(t, 14, 11, 4, 9, new Color32(171, 99, 206, 255));
            FillRect(t, 10, 14, 12, 2, new Color32(195, 133, 224, 255));
            FillRect(t, 2, 4, 28, 3, new Color32(133, 146, 164, 255));
        }

        private static void MakePurpleProjectilePixels(Texture2D t)
        {
            for (int y = 5; y < 27; y++)
                for (int x = 5; x < 27; x++)
                {
                    int dx = x - 16;
                    int dy = y - 16;
                    if (dx * dx + dy * dy <= 100) t.SetPixel(x, y, new Color32(149, 60, 215, 255));
                    if (dx * dx + dy * dy <= 36) t.SetPixel(x, y, new Color32(228, 180, 255, 255));
                }
        }

        private static void MakeArrowPixels(Texture2D t)
        {
            FillRect(t, 4, 15, 20, 2, Bone);
            FillRect(t, 22, 13, 6, 6, Bone);
            FillRect(t, 3, 12, 3, 3, new Color32(152, 93, 60, 255));
            FillRect(t, 3, 17, 3, 3, new Color32(152, 93, 60, 255));
        }

        private static void MakeChestPixels(Texture2D t)
        {
            FillRect(t, 5, 7, 22, 15, new Color32(101, 61, 35, 255));
            FillRect(t, 4, 19, 24, 5, new Color32(137, 88, 46, 255));
            FillRect(t, 5, 21, 22, 2, new Color32(224, 173, 70, 255));
            FillRect(t, 14, 8, 4, 10, new Color32(234, 194, 88, 255));
            FillRect(t, 14, 12, 4, 4, new Color32(62, 56, 64, 255));
        }

        private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color color)
        {
            for (int px = x; px < Mathf.Min(32, x + width); px++)
                for (int py = y; py < Mathf.Min(32, y + height); py++)
                    if (px >= 0 && py >= 0) texture.SetPixel(px, py, color);
        }
    }
}
