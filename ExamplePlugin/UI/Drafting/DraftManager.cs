using System;
using System.Collections.Generic;
using ArtifactsOfMight.Loadout.Corruption;
using ArtifactsOfMight.Loadout.Draft;
using ArtifactsOfMight.UI.Branding.Buttons;
using ArtifactsOfMight.UI.Drafting.Summary;
using ArtifactsOfMight.UI.Drafting.Tabs;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ArtifactsOfMight.UI.Drafting
{
    /// <summary>
    /// Responsible for bridinging logic across tabs and controling which tab is displayed
    /// 
    /// Example:
    /// 
    /// When a void item is picked, the DraftManager is responsible for ensuring the corresponding corrupted item
    /// is removed from the appropriate pool.
    /// </summary>
    public class DraftManager : MonoBehaviour
    {
        #region Debuggiong
        private static bool SHOW_DEBUG_COLORS = false;
        #endregion

        private DraftTabController[] draftTabs;

        private RectTransform draftingDialogRect;

        private DraftSummaryBar draftSummaryBar;
        private DraftTabsBar draftTabsBar;

        /// <summary>
        ///  Stores a reference to our DraftTabController based on the tier
        /// </summary>
        private static Dictionary<DraftItemTier, DraftTabController> tabControllersByTier = new();

        public void Initialize(RectTransform parentRectTransform)
        {
            Log.Info("DraftManager.Initialize");

            BuildDialogShell(parentRectTransform);
            BuildUI();
            // Used to set the mode to none then on restrict update state
            // Now we set that limit state on creation
            // TODO draft loaodut seed limits call
            RefreshTabs();
            draftSummaryBar.UpdateSummary();
        }

        #region Event Handlers

        private void HandleModeChangeRequest(DraftItemTier tabTier, bool isRestricted)
        {
            Log.Info("Outdated");
            //var currMode = DraftLoadout.Instance.GetMode(tabTier);
            //Log.Info($"[DraftArtifact] mode current: {currMode}");

            //var wantedMode = isRestricted ? DraftLimitMode.Restricted : DraftLimitMode.None;
            //Log.Info($"[DraftArtifact] switch to: {wantedMode}");
            //DraftLoadout.Instance.SetMode(tabTier, wantedMode);

            //// ask the tab to do its thing
            //var tabController = tabsByTier[tabTier].GetComponent<DraftTabController>();
            //tabController.RefreshMode();
        }

        private void HandleLimitIncreaseRequest(DraftItemTier tabTier)
        {
            UpdateLimitRequest(tabTier, +1);
        }

        private void HandleLimitDecreaseRequest(DraftItemTier tabTier)
        {
            UpdateLimitRequest(tabTier, -1);
        }

        private void HandleGridItemClicked(DraftItemTier tabTier, PickupDef pickupDef)
        {
            Log.Info($"DraftManager.HandleGridItemClicked ({tabTier}, {pickupDef.internalName}");
            // Nothing to do if you clicked a locked item
            if (DraftLoadout.Instance.IsLocked(pickupDef))
            {
                Log.Info("Item already locked");
                return;
            }

            var shouldUpdateSummary = AttemptItemChange(tabTier, pickupDef);
            if (shouldUpdateSummary)
            {
                draftSummaryBar.UpdateSummary();
            }
        }


        private void HandleRandomizeAll()
        {
            // handles the logic needed to randomize everything
            DraftLoadoutRandomizer.RandomizeAll();

            RefreshTabs();
            this.draftSummaryBar.UpdateSummary();
        }

        private void HandleRandomizeTabRequest(DraftItemTier tier)
        {
            DraftLoadoutRandomizer.RandomizeTab(tier);

            RefreshTabs();
            this.draftSummaryBar.UpdateSummary();
        }

        #endregion

        #region Event Handler Helpers

        /// <summary>
        /// Attempts to change the state of the given item, if possible
        /// </summary>
        /// <param name="tabTier"></param>
        /// <param name="pickupDef"></param>
        /// <returns>true if the loadout changed state due to this pick, false otherwise. True should cause a refresh of the UI</returns>
        private bool AttemptItemChange(DraftItemTier tabTier, PickupDef pickupDef)
        {

            if (tabTier == DraftItemTier.Purple)
            {
                return AttemptVoidItemChange(tabTier, pickupDef);
            }

            // TODO if its corrupted -> handle pick
            // else regular flow


            ToggleValidPick(pickupDef);

            // TODO if corrupted, unpick corresponding item
            // Reflect any tabs that have changed
            tabControllersByTier[tabTier].RefreshItems();

            return true;
        }

        /// <summary>
        /// Handles void item logic (locking/unlocking normal items)
        /// </summary>
        /// <param name="tabTier"></param>
        /// <param name="pickupDef"></param>
        /// <returns>true if the operation caused a state chagne for the loadout, false otherwise</returns>
        private bool AttemptVoidItemChange(DraftItemTier tabTier, PickupDef pickupDef)
        {
            // determine if its a pick or repick
            var currentlyPicked = DraftLoadout.Instance.IsPicked(pickupDef);
            if (currentlyPicked)
            {
                return UnpickVoid(pickupDef);
            }

            return PickVoid(pickupDef);
        }

        /// <summary>
        /// Unpicks a void and then handles unlocking normals
        /// </summary>
        /// <param name="voidDef"></param>
        private bool UnpickVoid(PickupDef voidDef)
        {
            DraftLoadout.Instance.UnPick(voidDef);

            var tabsToUpdate = new HashSet<DraftItemTier>
            {
                DraftItemTier.Purple
            };

            var unlockedItems = UnlockRelatedToVoid(voidDef.itemIndex);
            foreach (var unlockedItem in unlockedItems)
            {
                var unlockDef = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(unlockedItem));
                var tabForItem = DraftTierMaps.ToDraft(unlockDef.itemTier);
                tabsToUpdate.Add(tabForItem);
            }

            foreach (var tabTier in tabsToUpdate)
            {
                tabControllersByTier[tabTier].RefreshItems();
            }

            return true;
        }

        private bool PickVoid(PickupDef voidDef)
        {
            var wasPicked = DraftLoadout.Instance.TryPick(voidDef, out var _);
            if (!wasPicked)
            {
                return false;
            }

            var tabsToUpdate = new HashSet<DraftItemTier>
            {
                DraftItemTier.Purple
            };

            // picked so try to lock things
            var normalDefs = CorruptionMaps.GetNormalsPickupDefsForVoid(voidDef.itemIndex);
            foreach (var normalDef in normalDefs)
            {
                var tabForItem = DraftTierMaps.ToDraft(normalDef.itemTier);

                // if its picked, then unpick
                var normalPicked = DraftLoadout.Instance.IsPicked(normalDef);
                if (normalPicked)
                {
                    DraftLoadout.Instance.UnPick(normalDef);
                    tabsToUpdate.Add(tabForItem);
                }

                // TODO handle if it didn't work
                var couldLock = DraftLoadout.Instance.TryLock(normalDef);
                if (couldLock)
                {
                    tabsToUpdate.Add(tabForItem);
                }
            }

            foreach (var tabTier in tabsToUpdate)
            {
                tabControllersByTier[tabTier].RefreshItems();

            }

            return true;
        }

        private void ToggleValidPick(PickupDef pickupDef)
        {
            var currentlyPicked = DraftLoadout.Instance.IsPicked(pickupDef);
            if (currentlyPicked)
            {
                DraftLoadout.Instance.UnPick(pickupDef);
            }
            else
            {
                // TODO handle reason
                DraftLoadout.Instance.TryPick(pickupDef, out var _);
            }
        }

        private void UpdateLimitRequest(DraftItemTier tabTier, int amount)
        {
            var currLimit = DraftLoadout.Instance.GetLimit(tabTier);
            var newLimit = currLimit + amount;

            DraftLoadout.Instance.SetLimit(tabTier, newLimit);

            // only if we decrase do we reconsolidate
            if (amount < 0)
            {
                Log.Info("Reconsolidate trim");
                var removedItems = DraftLoadout.Instance.TrimToLimit(tabTier);
                Log.Info($"Removed {removedItems.Count}");
                if (tabTier == DraftItemTier.Purple && removedItems.Count > 0)
                {
                    UnlockAllRelatedToVoid(removedItems);
                }
            }

            var tabController = tabControllersByTier[tabTier];
            tabController.RefreshLimit();
        }


        private void RefreshTabs()
        {
            foreach (var tabEntry in tabControllersByTier)
            {
                tabEntry.Value.RefreshLimit();
            }
        }

        private void UnlockAllRelatedToVoid(HashSet<ItemIndex> voidIndexes)
        {
            foreach (var voidIndex in voidIndexes)
            {
                // dont need the result
                UnlockRelatedToVoid(voidIndex);
            }

            // refresh the possible tabs
            tabControllersByTier[DraftItemTier.White].RefreshItems();
            tabControllersByTier[DraftItemTier.Green].RefreshItems();
            tabControllersByTier[DraftItemTier.Red].RefreshItems();
        }

        /// <summary>
        /// Unlocks any currently locked items by the void
        /// Useful when the void is no longer picked and we need to unlock state
        /// </summary>
        /// <param name="voidIndex">the item index for the void</param>
        /// <returns>the item indexes that were unlocked</returns>
        private HashSet<ItemIndex> UnlockRelatedToVoid(ItemIndex voidIndex)
        {
            Log.Info($"Unlocking related to void {voidIndex}");

            var unlockedItems = new HashSet<ItemIndex>();

            var normalDefs = CorruptionMaps.GetNormalsPickupDefsForVoid(voidIndex);
            foreach (var normalDef in normalDefs)
            {
                // if its locked, then unlock
                var normalLocked = DraftLoadout.Instance.IsLocked(normalDef);
                if (normalLocked)
                {
                    DraftLoadout.Instance.Unlock(normalDef);
                    unlockedItems.Add(normalDef.itemIndex);
                }
            }

            return unlockedItems;
        }

        #endregion

        private void BuildDialogShell(RectTransform parentRectTransform)
        {
            var draftingDialog = new GameObject("DraftManagerDialog", typeof(RectTransform), typeof(VerticalLayoutGroup));
            FactoryUtils.ParentToRectTransform(draftingDialog, parentRectTransform);

            // fill the content root
            var center = new Vector2(0.5f, 0.5f);
            draftingDialogRect = (RectTransform)draftingDialog.transform;
            draftingDialogRect.anchorMin = Vector2.zero;
            draftingDialogRect.anchorMax = Vector2.one;
            draftingDialogRect.pivot = center;
            draftingDialogRect.offsetMin = Vector2.zero;
            draftingDialogRect.offsetMax = Vector2.zero;

            var layout = draftingDialog.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            // Debug BG
            var debugBG = new GameObject("BG", typeof(RectTransform));
            FactoryUtils.ParentToRectTransform(debugBG, draftingDialogRect);

            var debugBGRt = (RectTransform)debugBG.transform;
            debugBGRt.anchorMin = Vector2.zero;
            debugBGRt.anchorMax = Vector2.one;
            debugBGRt.pivot = new Vector2(0.5f, 0.5f);
            debugBGRt.offsetMin = Vector2.zero;   // no margins
            debugBGRt.offsetMax = Vector2.zero;

            if (SHOW_DEBUG_COLORS)
            {
                // greenish
                var img = debugBG.AddComponent<Image>();
                img.color = new Color(0.5f, 0, 0, 1);
                img.raycastTarget = true;
            }
        }


        private void BuildUI()
        {
            RectTransform headerBar = BuildHeaderBar();
            headerBar.SetParent(draftingDialogRect, false);
            var headerLE = headerBar.gameObject.AddComponent<LayoutElement>();
            headerLE.preferredHeight = 44;
            headerLE.flexibleHeight = 0;

            RectTransform tabsBar = BuildTabsBar();
            tabsBar.SetParent(draftingDialogRect, false);
            var tabsLE = tabsBar.gameObject.AddComponent<LayoutElement>();
            tabsLE.preferredHeight = 40;
            tabsLE.flexibleHeight = 0;

            // let it fill rest
            RectTransform contentArea = BuildContentArea();
            contentArea.SetParent(draftingDialogRect, false);
            var contentAreaLE = contentArea.gameObject.AddComponent<LayoutElement>();
            contentAreaLE.preferredHeight = 472;

            RectTransform bottomBar = BuildBottomBar();
            bottomBar.SetParent(draftingDialogRect, false);
            var bottomAreaLE = bottomBar.gameObject.AddComponent<LayoutElement>();
            bottomAreaLE.preferredHeight = 44;
            bottomAreaLE.flexibleHeight = 0;
        }

        static void LogRect(string name, RectTransform rt)
        {
            var r = rt.rect;
            Debug.Log($"[{name}] {r.width}x{r.height}  anchors {rt.anchorMin}->{rt.anchorMax}  pivot {rt.pivot}");
        }

        private void WipTabToggles(DraftItemTier desiredTier)
        {
            // TODO track inactive and swap
            foreach (var key in tabControllersByTier.Keys)
            {
                tabControllersByTier[key].gameObject.SetActive(false);
                this.draftTabsBar.UnselectTab(key);
            }

            tabControllersByTier[desiredTier].gameObject.SetActive(true);
            this.draftTabsBar.SelectTab(desiredTier);
        }

        private RectTransform BuildHeaderBar()
        {
            var root = new GameObject("Title and Subtitle", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            var rt = (RectTransform)root.transform;

            var vg = root.GetComponent<VerticalLayoutGroup>();
            // L/R = 16, top = 16
            vg.padding = new RectOffset(8, 8, 8, 8);
            vg.spacing = 8f;
            vg.childControlWidth = true;
            vg.childForceExpandWidth = true;
            vg.childControlHeight = true;
            vg.childForceExpandHeight = false;

            var le = root.GetComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.flexibleHeight = 0f;

            // TITLE
            var titleGO = new GameObject("Title Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var titleRT = (RectTransform)titleGO.transform;
            titleRT.SetParent(rt, false);

            var titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
            titleTMP.text = "Artifact of Choice";
            titleTMP.fontSize = 32;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = Color.white;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.enableWordWrapping = false;
            titleTMP.raycastTarget = false;

            //// DESCRIPTION / SUBTITLE
            //var subGO = new GameObject("Subtitle Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            //var subRT = (RectTransform)subGO.transform;
            //subRT.SetParent(rt, false);

            //var subTMP = subGO.GetComponent<TextMeshProUGUI>();
            //subTMP.text = "Only these <style=cIsUtility>Items</style> will be available when using the <style=cArtifact>Artifact of Command</style>.";
            //subTMP.fontSize = 22;
            //subTMP.color = new Color(0.641f, 0.667f, 0.670f, 1f);
            //subTMP.alignment = TextAlignmentOptions.Left;
            //subTMP.enableWordWrapping = true;
            //subTMP.richText = true;
            //subTMP.raycastTarget = false;

            // DIVIDER holder (lets us inset the line by 16 on each side while using a VLG)
            var dividerHolder = new GameObject("BorderBreak", typeof(RectTransform), typeof(LayoutElement));
            var divHoldRT = (RectTransform)dividerHolder.transform;
            divHoldRT.SetParent(rt, false);

            var divLE = dividerHolder.GetComponent<LayoutElement>();
            divLE.minHeight = 9f;
            divLE.preferredHeight = 9f;
            divLE.flexibleWidth = 1f;

            // Actual divider image stretches inside holder with L/R insets
            var divImgGO = new GameObject("Image", typeof(RectTransform), typeof(Image));
            var divImgRT = (RectTransform)divImgGO.transform;
            divImgRT.SetParent(divHoldRT, false);
            divImgRT.anchorMin = new Vector2(0, 0.5f);
            divImgRT.anchorMax = new Vector2(1, 0.5f);
            divImgRT.pivot = new Vector2(0.5f, 0.5f);
            divImgRT.sizeDelta = new Vector2(0, 9);
            divImgRT.offsetMin = new Vector2(16, -9);  // L = 16, bottom = -9 (matches your dump)
            divImgRT.offsetMax = new Vector2(-16, 0);  // R = 16

            var divImg = divImgGO.GetComponent<Image>();
            divImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHeaderSingle.png").WaitForCompletion();
            divImg.type = Image.Type.Sliced;
            divImg.color = new Color(0.753f, 0.753f, 0.753f, 1f);
            divImg.raycastTarget = false;

            return rt;
        }

        private RectTransform BuildTabsBar()
        {
            // build the tabs bar
            var tabsBar = new GameObject("TabsBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var tabsBarRt = (RectTransform)tabsBar.transform;

            var controlsHG = tabsBar.GetComponent<HorizontalLayoutGroup>();
            controlsHG.spacing = 4;
            controlsHG.childControlHeight = true;
            controlsHG.childControlWidth = true;
            controlsHG.childForceExpandHeight = false;
            controlsHG.childForceExpandWidth = false;
            controlsHG.childAlignment = TextAnchor.MiddleCenter;
            controlsHG.padding = new RectOffset(8, 8, 8, 8);

            draftTabsBar = tabsBar.AddComponent<DraftTabsBar>();

            // ==== Full-bleed blue background ====
            var bg = new GameObject("TabsBarBG", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)bg.transform;
            bgRt.SetParent(tabsBarRt, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            var img = bg.GetComponent<Image>();
            img.color = Color.blue;           // solid red to verify full stretch
            img.raycastTarget = true;        // optional click blocker


            var buttonDimensions = new Vector2(128, 48);
            foreach (DraftItemTier draftTier in Enum.GetValues(typeof(DraftItemTier)))
            {

                var draftTarButton = DraftTabButtonFactory.CreateTabButton(
                    draftTier, DraftTierLabels.GetUIName(draftTier),
                    buttonDimensions, DraftTabButtonPalette.GetColorsForTab(draftTier));
                var buttonGO = draftTarButton.gameObject;
                FactoryUtils.ParentToRectTransform(buttonGO, tabsBarRt);

                draftTabsBar.RegisterButton(draftTier, draftTarButton);
                draftTarButton.OnTabButtonClicked += WipTabToggles;
            }

            return tabsBarRt;
        }

        private static readonly string RimPath = "RoR2/Base/UI/texUIAnimateSliceNakedButton.png";
        private static readonly string CleanBtnPath = "RoR2/Base/UI/texUICleanButton.png";
        private static readonly string CleanPanelPath = "RoR2/Base/UI/texUICleanPanel.png";
        private static readonly string BackdropPath = "RoR2/Base/UI/texUIBackdrop.png";

        private RectTransform BuildContentArea()
        {
            var contentArea = new GameObject("ContentArea", typeof(RectTransform));
            var contentAreaRt = (RectTransform)contentArea.transform;

            // ==== Full-bleed blue background ====
            var bg = new GameObject("ContentAreaBG", typeof(RectTransform));
            var bgRt = (RectTransform)bg.transform;
            bgRt.SetParent(contentAreaRt, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            if(SHOW_DEBUG_COLORS)
            {
                var img = bg.AddComponent<Image>();
                img.color = Color.yellow;        
                img.raycastTarget = false;       
            }


            foreach (DraftItemTier draftTier in Enum.GetValues(typeof(DraftItemTier)))
            {
                var draftTab = DraftTabFactory.BuildDraftTabNew(contentAreaRt, nameof(draftTier) + "_Tab", draftTier);
                tabControllersByTier[draftTier] = draftTab;
                // all off
                draftTab.gameObject.SetActive(false);

                // bind listeners
                draftTab.OnItemClicked += HandleGridItemClicked;
                draftTab.OnModeChangeRequest += HandleModeChangeRequest;
                draftTab.OnLimitIncreaseRequest += HandleLimitIncreaseRequest;
                draftTab.OnLimitDecreaseRequest += HandleLimitDecreaseRequest;
                draftTab.OnRandomizeTabRequest += HandleRandomizeTabRequest;
            }

            MarkFirstTab(DraftItemTier.White);

            return contentAreaRt;
        }

        private void MarkFirstTab(DraftItemTier itemTier)
        {
            // Pick the white tab first
            tabControllersByTier[itemTier].gameObject.SetActive(true);
            draftTabsBar.SelectTab(itemTier);
        }

        private RectTransform BuildBottomBar()
        {
            // build the tabs bar
            var bottomSection = new GameObject("BottomBar", typeof(RectTransform), typeof(VerticalLayoutGroup));
            var tabsBarRt = (RectTransform)bottomSection.transform;

            // ==== Full-bleed blue background ====
            var bg = new GameObject("BottomBarBG", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)bg.transform;
            bgRt.SetParent(tabsBarRt, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            var img = bg.GetComponent<Image>();
            img.color = Color.green;           // solid red to verify full stretch
            img.raycastTarget = true;        // optional click blocker

            var layout = bottomSection.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            // our nice summary bar probs
            this.draftSummaryBar = DraftSummaryBarFactory.CreateBar(tabsBarRt);


            // bind stuff
            this.draftSummaryBar.OnRandomizeClick += HandleRandomizeAll;

            return tabsBarRt;
        }



        /// <summary>
        /// Black fill with double white outline. Neutral state only (no highlight/press).
        /// </summary>
        public static GameObject CreateDoubleRimButton(RectTransform parent, string text, Vector2 size, System.Action onClick = null)
        {
            var rimSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIAnimateSliceNakedButton.png").WaitForCompletion();
            var sheenSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIBackdropFadedEnds.png").WaitForCompletion();

            // Root
            var root = new GameObject($"Btn_{text}", typeof(RectTransform), typeof(Button));
            var rt = (RectTransform)root.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = size;
            var btn = root.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;

            var hitbox = root.AddComponent<Image>();
            hitbox.color = new Color(0, 0, 0, 0.001f);   // nearly invisible, still raycastable
            hitbox.raycastTarget = true;
            btn.targetGraphic = hitbox;

            // RimBase (dim)
            var baseGO = new GameObject("RimBase", typeof(RectTransform), typeof(Image));
            var baseRT = (RectTransform)baseGO.transform;
            baseRT.SetParent(rt, false);
            baseRT.anchorMin = Vector2.zero; baseRT.anchorMax = Vector2.one;
            var rimBase = baseGO.GetComponent<Image>();
            rimBase.sprite = rimSprite; rimBase.type = Image.Type.Sliced;
            // Dim outline: off-white + low alpha (tweak to taste)
            rimBase.color = new Color(1f, 1f, 1f, 0.35f);
            rimBase.raycastTarget = false;

            // ActiveRim (bright; hidden by default)
            var activeGO = new GameObject("ActiveRim", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var activeRT = (RectTransform)activeGO.transform;
            activeRT.SetParent(rt, false);
            activeRT.anchorMin = Vector2.zero; activeRT.anchorMax = Vector2.one;
            var rimActive = activeGO.GetComponent<Image>();
            rimActive.sprite = rimSprite; rimActive.type = Image.Type.Sliced;
            rimActive.color = Color.white;
            rimActive.raycastTarget = false;
            var activeCG = activeGO.GetComponent<CanvasGroup>();
            activeCG.alpha = 0f; // ← hidden until hover/selected

            // Fill (black)
            var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fillRT = (RectTransform)fillGO.transform;
            fillRT.SetParent(rt, false);
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(8, 8); fillRT.offsetMax = new Vector2(-8, -8);
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.color = Color.black; fillImg.raycastTarget = false;

            // Sheen (top gradient, matches Fill inset)
            var sheenGO = new GameObject("Sheen", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var sheenRT = (RectTransform)sheenGO.transform;
            sheenRT.SetParent(rt, false);
            sheenRT.anchorMin = Vector2.zero; sheenRT.anchorMax = Vector2.one;
            sheenRT.offsetMin = new Vector2(8, 8); sheenRT.offsetMax = new Vector2(-8, -8);
            var sheenImg = sheenGO.GetComponent<Image>();
            sheenImg.sprite = sheenSprite; sheenImg.type = Image.Type.Sliced;
            sheenImg.color = Color.white; sheenImg.raycastTarget = false;
            var sheenCG = sheenGO.GetComponent<CanvasGroup>();
            sheenCG.alpha = 0f;

            // Label
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.SetParent(rt, false);
            labelRT.anchorMin = labelRT.anchorMax = new Vector2(0.5f, 0.5f);
            var tmp = labelGO.GetComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text; tmp.color = Color.white; tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false; tmp.raycastTarget = false;

            // Hover/press animator
            var fx = root.AddComponent<RoR2ButtonFx>();
            fx.fill = fillRT;
            fx.fillImage = fillImg;
            fx.activeRim = activeCG;
            fx.sheen = sheenCG;
            // optional tuning
            fx.baseFillInset = 8f; fx.hoverFillInset = 10f; fx.pressedFillInset = 12f;
            fx.normalFill = Color.black;
            fx.hoverFill = new Color(0.12f, 0.12f, 0.12f);
            fx.pressedFill = new Color(0.20f, 0.20f, 0.20f);


            rimBase.raycastTarget = false;
            rimActive.raycastTarget = false;
            fillImg.raycastTarget = false;
            sheenImg.raycastTarget = false;
            tmp.raycastTarget = false;

            return root;
        }

    }
}
