using UnityEngine;
using UnityEngine.UI;

namespace ResourceSwitcherUI
{
    [RequireComponent(typeof(RectTransform))]
    public class PartPanel : MonoBehaviour
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable 0649

        [SerializeField]
        private Text TitleText;

        [SerializeField]
        private Text SubtitleText;

        [SerializeField]
        private Image Thumbnail;

#pragma warning restore 0649
#pragma warning restore IDE0044

        public void OnToggle(bool selected)
        {
            if (Switcher != null)
            {
                if (selected)
                {
                    Switcher.Controller.SelectSwitcher(this);
                }
                else
                {
                    Switcher.Controller.DeselectSwitcher(this);
                }
            }
        }

        public IResourceSwitcher Switcher { get; private set; }

        public void SetValues(IResourceSwitcher switcher)
        {
            Switcher = switcher;

            if (TitleText != null)
            {
                TitleText.text = switcher.DisplayName;
            }
            if (SubtitleText != null)
            {
                SubtitleText.text = switcher.Resources;
            }
            if (Thumbnail != null)
            {
                var texture = switcher.GetThumbnail();
                if (texture != null)
                {
                    Thumbnail.sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, 256, 256),
                        new Vector2(0.5f, 0.5f));
                }
            }
        }
    }
}
