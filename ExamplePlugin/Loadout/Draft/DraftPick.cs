using RoR2;

namespace ArtifactsOfMight.Loadout.Draft
{
    /// <summary>
    /// Used to track our picks in a specific order
    /// </summary>
    public struct DraftPick
    {
        public ItemIndex ItemIndex;
        public ItemTier ItemTier;

        public DraftPick(ItemIndex itemIndex, ItemTier itemTier)
        {
            ItemIndex = itemIndex;
            ItemTier = itemTier;
        }
    }
}
