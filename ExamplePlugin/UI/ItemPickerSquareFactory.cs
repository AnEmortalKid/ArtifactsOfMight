using ArtifactsOfMight.Loadout.Draft;
using ArtifactsOfMight.UI.Tooltips;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ArtifactsOfMight.UI
{
    public static class ItemPickerSquareFactory
    {

        private static Color DIMMING_MID_OVERLAY = new Color(0f, 0f, 0f, 0.45f);

        // Try with multiplying towards gray
        // old = new Color(0f, 0f, 0f, 0.9f)
        private static Color DIMMING_HARD_OVERLAY = new Color(.1f, .1f, .1f, 0.8f);

        // Bleed offset on selection
        private static float SELECT_OUTLINE_PAD = 3f;


        /// <summary>
        /// Visual alignment constants for hover outlines.
        /// 
        /// Risk of Rain 2’s UI outlines aren’t expanded equally in all directions — they’re
        /// intentionally shifted slightly to keep the border visually centered after
        /// expanding the rect on only the right and bottom sides.
        /// 
        /// Here's how it works conceptually:
        /// 
        /// - The outline’s anchors are stretched to fill the parent:  
        ///   <c>anchorMin = (0,0)</c>, <c>anchorMax = (1,1)</c>.
        /// 
        /// - To make the outline appear "thicker" and extend past the tile edge,
        ///   we expand the rect by increasing its offsets on two sides:
        ///   <c>offsetMin = (0, -PAD)</c> → pushes the bottom down  
        ///   <c>offsetMax = (+PAD, 0)</c> → pushes the right side out
        /// 
        ///   This makes the outline’s total size larger by <c>PAD</c> pixels
        ///   on those two edges only, causing the frame to visually shift
        ///   down and to the right.
        /// 
        /// - To re-center the enlarged outline so it still appears even around the tile,
        ///   we nudge it back by half that distance:
        ///   <c>anchoredPosition = (+PAD/2, -PAD/2)</c>.
        /// 
        ///   This offset perfectly cancels the visual drift — the outline
        ///   expands outward on all sides *as if* it were centered growth.
        /// 
        /// TL;DR:
        ///   offsetMin = (0, -HOVER_OUTLINE_PAD_LARGE)
        ///   offsetMax = (+HOVER_OUTLINE_PAD_LARGE, 0)
        ///   anchoredPosition = (+HOVER_OUTLINE_ANCHOR_OFFSET, -HOVER_OUTLINE_ANCHOR_OFFSET)
        ///   where HOVER_OUTLINE_ANCHOR_OFFSET = HOVER_OUTLINE_PAD_LARGE / 2.
        ///   
        /// We found this by using the hierarchy dumper on a selected character on the selection grid and
        /// looking at its offsets
        /// </summary>
        private static float HOVER_OUTLINE_PAD_LARGE = 8f;
        private static float HOVER_OUTLINE_ANCHOR_OFFSET = 4f;

        public static Transform CreateGrid(Transform parent, string name = "DraftGrid", Vector2 cellSize = default, Vector2 spacing = default, RectOffset padding = null)
        {
            var gridGo = new GameObject(name, typeof(RectTransform), typeof(ContentSizeFitter), typeof(GridLayoutGroup));
            gridGo.layer = LayerMask.NameToLayer("UI");
            gridGo.transform.SetParent(parent, worldPositionStays: false);

            var rt = (RectTransform)gridGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(48, 96);
            rt.offsetMax = new Vector2(-48, -96);

            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.cellSize = cellSize;
            grid.spacing = new Vector2(8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;

            return gridGo.transform;
        }

        public static ItemPickerSquareController CreateItemSquare(Transform parent, Vector2 size, Sprite iconSprite)
        {
            var rootGo = new GameObject("ItemSquareButton", typeof(RectTransform), typeof(Image), typeof(Button));
            rootGo.layer = LayerMask.NameToLayer("UI");
            rootGo.transform.SetParent(parent, false);

            var rt = (RectTransform)rootGo.transform;
            rt.sizeDelta = size;

            var bg = rootGo.GetComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            var btn = rootGo.GetComponent<Button>();
            btn.targetGraphic = bg;

            // Icon
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.layer = rootGo.layer;
            iconGo.transform.SetParent(rootGo.transform, false);
            var iconRt = (RectTransform)iconGo.transform;
            iconRt.anchorMin = new Vector2(0.1f, 0.25f);
            iconRt.anchorMax = new Vector2(0.9f, 0.95f);
            var icon = iconGo.GetComponent<Image>();
            icon.sprite = iconSprite;
            icon.preserveAspect = true;

            // Rarity stripe (top)
            var stripeGo = new GameObject("RarityStripe", typeof(RectTransform), typeof(Image));
            stripeGo.layer = rootGo.layer;
            stripeGo.transform.SetParent(rootGo.transform, false);
            var stripeRt = (RectTransform)stripeGo.transform;
            stripeRt.anchorMin = new Vector2(0, 1);
            stripeRt.anchorMax = new Vector2(1, 1);
            stripeRt.pivot = new Vector2(0.5f, 1);
            stripeRt.sizeDelta = new Vector2(0, 6);
            var stripeImg = stripeGo.GetComponent<Image>();
            stripeImg.color = Color.white * 0.85f;

            // No label
            //var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            //labelGo.layer = rootGo.layer;
            //labelGo.transform.SetParent(rootGo.transform, false);
            //var labelRt = (RectTransform)labelGo.transform;
            //labelRt.anchorMin = new Vector2(0, 0);
            //labelRt.anchorMax = new Vector2(1, 0);
            //labelRt.sizeDelta = new Vector2(0, 22);
            //var label = labelGo.GetComponent<TextMeshProUGUI>();
            //label.text = labelText ?? "";
            //label.alignment = TextAlignmentOptions.Center;
            //label.fontSize = 16;

            // Picked overlay
            var pickedGo = new GameObject("PickedOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            pickedGo.layer = rootGo.layer;
            pickedGo.transform.SetParent(rootGo.transform, false);
            var pickedRt = (RectTransform)pickedGo.transform;
            pickedRt.anchorMin = Vector2.zero;
            pickedRt.anchorMax = Vector2.one;
            var pickedImg = pickedGo.GetComponent<Image>();
            pickedImg.color = new Color(0.2f, 0.9f, 0.3f, 0.25f);
            var pickedCg = pickedGo.GetComponent<CanvasGroup>();
            pickedCg.alpha = 0f;
            pickedCg.blocksRaycasts = false;

            // Controller
            var ctrl = rootGo.AddComponent<ItemPickerSquareController>();
            //ctrl.button = btn;
            //ctrl.background = bg;
            //ctrl.icon = icon;
            // TODO
            //ctrl.rarityStripe = stripeImg;
            //ctrl.label = label;
            //ctrl.pickedOverlay = pickedCg;

            return ctrl;
        }

        public static GameObject TestItemSquare(RectTransform parentRectTransform, PickupDef pickupDef, Color squareOutline, Color hoverColor = default)
        {
            var draftTier = DraftTierMaps.ToDraft(pickupDef.itemTier);

            var parentObject = parentRectTransform.gameObject;

            var sq = new GameObject("ItemSquare", typeof(RectTransform), typeof(Image), typeof(Button));
            sq.layer = parentObject.layer;
            var sqRt = (RectTransform)sq.transform;
            sqRt.SetParent(parentRectTransform, worldPositionStays: false);

            // HID outline
            if(hoverColor==default)
            {
                hoverColor = squareOutline;
            }        

            var backgroundImage = sq.GetComponent<Image>();
            // Load it as white and get the sprite
            var spriteForITer = GetSpriteForTier(draftTier);
            backgroundImage.color = Color.white;
            backgroundImage.sprite = spriteForITer;
            // outline sits inside the tile
            sqRt.offsetMin = new Vector2(2, 2);
            sqRt.offsetMax = new Vector2(-2, -2);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.layer = parentObject.layer;
            iconGo.transform.SetParent(sqRt.transform, false);

            var iconRt = (RectTransform)iconGo.transform;
            iconRt.anchorMin = Vector2.zero;     // stretch to parent
            iconRt.anchorMax = Vector2.one;
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.offsetMin = new Vector2(8, 8);   // padding (left,bottom)
            iconRt.offsetMax = new Vector2(-8, -8); // padding (right,top)

            var iconSprite = pickupDef.iconSprite;
            var icon = iconGo.GetComponent<Image>();
            icon.sprite = iconSprite;
            icon.preserveAspect = true;


            var dimmingOverlay = CreateDarkeningOverlay(sqRt);
            var hoverOverlay = CreateHoverOverlay(sqRt, squareOutline);

            var selectionOverlay = CreateSelectionOverlay(sqRt, squareOutline);
            var lockOverlay = CreateLockOverlay(sqRt);

            var button = sq.GetComponent<Button>();
            // setup the controller
            var controller = sq.AddComponent<ItemPickerSquareController>();
            controller.BindComponents(pickupDef, button, backgroundImage, icon, null, lockOverlay,
                selectionOverlay.GetComponent<Image>(),
                dimmingOverlay.GetComponent<Image>(),
                hoverOverlay.GetComponent<Image>());


            // tooltip stuff
            var tooltipTrigger = sq.AddComponent<TooltipTrigger>();
            // snap relative to me
            tooltipTrigger.Anchor = sqRt;


            var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
            var itemName = Language.GetString(itemDef.nameToken);
            var description = Language.GetString(itemDef.descriptionToken);
            tooltipTrigger.Data = new TooltipData
            {
                Title = itemName,
                Body = description,
                HeaderColor = GetTooltipHeaderColor(draftTier)
            };

            return sq;
        }

        private static GameObject CreateLockOverlay(RectTransform parentRect)
        {
            var parentObject = parentRect.gameObject;

            // LOCK OVERLAY (doesn't affect layout)
            var overlayGo = new GameObject("LockOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            overlayGo.layer = parentObject.layer;
            overlayGo.transform.SetParent(parentRect, false);

            // stretch to full tile
            var overlayRt = (RectTransform)overlayGo.transform;
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.pivot = new Vector2(0.5f, 0.5f);
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;

            // dimmer
            var overlayImg = overlayGo.GetComponent<Image>();
            overlayImg.color = DIMMING_MID_OVERLAY;
            overlayImg.raycastTarget = false;

            // corrupt sprite bottom right
            var corruptSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/VoidIcon_2.png").WaitForCompletion();
            var corruptBadge = AddBadge(overlayRt, corruptSprite, new Vector2(32, 32), BadgeAnchor.Center, Vector2.zero);
            var corruptImg = corruptBadge.gameObject.GetComponent<Image>();
            corruptImg.color = ColorPalette.OutlinePurple;


            // ensure overlay renders above other children
            overlayGo.transform.SetAsLastSibling();
            overlayGo.SetActive(false);

            return overlayGo;
        }

        // TODO hover overlay vs selection overlay probs
        private static GameObject CreateHoverTint(RectTransform parentRect, Color tintColor)
        {
            var go = new GameObject("HoverTint", typeof(RectTransform), typeof(Image));
            go.layer = parentRect.gameObject.layer;
            go.transform.SetParent(parentRect, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.color = tintColor; 
            img.raycastTarget = false;
            img.type = Image.Type.Sliced;
            img.enabled = false;

            return go;
        }

        private static GameObject CreateSelectionOverlay(RectTransform parentRect, Color outlineColor)
        {
            var parentObject = parentRect.gameObject;

            var overlayGo = new GameObject("SelectOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            overlayGo.layer = parentObject.layer;
            overlayGo.transform.SetParent(parentRect, false);

            // stretch to full tile
            var overlayRt = (RectTransform)overlayGo.transform;
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.pivot = new Vector2(0.5f, 0.5f);
            overlayRt.offsetMin = new Vector2(-SELECT_OUTLINE_PAD, -SELECT_OUTLINE_PAD);
            overlayRt.offsetMax = new Vector2(+SELECT_OUTLINE_PAD, +SELECT_OUTLINE_PAD);

            // selection highlight
            var overlayImg = overlayGo.GetComponent<Image>();
            overlayImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightBoxOutlineThickIcon.png").WaitForCompletion();
            overlayImg.type = Image.Type.Sliced;
            overlayImg.color = outlineColor;
            overlayImg.raycastTarget = false;

            // deactivate the image
            overlayImg.enabled = false;

            return overlayGo;
        }

        /// <summary>
        /// A square we can use to darken the icons without messing with the root background and icon
        /// </summary>
        /// <param name="parentRect"></param>
        /// <returns></returns>
        private static GameObject CreateDarkeningOverlay(RectTransform parentRect)
        {
            var parentObject = parentRect.gameObject;

            var overlayGo = new GameObject("UnselectedOverlay", typeof(RectTransform), typeof(Image));
            overlayGo.layer = parentObject.layer;
            overlayGo.transform.SetParent(parentRect, false);

            // stretch to full tile
            var overlayRt = (RectTransform)overlayGo.transform;
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.pivot = new Vector2(0.5f, 0.5f);
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;

            // dimmer
            var overlayImg = overlayGo.GetComponent<Image>();
            overlayImg.color = DIMMING_HARD_OVERLAY;
            overlayImg.raycastTarget = false;

            return overlayGo;
        }

        private static GameObject CreateHoverOverlay(RectTransform parentRect, Color outlineColor)
        {
            var parentObject = parentRect.gameObject;

            var overlayGo = new GameObject("HoverOverlay", typeof(RectTransform), typeof(Image));
            overlayGo.layer = parentObject.layer;
            overlayGo.transform.SetParent(parentRect, false);

            // stretch to full tile
            var overlayRt = (RectTransform)overlayGo.transform;
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.pivot = new Vector2(0.5f, 0.5f);
            
            overlayRt.anchoredPosition = new Vector2(HOVER_OUTLINE_ANCHOR_OFFSET, -HOVER_OUTLINE_ANCHOR_OFFSET);

            overlayRt.offsetMin = new Vector2(0, -HOVER_OUTLINE_PAD_LARGE);
            overlayRt.offsetMax = new Vector2(+HOVER_OUTLINE_PAD_LARGE, 0);

            // selection highlight
            var overlayImg = overlayGo.GetComponent<Image>();
            overlayImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightBoxOutlineThick.png").WaitForCompletion();
            overlayImg.type = Image.Type.Sliced;
            overlayImg.color = outlineColor;
            overlayImg.raycastTarget = false;

            // deactivate the image
            overlayImg.enabled = false;

            return overlayGo;
        }

        public static Sprite MakePlaceholderSprite(Color c)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new[] { c, c, c, c }); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
        }

        private enum BadgeAnchor
        {
            Center, TL, TR, BL, BR
        }

        private static RectTransform AddBadge(RectTransform parent, Sprite sprite, Vector2 size, BadgeAnchor where, Vector2 margin)
        {
            var go = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            go.layer = parent.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = size;

            // anchor + pivot
            Vector2 a, p, pos;
            switch (where)
            {
                case BadgeAnchor.Center:
                    a = p = new Vector2(0.5f, 0.5f);
                    pos = Vector2.zero;
                    break;
                case BadgeAnchor.TL:
                    a = p = new Vector2(0, 1);
                    pos = new Vector2(margin.x, -margin.y);
                    break;
                case BadgeAnchor.TR:
                    a = p = new Vector2(1, 1);
                    pos = new Vector2(-margin.x, -margin.y);
                    break;
                case BadgeAnchor.BL:
                    a = p = new Vector2(0, 0);
                    pos = new Vector2(margin.x, margin.y);
                    break;
                default: // BR
                    a = p = new Vector2(1, 0);
                    pos = new Vector2(-margin.x, margin.y);
                    break;
            }
            rt.anchorMin = rt.anchorMax = a;
            rt.pivot = p;
            rt.anchoredPosition = pos;

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false; // let button/tooltips handle input

            go.transform.SetAsLastSibling(); // render on top
            return rt;
        }

        private static Color GetTooltipHeaderColor(DraftItemTier draftItemTier)
        {
            switch (draftItemTier)
            {
                case DraftItemTier.White:
                    return ColorPalette.HeaderWhite;
                case DraftItemTier.Green:
                    return ColorPalette.HeaderGreen;
                case DraftItemTier.Red:
                    return ColorPalette.HeaderRed;
                case DraftItemTier.Yellow:
                    return ColorPalette.HeaderYellow;
                case DraftItemTier.Purple:
                    return ColorPalette.HeaderPurple;
            }

            // Default and obvious
            return Color.cyan;
        }

        private static Sprite GetSpriteForTier(DraftItemTier draftItemTier)
        {

            //    "RoR2/Base/Common/texTier1BGIcon.png",
            //"RoR2/Base/Common/texTier2BGIcon.png",
            //"RoR2/Base/Common/texTier3BGIcon.png",
            //"RoR2/Base/Common/texBossBGIcon.png",
            //"RoR2/Base/Common/texEquipmentBGIcon.png",
            //"RoR2/Base/Common/texLunarBGIcon.png",
            //"RoR2/DLC1/Common/IconBackgroundTextures/texVoidBGIcon.png",
            switch (draftItemTier)
            {
                case DraftItemTier.White:
                    return Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier1BGIcon.png").WaitForCompletion();
                case DraftItemTier.Green:
                    return Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier2BGIcon.png").WaitForCompletion();
                case DraftItemTier.Red:
                    return Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texTier3BGIcon.png").WaitForCompletion();
                case DraftItemTier.Yellow:
                    return Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBossBGIcon.png").WaitForCompletion();
                case
                    DraftItemTier.Purple:
                    return Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/Common/IconBackgroundTextures/texVoidBGIcon.png").WaitForCompletion();
                default:
                    return Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/Common/IconBackgroundTextures/texLunarBGIcon.png").WaitForCompletion();
            }
        }
    }
}
