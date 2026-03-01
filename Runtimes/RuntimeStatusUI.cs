using UnityEngine;
using UnityEngine.UI;

// Lightweight runtime status UI that persists across scenes
// Displays Health, Stamina, and Mana/status bars using a single Canvas
public class RuntimeStatusUI : MonoBehaviour
{
    public static RuntimeStatusUI Instance { get; private set; }

    private Canvas _canvas;
    private Slider _healthBar;
    private Slider _manaBar;
    private Slider _staminaBar;
    // Labels
    private Text _healthLabel;
    private Text _manaLabel;
    private Text _staminaLabel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        CreateUI();
    }

    void CreateUI()
    {
        var go = new GameObject("RuntimeStatusCanvas");
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();

        // Simple stacked vertical UI in bottom-left
        _healthBar = CreateBar(go.transform, new Vector2(0.02f, 0.05f), new Vector2(0.22f, 0.025f));
        _manaBar = CreateBar(go.transform, new Vector2(0.02f, 0.03f), new Vector2(0.22f, 0.025f));
        _staminaBar = CreateBar(go.transform, new Vector2(0.02f, 0.01f), new Vector2(0.22f, 0.025f));
        // Labels
        _healthLabel = CreateLabel(go.transform, "Health", new Vector2(0.0f, 0.07f));
        _manaLabel = CreateLabel(go.transform, "Mana", new Vector2(0.0f, 0.04f));
        _staminaLabel = CreateLabel(go.transform, "Stamina", new Vector2(0.0f, 0.01f));
    }

    private Text CreateLabel(Transform parent, string text, Vector2 anchor)
    {
        var labelGO = new GameObject(text + "Label");
        labelGO.transform.SetParent(parent, false);
        var labelText = labelGO.AddComponent<Text>();
        labelText.text = text;
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 14;
        labelText.color = Color.white;
        var rt = labelGO.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(100, 20);
        return labelText;
    }

    Slider CreateBar(Transform parent, Vector2 anchor, Vector2 size)
    {
        var barGO = new GameObject("StatusBar");
        barGO.transform.SetParent(parent, false);
        var bar = barGO.AddComponent<Slider>();
        // Minimal UI visuals
        var bg = new GameObject("Background");
        bg.transform.SetParent(barGO.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f,0f,0f,0.5f);
        var fill = new GameObject("Fill");
        fill.transform.SetParent(bg.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.green;
        bar.fillRect = fill.GetComponent<RectTransform>();
        // RectTransform setup
        var rt = barGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchor.x, anchor.y);
        rt.anchorMax = new Vector2(anchor.x, anchor.y);
        rt.sizeDelta = new Vector2(Screen.width * size.x, Screen.height * size.y);
        // Interactivity off
        bar.interactable = false;
        return bar;
    }

    public void SetHealth(float t) { if (_healthBar != null) _healthBar.value = Mathf.Clamp01(t); }
    public void SetMana(float t) { if (_manaBar != null) _manaBar.value = Mathf.Clamp01(t); }
    public void SetStamina(float t) { if (_staminaBar != null) _staminaBar.value = Mathf.Clamp01(t); }
    public void SetAll(float health01, float mana01, float stamina01)
    {
        SetHealth(health01);
        SetMana(mana01);
        SetStamina(stamina01);
    }
}
