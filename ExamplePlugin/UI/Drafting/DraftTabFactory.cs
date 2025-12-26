using System.Collections.Generic;
using ArtifactsOfMight.Loadout.Draft;
using UnityEngine.UI;
using UnityEngine;
using RoR2;
using ArtifactsOfMight.UI.Drafting.Grid;
using UnityEngine.AddressableAssets;
using ArtifactsOfMight.Assets;

namespace ArtifactsOfMight.UI.Drafting
{
    public static class DraftTabFactory
    {

        /// <summary>
        ///  We use top-center
        /// </summary>
        private static Vector2 GRID_ANCHOR_PIVOT = new Vector2(0.5f, 1f);

  
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

            var cleanPanel = AssetCache.LoadSprite(AssetCacheKeys.texUICleanPanel);
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.sprite = cleanPanel;
            fillImg.type = Image.Type.Sliced;
            fillImg.color = new Color(0.16f, 0.17f, 0.20f, 1f);

            //fillImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIBackdrop.png").WaitForCompletion();
            //fillImg.type = Image.Type.Simple;
            //fillImg.color = Color.white;

            // anchor aligned with the base panel so grid always fits within those bounds
            var anchorGO = new GameObject("GridAnchor", typeof(RectTransform));
            var anchorRT = (RectTransform)anchorGO.transform;
            anchorRT.SetParent(fillRT, false);
            // Anchor stretch shell
            anchorRT.anchorMin = new Vector2(0f, 0f);
            anchorRT.anchorMax = new Vector2(1f, 1f);
            anchorRT.pivot = new Vector2(0.5f, 0.5f);

            // move a bit within that square
            const float innerPadX = 8f;
            const float innerPadTop = 12f;   
            const float innerPadBottom = 8f;
            // left, bottom
            anchorRT.offsetMin = new Vector2(innerPadX, innerPadBottom);
            // right, top
            anchorRT.offsetMax = new Vector2(-innerPadX, -innerPadTop);

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
                preferredHeight: 500,
                outlineTint: outlineTint
            );

            // Scrollview adds itself to the anchor
            var scrollGO = new GameObject(tabName + "_ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollGO.layer = parentObject.layer;

            var scrollRT = (RectTransform)scrollGO.transform;
            FactoryUtils.ParentToRectTransform(scrollGO, anchorRT);

            var scrollLE = scrollGO.AddComponent<LayoutElement>();
            scrollLE.flexibleHeight = 1f;
            scrollLE.preferredHeight = 0f;

            // stretch within its bounds
            scrollRT.anchorMin = new Vector2(0f, 0f);
            scrollRT.anchorMax = new Vector2(1f, 1f);
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;

            var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGO.layer = scrollGO.layer;

            FactoryUtils.ParentToRectTransform(viewportGO, scrollRT);

            // stretch within scroll rect
            var viewportRT = (RectTransform)viewportGO.transform;
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;

            // needed for a mask
            var viewportImage = viewportGO.GetComponent<Image>();

            var cleanSprite = AssetCache.LoadSprite(AssetCacheKeys.texUICleanPanel);
            viewportImage.sprite = cleanSprite;
            viewportImage.type = Image.Type.Sliced;
            // need 1 alpha to mask correctly
            viewportImage.color = Color.white;
            viewportImage.raycastTarget = false;
            // make it opaque
            var mask = viewportGO.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            // controller wiring
            var itemsControls = controlsBar.GetComponent<DraftTabControls>();
            var tabController = tabGO.GetComponent<DraftTabController>();
            tabController.SetTabTier(tabTier);
            tabController.SetControls(itemsControls);

            var gridController = CreateGridWithContent(tabName, parentObject, viewportRT, tabTier, outlineTint);
            tabController.SetGridController(gridController);

            var scrollRect = scrollGO.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRT;
            scrollRect.content = (RectTransform)gridController.transform;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20f;

            return tabController;
        }

        private static void CenterGrid(RectTransform gridRT, GridLayoutGroup grid, int itemCount)
        {
            // how many columns/rows in the grid
            int cols = Mathf.Max(1, grid.constraintCount);
            int rows = Mathf.CeilToInt(itemCount / (float)cols);

            var cs = grid.cellSize;
            var sp = grid.spacing;
            var pad = grid.padding;

            // total width and height including padding
            float width = cols * cs.x + (cols - 1) * sp.x + pad.left + pad.right;
            float height = rows * cs.y + (rows - 1) * sp.y + pad.top + pad.bottom;

            // gridRT is anchored at (0.5, 1) with pivot (0.5, 1),
            // so giving it a width/height centers it horizontally and sticks it to the top.
            gridRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            gridRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }


        private static ItemPickerGridController CreateGridWithContent(string tabName, GameObject parentForLayer, 
            RectTransform parentForGrid,DraftItemTier tabTier, Color outlineTint)
        {
            // actual Grid lives under the anchor
            var gridGO = new GameObject(tabName + "_Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ItemPickerGridController));
            gridGO.layer = parentForLayer.layer;
            var gridRt = (RectTransform)gridGO.transform;
            // parent to the desired rect
            gridRt.SetParent(parentForGrid, false);

            // anchor to the right pviot
            gridRt.anchorMin = GRID_ANCHOR_PIVOT;
            gridRt.anchorMax = GRID_ANCHOR_PIVOT;
            gridRt.pivot = GRID_ANCHOR_PIVOT;
            gridRt.anchoredPosition = Vector2.zero;

            // Grid settings (centered; no padding since shell provides spacing)
            var grid = gridGO.GetComponent<GridLayoutGroup>();
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.cellSize = new Vector2(72, 72);
            grid.spacing = new Vector2(10, 10);
            grid.padding = new RectOffset(0, 0, 8, 8);
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

            CenterGrid(gridRt, grid, draftPickups.Count);

            var fitter = gridGO.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var gridController = gridGO.GetComponent<ItemPickerGridController>();
            // hand refs back to the grid controller
            gridController.SetSquares(squareControllers);

            return gridController;
        }
    }
}
