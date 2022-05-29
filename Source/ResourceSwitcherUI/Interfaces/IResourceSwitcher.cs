using System.Collections.Generic;
using UnityEngine;

namespace ResourceSwitcherUI
{
    public interface IResourceSwitcher
    {
        IResourceSwitcherController Controller { get; }
        string DisplayName { get; }
        List<IResourceSwitcherLoadout> GetLoadouts();
        Texture2D GetThumbnail();
        string Resources { get; }
        void SelectLoadout(IResourceSwitcherLoadout loadout);
        string UniqueId { get; }
        uint VesselId { get; }
    }
}
