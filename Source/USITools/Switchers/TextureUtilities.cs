using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace USITools
{
    public static class TextureUtilities
    {
        public static void ApplyNormalMapToMaterials(List<Material> materials, string normalMapUrl)
        {
            if (materials == null || materials.Count == 0 || !GameDatabase.Instance.ExistsTexture(normalMapUrl))
            {
                return;
            }
            var normalMap = GameDatabase.Instance.GetTexture(normalMapUrl, true);
            foreach (var material in materials)
            {
                material.SetTexture("_BumpMap", normalMap);
            }
        }

        public static void ApplyTextureAndNormalMapToMaterials(List<Material> materials, string textureUrl, string normalMapPath)
        {
            if (materials == null ||
                materials.Count == 0 ||
                !GameDatabase.Instance.ExistsTexture(textureUrl) ||
                !GameDatabase.Instance.ExistsTexture(normalMapPath))
            {
                return;
            }
            var texture = GameDatabase.Instance.GetTexture(textureUrl, false);
            var normalMap = GameDatabase.Instance.GetTexture(normalMapPath, true);
            foreach (var material in materials)
            {
                material.mainTexture = texture;
                material.SetTexture("_BumpMap", normalMap);
            }
        }

        public static void ApplyTextureToMaterials(List<Material> materials, string textureUrl)
        {
            if (materials == null || materials.Count == 0 || !GameDatabase.Instance.ExistsTexture(textureUrl))
            {
                return;
            }
            var texture = GameDatabase.Instance.GetTexture(textureUrl, false);
            foreach (var material in materials)
            {
                material.mainTexture = texture;
            }
        }

        public static List<Material> GetMaterialsForPart(Part part)
        {
            if (part == null)
            {
                return null;
            }
            var renderers = part.FindModelRenderersCached();
            if (renderers == null)
            {
                return null;
            }
            var materials = new List<Material>();
            foreach (var renderer in renderers)
            {
                var material = renderer.material;
                if (material != null)
                {
                    materials.Add(material);
                }
            }
            return materials;
        }

        public static Texture2D GetTexture(string textureUrl)
        {
            if (!GameDatabase.Instance.ExistsTexture(textureUrl))
            {
                return null;
            }
            return GameDatabase.Instance.GetTexture(textureUrl, false);
        }
    }
}
