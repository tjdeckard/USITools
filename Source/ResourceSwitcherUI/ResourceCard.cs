using UnityEngine;
using UnityEngine.UI;

namespace ResourceSwitcherUI
{
    [RequireComponent(typeof(RectTransform))]
    public class ResourceCard : MonoBehaviour
    {
        [SerializeField]
        private Text TitleText;

        [SerializeField]
        private Text SubtitleText;

        public void SetValues(LoadoutMetadata loadout)
        {
            if (TitleText != null)
            {
                TitleText.text = loadout.ResourceDisplayName;
            }
            if (SubtitleText != null)
            {
                SubtitleText.text = $"{loadout.MaxUnits:F2} unit(s)";
            }
        }
    }
}
