using System;
using UnityEngine;

/// <summary>
/// Stores the gameplay data associated with a paper.
/// Paper attributes and submission success rate use a 0-100 range.
/// </summary>
[Serializable]
public class PaperInfo
{
    public string paperName;

    [Range(0f, 100f)]
    public float logicDegree;

    [Range(0f, 100f)]
    public float dataRigor;

    [Range(0f, 100f)]
    public float viewpointInnovation;

    [Range(0f, 100f)]
    public float complexity;

    public SubmissionStatus submissionStatus;

    public string submissionLevel;

    [Min(0)]
    public int submissionRound;

    [Min(0)]
    public int expectedReviewRound;

    [Min(0)]
    public int remainingRounds;

    [Range(0f, 100f)]
    public float submissionSuccessRate;

    [Min(0)]
    public int fusionCooldownRounds;

    [Min(0)]
    public int incorporatedViewpointCount;
}

public enum SubmissionStatus
{
    NotSubmitted,
    Submitted,
    Accepted,
    Rejected,
    ConstructionCompleted
}
