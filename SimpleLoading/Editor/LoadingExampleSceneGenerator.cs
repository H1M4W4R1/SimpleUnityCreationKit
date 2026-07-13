using System.IO;
using Systems.SimpleLoading.Components;
using Systems.SimpleLoading.Examples;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Systems.SimpleLoading.Editor
{
    /// <summary>Generates the package-local SimpleLoading example scene.</summary>
    public static class LoadingExampleSceneGenerator
    {
        private const string EXAMPLES_PATH = "Assets/Systems/SimpleLoading/Examples";
        private const string SCENE_PATH = EXAMPLES_PATH + "/Scene - Loading.unity";

        [MenuItem("Simple Loading/Regenerate Loading Example")]
        public static void Generate()
        {
            Directory.CreateDirectory(EXAMPLES_PATH);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject loadingSystemObject = new GameObject("Loading System");
            loadingSystemObject.AddComponent<LoadingSystem>();
            Transform player = CreatePlayer();
            DynamicWorldPart worldPart = CreateDynamicWorldPart();
            Slider progressBar = CreateLoadingScreen(out GameObject loadingScreenRoot);
            CreateCamera();
            CreateLight();

            GameObject controllerObject = new GameObject("Loading Example Controller");
            ExampleLoadingScene controller = controllerObject.AddComponent<ExampleLoadingScene>();
            controller.Configure(player, worldPart, loadingScreenRoot, progressBar);

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Transform CreatePlayer()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            player.name = "Player (Arrow Keys / WASD)";
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.AddComponent<ExampleLoadingPlayerMover>();
            return player.transform;
        }

        private static DynamicWorldPart CreateDynamicWorldPart()
        {
            GameObject partObject = new GameObject("Dynamic World Part Controller");
            DynamicWorldPart worldPart = partObject.AddComponent<DynamicWorldPart>();

            GameObject worldRoot = new GameObject("World Root (loads within 8m, unloads after 12m)");
            worldRoot.transform.SetParent(partObject.transform, false);
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Streamed World Visual";
            visual.transform.SetParent(worldRoot.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localScale = new Vector3(5f, 2f, 5f);
            Renderer renderer = visual.GetComponent<Renderer>();
            renderer.sharedMaterial = CreateMaterial();
            return worldPart;
        }

        private static Slider CreateLoadingScreen(out GameObject loadingScreenRoot)
        {
            GameObject canvasObject = new GameObject("Loading Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            loadingScreenRoot = new GameObject("Loading Screen");
            loadingScreenRoot.transform.SetParent(canvasObject.transform, false);
            RectTransform screenTransform = loadingScreenRoot.AddComponent<RectTransform>();
            screenTransform.anchorMin = new Vector2(0.5f, 0.5f);
            screenTransform.anchorMax = new Vector2(0.5f, 0.5f);
            screenTransform.sizeDelta = new Vector2(440f, 70f);
            Image screenImage = loadingScreenRoot.AddComponent<Image>();
            screenImage.color = new Color(0.05f, 0.08f, 0.14f, 0.9f);

            GameObject sliderObject = new GameObject("Progress");
            sliderObject.transform.SetParent(loadingScreenRoot.transform, false);
            RectTransform sliderTransform = sliderObject.AddComponent<RectTransform>();
            sliderTransform.anchorMin = new Vector2(0.08f, 0.3f);
            sliderTransform.anchorMax = new Vector2(0.92f, 0.7f);
            sliderTransform.offsetMin = Vector2.zero;
            sliderTransform.offsetMax = Vector2.zero;
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            loadingScreenRoot.SetActive(false);
            return slider;
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(9f, 9f, -11f);
            cameraObject.transform.LookAt(new Vector3(0f, 0f, 0f));
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

        private static Material CreateMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (ReferenceEquals(shader, null)) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            Color color = new Color(0.18f, 0.62f, 1f);
            material.color = color;
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            return material;
        }
    }
}
