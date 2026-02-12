using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivorSeries.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public async Awaitable LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);
            while (!op.isDone)
                await Awaitable.NextFrameAsync();
        }

        public async Awaitable UnloadSceneAsync(string sceneName)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);
            while (!op.isDone)
                await Awaitable.NextFrameAsync();
        }

        public bool IsSceneLoaded(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).isLoaded;
        }
    }
}
