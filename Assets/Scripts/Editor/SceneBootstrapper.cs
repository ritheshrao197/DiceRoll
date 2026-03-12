#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Editor-only script: Menu → DiceSpirit → Build Scene
/// Programmatically creates all GameObjects, Canvas hierarchy,
/// and wires up references so the scene is immediately playable.
///
/// Run this ONCE in a fresh/empty scene. After running, delete or
/// disable this script — it is not needed at runtime.
/// </summary>
public static class SceneBootstrapper
{
    [MenuItem("DiceSpirit/Build Scene")]
    public static void BuildScene()
    {
        // ── Canvas root ───────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── EventSystem ───────────────────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // ── Background panel ─────────────────────────────────────────────────
        var bg = CreateUIImage(canvasGO.transform, "Background",
            new Color(0.06f, 0.06f, 0.12f));
        StretchFull(bg.GetComponent<RectTransform>());

        // ── Dice area ─────────────────────────────────────────────────────────
        var dicePanel = CreateUIImage(canvasGO.transform, "DicePanel",
            new Color(0.12f, 0.12f, 0.22f));
        var diceRect  = dicePanel.GetComponent<RectTransform>();
        diceRect.anchorMin = diceRect.anchorMax = new Vector2(0.5f, 0.65f);
        diceRect.sizeDelta = new Vector2(200, 200);
        diceRect.anchoredPosition = Vector2.zero;

        var diceFaceTMP = CreateTMP(dicePanel.transform, "DiceFace", "1",
            fontSize: 96, color: Color.white);
        CenterStretch(diceFaceTMP.GetComponent<RectTransform>());

        // ── Roll button ───────────────────────────────────────────────────────
        var btnGO = CreateButton(canvasGO.transform, "RollButton", "ROLL",
            new Color(0.2f, 0.6f, 1f), new Vector2(220, 70), new Vector2(0.5f, 0.5f));
        var btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin = btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, -60);

        // ── Equation row ─────────────────────────────────────────────────────
        var eqGO = new GameObject("EquationRow");
        var eqTr = eqGO.AddComponent<RectTransform>();
        eqTr.SetParent(canvasGO.transform, false);
        eqTr.anchorMin = eqTr.anchorMax = new Vector2(0.5f, 0.38f);
        eqTr.sizeDelta = new Vector2(700, 80);
        eqTr.anchoredPosition = Vector2.zero;
        var hlg = eqGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;

        var pointsTMP     = CreateEquationTMP(eqTr, "PointsLabel",     "0");
        var timesTMP      = CreateEquationTMP(eqTr, "TimesLabel",       "×");
        var multiplierTMP = CreateEquationTMP(eqTr, "MultiplierLabel", "10");
        var equalsTMP     = CreateEquationTMP(eqTr, "EqualsLabel",      "=");
        var totalTMP      = CreateEquationTMP(eqTr, "TotalLabel",       "0", color: new Color(0.3f, 1f, 0.45f));

        // ── Spirit Cards ─────────────────────────────────────────────────────
        var cardA = BuildSpiritCard(canvasGO.transform, "SpiritCard_A",
            new Vector2(-320, -280), "Spirit A", "Dice=6\n×2 Multiplier",
            new Color(1f, 0.85f, 0.1f));

        var cardB = BuildSpiritCard(canvasGO.transform, "SpiritCard_B",
            new Vector2(320, -280), "Spirit B", "Dice=3\n+10 Points",
            new Color(0.4f, 0.8f, 1f));

        // ── History label ─────────────────────────────────────────────────────
        var histGO  = CreateTMP(canvasGO.transform, "HistoryLabel", "History: —",
            fontSize: 22, color: new Color(0.7f, 0.7f, 0.7f));
        var histRect = histGO.GetComponent<RectTransform>();
        histRect.anchorMin = histRect.anchorMax = new Vector2(0.5f, 0.08f);
        histRect.sizeDelta = new Vector2(900, 36);
        histRect.anchoredPosition = Vector2.zero;
        histGO.gameObject.AddComponent<RollHistoryView>();  // auto-hooks via OnEnable

