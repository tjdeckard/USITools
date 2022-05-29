using System.Collections.Generic;
using UnityEngine;

namespace ResourceSwitcherUI
{
    public interface IResourceSwitcherLoadout
    {
        string DisplayName { get; }
        List<LoadoutMetadata> GetLoadoutMetadata();
        Texture2D GetThumbnail();
        string UniqueId { get; }
    }
}
