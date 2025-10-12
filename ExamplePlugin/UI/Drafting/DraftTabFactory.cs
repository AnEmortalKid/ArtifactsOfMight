using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Loadout.Draft;
using UnityEngine.UI;
using UnityEngine;
using RoR2;

namespace ExamplePlugin.UI.Drafting
{
    public static class DraftTabFactory
    {

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
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            // here we will add toggles
            // TODO WIPS
            //var controlsBar = BuildTabControlsBar(tabRoot);
            var controlsBar = ItemPickerTabControlsFactory.CreateTabControls(tabName+"_Controls", tabRootRT);
            var controlsBarGO = controlsBar.gameObject;

            var tabsLE = controlsBarGO.AddComponent<LayoutElement>();
            tabsLE.preferredHeight = 64;
            tabsLE.flexibleHeight = 0;

            var gridGO = new GameObject(tabName+"_Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ItemPickerGridController));
            gridGO.layer = parentObject.layer;
            var gridRt = (RectTransform)gridGO.transform;
            gridRt.SetParent(tabRootRT, worldPositionStays: false);

            var itemsControls = controlsBar.GetComponent<DraftTabControls>();

            var tabController = tabGO.GetComponent<DraftTabController>();
            tabController.SetTabTier(tabTier);
            tabController.SetControls(itemsControls);

            var gridController = gridGO.GetComponent<ItemPickerGridController>();
            tabController.SetGridController(gridController);

            // stretch, with some margins so you can see edges
            gridRt.anchorMin = Vector2.zero;
            gridRt.anchorMax = Vector2.one;
            gridRt.pivot = new Vector2(0.5f, 0.5f);
            gridRt.offsetMin = new Vector2(16, 16);
            gridRt.offsetMax = new Vector2(-16, -16);

            var gridRtLE = gridGO.AddComponent<LayoutElement>();
            gridRtLE.flexibleHeight = 1;
            gridRtLE.preferredHeight = 440;

            var gridBG = gridGO.AddComponent<Image>();
            // blue gray ish
            gridBG.color = ColorPalette.FromRGB(49, 60, 77);
            gridBG.raycastTarget = true;

            //rootRt.anchorMin = Vector2.zero;
            //rootRt.anchorMax = Vector2.one;
            //rootRt.pivot = new Vector2(0.5f, 0.5f);
            //rootRt.offsetMin = Vector2.zero;
            //rootRt.offsetMax = Vector2.zero;

            var grid = gridGO.GetComponent<GridLayoutGroup>();
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.cellSize = new Vector2(72, 72);
            grid.spacing = new Vector2(8, 8);
            // TODO fix the alignment too

            // The command card uses 5 columns
            // but we have more real estate here so we will go with 8
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;

            var squareControllers = new List<ItemPickerSquareController>();
            var draftPickups = DraftPools.Instance.GetDraftablePickups(tabTier);
            var squareOutline = GetOutlineColor(tabTier);
            foreach (var pickIndex in draftPickups)
            {
                var pickDef = PickupCatalog.GetPickupDef(pickIndex);
                var singleSquare = ItemPickerSquareFactory.TestItemSquare(gridRt, pickDef, squareOutline);
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

        private static Color GetOutlineColor(DraftItemTier draftItemTier)
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
    }
}
