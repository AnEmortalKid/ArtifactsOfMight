using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Loadout.Corruption;
using RoR2;
using static RoR2.UI.CarouselController;

namespace ExamplePlugin.Loadout.Draft
{
    /// <summary>
    /// Handles the randomization options for the draft loadout
    /// </summary>
    public static class DraftLoadoutRandomizer
    {
        private static readonly Random SESSION_RANDOM = new Random();

        public static void RandomizeAll()
        {
            DraftLoadout.Instance.ClearAllPicksAndLocks();

            foreach (DraftItemTier draftTab in GetShuffleOrder())
            {
                RandomizeTabInternal(draftTab);
            }
        }

        public static int RandomizeTab(DraftItemTier tabTier)
        {
            if (tabTier == DraftItemTier.Purple)
            {
                return RandomizeVoidTab();
            }

            return RandomizeNonVoidtab(tabTier);
        }

        private static int RandomizeNonVoidtab(DraftItemTier tabTier)
        {
            // no special logic needed for all normals
            DraftLoadout.Instance.ClearPicks(tabTier);

            RandomizeTabInternal(tabTier);

            return 0;
        }

        private static int RandomizeVoidTab()
        {

            var previousVoids = DraftLoadout.Instance.ClearPicks(DraftItemTier.Purple);

            // unlock old locks
            foreach (var oldVoidPick in previousVoids)
            {
                foreach (var normalLockedDef in CorruptionMaps.GetNormalsPickupDefsForVoid(oldVoidPick))
                {
                    if (DraftLoadout.Instance.IsLocked(normalLockedDef))
                    {
                        DraftLoadout.Instance.Unlock(normalLockedDef);
                    }
                }
            }

            // pick new set
            var newVoids = RandomizeTabInternal(DraftItemTier.Purple);

            // Very possible peeps do not like this part and it should just unlock
            RandomizeTabInternal(DraftItemTier.White, assumeTabEmpty: false);
            RandomizeTabInternal(DraftItemTier.Green, assumeTabEmpty: false);
            RandomizeTabInternal(DraftItemTier.Red, assumeTabEmpty: false);
            // no yellow since we dont draft the newly hatched zoea

            return 0;
        }


        private static HashSet<ItemIndex> RandomizeTabInternal(DraftItemTier tabTier, bool assumeTabEmpty = true)
        {
            var draftable = DraftPools.Instance.GetDraftablePickups(tabTier);
            var tabCandidates = new List<PickupDef>(draftable.Count);
            foreach (var draftCandidate in draftable)
            {
                var def = PickupCatalog.GetPickupDef(draftCandidate);
                if (DraftLoadout.Instance.IsLocked(def))
                {
                    continue;
                }

                // if the tab's not empty, check already picked
                if (!assumeTabEmpty)
                {
                    if (DraftLoadout.Instance.IsPicked(def))
                    {
                        continue;
                    }
                }

                tabCandidates.Add(def);
            }

            // shuffle list
            FisherYates(tabCandidates, SESSION_RANDOM);

            int picked = 0;
            var tabLimit = DraftLoadout.Instance.GetLimit(tabTier);
            var pickedItems = new HashSet<ItemIndex>();
            foreach (var choiceDef in tabCandidates)
            {
                if (picked >= tabLimit)
                {
                    break;
                }

                // Safe try to pick since it handles locks
                if (DraftLoadout.Instance.TryPick(choiceDef, out var _))
                {
                    picked++;

                    LockCorruptedIfRequired(tabTier, choiceDef);

                    pickedItems.Add(choiceDef.itemIndex);
                }
            }

            return pickedItems;
        }

        /// <summary>
        /// Utility: deterministic shuffle when you pass a seeded Random.
        /// Apparently csharp doesn't have a Shuffle on list 
        /// </summary>
        private static void FisherYates<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                // good old one line swap
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Unpicks and locks the items that would be corrupted by the given void item
        /// </summary>
        /// <param name="tabTier">the tier we are working with</param>
        /// <param name="voidDef">the pickup def of the item to consider</param>
        private static void LockCorruptedIfRequired(DraftItemTier tabTier, PickupDef voidDef)
        {
            // Only the purple tab has this special logic
            if (tabTier != DraftItemTier.Purple)
            {
                return;
            }

            var normalDefs = CorruptionMaps.GetNormalsPickupDefsForVoid(voidDef.itemIndex);
            foreach (var normalDef in normalDefs)
            {
                // if its picked, then unpick
                var normalPicked = DraftLoadout.Instance.IsPicked(normalDef);
                if (normalPicked)
                {
                    DraftLoadout.Instance.UnPick(normalDef);
                }

                var couldLock = DraftLoadout.Instance.TryLock(normalDef);
                if (!couldLock)
                {
                    Log.Warning($"Could not lock {normalDef.nameToken}");
                }
            }
        }

        /// <summary>
        /// Always shuffle purple first and pick, so we don't have to re-consolidate normals
        /// based on locked items by purple
        /// </summary>
        /// <returns></returns>
        private static DraftItemTier[] GetShuffleOrder()
        {
            return new DraftItemTier[] {
                DraftItemTier.Purple,
                DraftItemTier.Yellow,
                DraftItemTier.Red,
                DraftItemTier.Green,
                DraftItemTier.White
            };
        }
    }
}
