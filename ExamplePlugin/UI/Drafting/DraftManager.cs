using System;
using System.Collections.Generic;
using ExamplePlugin.Loadout.Corruption;
using ExamplePlugin.Loadout.Draft;
using ExamplePlugin.UI.Branding.Buttons;
using ExamplePlugin.UI.Drafting.Summary;
using RoR2;
using RoR2BepInExPack.GameAssetPaths;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Drafting
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



        /// <summary>
        /// Game object that encompasses our dialog for drafting items
        /// </summary>
        private GameObject draftingDialog;


        private DraftTabController[] draftTabs;

        /// <summary>
        /// Parent for our full layout, 
        /// currently this is already stretched to the SafeArea on the Lobby UI
        /// </summary>
        private RectTransform rootParent;

        private RectTransform draftingDialogRect;

        private DraftSummaryBar draftSummaryBar;

        /// <summary>
        ///  MEGA WIP
        /// </summary>
        private static Dictionary<DraftItemTier, GameObject> tabsByTier = new();

        public void Initialize(RectTransform parentRectTransform)
        {
            Log.Info("DraftManager.Initialize");

            this.rootParent = parentRectTransform;

            BuildDialogShell(parentRectTransform);
            BuildUI();
            // Used to set the mode to none then on restrict update state
            // Now we set that limit state on creation
            // TODO draft loaodut seed limits call
            RefreshTabs();
            draftSummaryBar.UpdateSummary();

            // refresh all tabs so we're up to date
            // draftingDialog = this.BuildDraftPickerRootStructure(parentRectTransform);
        }


        public bool IsVisible()
        {
            var currVis = draftingDialog.activeSelf;
            Log.Info($"DraftManager.isVisible {currVis}");
            return currVis;
        }

        public void Show()
        {
            Log.Info("DraftManager.Show");
            draftingDialog.SetActive(true);
        }

        public void Hide()
        {
            Log.Info("DraftManager.Hide");
            draftingDialog.SetActive(false);
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
            tabsByTier[tabTier].GetComponent<DraftTabController>().RefreshItems();

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

            var normalDefs = GetNormalsForVoid(voidDef);
            foreach (var normalDef in normalDefs)
            {
                // if its locked, then unlock
                var normalLocked = DraftLoadout.Instance.IsLocked(normalDef);
                if (normalLocked)
                {
                    DraftLoadout.Instance.Unlock(normalDef);
                    var tabForItem = DraftTierMaps.ToDraft(normalDef.itemTier);
                    tabsToUpdate.Add(tabForItem);
                }
            }

            foreach (var tabTier in tabsToUpdate)
            {
                tabsByTier[tabTier].GetComponent<DraftTabController>().RefreshItems();
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
            var normalDefs = GetNormalsForVoid(voidDef);
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
                tabsByTier[tabTier].GetComponent<DraftTabController>().RefreshItems();

            }

            return true;
        }

        private List<PickupDef> GetNormalsForVoid(PickupDef voidDef)
        {
            var normalDefs = new List<PickupDef>();
            foreach (var normalIndex in CorruptionMaps.GetNormalsForVoid(voidDef.itemIndex))
            {
                var normalDef = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(normalIndex));
                if (normalDef == null)
                {
                    Log.Warning($"Got a null PickupDef with index {normalIndex}");
                }
                normalDefs.Add(normalDef);
            }

            return normalDefs;
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
                DraftLoadout.Instance.TrimToLimit(tabTier);
            }

            var tabController = tabsByTier[tabTier].GetComponent<DraftTabController>();
            tabController.RefreshLimit();
        }


        private void RefreshTabs()
        {
            foreach (var tabEntry in tabsByTier)
            {
                tabEntry.Value.GetComponent<DraftTabController>().RefreshLimit();
            }
        }

        #endregion

        private void BuildDialogShell(RectTransform parentRectTransform)
        {
            draftingDialog = new GameObject("DraftManagerDialog", typeof(RectTransform), typeof(VerticalLayoutGroup));
            FactoryUtils.ParentToRectTransform(draftingDialog, parentRectTransform);

            var center = new Vector2(0.5f, 0.5f);
            draftingDialogRect = (RectTransform)draftingDialog.transform;
            draftingDialogRect.sizeDelta = new Vector2(720, 720);
            draftingDialogRect.pivot = center;
            draftingDialogRect.anchorMin = center;
            draftingDialogRect.anchorMax = center;

            var layout = draftingDialog.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            // Debug BG
            var debugBG = new GameObject("BG", typeof(RectTransform), typeof(Image));
            FactoryUtils.ParentToRectTransform(debugBG, draftingDialogRect);

            var debugBGRt = (RectTransform)debugBG.transform;
            debugBGRt.anchorMin = Vector2.zero;
            debugBGRt.anchorMax = Vector2.one;
            debugBGRt.pivot = new Vector2(0.5f, 0.5f);
            debugBGRt.offsetMin = Vector2.zero;   // no margins
            debugBGRt.offsetMax = Vector2.zero;

            // greenish
            var img = debugBG.GetComponent<Image>();
            img.color = new Color(0.5f, 0, 0, 1);
            img.raycastTarget = true;
        }


        private void BuildUI()
        {
            RectTransform tabsBar = BuildTabsBar();
            tabsBar.SetParent(draftingDialogRect, false);
            var tabsLE = tabsBar.gameObject.AddComponent<LayoutElement>();
            tabsLE.preferredHeight = 64;
            tabsLE.flexibleHeight = 0;

            // let it fill rest
            RectTransform contentArea = BuildContentArea();
            contentArea.SetParent(draftingDialogRect, false);
            var contentAreaLE = contentArea.gameObject.AddComponent<LayoutElement>();
            contentAreaLE.flexibleHeight = 1;
            contentAreaLE.preferredHeight = 472;

            RectTransform bottomBar = BuildBottomBar();
            bottomBar.SetParent(draftingDialogRect, false);
            var bottomAreaLE = bottomBar.gameObject.AddComponent<LayoutElement>();
            bottomAreaLE.preferredHeight = 64;
            bottomAreaLE.flexibleHeight = 0;

            // TODO here we would have the Summary area

        }

        private GameObject BuildDraftPickerRootStructure(RectTransform parentTransform)
        {
            var draftDialogRoot = new GameObject("DraftManagerDialog", typeof(RectTransform));
            FactoryUtils.ParentToRectTransform(draftDialogRoot, parentTransform);

            // TODO figure out our preferred sizing            
            var draftDialogRectTransform = (RectTransform)draftDialogRoot.transform;
            draftDialogRectTransform.sizeDelta = new Vector2(720, 720);

            // ==== Full-bleed RED background ====
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)bg.transform;
            bgRt.SetParent(draftDialogRectTransform, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            var img = bg.GetComponent<Image>();
            img.color = new Color(1f, 0, 0, .25f);           // solid red to verify full stretch
            img.raycastTarget = true;        // optional click blocker

            // The content, we will tinker with the layout in a bit
            var tabsGroup = BuildTabsGroup(draftDialogRectTransform);

            // DEBUG: print rect sizes to verify
            LogRect("Parent", parentTransform);
            LogRect("Root", draftDialogRectTransform);
            LogRect("BG", bgRt);
            //  LogRect("TabsGroup", tabsGroup);

            return draftDialogRoot;
        }

        static void LogRect(string name, RectTransform rt)
        {
            var r = rt.rect;
            Debug.Log($"[{name}] {r.width}x{r.height}  anchors {rt.anchorMin}->{rt.anchorMax}  pivot {rt.pivot}");
        }

        private void WipTabToggles(DraftItemTier desiredTier)
        {

            foreach (var key in tabsByTier.Keys)
            {
                tabsByTier[key].SetActive(false);
            }

            tabsByTier[desiredTier].SetActive(true);
        }


        private RectTransform BuildTabsGroup(RectTransform parentRectTransform)
        {
            var group = new GameObject("TabGroup", typeof(RectTransform), typeof(VerticalLayoutGroup));

            FactoryUtils.ParentToRectTransform(group, parentRectTransform);

            var groupRt = (RectTransform)group.transform;
            FactoryUtils.StretchToFillParent(groupRt);

            // TODO iono why this is not respecting parent probably cause i do stretch?
            // groupRt.sizeDelta = new Vector2(720, 720);

            var layout = group.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            RectTransform tabsBar = BuildTabsBar();
            tabsBar.SetParent(groupRt, false);
            var tabsLE = tabsBar.gameObject.AddComponent<LayoutElement>();
            tabsLE.preferredHeight = 64;
            tabsLE.flexibleHeight = 0;

            // let it fill rest
            RectTransform contentArea = BuildContentArea();
            contentArea.SetParent(groupRt, false);
            var contentAreaLE = contentArea.gameObject.AddComponent<LayoutElement>();
            contentAreaLE.flexibleHeight = 1;
            contentAreaLE.preferredHeight = 472;

            RectTransform bottomBar = BuildBottomBar();
            bottomBar.SetParent(groupRt, false);
            var bottomAreaLE = bottomBar.gameObject.AddComponent<LayoutElement>();
            bottomAreaLE.preferredHeight = 64;
            bottomAreaLE.flexibleHeight = 0;

            // TODO here we would have the Summary area

            return groupRt;
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

            var draftTabsBar = tabsBar.AddComponent<DraftTabsBar>();

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


            var buttonDimensions = new Vector2(128, 64);
            foreach (DraftItemTier draftTier in Enum.GetValues(typeof(DraftItemTier)))
            {
                var tierButtonGO = DraftTabsBarFactory.CreateTextButton(
                    nameof(draftTier) + "_TabButton",
                    buttonDimensions,
                    DraftTierLabels.GetUIName(draftTier)
                );
                FactoryUtils.ParentToRectTransform(tierButtonGO, tabsBarRt);
                draftTabsBar.BindButton(draftTier, tierButtonGO.GetComponent<Button>());
            }

            draftTabsBar.OnTabButtonClicked += WipTabToggles;

            // Test add styled component
            var testb = CreateDoubleRimButton(tabsBarRt, "Test", new Vector2(128, 64));

            return tabsBarRt;
        }
        private static readonly string RimPath = "RoR2/Base/UI/texUIAnimateSliceNakedButton.png";
        private static readonly string CleanBtnPath = "RoR2/Base/UI/texUICleanButton.png";
        private static readonly string CleanPanelPath = "RoR2/Base/UI/texUICleanPanel.png";
        private static readonly string BackdropPath = "RoR2/Base/UI/texUIBackdrop.png";

        private GameObject BrandedButton(RectTransform parent, string text)
        {
            var rim = Addressables.LoadAssetAsync<Sprite>(RimPath).WaitForCompletion();
            Log.Info("rim: " + rim);
            var backdrop = Addressables.LoadAssetAsync<Sprite>(BackdropPath).WaitForCompletion();
            Log.Info("backdrop: " + backdrop);
            var clean = Addressables.LoadAssetAsync<Sprite>(CleanBtnPath).WaitForCompletion()
                     ?? Addressables.LoadAssetAsync<Sprite>(CleanPanelPath).WaitForCompletion();
            Log.Info("clean: " + clean);

            var root = new GameObject($"Btn_{text}", typeof(RectTransform), typeof(Button), typeof(LayoutElement));
            var rt = (RectTransform)root.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(128, 64);
            rt.pivot = new Vector2(0.5f, 0.5f);

            var buttonDimensions = new Vector2(64, 64);
            var le = root.GetComponent<LayoutElement>();
            le.preferredWidth = buttonDimensions.x;
            le.preferredHeight = buttonDimensions.y;
            le.minWidth = buttonDimensions.x;
            le.minHeight = buttonDimensions.y;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;

            // BG (rim)
            var bgGO = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var bgRT = (RectTransform)bgGO.transform;
            bgRT.SetParent(rt, false);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.sprite = rim;
            bgImg.type = Image.Type.Sliced;        // IMPORTANT for nine-slice
            bgImg.raycastTarget = true;

            // Fill (inner)
            var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fillRT = (RectTransform)fillGO.transform;
            fillRT.SetParent(rt, false);
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            // inset so the rim is visible
            fillRT.offsetMin = new Vector2(4f, 4f);
            fillRT.offsetMax = new Vector2(-4f, -4f);
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.sprite = clean;

            // Content + Label
            var content = new GameObject("Content", typeof(RectTransform));
            var contentRT = (RectTransform)content.transform;
            contentRT.SetParent(rt, false);
            contentRT.anchorMin = Vector2.zero; contentRT.anchorMax = Vector2.one;
            contentRT.offsetMin = new Vector2(8f, 6f);
            contentRT.offsetMax = new Vector2(-8f, -6f);

            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var labelRT = (RectTransform)label.transform;
            labelRT.SetParent(contentRT, false);
            labelRT.anchorMin = labelRT.anchorMax = new Vector2(0.5f, 0.5f);
            labelRT.sizeDelta = Vector2.zero;

            var tmp = label.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 14;
            tmp.fontSizeMax = 28;
            tmp.raycastTarget = false;
            tmp.color = Color.white;

            // Button tint targets the rim only
            var btn = root.GetComponent<Button>();
            btn.targetGraphic = bgImg;
            var colors = btn.colors;
            colors.fadeDuration = 0.08f;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, .9f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, .75f);
            colors.selectedColor = new Color(1f, 1f, 1f, .85f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.4f);
            btn.colors = colors;

            return root;
        }

        private RectTransform BuildContentArea()
        {
            var contentArea = new GameObject("ContentArea", typeof(RectTransform));
            var contentAreaRt = (RectTransform)contentArea.transform;

            // ==== Full-bleed blue background ====
            var bg = new GameObject("ContentAreaBG", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)bg.transform;
            bgRt.SetParent(contentAreaRt, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            var img = bg.GetComponent<Image>();
            img.color = Color.yellow;           // solid red to verify full stretch

            img.raycastTarget = true;        // optional click blocker


            foreach (DraftItemTier draftTier in Enum.GetValues(typeof(DraftItemTier)))
            {
                var draftTab = DraftTabFactory.BuildDraftTab(contentAreaRt, nameof(draftTier) + "_Tab", draftTier);
                tabsByTier[draftTier] = draftTab.gameObject;
                // all off
                draftTab.gameObject.SetActive(false);

                // bind listeners
                draftTab.OnItemClicked += HandleGridItemClicked;
                draftTab.OnModeChangeRequest += HandleModeChangeRequest;
                draftTab.OnLimitIncreaseRequest += HandleLimitIncreaseRequest;
                draftTab.OnLimitDecreaseRequest += HandleLimitDecreaseRequest;
            }

            // Pick the white tab first
            tabsByTier[DraftItemTier.White].gameObject.SetActive(true);

            return contentAreaRt;
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
