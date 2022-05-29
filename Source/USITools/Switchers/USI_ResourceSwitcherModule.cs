using KSP.Localization;
using ResourceSwitcherUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using USITools.Helpers;

namespace USITools
{
    public class USI_ResourceSwitcherModule : PartModule, IPartCostModifier, IResourceSwitcher
    {
        [KSPField(isPersistant = true)]
        public int CurrentLoadout = -1;

        [KSPField(isPersistant = true)]
        public string ModuleId;

        [KSPField]
        public float PartDryCost = 0f;

        [KSPField]
        public string PartInfoTitle;

        [KSPField]
        public string PartSwapCostTitle;

        [KSPField]
        public string PAWButtonLabel;

        [KSPField]
        public string PAWGroupDisplayName;

        [KSPField]
        public string ResourceCosts = string.Empty;

        [KSPField]
        public double Volume = 1d;

#pragma warning disable IDE0060 // Remove unused parameter

        [KSPAction(guiName = "#LOC_USI_ResourceSwitcher_PAWButtonLabel")]
        public void ShowWindowAction(KSPActionParam param)
        {
            ShowWindow();
        }

        [KSPEvent(
            guiName = "#LOC_USI_ResourceSwitcher_PAWButtonLabel",
            groupName = PAW_GROUP_NAME,
            groupDisplayName = "#LOC_USI_ResourceSwitcher_PAWButtonLabel",
            guiActive = true,
            guiActiveEditor = true,
            guiActiveUnfocused = false)]
        public void ShowWindowEvent()
        {
            ShowWindow();
        }

#pragma warning restore IDE0060

        protected const string PAW_GROUP_NAME = "usi-resource-switcher";
        protected readonly Dictionary<string, List<LoadoutMetadata>> _cachedLoadoutMetadata
            = new Dictionary<string, List<LoadoutMetadata>>();
        protected string _cachedPartInfo;
        protected IResourceSwitcherController _controller;
        protected List<USI_ResourceSwapOption> _loadouts;
        protected List<Material> _materials;
        protected USI_ResourceSwapOption _selectedLoadout;
        protected float _selectedLoadoutCost = 0f;
        protected List<ResourceRatio> _swapCosts;
        protected PartThumbnailService _thumbnailService;

        public IResourceSwitcherController Controller => _controller;
        public string DisplayName => part.name;
        public string Resources
        {
            get
            {
                if (_selectedLoadout == null)
                {
                    return "No loadout selected";
                }
                return _selectedLoadout.DisplayName;
            }
        }
        public float SelectedLoadoutCost => _selectedLoadoutCost;
        public string UniqueId
        {
            get
            {
                if (string.IsNullOrEmpty(ModuleId))
                {
                    ModuleId = Guid.NewGuid().ToString("N");
                }
                return ModuleId;
            }
        }
        public uint VesselId => vessel == null ? 0 : vessel.persistentId;

        protected void ApplyLoadout(USI_ResourceSwapOption loadout, bool updateResources = true)
        {
            if (!_cachedLoadoutMetadata.ContainsKey(loadout.UniqueId))
            {
                var metadata = loadout
                    .GetLoadoutMetadata()
                    .OrderBy(lm => lm.ResourceDisplayName)
                    .ToList();
                _cachedLoadoutMetadata[loadout.UniqueId] = metadata;
            }

            var loadoutMetadata = _cachedLoadoutMetadata[loadout.UniqueId];

            if (updateResources)
            {
                if (part.Resources.Any())
                {
                    var oldResources = part.Resources.ToArray();
                    for (int i = 0; i < oldResources.Length; i++)
                    {
                        part.Resources.Remove(oldResources[i]);
                    }
                }

                _selectedLoadoutCost = 0f;
                foreach (var metadata in loadoutMetadata)
                {
                    part.Resources.Add(
                        metadata.Resource,
                        metadata.DefaultUnits,
                        metadata.MaxUnits,
                        true,
                        true,
                        false,
                        true,
                        PartResource.FlowMode.Both);
                    _selectedLoadoutCost += metadata.MaxCost;
                }
                MonoUtilities.RefreshContextWindows(part);
            }
            else
            {
                _selectedLoadoutCost = loadoutMetadata.Sum(l => l.MaxCost);
            }

            if (_materials != null && _materials.Count > 0 && !string.IsNullOrEmpty(loadout.TexturePath))
            {
                if (!string.IsNullOrEmpty(loadout.NormalMapPath))
                {
                    TextureUtilities.ApplyTextureAndNormalMapToMaterials(_materials, loadout.TexturePath, loadout.NormalMapPath);
                }
                else
                {
                    TextureUtilities.ApplyTextureToMaterials(_materials, loadout.TexturePath);
                }
            }
        }

        protected bool CheckResourcesAvailable()
        {
            // TODO - Check for available resources
            return true;
        }

        protected void ConsumeResources()
        {
            // TODO - Do the thing
        }

        public override string GetInfo()
        {
            if (!string.IsNullOrEmpty(_cachedPartInfo))
            {
                return _cachedPartInfo;
            }

            var builder = new StringBuilder();
            builder
                .Append("Volume: ")
                .AppendFormat("{0:N0}", Volume)
                .AppendLine();

            if (_swapCosts != null && _swapCosts.Count > 0)
            {
                builder
                    .AppendLine(PartSwapCostTitle)
                    .AppendLine();

                foreach (var resource in _swapCosts)
                {
                    builder
                        .AppendFormat("{0} {1}", resource.Ratio, resource.ResourceName)
                        .AppendLine();
                }
            }
            _cachedPartInfo = builder.ToString();
            
            return _cachedPartInfo;
        }

