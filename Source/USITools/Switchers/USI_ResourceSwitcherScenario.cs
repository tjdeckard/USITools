using KSP.Localization;
using KSP.UI.Screens;
using ResourceSwitcherUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnityEngine;

namespace USITools
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.EDITOR, GameScenes.FLIGHT)]
    public class USI_ResourceSwitcherScenario : ScenarioModule, IResourceSwitcherController
    {
        private static bool UIPrefabsLoaded { get; set; }

        private IServiceManager _serviceManager;
        private readonly List<IResourceSwitcher> _switchers
            = new List<IResourceSwitcher>();
        private ApplicationLauncherButton _toolbarButton;
        private ResourceSwitcherWindow _window;
        private WindowManager _windowManager;

        public string ApplyButtonText { get; private set; }
            = "#LOC_USI_ResourceSwitcher_ApplyButtonText";
        public string Column1HeaderText { get; private set; }
            = "#LOC_USI_ResourceSwitcher_Column1HeaderText";
        public string Column2HeaderText { get; private set; }
            = "#LOC_USI_ResourceSwitcher_Column2HeaderText";
        public string Column3HeaderText { get; private set; }
            = "#LOC_USI_ResourceSwitcher_Column3HeaderText";
        public string Column1Instructions { get; private set; }
            = "#LOC_USI_ResourceSwitcher_Column1Instructions";
        public string Column2Instructions { get; private set; }
            = "#LOC_USI_ResourceSwitcher_Column2Instructions";
        public string Column3Instructions { get; private set; }
            = "#LOC_USI_ResourceSwitcher_Column3Instructions";
        public string TitleBarText { get; private set; }
            = "#LOC_USI_ResourceSwitcher_TitleBarText";

        public Canvas Canvas => MainCanvasUtil.MainCanvas;
        public GameObject ResourceSwitcherLoadoutPanelPrefab { get; private set; }
        public GameObject ResourceSwitcherPartPanelPrefab { get; private set; }
        public GameObject ResourceSwitcherResourceCardPrefab { get; private set; }
        public GameObject ResourceSwitcherWindowPrefab { get; private set; }

        public void AddSwitcher(IResourceSwitcher switcher)
        {
            if (!_switchers.Contains(switcher))
            {
                _switchers.Add(switcher);
                _window.SwitcherAdded(switcher);
            }
        }

        public void DeselectSwitcher(PartPanel partPanel)
        {
            _window.PartSelected(null);
        }

        protected void GetLocalizedLabels()
        {
            if (Localizer.TryGetStringByTag(
                "#LOC_USI_ResourceSwitcher_ApplyButtonText",
                out string applyButtonText))
            {
                ApplyButtonText = applyButtonText.ToUpper();
            }
            if (Localizer.TryGetStringByTag(
                "#LOC_USI_ResourceSwitcher_Column1HeaderText",
                out string column1HeaderText))
            {
                Column1HeaderText = column1HeaderText;
            }
            if (Localizer.TryGetStringByTag(
                "#LOC_USI_ResourceSwitcher_Column2HeaderText",
                out string column2HeaderText))
            {
                Column2HeaderText = column2HeaderText;
            }
            if (Localizer.TryGetStringByTag(
                "#LOC_USI_ResourceSwitcher_Column3HeaderText",
                out string column3HeaderText))
            {
                Column3HeaderText = column3HeaderText;
            }
            if (Localizer.TryGetStringByTag(
                "#LOC_USI_ResourceSwitcher_Column1Instructions",
                out string column1Instructions))
            {
                Column1Instructions = column1Instructions;
            }
            if (Localizer.TryGetStringByTag(
                "#LOC_USI_ResourceSwitcher_Column2Instructions",
                out string column2Instructions))
            {
                Column2Instructions = column2Instructions;
            }
            if (Localizer.TryGetStringByTag(
                "#LOC_USI_ResourceSwitcher_Column3Instructions",
                out string column3Instructions))
            {
                Column3Instructions = column3Instructions;
            }
            if (Localizer.TryGetStringByTag(
                "#LOC_USI_ResourceSwitcher_TitleBarText",
                out string titleBarText))
            {
                TitleBarText = titleBarText;
            }
        }

        public List<IResourceSwitcher> GetSwitchers(bool activeVesselOnly = true)
        {
            if (activeVesselOnly && HighLogic.LoadedSceneIsFlight)
            {
                var vesselId = FlightGlobals.ActiveVessel.persistentId;
                return _switchers.Where(s => s.VesselId == vesselId).ToList();
            }
            // Make a copy of the cache to prevent modification by other classes
            return _switchers.ToList();
        }

        public void HideWindow()
        {
            if (_window == null)
            {
                return;
            }
            _window.Close();
        }

        public override void OnAwake()
        {
            base.OnAwake();

            GetLocalizedLabels();

            // Cache service references and load UI prefabs
            var provider = USI_AddonServiceManager.Instance;
            if (provider != null)
            {
                _serviceManager = provider.ServiceManager;
                _windowManager = _serviceManager.GetService<WindowManager>();

                if (!UIPrefabsLoaded)
                {
                    try
                    {
                        // Load UI prefabs
                        var filePath = Path.Combine(
                            KSPUtil.ApplicationRootPath,
                            "GameData/000_USITools/Assets/UI/ResourceSwitcher.prefabs");
                        var prefabs = AssetBundle.LoadFromFile(filePath);
                        ResourceSwitcherLoadoutPanelPrefab = prefabs.LoadAsset<GameObject>("LoadoutPicker");
                        ResourceSwitcherPartPanelPrefab = prefabs.LoadAsset<GameObject>("PartPicker");
                        ResourceSwitcherResourceCardPrefab = prefabs.LoadAsset<GameObject>("ResourceCard");
                        ResourceSwitcherWindowPrefab = prefabs.LoadAsset<GameObject>("ResourceSwitcherWindow");

                        _windowManager.RegisterWindow<ResourceSwitcherWindow>(ResourceSwitcherWindowPrefab);
                        _windowManager.RegisterPrefab<LoadoutPanel>(ResourceSwitcherLoadoutPanelPrefab);
                        _windowManager.RegisterPrefab<PartPanel>(ResourceSwitcherPartPanelPrefab);
                        _windowManager.RegisterPrefab<ResourceCard>(ResourceSwitcherResourceCardPrefab);

                        UIPrefabsLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[USITools] {ClassName}: {ex.Message}");
                    }
                }
            }

            // Setup UI
            _window = _windowManager.GetWindow<ResourceSwitcherWindow>();
            _window.Initialize(_windowManager, this);

            // Create toolbar button
            var textureService = _serviceManager.GetService<TextureService>();
            var toolbarIcon = textureService
                .GetTexture("GameData/000_USITools/Assets/UI/Logistics_36x36.png", 36, 36);
            var showInScenes = ApplicationLauncher.AppScenes.FLIGHT |
                ApplicationLauncher.AppScenes.SPH |
                ApplicationLauncher.AppScenes.VAB;
            _toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                ShowWindow,
                HideWindow,
                null,
                null,
                null,
                null,
                showInScenes,
                toolbarIcon);
        }

        /// <summary>
        /// Implementation of <see cref="MonoBehaviour"/>.OnDestroy().
        /// </summary>
        [SuppressMessage("CodeQuality",
            "IDE0051:Remove unused private members",
            Justification = "Because MonoBehaviour")]
        private void OnDestroy()
        {
            if (_toolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(_toolbarButton);
                _toolbarButton = null;
            }
        }

        public void RemoveSwitcher(IResourceSwitcher switcher)
        {
            if (_switchers.Contains(switcher))
            {
                _switchers.Remove(switcher);
                _window.SwitcherRemoved(switcher);
            }
        }

        public void SelectSwitcher(PartPanel partPanel)
        {
            _window.PartSelected(partPanel);
        }

        public void ShowWindow()
        {
            if (_windowManager != null)
            {
                var window = _windowManager.GetWindow<ResourceSwitcherWindow>();
                window.ShowWindow();
            }
        }
    }
}
