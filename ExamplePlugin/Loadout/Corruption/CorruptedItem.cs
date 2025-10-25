using System;
using System.Collections.Generic;
using System.Text;

namespace ArtifactsOfMight.Loadout.Corruption
{
    /// <summary>
    /// An enumeration of corrupted items,
    /// The RoR2Contents.Items class does not provide these and we load them from addressabless once
    /// Use CorruptedItemMaps to retrieve their definitions based on this enum
    /// </summary>
    public enum CorruptedItem
    {
        /// <summary>
        /// CloverVoid
        /// </summary>
        BenthicBloom,
        /// <summary>
        /// TreasureCacheVoid
        /// </summary>
        EncrustedKey,
        /// <summary>
        /// CritGlassesVoid
        /// </summary>
        LostSeersLenses,
        /// <summary>
        /// EquipmentMagazineVoid
        /// </summary>
        LysateCell,
        /// <summary>
        /// BleedOnHitVoid
        /// </summary>
        Needletick,
        /// <summary>
        /// VoidMegaCrabItem
        /// </summary>
        NewlyHatchedZoea,
        /// <summary>
        /// MissileVoid
        /// </summary>
        PlasmaShrimp,
        /// <summary>
        /// ExtraLifeVoid
        /// </summary>
        PluripotentLarva,
        /// <summary>
        /// ChainLightningVoid
        /// </summary>
        Polylute,
        /// <summary>
        /// BearVoid
        /// </summary>
        SaferSpaces,
        /// <summary>
        /// ElementalRingVoid
        /// </summary>
        SingularityBand,
        /// <summary>
        /// SlowOnHitVoid
        /// </summary>
        Tentabauble,
        /// <summary>
        /// ExplodeOnDeathVoid
        /// </summary>
        VoidsentFlame,
        /// <summary>
        /// MushroomVoid
        /// </summary>
        WeepingFungus
    }
}
