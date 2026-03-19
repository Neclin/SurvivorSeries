using UnityEngine;
using UnityEngine.UI;

namespace SurvivorSeries.Audio
{
    [RequireComponent(typeof(Button))]
    public class ButtonClickSfx : MonoBehaviour
    {
        [SerializeField] private SfxId _sfxId = SfxId.UIClick;

        private void Awake()
        {
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(PlayClick);
        }

        private void PlayClick() => AudioManager.Play(_sfxId);
    }
}
