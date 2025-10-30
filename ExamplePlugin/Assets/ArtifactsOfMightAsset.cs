using UnityEngine;
using System.IO;

namespace ArtifactsOfMight.Assets
{
    /// <summary>
    /// Static class to access our addressable bundle
    /// </summary>
    public static class ArtifactsOfMightAsset
    {
        public static AssetBundle mainBundle;

        public const string bundleName = "artifacts_of_might.bundle";

        public const string assetBundleFolder = "assetbundles";

        public static string AssetBundlePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(ArtifactsOfMight.PluginInfo.Location), assetBundleFolder, bundleName);
            }
        }

        public static void Init()
        {
            //Loads the assetBundle from the Path, and stores it in the static field.
            Log.Info($"Loading from {AssetBundlePath}");
            mainBundle = AssetBundle.LoadFromFile(AssetBundlePath);
        }

        /// <summary>
        /// Attempts to load a sprite from this bundle
        /// </summary>
        /// <param name="path">the bundle path</param>
        /// <returns></returns>
        public static Sprite LoadSprite(string path)
        {
            return mainBundle.LoadAsset<Sprite>(path);
        }
    }
}
