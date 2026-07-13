using System.IO;
using Systems.SimpleBuilding.Components;
using Systems.SimpleBuilding.Data;
using Systems.SimpleBuilding.Examples;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Systems.SimpleBuilding.Editor
{
    /// <summary>
    ///     Rebuilds the package-local Building Playground example assets and scene.
    /// </summary>
    public static class BuildingExampleSceneGenerator
    {
        private const string EXAMPLES_PATH = "Assets/Systems/SimpleBuilding/Examples";
        private const string PREFABS_PATH = EXAMPLES_PATH + "/Prefabs";
        private const string MATERIALS_PATH = EXAMPLES_PATH + "/Materials";
        private const string ENTRIES_PATH = EXAMPLES_PATH + "/Entries";
        private const string SCENE_PATH = EXAMPLES_PATH + "/Scene - Building Playground.unity";

        [MenuItem("Simple Building/Regenerate Building Playground")]
        public static void Generate()
        {
            EnsureFolders();

            Material freeMaterial = GetOrCreateMaterial(MATERIALS_PATH + "/Free Building.mat", new Color(0.35f, 0.62f, 0.95f));
            Material slotMaterial = GetOrCreateMaterial(MATERIALS_PATH + "/Slot Building.mat", new Color(0.94f, 0.59f, 0.22f));
            Material validGhostMaterial = GetOrCreateMaterial(MATERIALS_PATH + "/Ghost Valid.mat", new Color(0.15f, 1f, 0.4f, 0.55f));
            Material invalidGhostMaterial = GetOrCreateMaterial(MATERIALS_PATH + "/Ghost Invalid.mat", new Color(1f, 0.15f, 0.2f, 0.55f));

            BuildingBase freeBuildingPrefab = CreateBuildingPrefab<ExampleBuilding>(
                PREFABS_PATH + "/Example Free Building.prefab", "Example Free Building", PrimitiveType.Cube, freeMaterial);
            BuildingBase slotBuildingPrefab = CreateBuildingPrefab<ExampleSlotBuilding>(
                PREFABS_PATH + "/Example Slot Building.prefab", "Example Slot Building", PrimitiveType.Cylinder, slotMaterial);
            ExampleBuildingEntry freeBuildingEntry = GetOrCreateEntry(
                ENTRIES_PATH + "/Example Free Building.asset", freeBuildingPrefab);
            ExampleBuildingEntry slotBuildingEntry = GetOrCreateEntry(
                ENTRIES_PATH + "/Example Slot Building.asset", slotBuildingPrefab);
            freeBuildingEntry.SetSaveIdentifier("example-free-building");
            slotBuildingEntry.SetSaveIdentifier("example-slot-building");
            BuildingGhostMaterialConfiguration ghostConfiguration = GetOrCreateGhostConfiguration(
                MATERIALS_PATH + "/Building Ghost Material Configuration.asset",
                validGhostMaterial,
                invalidGhostMaterial);

            CreateScene(freeBuildingEntry, slotBuildingEntry, ghostConfiguration);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateScene(
            ExampleBuildingEntry freeBuildingEntry,
            ExampleBuildingEntry slotBuildingEntry,
            BuildingGhostMaterialConfiguration ghostConfiguration)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject buildingsRoot = new GameObject("Placed Buildings");
            CreateGround();
            CreateSlots();
            Camera camera = CreateCamera();
            CreateLight();

            GameObject controllerObject = new GameObject("Building Controller");
            BuildingGhostPreview ghostPreview = controllerObject.AddComponent<BuildingGhostPreview>();
            ghostPreview.Configure(ghostConfiguration);
            PointerBuildingRaycaster raycaster = controllerObject.AddComponent<PointerBuildingRaycaster>();
            raycaster.Configure(buildingsRoot.transform, ghostPreview, Physics.DefaultRaycastLayers);
            raycaster.ConfigureCamera(camera);
            ExampleBuildingScene exampleScene = controllerObject.AddComponent<ExampleBuildingScene>();
            exampleScene.Configure(freeBuildingEntry, slotBuildingEntry, raycaster);

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
        }

        private static void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Free Placement Ground";
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
        }

        private static void CreateSlots()
        {
            for (int tileIndex = 0; tileIndex < 4; tileIndex++)
            {
                GameObject slotObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slotObject.name = "Building Slot " + (tileIndex + 1);
                slotObject.transform.position = new Vector3(4f + tileIndex % 2 * 1.2f, 0.1f, tileIndex / 2 * 1.2f);
                slotObject.transform.localScale = new Vector3(1f, 0.2f, 1f);
                BuildingSlot slot = slotObject.AddComponent<BuildingSlot>();
                slot.SetSaveIdentifier("example-building-slot-" + (tileIndex + 1));
            }
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(9f, 9f, -11f);
            cameraObject.transform.LookAt(new Vector3(1.5f, 0f, 1f));
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            return camera;
        }

        private static void CreateLight()
        {
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
        }

        private static BuildingBase CreateBuildingPrefab<TBuildingType>(
            string assetPath,
            string objectName,
            PrimitiveType primitiveType,
            Material material)
            where TBuildingType : BuildingBase
        {
            GameObject root = new GameObject(objectName);
            root.AddComponent<TBuildingType>();
            GameObject visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.up * 0.5f;
            Renderer renderer = visual.GetComponent<Renderer>();
            renderer.sharedMaterial = material;

            GameObject prefabObject = PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            Object.DestroyImmediate(root);
            return prefabObject.GetComponent<TBuildingType>();
        }

        private static ExampleBuildingEntry GetOrCreateEntry(string assetPath, BuildingBase prefab)
        {
            ExampleBuildingEntry entry = AssetDatabase.LoadAssetAtPath<ExampleBuildingEntry>(assetPath);
            if (ReferenceEquals(entry, null))
            {
                entry = ScriptableObject.CreateInstance<ExampleBuildingEntry>();
                AssetDatabase.CreateAsset(entry, assetPath);
            }

            entry.Configure(prefab);
            EditorUtility.SetDirty(entry);
            return entry;
        }

        private static BuildingGhostMaterialConfiguration GetOrCreateGhostConfiguration(
            string assetPath,
            Material validMaterial,
            Material invalidMaterial)
        {
            BuildingGhostMaterialConfiguration configuration =
                AssetDatabase.LoadAssetAtPath<BuildingGhostMaterialConfiguration>(assetPath);
            if (ReferenceEquals(configuration, null))
            {
                configuration = ScriptableObject.CreateInstance<BuildingGhostMaterialConfiguration>();
                AssetDatabase.CreateAsset(configuration, assetPath);
            }

            SerializedObject serializedConfiguration = new SerializedObject(configuration);
            serializedConfiguration.FindProperty("_validMaterial").objectReferenceValue = validMaterial;
            serializedConfiguration.FindProperty("_invalidMaterial").objectReferenceValue = invalidMaterial;
            serializedConfiguration.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(configuration);
            return configuration;
        }

        private static Material GetOrCreateMaterial(string assetPath, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (ReferenceEquals(material, null))
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (ReferenceEquals(shader, null)) shader = Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }

            material.color = color;
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(PREFABS_PATH);
            Directory.CreateDirectory(MATERIALS_PATH);
            Directory.CreateDirectory(ENTRIES_PATH);
        }
    }
}