        // ── Manager GameObjects ───────────────────────────────────────────────
        var managersGO = new GameObject("Managers");

        // GameCalculator
        var calcGO = new GameObject("GameCalculator");
        calcGO.transform.SetParent(managersGO.transform);
        var calc = calcGO.AddComponent<GameCalculator>();

        // SpiritCardManager
        var scmGO = new GameObject("SpiritCardManager");
        scmGO.transform.SetParent(managersGO.transform);
        var scm = scmGO.AddComponent<SpiritCardManager>();

        // DiceRoller
        var rollerGO = new GameObject("DiceRoller");
        rollerGO.transform.SetParent(managersGO.transform);
        var roller = rollerGO.AddComponent<DiceRoller>();

        // UIEquationView
        var eqViewGO = new GameObject("UIEquationView");
        eqViewGO.transform.SetParent(managersGO.transform);
        var eqView = eqViewGO.AddComponent<UIEquationView>();

        // ── Wire up serialised fields via SerializedObject ────────────────────
        // (So Unity properly marks the scene dirty for saving)
        WireCalculator(calc, scm);
        WireDiceRoller(roller, btnGO.GetComponent<Button>(),
                       diceFaceTMP, diceRect, calc, scm);
        WireEquationView(eqView, pointsTMP, multiplierTMP, totalTMP, timesTMP, equalsTMP);
        WireSpiritCardViews(scm, cardA, cardB);

