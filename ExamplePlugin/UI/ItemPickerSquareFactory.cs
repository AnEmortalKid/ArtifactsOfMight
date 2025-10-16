using ExamplePlugin.Loadout.Draft;
using ExamplePlugin.UI.Tooltips;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ExamplePlugin.UI
{
    public static class ItemPickerSquareFactory
    {

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

        public static GameObject TestItemSquare(RectTransform parentRectTransform, PickupDef pickupDef, Color squareOutline)
        {
            var parentObject = parentRectTransform.gameObject;

            var sq = new GameObject("ItemSquare", typeof(RectTransform), typeof(Image), typeof(Button));
            sq.layer = parentObject.layer;
            var sqRt = (RectTransform)sq.transform;
            sqRt.SetParent(parentRectTransform, worldPositionStays: false);

            var uiOutline = sq.AddComponent<Outline>();
            uiOutline.effectColor = squareOutline;            // color sent to us
            uiOutline.effectDistance = new Vector2(2, 2);  // “thickness”
            uiOutline.enabled = true;                      // toggle on/off

            var backgroundImage = sq.GetComponent<Image>();
            backgroundImage.color = new Color(0.25f, 0.25f, 0.25f, 1f); // dark gray background
            // optionally inset image so outline sits inside the tile
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


            var lockOverlay = CreateLockOverlay(sqRt);

            var button = sq.GetComponent<Button>();
            // setup the controller
            var controller = sq.AddComponent<ItemPickerSquareController>();
            controller.BindComponents(pickupDef, button, backgroundImage, icon, uiOutline, lockOverlay);


            // tooltip stuff
            var tooltipTrigger = sq.AddComponent<TooltipTrigger>();
            // snap relative to me
            tooltipTrigger.Anchor = sqRt;

            var draftTier = DraftTierMaps.ToDraft(pickupDef.itemTier);
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
            overlayImg.color = new Color(0f, 0f, 0f, 0.45f); // semi-transparent black
            overlayImg.raycastTarget = false;                // let clicks pass if you want (or keep true to eat clicks)

            // lock icon on top
            // Alternative lock icon and have the item that's locking it
            // var lockIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();
            // AddBadge(overlayRt, lockIconSprite, new Vector2(32, 32), BadgeAnchor.Center, Vector2.zero);

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
    }
}
