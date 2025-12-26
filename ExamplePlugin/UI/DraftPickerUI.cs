using UnityEngine;
using UnityEngine.SceneManagement;
using RoR2;
using System.Text;
using UnityEngine.UI;
using System.Collections.Generic;
using ArtifactsOfMight.Loadout.Draft;
using ArtifactsOfMight.UI.Drafting;
using System.Diagnostics.CodeAnalysis;
using ArtifactsOfMight.UI.Tooltips;
using ArtifactsOfMight.UI.Utils;
using ArtifactsOfMight.UI.Branding.Inspection;
using ArtifactsOfMight.UI.Branding.Panel;

namespace ArtifactsOfMight.UI
{
    /// <summary>
    /// Controls the UI components for the draft picker
    /// 
    /// TODO: this just is a hook for initializing the draft manager and displaying it
    /// draft manager is self contained
    /// </summary>
    public class DraftPickerUI : MonoBehaviour
    {

        /// <summary>
        /// SafeArea named rect transform that we will parent everything under
        /// This is attached by the plugin since it knows when to load
        /// </summary>
        public RectTransform SafeArea;

        /// <summary>
        /// The name for the game object we are binding our DraftManager to
        /// </summary>
        private const string DraftManagerRootName = "DraftManagerRoot";

        /// <summary>
        /// Track whether we have spun up the UI once or not on the first request of UI
        /// </summary>
        private bool gridsInitialized;

        /// <summary>
        /// GameObject where the whole UI is attached to
        /// </summary>
        private GameObject rootGameObject;

        /// <summary>
        ///  MEGA WIP
        /// </summary>
        private static Dictionary<DraftItemTier, GameObject> tabsByTier = new();

        private GameObject testDraftMObject;

        private GameObject testAssetGrid;
        private GameObject testSecondGrid;

        RectTransform _glassContent;
        RectTransform _glassHolder;

        void Awake()
        {
            Log.Info("[DraftArtifact] PickerUI Awake");
            //DontDestroyOnLoad(gameObject);
            //SceneManager.activeSceneChanged += OnActiveSceneChanged;
            //// Initialize for current scene on boot/reload
            //OnActiveSceneChanged(default, SceneManager.GetActiveScene());

            //if (pickerCanvas)
            //    pickerCanvas.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            //SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        void OnActiveSceneChanged(Scene _, Scene newScene)
        {
            //// RoR2 lobby scene is literally named "lobby"
            //inLobby = newScene.name == "lobby";

            //// Extra safety: require the PreGameController to exist (this is the screen with the Ready button)
            //if (inLobby)
            //    inLobby = PreGameController.instance != null;

            //// Auto-hide when leaving lobby
            //if (!inLobby && pickerCanvas)
            //    pickerCanvas.gameObject.SetActive(false);
        }

        void OnEnable() => Log.Info("[DraftArtifact] PickerUI OnEnable");
        void Start()
        {
            Log.Info("[DraftArtifact] PickerUI Start");

            // init our stuff once
            if (SafeArea == null)
            {
                Log.Warning("[DraftArtifact] Safe area not exist");
                return;
            }
        }

        private static void WipTabToggles(DraftItemTier desiredTier)
        {

            foreach (var key in tabsByTier.Keys)
            {
                tabsByTier[key].SetActive(false);
            }

            tabsByTier[desiredTier].SetActive(true);
        }

        public void ToggleVisibility()
        {
            var currentlyActive = rootGameObject.activeSelf;
            rootGameObject.SetActive(!currentlyActive);
        }

        /// <summary>
        /// Builds the root game object that all our panels and things will attach to
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private GameObject BuildDraftPickerRootStructure(string name)
        {
            var root = new GameObject(name, typeof(RectTransform));
            var rootRt = (RectTransform)root.transform;
            rootRt.SetParent(SafeArea, worldPositionStays: false);

            // Stretch to SafeArea
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            // Make sure we're on the UI layer so the UI camera draws it
            SetLayerRecursively(root, SafeArea.gameObject.layer);

            // ==== Full-bleed RED background ====
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)bg.transform;
            bgRt.SetParent(rootRt, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            var img = bg.GetComponent<Image>();
            img.color = new Color(1f, 0, 0, .25f);           // solid red to verify full stretch
            img.raycastTarget = true;        // optional click blocker


            // DEBUG: print rect sizes to verify
            LogRect("SafeArea", SafeArea);
            LogRect("Root", rootRt);
            LogRect("BG", bgRt);

            return root;
        }

        private GameObject BuildDraftPickerRootStructureNoContents(string name)
        {
            var root = new GameObject(name, typeof(RectTransform));
            var rootRt = (RectTransform)root.transform;
            rootRt.SetParent(SafeArea, worldPositionStays: false);

            // Stretch to SafeArea
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            // Make sure we're on the UI layer so the UI camera draws it
            SetLayerRecursively(root, SafeArea.gameObject.layer);

            // ==== Full-bleed RED background ====
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)bg.transform;
            bgRt.SetParent(rootRt, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            var img = bg.GetComponent<Image>();
            img.color = new Color(0f, 0, 1f, .25f);           // solid red to verify full stretch
            img.raycastTarget = true;        // optional click blocker

            // DEBUG: print rect sizes to verify
            LogRect("SafeArea", SafeArea);
            LogRect("Root", rootRt);
            LogRect("BG", bgRt);

            return root;
        }


        private GameObject BuildDraftPickerRootStructureFull()
        {
            var draftManagerRoot = DraftManagerFactory.Create(SafeArea);
            return draftManagerRoot;
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; i++)
                SetLayerRecursively(go.transform.GetChild(i).gameObject, layer);
        }

