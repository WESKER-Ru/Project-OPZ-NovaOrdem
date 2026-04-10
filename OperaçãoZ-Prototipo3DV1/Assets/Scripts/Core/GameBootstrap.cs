// Assets/Scripts/Core/GameBootstrap.cs
using UnityEngine;

namespace OPZ.Core
{
    /// <summary>
    /// First script to execute. Validates managers exist and initializes game state.
    /// Attach to a "GameBootstrap" GameObject in the scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void EnsureBootstrap()
        {
            // Optional: could instantiate a prefab here if needed
        }

        void Awake()
        {
            ValidateManagers();
        }

        void ValidateManagers()
        {
            RequireSingleton<GameManager>("GameManager");
            RequireSingleton<Economy.EconomyManager>("EconomyManager");
            RequireSingleton<SelectionManager>("SelectionManager");
            RequireSingleton<CommandSystem>("CommandSystem");
        }

        void RequireSingleton<T>(string name) where T : MonoBehaviour
        {
            if (FindAnyObjectByType<T>() == null)
            {
                Debug.LogError($"[GameBootstrap] Missing required manager: {name}. Creating fallback.");
                var go = new GameObject(name);
                go.AddComponent<T>();
            }
        }
    }
}
