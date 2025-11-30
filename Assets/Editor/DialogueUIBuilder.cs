using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Generates a reusable Dialogue Canvas prefab entirely from built-in UI components
/// and wires the DialogueUI script so inspectors are ready out of the box.
/// </summary>
public static class DialogueUIBuilder
{
    private const string DialogueCanvasPath = "Assets/Prefabs/UI/DialogueCanvas.prefab";
    private const string ChoiceButtonPath = "Assets/Prefabs/UI/ChoiceButton.prefab";

    [InitializeOnLoadMethod]
    private static void AutoBuildOncePerSession()
    {
        const string sessionKey = "DialogueUIBuilder_Autobuild";
        if (SessionState.GetBool(sessionKey, false)) return;
        SessionState.SetBool(sessionKey, true);
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            CreateDialogueUIPrefabs();
        };
    }

    [MenuItem("Tools/Dialogue/Create Dialogue UI Prefabs")]
    public static void CreateDialogueUIPrefabs()
    {
        EnsureFolders();

        GameObject canvasGO = new GameObject("DialogueCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        // --- Build Hierarchy ---
        var bgLayer = CreateRect("BackgroundLayer", canvasGO.transform);
        ConfigureStretch(bgLayer);
        var bgImage = CreateRect("BGImage", bgLayer.transform, typeof(Image), typeof(CanvasGroup));
        ConfigureStretch(bgImage);
        var bgImgComp = bgImage.GetComponent<Image>();
        bgImgComp.color = new Color(0.08f, 0.08f, 0.08f, 1f);
        bgImgComp.raycastTarget = false;

        var charLayer = CreateRect("CharacterLayer", canvasGO.transform);
        ConfigureStretch(charLayer);
        var leftPortrait = CreatePortrait(charLayer.transform, "LeftPortrait", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(-860f, 120f));
        var rightPortrait = CreatePortrait(charLayer.transform, "RightPortrait", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(860f, 120f));
        var fxLayer = CreateRect("FXLayer", charLayer.transform, typeof(CanvasGroup));
        ConfigureStretch(fxLayer);
        fxLayer.GetComponent<CanvasGroup>().blocksRaycasts = false;

        var dialogueLayer = CreateRect("DialogueLayer", canvasGO.transform);
        ConfigureStretch(dialogueLayer);
        var dialogueBox = CreateRect("DialogueBox", dialogueLayer.transform, typeof(Image), typeof(CanvasGroup));
        ConfigureRect(dialogueBox, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(1500f, 280f));
        var dialogueBG = dialogueBox.GetComponent<Image>();
        dialogueBG.color = new Color(0f, 0f, 0f, 0.65f);

        var nameTag = CreateRect("NameTag", dialogueBox.transform, typeof(Image));
        ConfigureRect(nameTag, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(300f, 80f));
        nameTag.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f, 0.9f);
        var nameText = CreateTMP(nameTag.transform, "NameText", "角色名");
        ConfigureStretch(nameText.rectTransform);
        nameText.fontSize = 36f;
        nameText.alignment = TextAlignmentOptions.MidlineLeft;
        nameText.margin = new Vector4(16, 10, 16, 10);

        var dialogueText = CreateTMP(dialogueBox.transform, "DialogueText", "这里显示对白内容……");
        ConfigureRect(dialogueText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        dialogueText.margin = new Vector4(40, 80, 40, 40);
        dialogueText.textWrappingMode = TextWrappingModes.Normal;
        dialogueText.fontSize = 34f;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;

        var continueIndicator = CreateRect("ContinueIndicator", dialogueBox.transform, typeof(Image), typeof(CanvasGroup));
        ConfigureRect(continueIndicator, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 20f), new Vector2(48f, 48f));
        continueIndicator.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.8f);
        continueIndicator.gameObject.SetActive(false);

        var autoPlayIcon = CreateRect("AutoPlayIcon", dialogueBox.transform, typeof(Image));
        ConfigureRect(autoPlayIcon, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(48f, 48f));
        autoPlayIcon.GetComponent<Image>().color = new Color(0.7f, 0.9f, 0.4f, 0.8f);
        autoPlayIcon.gameObject.SetActive(false);

        var choicePanel = CreateRect("ChoicePanel", dialogueLayer.transform, typeof(CanvasGroup), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        ConfigureRect(choicePanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, 330f), Vector2.zero);
        var vlg = choicePanel.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 20f;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        var csf = choicePanel.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var choiceGroup = choicePanel.GetComponent<CanvasGroup>();
        choiceGroup.alpha = 1f;
        choiceGroup.interactable = true;
        choiceGroup.blocksRaycasts = true;
        choicePanel.gameObject.SetActive(false);

        var controlLayer = CreateRect("ControlLayer", canvasGO.transform);
        ConfigureStretch(controlLayer);
        var skipButton = CreateButton(controlLayer.transform, "SkipButton", new Vector2(-60f, -60f));
        var autoButton = CreateButton(controlLayer.transform, "AutoButton", new Vector2(-240f, -60f));
        var historyButton = CreateButton(controlLayer.transform, "HistoryButton", new Vector2(-420f, -60f));

        var historyPanel = CreateRect("HistoryPanel", canvasGO.transform, typeof(Image), typeof(CanvasGroup), typeof(ScrollRect));
        ConfigureRect(historyPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 820f));
        var historyImage = historyPanel.GetComponent<Image>();
        historyImage.color = new Color(0.07f, 0.07f, 0.11f, 0.95f);
        var historyGroup = historyPanel.GetComponent<CanvasGroup>();
        historyGroup.alpha = 1f;
        historyGroup.interactable = true;
        historyGroup.blocksRaycasts = true;
        historyPanel.gameObject.SetActive(false);
        var viewport = CreateRect("Viewport", historyPanel.transform, typeof(Image), typeof(Mask));
        ConfigureStretch(viewport);
        viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.0001f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        var content = CreateRect("Content", viewport.transform, typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        ConfigureRect(content, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
        var contentLayout = content.GetComponent<VerticalLayoutGroup>();
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.padding = new RectOffset(20, 20, 20, 20);
        contentLayout.spacing = 12f;
        var contentFit = content.GetComponent<ContentSizeFitter>();
        contentFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var historyText = CreateTMP(content.transform, "HistoryText", string.Empty);
        ConfigureStretch(historyText.rectTransform);
        historyText.textWrappingMode = TextWrappingModes.Normal;
        historyText.fontSize = 30f;
        historyText.alignment = TextAlignmentOptions.TopLeft;

        var scrollRect = historyPanel.GetComponent<ScrollRect>();
        scrollRect.viewport = viewport;
        scrollRect.content = content;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 45f;

        // --- Choice Button Prefab ---
        var choiceButtonInstance = CreateChoiceButton();
        var choiceButtonPrefab = PrefabUtility.SaveAsPrefabAsset(choiceButtonInstance, ChoiceButtonPath);

        // --- Attach DialogueUI script and wire references ---
        var dialogueUIType = System.Type.GetType("DialogueUI,Assembly-CSharp");
        if (dialogueUIType != null)
        {
            var dialogueUI = canvasGO.AddComponent(dialogueUIType);
            SerializedObject so = new SerializedObject(dialogueUI);
            SetProp(so, "bgImage", bgImgComp);
            SetProp(so, "bgCanvasGroup", bgImage.GetComponent<CanvasGroup>());
            SetProp(so, "leftPortrait", leftPortrait.GetComponent<Image>());
            SetProp(so, "leftPortraitCanvasGroup", leftPortrait.GetComponent<CanvasGroup>());
            SetProp(so, "rightPortrait", rightPortrait.GetComponent<Image>());
            SetProp(so, "rightPortraitCanvasGroup", rightPortrait.GetComponent<CanvasGroup>());
            SetProp(so, "fxLayer", fxLayer.gameObject);
            SetProp(so, "dialogueBox", dialogueBox.gameObject);
            SetProp(so, "nameTag", nameText);
            SetProp(so, "dialogueText", dialogueText);
            SetProp(so, "continueIndicator", continueIndicator.gameObject);
            SetProp(so, "autoPlayIcon", autoPlayIcon.gameObject);
            SetProp(so, "choicePanel", choicePanel.gameObject);
            if (choiceButtonPrefab != null)
            {
                SetProp(so, "choicePrefab", choiceButtonPrefab.GetComponent<DialogueChoiceButton>());
            }
            SetProp(so, "skipButton", skipButton.GetComponent<Button>());
            SetProp(so, "autoButton", autoButton.GetComponent<Button>());
            SetProp(so, "historyButton", historyButton.GetComponent<Button>());
            SetProp(so, "historyWindow", historyPanel.gameObject);
            SetProp(so, "historyScrollRect", scrollRect);
            SetProp(so, "historyText", historyText);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // --- Save Dialogue Canvas prefab ---
        PrefabUtility.SaveAsPrefabAsset(canvasGO, DialogueCanvasPath);

        Object.DestroyImmediate(canvasGO);
        Object.DestroyImmediate(choiceButtonInstance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Dialogue UI prefabs regenerated. You can now reuse DialogueCanvas.prefab across scenes.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }
    }

    private static RectTransform CreateRect(string name, Transform parent, params System.Type[] extraComponents)
    {
        var components = new System.Type[extraComponents.Length + 1];
        components[0] = typeof(RectTransform);
        extraComponents.CopyTo(components, 1);
        var go = new GameObject(name, components);
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void ConfigureStretch(RectTransform rt)
    {
        ConfigureRect(rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
    }

    private static void ConfigureRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
    }

    private static TextMeshProUGUI CreateTMP(Transform parent, string name, string sampleText)
    {
        var rt = CreateRect(name, parent, typeof(TextMeshProUGUI));
        var tmp = rt.GetComponent<TextMeshProUGUI>();
        tmp.text = sampleText;
        tmp.fontSize = 32f;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.color = Color.white;
        return tmp;
    }

    private static RectTransform CreatePortrait(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos)
    {
        var rt = CreateRect(name, parent, typeof(Image), typeof(CanvasGroup));
        ConfigureRect(rt, anchorMin, anchorMax, pivot, anchoredPos, new Vector2(600f, 900f));
        rt.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        rt.GetComponent<CanvasGroup>().alpha = 0.5f;
        return rt;
    }

    private static RectTransform CreateButton(Transform parent, string name, Vector2 anchoredPos)
    {
        var rt = CreateRect(name, parent, typeof(Image), typeof(Button));
        ConfigureRect(rt, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), anchoredPos, new Vector2(160f, 60f));
        var img = rt.GetComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        var label = CreateTMP(rt, "Label", name.Replace("Button", ""));
        ConfigureStretch(label.rectTransform);
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 28f;
        return rt;
    }

    private static GameObject CreateChoiceButton()
    {
        var go = new GameObject("ChoiceButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(DialogueChoiceButton));
        var rt = go.GetComponent<RectTransform>();
        ConfigureRect(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 120f));
        var bg = go.GetComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        var outline = CreateRect("Outline", go.transform, typeof(Image));
        ConfigureStretch(outline);
        var outlineImg = outline.GetComponent<Image>();
        outlineImg.color = new Color(0.5f, 0.8f, 1f, 0.4f);
        outline.gameObject.SetActive(false);

        var label = CreateTMP(go.transform, "Label", "选项文本");
        ConfigureStretch(label.rectTransform);
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.margin = new Vector4(32, 20, 32, 20);

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        var cb = go.GetComponent<DialogueChoiceButton>();
        SerializedObject so = new SerializedObject(cb);
        SetProp(so, "textBox", label);
        SetProp(so, "backgroundImage", bg);
        SetProp(so, "outlineImage", outlineImg);
        so.ApplyModifiedPropertiesWithoutUndo();

        return go;
    }

    private static void SetProp(SerializedObject so, string name, Object value)
    {
        if (so == null) return;
        var prop = so.FindProperty(name);
        if (prop == null) return;
        prop.objectReferenceValue = value;
    }
}
