using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ArtifactsOfMight.Assets
{

    /// <summary>
    /// Centralized spot to load addressables so i stop getting roasted on the discord
    /// for loading the same asset 5 times
    /// </summary>
    public static class AssetCache
    {
        private static Dictionary<string, Sprite> loadedSprites = new();

        /// <summary>
        /// Loads the sprite from the addressables (if needed) based on the given key
        /// </summary>
        /// <param name="assetKey">the path</param>
        /// <returns>a sprite hopefully</returns>
        public static Sprite LoadSprite(string assetKey)
        {
            if(!loadedSprites.TryGetValue(assetKey, out Sprite storedSprite))
            {
                // load and store
                storedSprite = Addressables.LoadAssetAsync<Sprite>(assetKey).WaitForCompletion();
                loadedSprites[assetKey] = storedSprite;
            }

            return storedSprite;
        }

    }
}
