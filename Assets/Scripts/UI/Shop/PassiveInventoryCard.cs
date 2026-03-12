using UnityEngine;
using TMPro;
using SurvivorSeries.Passives.Data;

namespace SurvivorSeries.UI.Shop
{
    public class PassiveInventoryCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject _emptyOverlay;

        public void Bind(PassiveItemDataSO data, int level, int maxLevel)
        {
            if (_emptyOverlay != null) _emptyOverlay.SetActive(data == null);
            if (data == null)
            {
                _nameText.text = "—";
                _levelText.text = "";
                return;
            }
            _nameText.text = data.ItemName;
            _levelText.text = level >= maxLevel ? $"Lv. {level} (MAX)" : $"Lv. {level} / {maxLevel}";
        }
    }
}