        Debug.Log("[SceneBootstrapper] Scene built successfully. " +
                  "Assign SpiritCardData assets in SpiritCardManager, then press Play.");
    }

    // ── Builder helpers ───────────────────────────────────────────────────────

    private static GameObject CreateUIImage(Transform parent, string name, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private static TextMeshProUGUI CreateTMP(Transform parent, string name, string text,
                                              float fontSize = 48, Color? color = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = color ?? Color.white;
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120, 72);
        return tmp;
    }

    private static TextMeshProUGUI CreateEquationTMP(Transform parent, string name, string text,
                                                      Color? color = null)
    {
        var tmp  = CreateTMP(parent, name, text, fontSize: 56, color: color);
        var rect = tmp.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120, 80);
        return tmp;
    }

    private static GameObject CreateButton(Transform parent, string name, string label,
                                            Color bgColor, Vector2 size, Vector2 pivot)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();

        var labelGO  = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var tmp      = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        CenterStretch(tmp.GetComponent<RectTransform>());

        var rect   = go.GetComponent<RectTransform>();
        rect.pivot = pivot;
        rect.sizeDelta = size;
        return go;
    }

    private static GameObject BuildSpiritCard(Transform parent, string name,
                                               Vector2 anchoredPos, string cardName,
                                               string cardDesc, Color accentColor)
    {
        var card = new GameObject(name);
        card.transform.SetParent(parent, false);
        var rect = card.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(200, 280);
        rect.anchoredPosition = anchoredPos;

        // Background
        var bg    = card.AddComponent<Image>();
        bg.color  = new Color(0.1f, 0.1f, 0.18f);

        // Glow overlay
        var glowGO   = CreateUIImage(card.transform, "Glow", accentColor);
        var glowRect = glowGO.GetComponent<RectTransform>();
        StretchFull(glowRect);
        var glowImg   = glowGO.GetComponent<Image>();
        var glowColor = accentColor;
        glowColor.a   = 0f;
        glowImg.color = glowColor;

        // Name
        var nameTMP = CreateTMP(card.transform, "NameLabel", cardName, 20, Color.white);
        var nameRect = nameTMP.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.75f);
        nameRect.anchorMax = new Vector2(1, 1f);
        nameRect.offsetMin = nameRect.offsetMax = Vector2.zero;

        // Description
        var descTMP = CreateTMP(card.transform, "DescLabel", cardDesc, 16,
                                new Color(0.8f, 0.8f, 0.8f));
        var descRect = descTMP.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0.2f);
        descRect.anchorMax = new Vector2(1, 0.75f);
        descRect.offsetMin = descRect.offsetMax = Vector2.zero;
        descTMP.GetComponent<TextMeshProUGUI>().enableWordWrapping = true;

        // SpiritCardView component
        var view = card.AddComponent<SpiritCardView>();
        // References are set in WireSpiritCardViews via SerializedObject

        return card;
    }

    // ── SerializedObject wiring ───────────────────────────────────────────────

    private static void WireCalculator(GameCalculator calc, SpiritCardManager scm)
    {
        var so = new SerializedObject(calc);
        so.FindProperty("spiritCardManager").objectReferenceValue = scm;
        so.ApplyModifiedProperties();
    }

    private static void WireDiceRoller(DiceRoller roller, Button btn,
                                        TextMeshProUGUI faceLabel,
                                        RectTransform diceRect,
                                        GameCalculator calc,
                                        SpiritCardManager scm)
    {
        var so = new SerializedObject(roller);
        so.FindProperty("rollButton").objectReferenceValue        = btn;
        so.FindProperty("diceFaceLabel").objectReferenceValue     = faceLabel;
        so.FindProperty("diceRect").objectReferenceValue          = diceRect;
        so.FindProperty("calculator").objectReferenceValue        = calc;
        so.FindProperty("spiritCardManager").objectReferenceValue = scm;
        so.ApplyModifiedProperties();
    }

    private static void WireEquationView(UIEquationView view,
                                          TextMeshProUGUI points,
                                          TextMeshProUGUI multiplier,
                                          TextMeshProUGUI total,
                                          TextMeshProUGUI times,
                                          TextMeshProUGUI equals)
    {
        var so = new SerializedObject(view);
        so.FindProperty("pointsLabel").objectReferenceValue     = points;
        so.FindProperty("multiplierLabel").objectReferenceValue = multiplier;
        so.FindProperty("totalLabel").objectReferenceValue      = total;
        so.FindProperty("timesLabel").objectReferenceValue      = times;
        so.FindProperty("equalsLabel").objectReferenceValue     = equals;
        so.ApplyModifiedProperties();
    }

    private static void WireSpiritCardViews(SpiritCardManager scm,
                                             GameObject cardA, GameObject cardB)
    {
        // Wire SpiritCardView internal fields
        foreach (var card in new[] { cardA, cardB })
        {
            var view = card.GetComponent<SpiritCardView>();
            var so   = new SerializedObject(view);
            so.FindProperty("cardBackground").objectReferenceValue =
                card.GetComponent<Image>();
            var glowTr = card.transform.Find("Glow");
            if (glowTr)
                so.FindProperty("glowImage").objectReferenceValue =
                    glowTr.GetComponent<Image>();
            so.FindProperty("nameLabel").objectReferenceValue =
                card.transform.Find("NameLabel")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("descLabel").objectReferenceValue =
                card.transform.Find("DescLabel")?.GetComponent<TextMeshProUGUI>();
            so.ApplyModifiedProperties();
        }

        // Note: SpiritCardData assets must be assigned manually in Inspector
        // after creating them via Assets → Create → DiceSpirit → SpiritCard
        Debug.Log("[SceneBootstrapper] Reminder: assign SpiritCardData assets " +
                  "in the SpiritCardManager component.");
    }

    // ── Rect helpers ─────────────────────────────────────────────────────────

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin  = Vector2.zero;
        rect.anchorMax  = Vector2.one;
        rect.offsetMin  = Vector2.zero;
        rect.offsetMax  = Vector2.zero;
    }

    private static void CenterStretch(RectTransform rect)
    {
        rect.anchorMin      = Vector2.zero;
        rect.anchorMax      = Vector2.one;
        rect.offsetMin      = Vector2.zero;
        rect.offsetMax      = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }
}
#endif
