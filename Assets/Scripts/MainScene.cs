using UnityEngine;

public class MainScene : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<MainScene>() != null)
        {
            return;
        }

        var bootstrapObject = new GameObject("MainSceneBootstrap");
        bootstrapObject.AddComponent<MainScene>();
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.1f, 0.15f, 0.25f);
        }
        Debug.Log("MainScene initialized.");
    }
}
