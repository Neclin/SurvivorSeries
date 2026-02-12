using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivorSeries.Core
{
    /// <summary>
    /// Placed in the Bootstrap scene (build index 0).
    /// Ensures persistent managers are loaded, then transitions to MainMenu.
    /// </summary>
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string _firstScene = "MainMenu";

        private async void Start()
        {
            // Give managers in this scene one frame to register themselves
            await Awaitable.NextFrameAsync();

            if (!SceneManager.GetSceneByName(_firstScene).isLoaded)
                await SceneLoader.Instance.LoadSceneAsync(_firstScene, LoadSceneMode.Additive);

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_firstScene));
        }
    }
}
