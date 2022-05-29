using ResourceSwitcherUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace USITools
{
    public class USI_ResourceSwapOption : PartModule, IResourceSwitcherLoadout
    {
        [KSPField]
        public double DefaultFillPercentage = 1d;

        [KSPField]
        public string LoadoutName;

        [KSPField(isPersistant = true)]
        public string ModuleId;

        [KSPField]
        public string NormalMapPath = string.Empty;

        [KSPField]
        public string PartInfoTitle;

        [KSPField]
        public string TexturePath = string.Empty;

        protected bool _initialized;
        protected List<LoadoutMetadata> _loadoutMetadata;
        protected List<ResourceCompartment> _resources;
        protected USI_ResourceSwitcherModule _switcher;
        protected Texture2D _thumbnail;

        public string DisplayName
        {
            get
            {
                return LoadoutName ?? "Missing LoadoutName in part config";
            }
        }

        private string _uniqueId;
        public string UniqueId
        {
            get
            {
                if (string.IsNullOrEmpty(ModuleId))
                {
                    ModuleId = Guid.NewGuid().ToString("N");
                }
                if (string.IsNullOrEmpty(_uniqueId))
                {
                    _uniqueId = HighLogic.LoadedSceneIsFlight ? $"{vessel?.persistentId}-{ModuleId}" : ModuleId;
                }
                return _uniqueId;
            }
        }

        public override string GetInfo()
        {
            var output = new StringBuilder();
            output
                .AppendLine(LoadoutName)
                .AppendLine();

            if (_resources != null && _resources.Count > 0)
            {
                foreach (var resource in _resources)
                {
                    var resourceDefinition = PartResourceLibrary.Instance
                        .GetDefinition(resource.ResourceName);
                    output
                        .Append(" - ")
                        .Append(resourceDefinition.displayName)
                        .Append(": ")
                        .AppendLine($"{resource.Ratio:P0}");
                }
            }
            return output.ToString();
        }

        public List<LoadoutMetadata> GetLoadoutMetadata()
        {
            if (_loadoutMetadata != null)
            {
                return _loadoutMetadata;
            }
            if (_resources == null || _resources.Count < 1)
            {
                var moduleConfigNode = GetModuleConfigNode();
                InitializeCompartments(moduleConfigNode);
                if (_resources == null || _resources.Count < 1)
                {
                    return null;
                }
            }
            var volume = _switcher.Volume;
            _loadoutMetadata = _resources
                .Select(r =>
                {
                    var resourceDefinition = PartResourceLibrary.Instance.GetDefinition(r.ResourceName);
                    var maxUnits = r.Ratio * r.Compression * _switcher.Volume / resourceDefinition.volume;
                    var metadata = new LoadoutMetadata
                    {
                        Resource = resourceDefinition.name,
                        ResourceDisplayName = resourceDefinition.displayName,
                        MaxCost = (float)(maxUnits * resourceDefinition.unitCost),
                        MaxUnits = maxUnits,
                        DefaultUnits = Math.Round(DefaultFillPercentage * maxUnits, 0),
                    };
                    return metadata;
                })
                .ToList();
            return _loadoutMetadata;
        }

        private ConfigNode GetModuleConfigNode()
        {
            var loadedParts = PartLoader.Instance.loadedParts;
            var thisPart = loadedParts.FirstOrDefault(p => p.name == part.name);
            if (thisPart == null)
            {
                return null;
            }

            var moduleIndex = part.Modules.IndexOf(this);
            var partConfigNode = thisPart.partConfig;
            var moduleConfigs = partConfigNode.GetNodes("MODULE");
            if (moduleConfigs == null || moduleConfigs.Length <= moduleIndex)
            {
                return null;
            }

            return moduleConfigs[moduleIndex];
        }

        public override string GetModuleDisplayName()
        {
            return PartInfoTitle ?? base.GetModuleDisplayName();
        }

        public Texture2D GetThumbnail()
        {
            if (_thumbnail != null)
            {
                return _thumbnail;
            }
            if (string.IsNullOrEmpty(TexturePath))
            {
                return null;
            }
            _thumbnail = TextureUtilities.GetTexture(TexturePath);
            return _thumbnail;
        }

        private void InitializeCompartments(ConfigNode node)
        {
            if (_initialized)
            {
                return;
            }
            var compartmentNodes = node.GetNodes("COMPARTMENT");
            if (compartmentNodes != null && compartmentNodes.Length > 0)
            {
                try
                {
                    _resources = new List<ResourceCompartment>();
                    var resourceName = nameof(ResourceCompartment.ResourceName);
                    for (int i = 0; i < compartmentNodes.Length; i++)
                    {
                        var compartmentNode = compartmentNodes[i];
                        if (!compartmentNode.HasValue(resourceName))
                        {
                            Debug.LogError($"[USITools] {ClassName}: Error loading resource config node (missing {resourceName}).");
                            enabled = false;
                            return;
                        }

                        var resource = new ResourceCompartment();
                        resource.Load(compartmentNode);

                        var resourceDefinition = PartResourceLibrary.Instance.GetDefinition(resource.ResourceName);
                        if (resourceDefinition == null)
                        {
                            Debug.LogError($"[USITools] {ClassName}: Error in config for {DisplayName}. No resource definition found for {resource.ResourceName}.");
                            enabled = false;
                            return;
                        }

                        _resources.Add(resource);
                    }

                    // Check that the resource ratios all add up to 1 (100%)
                    var unusedVolume = _resources.Sum(r => r.Ratio) - 1d;
                    if (unusedVolume > ResourceUtilities.FLOAT_TOLERANCE || unusedVolume < 0d)
                    {
                        Debug.LogError($"[USITools] {ClassName}: Loadout {DisplayName} has invalid resource ratios (must total 1).");
                        enabled = false;
                        return;
                    }

                    _initialized = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[USITools] {ClassName}: {ex.Message} {ex.StackTrace}");
                    enabled = false;
                    return;
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            InitializeCompartments(node);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            _switcher = part.GetComponent<USI_ResourceSwitcherModule>();
            if (_switcher == null)
            {
                Debug.LogError($"[USITools] {ClassName}: Error in {part.name} config. Missing {nameof(USI_ResourceSwitcherModule)}.");
                enabled = false;
                return;
            }

            if (!string.IsNullOrEmpty(TexturePath) &&
                !GameDatabase.Instance.ExistsTexture(TexturePath))
            {
                Debug.LogError($"[USITools] {ClassName}: Error in config for {DisplayName}. No texture exists at {TexturePath}.");
                enabled = false;
                return;
            }

            if (!string.IsNullOrEmpty(NormalMapPath) &&
                !GameDatabase.Instance.ExistsTexture(NormalMapPath))
            {
                Debug.LogError($"[USITools] {ClassName}: Error in config for {DisplayName}. No normal map exists at {NormalMapPath}.");
                enabled = false;
                return;
            }

            var moduleConfigNode = GetModuleConfigNode();
            if (moduleConfigNode == null)
            {
                Debug.LogError($"[USITools] {ClassName}: Could find module config for {DisplayName}.");
                enabled = false;
                return;
            }

            InitializeCompartments(moduleConfigNode);

            if (DefaultFillPercentage < 0d)
            {
                DefaultFillPercentage = 0d;
            }
        }
    }
}
