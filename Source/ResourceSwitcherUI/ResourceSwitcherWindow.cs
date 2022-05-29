using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using USIToolsUI;
using USIToolsUI.Interfaces;

namespace ResourceSwitcherUI
{
    [RequireComponent(typeof(RectTransform))]
    public class ResourceSwitcherWindow : AbstractWindow
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable 0169
#pragma warning disable 0649

        [SerializeField]
        private Text AlertText;

        [SerializeField]
        private Text ApplyButtonText;

        [SerializeField]
        private GameObject Column1;

        [SerializeField]
        private GameObject Column2;

        [SerializeField]
        private GameObject Column3;

        [SerializeField]
        private Text Column1HeaderText;

        [SerializeField]
        private Text Column2HeaderText;

        [SerializeField]
        private Text Column3HeaderText;

        [SerializeField]
        private Text Column1Instructions;

        [SerializeField]
        private Text Column2Instructions;

        [SerializeField]
        private Text Column3Instructions;

        [SerializeField]
        private Transform LoadoutsList;

        [SerializeField]
        private Transform PartsList;

        [SerializeField]
        private Transform ResourcesList;

        [SerializeField]
        private Text TitleBarText;

#pragma warning restore 0649
#pragma warning restore 0169
#pragma warning restore IDE0044

        protected uint _activeVesselId;
        protected Dictionary<string, LoadoutPanel> _loadoutPanels
            = new Dictionary<string, LoadoutPanel>();
        protected Dictionary<string, PartPanel> _partPanels
            = new Dictionary<string, PartPanel>();
        protected Dictionary<string, ResourceCard> _resourceCards
            = new Dictionary<string, ResourceCard>();
        protected IPrefabInstantiator _prefabInstantiator;
        protected LoadoutPanel _selectedLoadout;
        protected PartPanel _selectedPart;
        protected IResourceSwitcherController _switcherController;

        public override Canvas Canvas
        {
            get { return _switcherController?.Canvas; }
        }

        public void ApplySelectedLoadout()
        {
            var switcher = _selectedPart.Switcher;
            if (switcher == null)
            {
                return;
            }
            _selectedPart.Switcher.SelectLoadout(_selectedLoadout.Loadout);
            _partPanels[switcher.UniqueId].SetValues(switcher);
            ClearResourceCards();
            HideColumn(Column3);
        }

        public void ClearLoadouts()
        {
            if (_loadoutPanels != null && _loadoutPanels.Any())
            {
                foreach (var panel in _loadoutPanels)
                {
                    panel.Value.LoadoutSelected -= LoadoutSelected;
                    Destroy(panel.Value.gameObject);
                }
                _loadoutPanels.Clear();
            }
        }

        public void ClearParts()
        {
            if (_partPanels != null && _partPanels.Any())
            {
                foreach (var panel in _partPanels)
                {
                    Destroy(panel.Value.gameObject);
                }
                _partPanels.Clear();
            }
        }

        public void ClearResourceCards()
        {
            if (_resourceCards != null && _resourceCards.Any())
            {
                foreach (var card in _resourceCards)
                {
                    Destroy(card.Value.gameObject);
                }
                _resourceCards.Clear();
            }
        }

