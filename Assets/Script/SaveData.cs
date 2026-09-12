using System;
using System.Collections.Generic;

/// <summary>
/// JSON 存档的根数据结构。
/// 对应 Application.persistentDataPath/save.json
/// </summary>
[Serializable]
public class SaveData
{
    // ── 主状态 ────────────────────────────────────────────────────
    public int   currentRound          = 1;
    public float remainingTimeSeconds  = 0f;
    public int   stamina               = 100;
    public float inspiration           = 0f;

    // ── 论文列表 ──────────────────────────────────────────────────
    public List<PaperSaveData> papers  = new List<PaperSaveData>();
}

/// <summary>
/// 单篇论文的存档数据（对应 PaperInfo，去掉 ScriptableObject 依赖）。
/// </summary>
[Serializable]
public class PaperSaveData
{
    public string paperName;
    public float  logicDegree;
    public float  dataRigor;
    public float  viewpointInnovation;
    public float  complexity;
    public int    submissionStatus;      // SubmissionStatus 枚举转 int
    public string submissionLevel;
    public int    submissionRound;
    public int    expectedReviewRound;
    public int    remainingRounds;
    public float  submissionSuccessRate;
    public int    fusionCooldownRounds;
    public int    incorporatedViewpointCount;

    // ── 互转工具 ──────────────────────────────────────────────────
    public static PaperSaveData FromPaperInfo(PaperInfo info)
    {
        return new PaperSaveData
        {
            paperName                = info.paperName,
            logicDegree              = info.logicDegree,
            dataRigor                = info.dataRigor,
            viewpointInnovation      = info.viewpointInnovation,
            complexity               = info.complexity,
            submissionStatus         = (int)info.submissionStatus,
            submissionLevel          = info.submissionLevel,
            submissionRound          = info.submissionRound,
            expectedReviewRound      = info.expectedReviewRound,
            remainingRounds          = info.remainingRounds,
            submissionSuccessRate    = info.submissionSuccessRate,
            fusionCooldownRounds     = info.fusionCooldownRounds,
            incorporatedViewpointCount = info.incorporatedViewpointCount,
        };
    }

    public PaperInfo ToPaperInfo()
    {
        return new PaperInfo
        {
            paperName                = paperName,
            logicDegree              = logicDegree,
            dataRigor                = dataRigor,
            viewpointInnovation      = viewpointInnovation,
            complexity               = complexity,
            submissionStatus         = (SubmissionStatus)submissionStatus,
            submissionLevel          = submissionLevel,
            submissionRound          = submissionRound,
            expectedReviewRound      = expectedReviewRound,
            remainingRounds          = remainingRounds,
            submissionSuccessRate    = submissionSuccessRate,
            fusionCooldownRounds     = fusionCooldownRounds,
            incorporatedViewpointCount = incorporatedViewpointCount,
        };
    }
}
