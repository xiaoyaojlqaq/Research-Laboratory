using UnityEngine;
using UnityEngine.UI;

public class RoundTimer : MonoBehaviour
{
    private const int MaximumStamina = 100;

    [SerializeField] private int roundDurationSeconds = 8 * 60;
    [SerializeField] private MainState state = new MainState();

    private Text roundText;
    private Text timeText;
    private Text staminaText;
    private Text inspirationText;

    public event System.Action RoundAdvanced;

    public MainState State
    {
        get { return state; }
    }

    private void Awake()
    {
        roundText = FindTextAtIndex(0, "当前回合");
        timeText = FindTextAtIndex(1, "时间");
        staminaText = FindTextAtIndex(2, "体力");
        inspirationText = FindTextAtIndex(3, "灵感");
        if (state == null)
        {
            state = new MainState();
        }

        state.CurrRound = Mathf.Max(1, state.CurrRound);
        state.RemainingTimeSeconds = roundDurationSeconds;
        state.Strength = MaximumStamina;
        UpdateDisplay();
    }

    private void Update()
    {
        state.RemainingTimeSeconds -= Time.deltaTime;

        if (state.RemainingTimeSeconds <= 0f || state.Strength <= 0)
        {
            AdvanceRound();
        }

        UpdateDisplay();
    }

    private void AdvanceRound()
    {
        state.CurrRound++;
        state.RemainingTimeSeconds = roundDurationSeconds;
        state.Strength = MaximumStamina;
        RoundAdvanced?.Invoke();
    }

    public bool TryUseLiteratureSearch()
    {
        const int staminaCost = 10;
        const float timeCostSeconds = 60f;
        if (state.Strength < staminaCost || state.RemainingTimeSeconds < timeCostSeconds)
        {
            AdvanceRound();
            UpdateDisplay();
            return false;
        }

        state.Strength -= staminaCost;
        state.RemainingTimeSeconds -= timeCostSeconds;
        UpdateDisplay();
        return true;
    }

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
        {
            Debug.LogError("RoundTimer could not find a Text component for " + displayName + ".", this);
        }

        return text;
    }

    private void UpdateDisplay()
    {
        if (roundText != null)
        {
            roundText.text = "当前回合:" + state.CurrRound;
        }

        if (timeText != null)
        {
            int displayedSeconds = Mathf.CeilToInt(state.RemainingTimeSeconds);
            int minutes = displayedSeconds / 60;
            int seconds = displayedSeconds % 60;
            timeText.text = string.Format("时间 {0}:{1:00}", minutes, seconds);
        }

        if (staminaText != null)
        {
            staminaText.text = "体力:" + state.Strength;
        }

        if (inspirationText != null)
        {
            inspirationText.text = "灵感:" + state.Inspiration.ToString("0.0");
        }
    }
}
