using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurvivorSeriesEditor
{
    public static class GenerateFlatSprite
    {
        public const string AssetPath = "Assets/UI/FlatWhite.png";

        public static Sprite EnsureFlatSprite()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(AssetPath);
            if (existing != null) return existing;

            Directory.CreateDirectory("Assets/UI");

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var px = new Color[16];
            for (int i = 0; i < 16; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();

            File.WriteAllBytes(AssetPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceSynchronousImport);

            var imp = AssetImporter.GetAtPath(AssetPath) as TextureImporter;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.filterMode = FilterMode.Bilinear;
            imp.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(AssetPath);
        }
    }
}
