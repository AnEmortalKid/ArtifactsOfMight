using UnityEngine;
using UnityEngine.SceneManagement;
using RoR2;
using System.Text;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using ExamplePlugin.Loadout.Draft;
using ExamplePlugin.UI.Drafting;
using System.Diagnostics.CodeAnalysis;
using ExamplePlugin.UI.Tooltips;
using ExamplePlugin.UI.Utils;
using ExamplePlugin.UI.Branding.Inspection;

namespace ExamplePlugin.UI
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

        private GameObject wipSpawnOverlay;

        private GameObject testAssetGrid;

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

            // Add the F3 toggle listener here (lives only during lobby)
            //var toggle = this.gameObject.AddComponent<DraftPickerToggleDuringLobby>();

            //if (!SafeArea.Find("DraftPickerCanvas"))
            //{
            //    var picker = BuildPickerRoot("DraftPickerCanvas");
            //    var rt = picker.GetComponent<RectTransform>();
            //    rt.SetParent(SafeArea, worldPositionStays: false);
            //    StretchFull(rt);

            //    // Start hidden
            //    picker.SetActive(false);
            //    //toggle.RootObject = picker;
            //    PanelRoot = rt;
            //    PanelsGO = picker;

            //    //   Log.Info("[DraftArtifact] toggle RootObject=" + toggle.RootObject);

            //    Log.Info("[DraftArtifact] DraftPickerCanvas attached under SafeArea.");

            //    Log.Info("Picker layer=" + LayerMask.LayerToName(picker.layer));
            //}
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


            var tabsGroup = BuildTabsGroup(rootRt);

            // DEBUG: print rect sizes to verify
            LogRect("SafeArea", SafeArea);
            LogRect("Root", rootRt);
            LogRect("BG", bgRt);
            LogRect("TabsGroup", tabsGroup);

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


        private GameObject BuildDraftPickerRootStructureNoWorky()
        {
            var draftManagerRoot = new GameObject(DraftManagerRootName, typeof(RectTransform));
            FactoryUtils.ParentToRectTransform(draftManagerRoot, SafeArea);

            var draftManagerRootRT = (RectTransform)draftManagerRoot.transform;
            FactoryUtils.StretchToFillParent(draftManagerRootRT);

            // ==== Debug Teal Background ====
            var debugBG = new GameObject(DraftManagerRootName + "_DebugBG", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)debugBG.transform;
            bgRt.SetParent(draftManagerRootRT, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            var img = debugBG.GetComponent<Image>();
            img.color = new Color(0f, 0.5f, 0.5f, .25f);           // some sorta blueish
            img.raycastTarget = true;        // optional click blocker


            var draftManager = draftManagerRoot.AddComponent<DraftManager>();
            draftManager.Initialize(draftManagerRootRT);

            // DEBUG: print rect sizes to verify
            LogRect("SafeArea", SafeArea);
            LogRect("Root", draftManagerRootRT);
            LogRect("BG", bgRt);

            return draftManagerRoot;
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; i++)
                SetLayerRecursively(go.transform.GetChild(i).gameObject, layer);
        }

        private void TestPopulateGrid()
        {
            // Attach draft manager to our root and then let it do its thing

            // add fraft
            // draft manager initialize()

            //var draftManagerRoot = new GameObject(DraftManagerRootName, typeof(RectTransform));
            //FactoryUtils.ParentToRectTransform(draftManagerRoot, SafeArea);

            //// Make sure we're on the UI layer so the UI camera draws it
            //SetLayerRecursively(draftManagerRoot, SafeArea.gameObject.layer);

            //var draftManager = draftManagerRoot.AddComponent<DraftManager>();
            //draftManager.Initialize(draftManagerRoot.GetComponent<RectTransform>());
            // todo pass whatever
            // draftManager.Initialize();

            // TODO this will be the draft manager probs
            rootGameObject = BuildDraftPickerRootStructure("DraftPickerCanvas");
            var rootRectTransform = rootGameObject.GetComponent<RectTransform>();
        }

        private GameObject TestDraftManagerParenting()
        {
            //var testMO = BuildDraftPickerRootStructureNoContents("TestNoContents");
            var testMO = BuildDraftPickerRootStructureNoWorky();
            var tooltipGO = BuildToolTipStructure();

            return testMO;
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


            if (Input.GetKeyDown(KeyCode.F3))
            {
                var sceneName = SceneManager.GetActiveScene().name;
                if (sceneName != "lobby")
                {
                    Log.Warning("DraftPickerToggleDuringLobby in scene " + sceneName);
                    return;
                }

                Log.Info("DraftPickerToggle Respond F3");
                if (!gridsInitialized)
                {
                    Log.Info("Initializing Grids");
                    gridsInitialized = true;
                    // starts active
                    TestPopulateGrid();
                    return;
                }

                if (!rootGameObject)
                {
                    Log.Warning("DraftPickerToggleDuringLobby no RootObject after init");
                    return;
                }

                ToggleVisibility();
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                if(!testAssetGrid)
                {
                    testAssetGrid = AssetDebugGridFactory.BuildTestGrid(SafeArea);
                }
                else
                {
                    testAssetGrid.SetActive(!testAssetGrid.activeSelf);
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
                    testDraftMObject = TestDraftManagerParenting();
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
                var hierarchyResult = HierarchyDumper.DumpActiveHierarchy("SceneHierarchy");
                Log.Debug(hierarchyResult);
            }

            // We know this works
            //if(Input.GetKeyDown(KeyCode.F8))
            //{
            //    var canvas = SafeArea.GetComponentInParent<Canvas>();
            //    var uiCam = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            //        ? canvas.worldCamera
            //        : null;

            //    this.wipSpawnOverlay.SetActive(false);

            //    var overlayRect = wipSpawnOverlay.GetComponent<RectTransform>();

            //    bool nowActive = !wipSpawnOverlay.activeSelf;
            //    wipSpawnOverlay.SetActive(nowActive);

            //    if (nowActive)
            //    {
            //        Vector2 local;
            //        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //            SafeArea, Input.mousePosition, uiCam, out local);

            //        // optional: set anchors to top-left so anchoredPosition behaves consistently
            //        overlayRect.anchorMin = overlayRect.anchorMax = new Vector2(0, 1);
            //        overlayRect.pivot = new Vector2(0, 1);

            //        // local is centered; convert to top-left space (safeArea pivot = 0.5,0.5)
            //        Vector2 half = SafeArea.rect.size * 0.5f;
            //        Vector2 anchored = local + new Vector2(half.x, -half.y);

            //        overlayRect.anchoredPosition = anchored;

            //        Debug.Log($"[SpawnOverlay] Activated at mouse {Input.mousePosition} => anchored {anchored}");
            //    }
            //    else
            //    {
            //        Debug.Log("[SpawnOverlay] Deactivated");
            //    }
            //}
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


        private static RectTransform BuildTabsGroup(RectTransform parentRectTransform)
        {
            // TODO add the draft manager here


            var group = new GameObject("TabGroup", typeof(RectTransform), typeof(VerticalLayoutGroup));
            var groupRt = (RectTransform)group.transform;
            groupRt.SetParent(parentRectTransform, false);
            // Possibly could do like 680 x 780
            // The artifacts one is like tilted a bit and is 660 by like 690
            // character select is like 657 by 850
            // This might be a good group for the like Overall Floating Panel Tab Group Holder thing
            groupRt.sizeDelta = new Vector2(720, 720);

            // probs do want scroll rects in the inner grids

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

        public static RectTransform BuildContentArea()
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

            var whiteTab = DraftTabFactory.BuildDraftTab(contentAreaRt, "Whites", DraftItemTier.White);
            tabsByTier[DraftItemTier.White] = whiteTab.gameObject;

            var greenTab = DraftTabFactory.BuildDraftTab(contentAreaRt, "Greens", DraftItemTier.Green);
            tabsByTier[DraftItemTier.Green] = greenTab.gameObject;

            // turn green off
            greenTab.gameObject.SetActive(false);

            //BuildWhiteTabs(contentAreaRt);

            return contentAreaRt;
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


        private static RectTransform BuildWhiteTabs(RectTransform parentRectTransform)
        {
            var parentObject = parentRectTransform.gameObject;

            // set tab up correctly then everything hangs off tab
            var tabGO = new GameObject("WhiteTab", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(DraftTabController));
            var tabRoot = (RectTransform)tabGO.transform;
            tabGO.layer = parentObject.layer;
            tabRoot.SetParent(parentRectTransform, false);
            // stretch tab to fill
            StretchFull(tabRoot);


            var layout = tabGO.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            // here we will add toggles
            // TODO WIPS
            //var controlsBar = BuildTabControlsBar(tabRoot);
            var controlsBar = ItemPickerTabControlsFactory.CreateTabControls("WhiteGridControls", tabRoot);
            var controlsBarGO = controlsBar.gameObject;

            var tabsLE = controlsBarGO.AddComponent<LayoutElement>();
            tabsLE.preferredHeight = 64;
            tabsLE.flexibleHeight = 0;

            var gridGO = new GameObject("WhiteGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ItemPickerGridController));
            gridGO.layer = parentObject.layer;
            var gridRt = (RectTransform)gridGO.transform;
            gridRt.SetParent(tabRoot, worldPositionStays: false);

            var itemsControls = controlsBar.GetComponent<DraftTabControls>();

            var tabController = tabGO.GetComponent<DraftTabController>();
            tabController.SetTabTier(DraftItemTier.White);
            tabController.SetControls(itemsControls);

            var gridController = gridGO.GetComponent<ItemPickerGridController>();
            tabController.SetGridController(gridController);

            // stretch, with some margins so you can see edges
            gridRt.anchorMin = Vector2.zero;
            gridRt.anchorMax = Vector2.one;
            gridRt.pivot = new Vector2(0.5f, 0.5f);
            gridRt.offsetMin = new Vector2(12, 12);
            gridRt.offsetMax = new Vector2(-12, -12);

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
            grid.cellSize = new Vector2(80, 80);
            grid.spacing = new Vector2(4, 4);

            // The command card uses 5 columns
            // but we have more real estate here so we will go with 8
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 7;

            // I know this one works
            //for (int i = 0; i < 21; i++)
            //{
            //    var sq = new GameObject("ItemSquare", typeof(RectTransform), typeof(Image), typeof(Button));
            //    sq.layer = gridGO.layer;
            //    var sqRt = (RectTransform)sq.transform;
            //    sqRt.SetParent(gridRt, worldPositionStays: false);
            //    // Do NOT set size/pos; GridLayoutGroup will handle it.
            //    // (If you want a visible icon, set an Image sprite)
            //    sq.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f); // dark tile bg
            //}

            //PickupDef hoofDef = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(RoR2Content.Items.Hoof.itemIndex));
            //ItemPickerSquareFactory.TestItemSquare(gridRt, hoofDef.iconSprite);

            var squareControllers = new List<ItemPickerSquareController>();
            var whitePickups = DraftPools.Instance.GetDraftablePickups(DraftItemTier.White);
            foreach (var pickIndex in whitePickups)
            {
                var pickDef = PickupCatalog.GetPickupDef(pickIndex);
                var singleSquare = ItemPickerSquareFactory.TestItemSquare(gridRt, pickDef, Color.cyan);
                var singleController = singleSquare.GetComponent<ItemPickerSquareController>();
                squareControllers.Add(singleController);
            }

            // set our reffies
            gridController.SetSquares(squareControllers);

            return tabRoot;
        }

        private static GameObject BuildTabControlsBar(RectTransform parentRectTransform)
        {

            // TODO NAME
            var controlsBar = new GameObject("WhiteTabControls", typeof(RectTransform), typeof(DraftTabControls));
            var controlsBarRt = controlsBar.GetComponent<RectTransform>();
            controlsBarRt.SetParent(parentRectTransform);

            var img = controlsBar.AddComponent<Image>();
            img.color = Color.magenta;           // solid red to verify full stretch
            img.raycastTarget = true;        // optional click blocker

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.layer = controlsBar.layer;
            labelGo.transform.SetParent(controlsBar.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = new Vector2(0, 0);
            labelRt.anchorMax = new Vector2(1, 0);
            labelRt.sizeDelta = new Vector2(0, 22);

            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = "WIP";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 16;

            var buttonGo = new GameObject("Button", typeof(Button), typeof(Image));
            var button = buttonGo.GetComponent<Button>();
            buttonGo.transform.SetParent(labelRt.transform, false);

            var buttonBG = buttonGo.GetComponent<Image>();
            buttonBG.color = Color.black;

            //var controller = controlsBar.GetComponent<ItemPickerTabControls>();
            //controller.BindControls(label, button);

            return controlsBar;
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
    }
}
