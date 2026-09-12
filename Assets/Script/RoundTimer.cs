using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class RoundTimer : MonoBehaviour
{
    private const int MaximumStamina = 100;

    [SerializeField] private int roundDurationSeconds = 8 * 60;
    [SerializeField] private MainState state = new MainState();

    /// <summary>
    /// 挂载了 ChangeTurnAnim.playable 的 PlayableDirector。
    /// 若未在 Inspector 赋值，Awake 时自动查找名为 "ChangeTurnDirector" 的 GameObject。
    /// </summary>
    [SerializeField] private PlayableDirector changeTurnDirector;

    /// <summary>
    /// Timeline 播放期间是否禁用所有按钮交互。
    /// false（默认）= 播放时按钮仍然可以点击；
    /// true          = 播放时所有 Button.interactable = false，结束后恢复。
    /// </summary>
    [SerializeField] private bool blockInputDuringTimeline = false;

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

        state.CurrRound = Mathf.Max(1, state.CurrRound);
        state.RemainingTimeSeconds = roundDurationSeconds;
        state.Strength = MaximumStamina;

        // 自动查找
        if (changeTurnDirector == null)
        {
            GameObject dirObj = GameObject.Find("ChangeTurnDirector");
            if (dirObj != null)
                changeTurnDirector = dirObj.GetComponent<PlayableDirector>();

            if (changeTurnDirector == null)
                Debug.LogWarning("RoundTimer：未找到 ChangeTurnDirector，回合切换动画将不会播放。", this);
        }

        // 订阅 Timeline 播放完成事件
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
        // Timeline 播放中跳过时间/体力检测，避免动画结束前再次触发 AdvanceRound
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

        PlayChangeTurnTimeline();   // 播放动画（内部按需禁用按钮）

        RoundAdvanced?.Invoke();
        UpdateDisplay();
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

    /// <summary>
    /// PlayableDirector.stopped 回调：Timeline 结束或被 Stop() 时触发。
    /// </summary>
    private void OnTimelineStopped(PlayableDirector pd)
    {
        if (blockInputDuringTimeline)
            SetAllButtonsInteractable(true);
    }

    // ──────────────────────────────────────────────
    private bool IsTimelinePlaying()
        => changeTurnDirector != null && changeTurnDirector.state == PlayState.Playing;

    /// <summary>
    /// 启用或禁用场景中所有激活的 Button。
    /// </summary>
    private void SetAllButtonsInteractable(bool interactable)
    {
        Button[] allButtons = FindObjectsOfType<Button>(false); // false = 只找激活的
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
