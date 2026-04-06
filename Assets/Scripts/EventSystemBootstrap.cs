using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class EventSystemBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureEventSystemsReady();
    }

    static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureEventSystemsReady();
    }

    static void EnsureEventSystemsReady()
    {
        EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (eventSystems.Length == 0)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            ConfigureModule(eventSystemObject.GetComponent<InputSystemUIInputModule>());
            return;
        }

        for (int i = 0; i < eventSystems.Length; i++)
        {
            InputSystemUIInputModule module = eventSystems[i].GetComponent<InputSystemUIInputModule>();
            if (module == null)
                module = eventSystems[i].gameObject.AddComponent<InputSystemUIInputModule>();

            ConfigureModule(module);
        }
    }

    static void ConfigureModule(InputSystemUIInputModule module)
    {
        if (module == null)
            return;

        if (module.actionsAsset == null)
            module.AssignDefaultActions();
    }
}
