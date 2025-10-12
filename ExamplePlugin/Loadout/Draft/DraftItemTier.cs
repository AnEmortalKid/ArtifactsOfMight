namespace ExamplePlugin.Loadout.Draft
{
    /// <summary>
    /// How the UI/Player understands the tiers
    /// </summary>
    public enum DraftItemTier
    {
        /// <summary>
        /// White items, effectively ItemTier.Tier1
        /// </summary>
        White,
        /// <summary>
        /// Green items, effectively ItemTier.Tier2
        /// </summary>
        Green,
        /// <summary>
        /// Red items, effectively ItemTier.Tier3
        /// </summary>
        Red,
        /// <summary>
        /// Boss items, effectively ItemTier.Boss
        /// </summary>
        Yellow,
        /// <summary>
        /// Void items, effectively the 3 ItemTier.VoidTier1 ItemTier.VoidTier2 ItemTier.VoidTier3
        /// </summary>
        Purple,
        // intentionally no Blue/Orange for now
    }
}
