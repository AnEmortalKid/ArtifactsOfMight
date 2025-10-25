using System;
using System.Collections.Generic;
using System.Text;
using ArtifactsOfMight.Loadout.Draft;
using UnityEngine.UI;
using UnityEngine;
using RoR2;
using ArtifactsOfMight.UI.Drafting.Grid;
using UnityEngine.AddressableAssets;

namespace ArtifactsOfMight.UI.Drafting
{
    public static class DraftTabFactory
    {

        /// <summary>
        ///  Use Top-Center as our anchoring point for the grid to position itself within the shell with the outline
        /// </summary>
        private static Vector2 GRID_ANCHOR_PIVOT = new Vector2(0.5f, 1f);

        public static DraftTabController BuildDraftTab(RectTransform parentRectTransform,
            string tabName, DraftItemTier tabTier)
        {
            var parentObject = parentRectTransform.gameObject;

            // set tab up correctly then everything hangs off tab
            var tabGO = new GameObject(tabName, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(DraftTabController));
            FactoryUtils.ParentToRectTransform(tabGO, parentRectTransform);

            var tabRootRT = (RectTransform)tabGO.transform;
            // stretch tab to fill
            StretchFull(tabRootRT);

            var layout = tabGO.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            // dont let it expand
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            var controlsBar = ItemPickerTabControlsFactory.CreateTabControls(tabName + "_Controls", tabRootRT, tabTier);
            var controlsBarGO = controlsBar.gameObject;

            // shrink it
            var tabsLE = controlsBarGO.AddComponent<LayoutElement>();
            tabsLE.preferredHeight = 36;
            tabsLE.flexibleHeight = 0;
            tabsLE.flexibleWidth = 1;

            var gridGO = new GameObject(tabName + "_Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ItemPickerGridController));
            gridGO.layer = parentObject.layer;
            var gridRt = (RectTransform)gridGO.transform;
            gridRt.SetParent(tabRootRT, worldPositionStays: false);

            var itemsControls = controlsBar.GetComponent<DraftTabControls>();

            var tabController = tabGO.GetComponent<DraftTabController>();
            tabController.SetTabTier(tabTier);
            tabController.SetControls(itemsControls);

            var gridController = gridGO.GetComponent<ItemPickerGridController>();
            tabController.SetGridController(gridController);

            // gonna use top left corner to position
            gridRt.anchorMin = gridRt.anchorMax = new Vector2(0f, 1f);
            gridRt.pivot = new Vector2(0f, 1f);

            gridRt.offsetMin = new Vector2(16, 16);
            gridRt.offsetMax = new Vector2(-16, -16);

            var gridRtLE = gridGO.AddComponent<LayoutElement>();
            //gridRtLE.flexibleHeight = 1;
            gridRtLE.preferredHeight = 472;

            var gridBG = gridGO.AddComponent<Image>();
            // blue gray ish
            gridBG.color = ColorPalette.FromRGB(49, 60, 77);
            gridBG.raycastTarget = false;

            var grid = gridGO.GetComponent<GridLayoutGroup>();
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;

            grid.cellSize = new Vector2(72, 72);
            // formula is selection offset * 2 + 4
            grid.spacing = new Vector2(10, 10);
            grid.padding = new RectOffset(10, 10, 10, 10);

            // The command card uses 5 columns
            // but we have more real estate here so we will go with 8
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;

            var squareControllers = new List<ItemPickerSquareController>();
            var draftPickups = DraftPools.Instance.GetDraftablePickups(tabTier);
            var squareOutline = GetSelectionOutlineColor(tabTier);
            var hoverColor = GridSquarePalette.GetHoverOutlineColor(tabTier);
            foreach (var pickIndex in draftPickups)
            {
                var pickDef = PickupCatalog.GetPickupDef(pickIndex);
                var singleSquare = ItemPickerSquareFactory.TestItemSquare(gridRt, pickDef, squareOutline, hoverColor);
                var singleController = singleSquare.GetComponent<ItemPickerSquareController>();
                squareControllers.Add(singleController);
            }

            // set our reffies
            gridController.SetSquares(squareControllers);

            return tabController;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;   // bottom-left
            rt.anchorMax = Vector2.one;    // top-right
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;   // no left/bottom padding
            rt.offsetMax = Vector2.zero;   // no right/top padding
            rt.anchoredPosition = Vector2.zero;
        }

        private static Color GetSelectionOutlineColor(DraftItemTier draftItemTier)
        {
            switch (draftItemTier)
            {
                case DraftItemTier.White:
                    return ColorPalette.OutlineWhite;
                case DraftItemTier.Green:
                    return ColorPalette.OutlineGreen;
                case DraftItemTier.Red:
                    return ColorPalette.OutlineRed;
                case DraftItemTier.Yellow:
                    return ColorPalette.OutlineYellow;
                case DraftItemTier.Purple:
                    return ColorPalette.OutlinePurple;
            }

            // Default and obvious
            return Color.cyan;
        }


        private static (RectTransform shell, Image outline, RectTransform fillRT, RectTransform gridAnchor) CreateTieredGridShell(
            RectTransform parent, int preferredHeight, Color outlineTint)
        {
            // Shell (the square/rectangle that holds everything)
            var shellGO = new GameObject("GridPanelShell", typeof(RectTransform), typeof(LayoutElement));
            var shellRT = (RectTransform)shellGO.transform;
            shellRT.SetParent(parent, false);
            shellRT.anchorMin = Vector2.zero;  // stretch horizontally
            shellRT.anchorMax = Vector2.one;
            shellRT.pivot = new Vector2(0.5f, 0.5f);
            shellRT.offsetMin = new Vector2(32, 16);   // L, B
            shellRT.offsetMax = new Vector2(-32, -16); // R, T

            var shellLE = shellGO.GetComponent<LayoutElement>();
            shellLE.flexibleWidth = 1;
            shellLE.flexibleHeight = 0;
            shellLE.preferredHeight = preferredHeight;

            // Fill (soft gray) under the outline
            var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fillRT = (RectTransform)fillGO.transform;
            fillRT.SetParent(shellRT, false);
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;

            fillRT.offsetMin = new Vector2(2, 2);
            fillRT.offsetMax = new Vector2(-2, -2);

            var fillImg = fillGO.GetComponent<Image>();
            fillImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUICleanPanel.png").WaitForCompletion();
            fillImg.type = Image.Type.Sliced;
            fillImg.color = new Color(0.16f, 0.17f, 0.20f, 1f);

            //fillImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIBackdrop.png").WaitForCompletion();
            //fillImg.type = Image.Type.Simple;
            //fillImg.color = Color.white;

            // anchor aligned with the base panel so grid always fits within those bounds
            var anchorGO = new GameObject("GridAnchor", typeof(RectTransform));
            var anchorRT = (RectTransform)anchorGO.transform;
            anchorRT.SetParent(fillRT, false);
            anchorRT.anchorMin = GRID_ANCHOR_PIVOT;
            anchorRT.anchorMax = GRID_ANCHOR_PIVOT;
            anchorRT.pivot = GRID_ANCHOR_PIVOT;

            // move down a bit within that square
            const float topPad = 12f; 
            anchorRT.anchoredPosition = new Vector2(0f, -topPad);

            // Outline (9-slice, tintable)
            var outlineGO = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            var outlineRT = (RectTransform)outlineGO.transform;
            outlineRT.SetParent(shellRT, false);
            outlineRT.anchorMin = Vector2.zero; outlineRT.anchorMax = Vector2.one;
            outlineRT.offsetMin = Vector2.zero; outlineRT.offsetMax = Vector2.zero;

            var outlineImg = outlineGO.GetComponent<Image>();
            outlineImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIOutlineOnly.png").WaitForCompletion();
            outlineImg.type = Image.Type.Sliced;
            outlineImg.color = outlineTint;
            outlineImg.raycastTarget = false;

            return (shellRT, outlineImg, fillRT, anchorRT);
        }

        public static DraftTabController BuildDraftTabNew(RectTransform parentRectTransform, string tabName, DraftItemTier tabTier)
        {
            var parentObject = parentRectTransform.gameObject;

            var tabGO = new GameObject(tabName, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(DraftTabController));
            FactoryUtils.ParentToRectTransform(tabGO, parentRectTransform);

            var tabRootRT = (RectTransform)tabGO.transform;
            StretchFull(tabRootRT);

            var layout = tabGO.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var controlsBar = ItemPickerTabControlsFactory.CreateTabControls(tabName + "_Controls", tabRootRT, tabTier);
            var controlsBarGO = controlsBar.gameObject;

            var tabsLE = controlsBarGO.AddComponent<LayoutElement>();
            tabsLE.preferredHeight = 36;
            tabsLE.flexibleHeight = 0;
            tabsLE.flexibleWidth = 1;

            // tint for the outline based on tier
            var outlineTint = GetSelectionOutlineColor(tabTier);

            // make the shell + anchor
            var (shellRT, outlineImg, fillRT, anchorRT) = CreateTieredGridShell(
                parent: tabRootRT,
                preferredHeight: 472,
                outlineTint: outlineTint
            );

            // actual Grid lives under the anchor
            var gridGO = new GameObject(tabName + "_Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ItemPickerGridController));
            gridGO.layer = parentObject.layer;
            var gridRt = (RectTransform)gridGO.transform;
            // parent to the anchor rect
            gridRt.SetParent(anchorRT, false);

            gridRt.anchorMin = GRID_ANCHOR_PIVOT;
            gridRt.anchorMax = GRID_ANCHOR_PIVOT;
            gridRt.pivot = GRID_ANCHOR_PIVOT;

            // controller wiring (unchanged)
            var itemsControls = controlsBar.GetComponent<DraftTabControls>();
            var tabController = tabGO.GetComponent<DraftTabController>();
            tabController.SetTabTier(tabTier);
            tabController.SetControls(itemsControls);

            var gridController = gridGO.GetComponent<ItemPickerGridController>();
            tabController.SetGridController(gridController);

            // Grid settings (centered; no padding since shell provides spacing)
            var grid = gridGO.GetComponent<GridLayoutGroup>();
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.cellSize = new Vector2(72, 72);
            grid.spacing = new Vector2(10, 10);
            grid.padding = new RectOffset(0, 0, 0, 0);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;

            // build the squares
            var squareControllers = new List<ItemPickerSquareController>();
            var draftPickups = DraftPools.Instance.GetDraftablePickups(tabTier);
            var squareOutline = outlineTint;
            var hoverColor = GridSquarePalette.GetHoverOutlineColor(tabTier);

            foreach (var pickIndex in draftPickups)
            {
                var pickDef = PickupCatalog.GetPickupDef(pickIndex);
                var singleSquare = ItemPickerSquareFactory.TestItemSquare(gridRt, pickDef, squareOutline, hoverColor);
                squareControllers.Add(singleSquare.GetComponent<ItemPickerSquareController>());
            }

            CenterGrid(gridRt, anchorRT, grid, draftPickups.Count);

            // hand refs back to the grid controller
            gridController.SetSquares(squareControllers);

            return tabController;
        }

        private static void CenterGrid(RectTransform gridRT, RectTransform anchorRT, GridLayoutGroup grid, int itemCount)
        {
            int cols = Mathf.Max(1, grid.constraintCount);
            int rows = Mathf.CeilToInt(itemCount / (float)cols);

            var cs = grid.cellSize;
            var sp = grid.spacing;

            float w = cols * cs.x + (cols - 1) * sp.x;
            float h = Mathf.Max(1, rows) * cs.y + (rows - 1) * sp.y;

            // Size grid and anchor exactly; anchor is middle-center so this centers the grid visually.
            gridRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            gridRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            anchorRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            anchorRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        }


    }
}