        public void Close()
        {
            HideAlert();
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Gets all parts with resource switchers on the active vessel.
        /// </summary>
        /// <remarks>
        /// Also creates the panel prefabs to display the parts in the UI.
        /// </remarks>
        protected void GetSwitchers()
        {
            if (_switcherController != null &&
                _prefabInstantiator != null &&
                _partPanels != null)
            {
                var switchers = _switcherController
                    .GetSwitchers()
                    .OrderBy(s => s.DisplayName)
                    .ThenBy(s => s.Resources);
                if (switchers != null)
                {
                    foreach (var switcher in switchers)
                    {
                        if (!_partPanels.ContainsKey(switcher.UniqueId))
                        {
                            var panel = _prefabInstantiator
                                .InstantiatePrefab<PartPanel>(PartsList);
                            panel.SetValues(switcher);
                            _partPanels.Add(switcher.UniqueId, panel);
                        }
                    }
                }
            }
        }

        public void HideAlert()
        {
            if (AlertText != null && AlertText.gameObject.activeSelf)
            {
                AlertText.gameObject.SetActive(false);
            }
        }

        public void HideColumn(GameObject column)
        {
            if (column != null && column.activeSelf)
            {
                column.SetActive(false);
            }
        }

        public void HideColumns()
        {
            HideColumn(Column2);
            HideColumn(Column3);
        }

        public void Initialize(
            IPrefabInstantiator prefabInstantiator,
            IResourceSwitcherController switcherController)
        {
            _prefabInstantiator = prefabInstantiator;
            _switcherController = switcherController;

            HideAlert();
            HideColumns();

            // Get localized label text
            if (ApplyButtonText != null)
            {
                ApplyButtonText.text = switcherController.ApplyButtonText;
            }
            if (Column1HeaderText != null)
            {
                Column1HeaderText.text = switcherController.Column1HeaderText;
            }
            if (Column2HeaderText != null)
            {
                Column2HeaderText.text = switcherController.Column2HeaderText;
            }
            if (Column3HeaderText != null)
            {
                Column3HeaderText.text = switcherController.Column3HeaderText;
            }
            if (Column1Instructions != null)
            {
                Column1Instructions.text = switcherController.Column1Instructions;
            }
            if (Column2Instructions != null)
            {
                Column2Instructions.text = switcherController.Column2Instructions;
            }
            if (Column3Instructions != null)
            {
                Column3Instructions.text = switcherController.Column3Instructions;
            }
            if (TitleBarText != null)
            {
                TitleBarText.text = switcherController.TitleBarText;
            }

            if (LoadoutsList == null || PartsList == null)
            {
                HideColumn(Column1);
                ShowAlert("Dev alert: Window is misconfigured.");
                Debug.LogError(
                    $"[USITools] {nameof(ResourceSwitcherWindow)}: Missing reference to {nameof(LoadoutsList)} and/or {nameof(PartsList)}.");
            }
        }

        public void LoadoutSelected(LoadoutPanel loadoutPanel)
        {
            if (_selectedLoadout != loadoutPanel)
            {
                _selectedLoadout = loadoutPanel;
                ClearResourceCards();
                HideAlert();

                var loadoutMetadata = loadoutPanel.Loadout.GetLoadoutMetadata();
                if (loadoutMetadata != null && loadoutMetadata.Count > 0)
                {
                    foreach (var resource in loadoutMetadata)
                    {
                        if (!_resourceCards.ContainsKey(resource.Resource))
                        {
                            var resourceCard = _prefabInstantiator
                                .InstantiatePrefab<ResourceCard>(ResourcesList);
                            resourceCard.SetValues(resource);
                            _resourceCards.Add(resource.Resource, resourceCard);
                        }
                    }
                }
                ShowColumn(Column3);
            }
        }

        /// <summary>
        /// Gets the loadouts for the selected resource switcher.
        /// </summary>
        /// <remarks>
        /// Also creates the panel prefabs to display the loadouts in the UI.
        /// </remarks>
        /// <param name="partPanel"></param>
        public void PartSelected(PartPanel partPanel)
        {
            if (_selectedPart != partPanel)
            {
                HideAlert();
                ClearLoadouts();
                ClearResourceCards();
                _selectedPart = partPanel;

                if (_selectedPart != null && _selectedPart.Switcher != null)
                {
                    var loadouts = _selectedPart.Switcher
                        .GetLoadouts()
                        .OrderBy(l => l.DisplayName);
                    if (loadouts != null && loadouts.Any())
                    {
                        foreach (var loadout in loadouts)
                        {
                            if (!_loadoutPanels.ContainsKey(loadout.UniqueId))
                            {
                                var panel = _prefabInstantiator
                                    .InstantiatePrefab<LoadoutPanel>(LoadoutsList);
                                panel.SetValues(loadout);
                                panel.LoadoutSelected += LoadoutSelected;
                                _loadoutPanels.Add(loadout.UniqueId, panel);
                            }
                        }
                        ShowColumn(Column2);
                    }
                    else
                    {
                        ShowAlert("No loadouts configured!");
                    }
                }
            }
        }

        public override void Reset()
        {
            ClearLoadouts();
            ClearParts();
            ClearResourceCards();
            HideAlert();
            HideColumns();
        }

        public void ShowAlert(string message)
        {
            if (AlertText != null)
            {
                AlertText.text = message;
                if (!AlertText.gameObject.activeSelf)
                {
                    AlertText.gameObject.SetActive(true);
                }
            }
        }

        protected void ShowColumn(GameObject column)
        {
            if (column != null && !column.activeSelf)
            {
                column.SetActive(true);
            }
        }

        public void ShowWindow()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        public void SwitcherAdded(IResourceSwitcher switcher)
        {
            if (switcher.VesselId != _activeVesselId)
            {
                Reset();
                _activeVesselId = switcher.VesselId;
            }
            GetSwitchers();
        }

        public void SwitcherRemoved(IResourceSwitcher switcher)
        {
            if (_selectedPart != null && switcher == _selectedPart.Switcher)
            {
                ClearLoadouts();
                ClearResourceCards();
                _selectedPart = null;
            }
            if (!_partPanels.ContainsKey(switcher.UniqueId))
            {
                return;
            }
            var partPanel = _partPanels[switcher.UniqueId];
            Destroy(partPanel.gameObject);
            _partPanels.Remove(switcher.UniqueId);
        }
    }
}
