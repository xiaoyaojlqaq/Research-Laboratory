using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class RoundTimer : MonoBehaviour
{
    private const int MaximumStamina = 100;

    [SerializeField] private int roundDurationSeconds = 8 * 60;
    [SerializeField] private MainState state = new MainState();

    [SerializeField] private PlayableDirector changeTurnDirector;

    [SerializeField] private bool blockInputDuringTimeline = true;

    private Text roundText;
    private Text timeText;
    private Text staminaText;
    private Text inspirationText;

    public event System.Action RoundAdvanced;

    public MainState State => state;

    // ──────────────────────────────────────────────
    private void Awake()
    {
        roundText       = FindTextAtIndex(0, "当前回合");
        timeText        = FindTextAtIndex(1, "时间");
        staminaText     = FindTextAtIndex(2, "体力");
        inspirationText = FindTextAtIndex(3, "灵感");

        if (state == null) state = new MainState();

        // ── 读取存档（SaveManager 已在 BeforeSceneLoad 阶段自动初始化并加载存档）──
        SaveData save = SaveManager.Instance != null ? SaveManager.Instance.Data : null;

        if (save != null)
        {
            state.CurrRound            = save.currentRound;
            state.RemainingTimeSeconds = save.remainingTimeSeconds > 0f
                                         ? save.remainingTimeSeconds
                                         : roundDurationSeconds;
            state.Strength             = save.stamina > 0 ? save.stamina : MaximumStamina;
            state.Inspiration          = save.inspiration;
        }
        else
        {
            state.CurrRound            = 1;
            state.RemainingTimeSeconds = roundDurationSeconds;
            state.Strength             = MaximumStamina;
        }

        // 自动查找 ChangeTurnDirector
        if (changeTurnDirector == null)
        {
            GameObject dirObj = GameObject.Find("ChangeTurnDirector");
            if (dirObj != null)
                changeTurnDirector = dirObj.GetComponent<PlayableDirector>();

            if (changeTurnDirector == null)
                Debug.LogWarning("RoundTimer：未找到 ChangeTurnDirector，回合切换动画将不会播放。", this);
        }

        if (changeTurnDirector != null)
            changeTurnDirector.stopped += OnTimelineStopped;

        UpdateDisplay();
    }

    private void OnDestroy()
    {
        if (changeTurnDirector != null)
            changeTurnDirector.stopped -= OnTimelineStopped;
    }

    // ──────────────────────────────────────────────
    private void Update()
    {
        if (IsTimelinePlaying()) return;

        state.RemainingTimeSeconds -= Time.deltaTime;

        if (state.RemainingTimeSeconds <= 0f || state.Strength <= 0)
            AdvanceRound();

        UpdateDisplay();
    }

    // ──────────────────────────────────────────────
    private void AdvanceRound()
    {
        state.CurrRound++;
        state.RemainingTimeSeconds = roundDurationSeconds;
        state.Strength = MaximumStamina;

        PlayChangeTurnTimeline();

        RoundAdvanced?.Invoke();
        UpdateDisplay();

        // 每次回合推进时保存主状态
        SaveState();
    }

    // ──────────────────────────────────────────────
    /// <summary>将当前 state 写入存档（论文由 PaperWorkspaceNavigator 负责写）。</summary>
    public void SaveState()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveState(state);
    }

    // ──────────────────────────────────────────────
    private void PlayChangeTurnTimeline()
    {
        if (changeTurnDirector == null) return;

        if (blockInputDuringTimeline)
            SetAllButtonsInteractable(false);

        changeTurnDirector.time = 0;
        changeTurnDirector.Play();
    }

    private void OnTimelineStopped(PlayableDirector pd)
    {
        if (blockInputDuringTimeline)
            SetAllButtonsInteractable(true);
    }

    private bool IsTimelinePlaying()
        => changeTurnDirector != null && changeTurnDirector.state == PlayState.Playing;

    private void SetAllButtonsInteractable(bool interactable)
    {
        Button[] allButtons = FindObjectsOfType<Button>(false);
        foreach (Button btn in allButtons)
            btn.interactable = interactable;
    }

    // ──────────────────────────────────────────────
    public bool TryUseLiteratureSearch()
    {
        const int staminaCost = 10;
        const float timeCostSeconds = 60f;

        if (state.Strength < staminaCost || state.RemainingTimeSeconds < timeCostSeconds)
        {
            AdvanceRound();
            return false;
        }

        state.Strength -= staminaCost;
        state.RemainingTimeSeconds -= timeCostSeconds;
        UpdateDisplay();

        // 消耗体力/时间后也保存
        SaveState();
        return true;
    }

    // ──────────────────────────────────────────────
    private Text FindTextAtIndex(int childIndex, string displayName)
    {
        if (childIndex < 0 || childIndex >= transform.childCount)
        {
            Debug.LogError("RoundTimer could not find the " + displayName + " display under " + name + ".", this);
            return null;
        }

        Transform panel = transform.GetChild(childIndex);
        Text text = panel.GetComponentInChildren<Text>();
        if (text == null)
            Debug.LogError("RoundTimer could not find a Text component for " + displayName + ".", this);

        return text;
    }

    private void UpdateDisplay()
    {
        if (roundText != null)
            roundText.text = "当前回合:" + state.CurrRound;

        if (timeText != null)
        {
            int displayedSeconds = Mathf.CeilToInt(state.RemainingTimeSeconds);
            timeText.text = string.Format("时间 {0}:{1:00}", displayedSeconds / 60, displayedSeconds % 60);
        }

        if (staminaText != null)
            staminaText.text = "体力:" + state.Strength;

        if (inspirationText != null)
            inspirationText.text = "灵感:" + state.Inspiration.ToString("0.0");
    }
}