        public List<IResourceSwitcherLoadout> GetLoadouts()
        {
            if (_loadouts == null)
            {
                _loadouts = part.FindModulesImplementing<USI_ResourceSwapOption>();
                if (_loadouts == null)
                {
                    return null;
                }
            }
            return _loadouts.Select(l => l as IResourceSwitcherLoadout).ToList();   
        }

        protected void GetLocalizedDisplayNames()
        {
            // These values can be customized via the part config
            if (string.IsNullOrEmpty(PartSwapCostTitle))
            {
                if (!Localizer.TryGetStringByTag(
                    "#LOC_USI_ResourceSwapper_PartInfoTitle",
                    out PartSwapCostTitle))
                {
                    PartSwapCostTitle = "#LOC_USI_ResourceSwapper_PartInfoTitle";
                }
            }

            if (string.IsNullOrEmpty(PAWGroupDisplayName))
            {
                if (!Localizer.TryGetStringByTag(
                "#LOC_USI_Deployable_PAWGroupDisplayName",
                out PAWGroupDisplayName))
                {
                    PAWGroupDisplayName = "#LOC_USI_Deployable_PAWGroupDisplayName";
                }
            }
            Events[nameof(ShowWindowEvent)].group.displayName = PAWGroupDisplayName;

            if (string.IsNullOrEmpty(PAWButtonLabel))
            {
                if (!Localizer.TryGetStringByTag(
                "#LOC_USI_Deployable_P",
                out PAWButtonLabel))
                {
                    PAWButtonLabel = "#LOC_USI_Deployable_PAWGroupDisplayName";
                }
            }
            Actions[nameof(ShowWindowAction)].guiName = PAWButtonLabel;
            Events[nameof(ShowWindowEvent)].guiName = PAWButtonLabel;
        }

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit) => SelectedLoadoutCost;

        public ModifierChangeWhen GetModuleCostChangeWhen() => ModifierChangeWhen.CONSTANTLY;

        public override string GetModuleDisplayName()
        {
            return PartInfoTitle ?? base.GetModuleDisplayName();
        }

        protected void GetServices()
        {
            var serviceManagerAddon = FindObjectOfType<USI_AddonServiceManager>();
            _thumbnailService = serviceManagerAddon.ServiceManager
                .GetService<PartThumbnailService>();
            if (_thumbnailService == null)
            {
                throw new Exception($"Could not find {nameof(PartThumbnailService)}");
            }

            _controller = FindObjectOfType<USI_ResourceSwitcherScenario>();
            if (_controller == null)
            {
                throw new Exception($"Could not find {nameof(USI_ResourceSwitcherScenario)}");
            }
            _controller.AddSwitcher(this);
        }

        protected void GetSwapCosts()
        {
            if (string.IsNullOrEmpty(ResourceCosts))
            {
                return;
            }

            try
            {
                _swapCosts = ResourceHelpers.DeserializeResourceRatios(ResourceCosts);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[USITools] {ClassName}: {ex.Message}");
                _swapCosts = null;
            }
        }

        public Texture2D GetThumbnail()
        {
            if (_thumbnailService == null)
            {
                return null;
            }
            return _thumbnailService.GetThumbnail(part);
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.RemoveSwitcher(this);
            }
        }

        private void OnFlightReady()
        {
            if (FlightGlobals.ActiveVessel == vessel)
            {
                _controller.AddSwitcher(this);
            }
            GameEvents.onFlightReady.Remove(OnFlightReady);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            try
            {
                GetServices();
                if (HighLogic.LoadedSceneIsFlight)
                {
                    GameEvents.onFlightReady.Add(OnFlightReady);
                }
                else
                {
                    _controller.AddSwitcher(this);
                }
                GetLoadouts();
                GetLocalizedDisplayNames();
                GetSwapCosts();
                _materials = TextureUtilities.GetMaterialsForPart(part);
                if (_loadouts != null &&
                    _loadouts.Count > CurrentLoadout &&
                    CurrentLoadout >= 0)
                {
                    SelectLoadout(CurrentLoadout, false, false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[USITools] {ClassName}: {ex.Message}");
                enabled = false;
            }
        }

        public void SelectLoadout(int loadoutIndex, bool updateSymmetryCounterparts = false, bool updateResources = true)
        {
            if (_loadouts != null && _loadouts.Count > loadoutIndex)
            {
                CurrentLoadout = loadoutIndex;
                _selectedLoadout = _loadouts[loadoutIndex];
                ApplyLoadout(_loadouts[loadoutIndex]);

                if (updateSymmetryCounterparts &&
                    HighLogic.LoadedSceneIsEditor &&
                    part.symmetryCounterparts.Any())
                {
                    for (int i = 0; i < part.symmetryCounterparts.Count; i++)
                    {
                        var switcher = part.symmetryCounterparts[i].GetComponent<USI_ResourceSwitcherModule>();
                        switcher.SelectLoadout(loadoutIndex, false, updateResources);
                    }
                }
            }
        }

        public void SelectLoadout(IResourceSwitcherLoadout loadout)
        {
            if (loadout is USI_ResourceSwapOption)
            {
                SelectLoadout(loadout as USI_ResourceSwapOption);
            }
        }

        public void SelectLoadout(USI_ResourceSwapOption loadout)
        {
            if (_loadouts != null)
            {
                var loadoutIndex = _loadouts.IndexOf(loadout);
                if (loadoutIndex != -1)
                {
                    SelectLoadout(loadoutIndex, true);
                }
            }
        }

        protected void ShowWindow()
        {
            if (_controller != null)
            {
                _controller.ShowWindow();
            }
        }
    }
}
