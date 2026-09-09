using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SubmissionOption", menuName = "Research Laboratory/Submission Option")]
public class SubmissionOptionScriptableObject : ScriptableObject
{
    public string journalLevel;
    public int openRound;
    public int deadlineRound;
    public int reviewDurationRounds;
    [Range(0f, 100f)] public float successRate;
    public float minimumLogic;
    public float minimumRigor;
    public float minimumInnovation;
    public List<string> sourceTexts = new List<string>();
}