        private GameObject BuildDraftManagerCorrectly()
        {
            var testHierarchyObject = BuildDraftPickerRootStructureFull();

            return testHierarchyObject;
        }

        private GameObject BuildToolTipStructure()
        {
            var tooltipRoot = new GameObject("TooltipTest", typeof(RectTransform), typeof(TooltipSystem));
            FactoryUtils.ParentToRectTransform(tooltipRoot, SafeArea);

            var tooltipRootRT = (RectTransform)tooltipRoot.transform;
            FactoryUtils.StretchToFillParent(tooltipRootRT);

            // ==== Debug Teal Background ====
            var debugBG = new GameObject("ToolTipTest" + "_DebugBG", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)debugBG.transform;
            bgRt.SetParent(tooltipRootRT, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            // Debug only
            var img = debugBG.GetComponent<Image>();
            // set it to transparent, turn it back to .25f for debug
            img.color = new Color(0.5f, 0f, 0.5f, 0f);
            img.raycastTarget = false;

            // DEBUG: print rect sizes to verify
            LogRect("SafeArea", SafeArea);
            LogRect("Root", tooltipRootRT);
            LogRect("BG", bgRt);

            var system = tooltipRoot.GetComponent<TooltipSystem>();
            var tooltipView = TooltipFactory.CreateTooltipView(tooltipRootRT);
            system.Initialize(tooltipRootRT, tooltipView);
            tooltipView.transform.SetAsLastSibling();

            return tooltipRoot;
        }

        GameObject SpawnOverlayProbe(RectTransform parent)
        {
            var go = new GameObject("TooltipProbe", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(220, 80);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.color = Color.blue;
            img.raycastTarget = false;
            return go;
        }

        private GameObject TestDraftManagerFlow()
        {
            var draftManagerRoot = new GameObject(DraftManagerRootName, typeof(RectTransform));
            FactoryUtils.ParentToRectTransform(draftManagerRoot, SafeArea);
            var draftManagerRootRt = draftManagerRoot.GetComponent<RectTransform>();
            StretchFull(draftManagerRootRt);

            // Is it this?
            draftManagerRootRt.SetParent(SafeArea, worldPositionStays: false);

            // Make sure we're on the UI layer so the UI camera draws it
            SetLayerRecursively(draftManagerRoot, SafeArea.gameObject.layer);

            // ==== Full-bleed blue background ====
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)bg.transform;
            bgRt.SetParent(draftManagerRootRt, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            var img = bg.GetComponent<Image>();
            img.color = new Color(0f, 0f, 1f, .25f);           // solid red to verify full stretch
            img.raycastTarget = true;        // optional click blocker

            //Log.Info("draftManagerRoot.active: " + draftManagerRoot.activeInHierarchy);
            //Log.Info("draftManagerRootRt: " + draftManagerRoot);

            //LogRect("Parent", SafeArea);
            //LogTree(draftManagerRootRt);



            //var draftManager = draftManagerRoot.AddComponent<DraftManager>();
            //draftManager.Initialize(draftManagerRootRt);

            //// 
            //Log.Info("Tree Post Init");
            //LogTree(draftManagerRootRt);

            return draftManagerRoot;
        }

        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        void Update()
        {
            //if (!inLobby || pickerCanvas == null)
            //    return;

            //// Don’t toggle if user is typing in chat or any input field has focus
            //var mpES = MPEventSystemManager.primaryEventSystem;
            //if (mpES != null && (mpES.isActiveAndEnabled))
            //    return;

            if (Input.GetKeyDown(KeyCode.F6))
            {
                if (!testAssetGrid)
                {
                    testAssetGrid = AssetDebugGridFactory.BuildTestGrid(SafeArea);
                }
                else
                {
                    testAssetGrid.SetActive(!testAssetGrid.activeSelf);
                }
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (!testSecondGrid)
                {
                    testSecondGrid = AssetDebugGridFactory.BuildTestGrid(SafeArea);
                }
                else
                {
                    testSecondGrid.SetActive(!testSecondGrid.activeSelf);
                }
            }


            if (Input.GetKeyDown(KeyCode.F7))
            {
                var sceneName = SceneManager.GetActiveScene().name;
                if (sceneName != "lobby")
                {
                    Log.Warning("DraftPickerToggleDuringLobby in scene " + sceneName);
                    return;
                }

                Log.Info("DraftPickerToggle Respond F7");
                if (testDraftMObject == null)
                {
                    Log.Info("DraftPickerToggle TestDraftManagerFlow");
                    testDraftMObject = BuildDraftManagerCorrectly();
                    return;
                }

                if (!testDraftMObject)
                {
                    Log.Warning("DraftPickerToggleDuringLobby no TestDraftManager after init");
                    return;
                }

                if (testDraftMObject.activeSelf)
                {
                    testDraftMObject.SetActive(false);
                }
                else
                {
                    testDraftMObject.SetActive(true);
                }
            }


            if (Input.GetKeyDown(KeyCode.F5))
            {
                var hierarchyResult = HierarchyDumper.DumpActiveHierarchy("SceneHierarchy", 20);
                Log.Debug(hierarchyResult);
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                //EnsureGlassBuilt();

                //var currVis = _glassHolder.gameObject.activeSelf;
                //Log.Debug("F8: " + currVis + " => " + !currVis);
                //_glassHolder.gameObject.SetActive(!currVis);
                // HierarchyDumper.TryDumpAllTooltips();
                ItemCatalogDumper.ParseAndLog();
            }

        }


        static void LogRect(string name, RectTransform rt)
        {
            var r = rt.rect;
            Debug.Log($"[{name}] {r.width}x{r.height}  anchors {rt.anchorMin}->{rt.anchorMax}  pivot {rt.pivot}");
        }

        static string PathOf(Transform t)
        {
            if (!t) return "<null>";
            var sb = new System.Text.StringBuilder(t.name);
            var p = t.parent;
            while (p)
            {
                sb.Insert(0, p.name + "/");
                p = p.parent;           // advance AFTER using p
            }
            return sb.ToString();
        }

        static void LogTree(RectTransform rt)
        {
            var go = rt.gameObject;
            Log.Info("LogTree " + rt.gameObject.name);

            if (go.TryGetComponent<CanvasGroup>(out var cg))
            {
                Log.Info(
                   $"[UI] path={PathOf(rt)} active={go.activeInHierarchy} layer={go.layer} " +
                   $"rect={rt.rect.size.x}x{rt.rect.size.y} anchors ({rt.anchorMin.x:F2},{rt.anchorMin.y:F2})->({rt.anchorMax.x:F2},{rt.anchorMax.y:F2}) " +
                   $"cg={(cg ? $"a={cg.alpha},int={cg.interactable},blk={cg.blocksRaycasts}" : "none")}"
                );
            }
            else
            {
                Log.Info(
                    $"[UI] path={PathOf(rt)} active={go.activeInHierarchy} layer={go.layer} " +
                    $"rect={rt.rect.size.x}x{rt.rect.size.y} anchors ({rt.anchorMin.x:F2},{rt.anchorMin.y:F2})->({rt.anchorMax.x:F2},{rt.anchorMax.y:F2})"
                );
            }

            // check parents for Canvas/Mask
            for (var p = rt.parent; p; p = p.parent)
            {
                var cnv = p.GetComponent<Canvas>();
                var m1 = p.GetComponent<UnityEngine.UI.Mask>();
                var m2 = p.GetComponent<UnityEngine.UI.RectMask2D>();
                if (cnv || m1 || m2)
                    UnityEngine.Debug.Log($"[UI] parent {p.name}: Canvas={cnv != null}, Mask={m1 != null}, RectMask2D={m2 != null}");
            }
        }


        static void DumpActiveHierarchy(string title = "Lobby Dump", int maxDepth = 5)
        {

            var scene = SceneManager.GetActiveScene();
            Log.Info($"Dumping: {scene.name}");

            var sb = new StringBuilder();
            sb.AppendLine($"==== {title}: Scene '{scene.name}' ====");

            foreach (var root in scene.GetRootGameObjects())
            {
                Recurse(root.transform, 0, maxDepth, sb);
            }

            Log.Info(sb.ToString());

            static void Recurse(Transform t, int depth, int maxDepth, StringBuilder sb)
            {
                if (!t || !t.gameObject.activeInHierarchy)
                {
                    return;
                }

                if (depth > maxDepth) { return; }

                Log.Info($"Recurse {t}");

                var rt = t as RectTransform;
                var tag = rt ? " [RectTransform]" : "";
                sb.AppendLine($"{new string(' ', depth * 2)}• {t.name}{tag}");
                sb.AppendLine($" Layer: ${LayerMask.LayerToName(rt.gameObject.layer)}");

                for (int i = 0; i < t.childCount; i++)
                    Recurse(t.GetChild(i), depth + 1, maxDepth, sb);
            }
        }


       private static RectTransform BuildTabsBar()
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


            var whiteButton = DraftTabsBarFactory.CreateTextButton("white", new Vector2(64, 64),
                "white");
            FactoryUtils.ParentToRectTransform(whiteButton, tabsBarRt);
            draftTabsBar.BindButton(DraftItemTier.White, whiteButton.GetComponent<Button>());


            var greenButton = DraftTabsBarFactory.CreateTextButton("green", new Vector2(64, 64),
                "green");
            FactoryUtils.ParentToRectTransform(greenButton, tabsBarRt);
            draftTabsBar.BindButton(DraftItemTier.Green, greenButton.GetComponent<Button>());

            draftTabsBar.OnTabButtonClicked += WipTabToggles;

            return tabsBarRt;
        }


        private static RectTransform BuildBottomBar()
        {
            // build the tabs bar
            var tabsBar = new GameObject("BottomBar", typeof(RectTransform));
            var tabsBarRt = (RectTransform)tabsBar.transform;


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

            return tabsBarRt;
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

        // Build once, keep hidden
        void EnsureGlassBuilt()
        {
            if (_glassHolder)
            {
                Log.Debug("Glass built");
                return;
            }

            Log.Debug("Build Test Glass");
            var safe = SafeArea;


            // 1) Full-screen holder under SafeArea (blocks clicks if you want)
            var root = new GameObject("TestGridPanelRoot", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            root.SetParent(SafeArea, false);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.pivot = new Vector2(0.5f, 0.5f);
            root.offsetMin = root.offsetMax = Vector2.zero;

            // set to transparent but give option to add color for debug viz
            var rootImg = root.GetComponent<Image>();
            rootImg.color = new Color(0, 0, 0, 0);
            rootImg.raycastTarget = true;

            var desiredPanelSize = new Vector2(720, 780);

            // 2) Anchor node you can position/size
            var anchorRect = new GameObject("PanelAnchor", typeof(RectTransform)).GetComponent<RectTransform>();
            anchorRect.SetParent(root, false);
            anchorRect.anchorMin = anchorRect.anchorMax = new Vector2(0.5f, 0.5f); // start centered
            anchorRect.pivot = new Vector2(0.5f, 0.5f);
            anchorRect.sizeDelta = desiredPanelSize;

            anchorRect.anchoredPosition = Vector2.zero;

            var glassContainer = GlassPanelFactory.BuildGlassBody("anchorTest", anchorRect,
                desiredPanelSize, shouldBlockClicks: false,
                borderOptions: new BorderOptions
                {
                    Style = BorderStyle.None,
                    Color = new Color(0.363f, 0.376f, 0.472f, 1f),
                    RespectSpritePadding = true
                });
            // no reflow since we want to not be inset for the tooltip
            _glassHolder = glassContainer.glassPanelHolder.GetComponent<RectTransform>();

            // try placing in the same spot as in inkscape
            // inkscape is top-left coordinate so this should align nicely
            anchorRect.anchorMin = new Vector2(0f, 1f);
            anchorRect.anchorMax = new Vector2(0f, 1f);
            anchorRect.pivot = new Vector2(0f, 1f);
            // TODO need to do some math
            anchorRect.anchoredPosition = new Vector2(660, -60);

            // PlaceCenter(anchorRect);

            //// Holder (anchors center — adjust if needed)
            //_glassHolder = new GameObject("GlassOverlayHolder",
            //    typeof(RectTransform), typeof(CanvasGroup)).GetComponent<RectTransform>();
            //_glassHolder.SetParent(safe, false);
            //_glassHolder.anchorMin = _glassHolder.anchorMax = new Vector2(0.5f, 0.5f);
            //_glassHolder.pivot = new Vector2(0.5f, 0.5f);
            //_glassHolder.sizeDelta = new Vector2(520, 220);
            //_glassHolder.anchoredPosition = Vector2.zero;

            //var _glassCG = _glassHolder.GetComponent<CanvasGroup>();
            //_glassCG.alpha = 1f;


            //var glassHolderRT = _glassHolder.GetComponent<RectTransform>();
            //var glassPanel = GlassPanelFactory.BuildGlassBody(glassHolderRT, new Vector2(520, 220), false);
        }

        public static void PlaceCenter(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        public static Vector2 InkscapeToAnchoredTopLeft(Vector2 inkscapePos, Vector2 parentSize, Vector2 inkscapeCanvas = default)
        {
            // default to 1920x1080 if not provided
            if (inkscapeCanvas == default)
            {
                inkscapeCanvas = new Vector2(1920, 1080);
            }

            var sx = parentSize.x / inkscapeCanvas.x;
            var sy = parentSize.y / inkscapeCanvas.y;
            return new Vector2(inkscapePos.x * sx, -inkscapePos.y * sy);
        }

        /// <summary>
        /// Determines the center based anchor position based on a set of coordinates
        /// found when positioning things in Inkscape (a 1920x1080) document, scaled
        /// correctly to the size of the SafeArea(parentSize), so things are in the right spot
        /// </summary>
        /// <param name="inkscapeCenter">the center position on our inkscape mockup</param>
        /// <param name="parentSize">the size of the parent, usually SafeArea</param>
        /// <param name="inkscapeCanvas">the size of our inkscape reference canvas</param>
        /// <returns></returns>
        public static Vector2 InkscapeCenterToAnchoredCenter(
            Vector2 inkscapeCenter, Vector2 parentSize, Vector2 inkscapeCanvas = default)
        {
            if (inkscapeCanvas == default)
            {
                inkscapeCanvas = new Vector2(1920, 1080);
            }

            var scaledX = parentSize.x / inkscapeCanvas.x;
            var scaledY = parentSize.y / inkscapeCanvas.y;
            var centeredX = inkscapeCenter.x * scaledX;
            var centeredY = inkscapeCenter.y * scaledY;
            return new Vector2(centeredX - parentSize.x * 0.5f, -centeredY + parentSize.y * 0.5f);
        }

    }
}
