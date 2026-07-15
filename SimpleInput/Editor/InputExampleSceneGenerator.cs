using System.IO;
using Systems.SimpleInput.Examples;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systems.SimpleInput.Editor
{
    /// <summary>
    ///     Regenerates the package-local SimpleInput rebinding example scene.
    /// </summary>
    public static class InputExampleSceneGenerator
    {
        private const string EXAMPLES_PATH = "Assets/Systems/SimpleInput/Examples";
        private const string SCENE_PATH = EXAMPLES_PATH + "/Scene - Input Rebinding.unity";

        [MenuItem("Simple Input/Regenerate Input Rebinding Example")]
        public static void Generate()
        {
            Directory.CreateDirectory(EXAMPLES_PATH);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            GameObject controllerObject = new GameObject("Input Rebinding Example");
            controllerObject.AddComponent<ExampleInputRebindingScene>();

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(11, 11, 9, 255);
            cameraObject.AddComponent<AudioListener>();
        }
    }
}
