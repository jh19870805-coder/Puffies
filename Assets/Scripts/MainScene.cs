using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private static bool sHookedSceneLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!sHookedSceneLoaded)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            sHookedSceneLoaded = true;
        }

        TryBootstrap(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBootstrap(scene);
    }

    private static void TryBootstrap(Scene scene)
    {
        if (!IsMainScene(scene))
        {
            return;
        }

        if (FindObjectOfType<MainScene>() == null)
        {
            var bootstrapObject = new GameObject("MainSceneBootstrap");
            bootstrapObject.AddComponent<MainScene>();
        }
    }

    private static bool IsMainScene(Scene scene)
    {
        if (scene.name.Equals("MainScene", StringComparison.Ordinal))
        {
            return true;
        }

        return scene.path.EndsWith("/MainScene.unity", StringComparison.OrdinalIgnoreCase);
    }

    private void Start()
    {
        if (!IsMainScene(SceneManager.GetActiveScene()))
        {
            Destroy(gameObject);
            return;
        }

        GameManager.CreateInstance();

        if (Camera.main != null)
        {
            SetupMainCamera(Camera.main);
        }

        Debug.Log("MainScene init-only bootstrap completed.");
    }

    private static void SetupMainCamera(Camera camera)
    {
        camera.orthographic = true;
        camera.orthographicSize = ReferenceHeight / (2f * PixelsPerUnit);
    }
}
