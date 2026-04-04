using UnityEngine;
using UnityEngine.SceneManagement;

public class KnifeSelectSceneBootstrap : MonoBehaviour
{
    static KnifeSelectSceneBootstrap instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (instance != null)
            return;

        GameObject bootstrapObject = new GameObject(nameof(KnifeSelectSceneBootstrap));
        DontDestroyOnLoad(bootstrapObject);
        instance = bootstrapObject.AddComponent<KnifeSelectSceneBootstrap>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name != KnifeSelectManager.SceneName)
            return;

        KnifeSelectManager.EnsureRuntimeSceneExists();
    }
}
