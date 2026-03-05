using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivorSeries.Core
{
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string _firstScene = "MainMenu";

        private async void Start()
        {
            await Awaitable.NextFrameAsync();

            if (!SceneManager.GetSceneByName(_firstScene).isLoaded)
                await SceneLoader.Instance.LoadSceneAsync(_firstScene, LoadSceneMode.Additive);

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_firstScene));
        }
    }
}