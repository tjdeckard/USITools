using System.Collections.Generic;
using UnityEngine;

namespace ResourceSwitcherUI
{
    public interface IResourceSwitcherController
    {
        void AddSwitcher(IResourceSwitcher switcher);
        string ApplyButtonText { get; }
        Canvas Canvas { get; }
        string Column1HeaderText { get; }
        string Column2HeaderText { get; }
        string Column3HeaderText { get; }
        string Column1Instructions { get; }
        string Column2Instructions { get; }
        string Column3Instructions { get; }
        void DeselectSwitcher(PartPanel partPanel);
        List<IResourceSwitcher> GetSwitchers(bool activeVesselOnly = true);
        void SelectSwitcher(PartPanel partPanel);
        void RemoveSwitcher(IResourceSwitcher switcher);
        void ShowWindow();
        string TitleBarText { get; }
    }
}
