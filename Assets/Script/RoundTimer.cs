using UnityEngine;
using UnityEngine.UI;

public class RoundTimer : MonoBehaviour
{
    [SerializeField] private int roundDurationSeconds = 8 * 60;

    private Text roundText;
    private Text timeText;
    private int currentRound = 1;
    private float remainingSeconds;

    private void Awake()
    {
        roundText = FindText("CurrRound");
        timeText = FindText("CurrTime");
        remainingSeconds = roundDurationSeconds;
        UpdateDisplay();
    }

    private void Update()
    {
        remainingSeconds -= Time.deltaTime;

        if (remainingSeconds <= 0f)
        {
            currentRound++;
            remainingSeconds = roundDurationSeconds;
        }

        UpdateDisplay();
    }

    private Text FindText(string panelName)
    {
        Transform panel = transform.Find(panelName);
        if (panel == null)
        {
            Debug.LogError("RoundTimer could not find " + panelName + " under " + name + ".", this);
            return null;
        }

        Text text = panel.GetComponentInChildren<Text>();
        if (text == null)
        {
            Debug.LogError("RoundTimer could not find a Text component under " + panelName + ".", this);
        }

        return text;
    }

    private void UpdateDisplay()
    {
        if (roundText != null)
        {
            roundText.text = "当前回合:" + currentRound;
        }

        if (timeText != null)
        {
            int displayedSeconds = Mathf.CeilToInt(remainingSeconds);
            int minutes = displayedSeconds / 60;
            int seconds = displayedSeconds % 60;
            timeText.text = string.Format("时间 {0}:{1:00}", minutes, seconds);
        }
    }
}
