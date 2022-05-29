using System;

namespace USITools
{
    public class ResourceCompartment
    {
        private double _compression = 1d;
        private double _ratio = 1d;
        private string _resourceName;

        public double Compression => _compression;
        public double Ratio => _ratio;
        public string ResourceName => _resourceName;

        public ResourceCompartment(
            string resourceName,
            double ratio,
            double compression)
        {
            _resourceName = resourceName;
            _ratio = Math.Min(1, Math.Abs(ratio));
            _compression = Math.Abs(compression);
        }

        public ResourceCompartment(string resourceName, double ratio)
            : this(resourceName, ratio, 1d)
        {
        }

        /// <summary>
        /// This constructor should only be used for loading from <see cref="ConfigNode"/>.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ResourceCompartment(string, double, double)"/> or
        /// <see cref="ResourceCompartment(string, double)"/> instead.
        /// </remarks>
        public ResourceCompartment()
        {
        }

        public void Load(ConfigNode node)
        {
            if (!node.TryGetValue(nameof(Compression), ref _compression))
            {
                _compression = 1d;
            }
            if (!node.TryGetValue(nameof(Ratio), ref _ratio))
            {
                _ratio = 1d;
            }
            if (!node.TryGetValue(nameof(ResourceName), ref _resourceName))
            {
                throw new Exception($"{nameof(ResourceCompartment)}: Missing value for {nameof(ResourceName)}.");
            }
        }

        public void Save(ConfigNode node)
        {
            node.AddValue(nameof(Compression), Compression);
            node.AddValue(nameof(Ratio), Ratio);
            node.AddValue(nameof(ResourceName), ResourceName);
        }
    }
}
