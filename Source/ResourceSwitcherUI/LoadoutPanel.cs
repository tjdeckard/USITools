using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ResourceSwitcherUI
{
    [RequireComponent(typeof(RectTransform))]
    public class LoadoutPanel : MonoBehaviour
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable 0169
#pragma warning disable 0649

        [SerializeField]
        private Text TitleText;

        [SerializeField]
        private Text SubtitleText;

        [SerializeField]
        private Image Thumbnail;

#pragma warning restore 0649
#pragma warning restore 0169
#pragma warning restore IDE0044

        protected List<LoadoutMetadata> _loadoutMetadata;

        public IResourceSwitcherLoadout Loadout { get; private set; }
        public event Action<LoadoutPanel> LoadoutSelected;

        public void OnSelect()
        {
            LoadoutSelected?.Invoke(this);
        }

        public void SetValues(IResourceSwitcherLoadout loadout)
        {
            Loadout = loadout;
            _loadoutMetadata = Loadout.GetLoadoutMetadata();

            if (TitleText != null)
            {
                TitleText.text = loadout.DisplayName;
            }
            if (SubtitleText != null &&
                _loadoutMetadata != null &&
                _loadoutMetadata.Any())
            {
                var resources = _loadoutMetadata.Select(l => l.ResourceDisplayName);
                SubtitleText.text = string.Join(" | ", resources);
            }
            if (Thumbnail != null)
            {
                var thumbnailTexture = loadout.GetThumbnail();
                Thumbnail.sprite = Sprite.Create(
                    thumbnailTexture,
                    new Rect(0, 0, 32, 32),
                    new Vector2(0.5f, 0.5f));
            }
        }
    }
}
