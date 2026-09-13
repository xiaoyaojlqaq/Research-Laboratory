using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PaperWorkspaceNavigator : MonoBehaviour
{
    private GameObject chooseCoreArgument;
    private GameObject chooseArgument;
    private GameObject writeThesisMain;
    private GameObject submitApplication;
    private GameObject continueArgument;
    private GameObject argumentState;
    private GameObject peerReview;
    private Transform peerReviewContent;
    private GameObject reviewArgumentPrefab;
    private RoundTimer roundTimer;
    private Button incorporateViewpointButton;
    private Button literatureSearchButton;
    private Button finalizeFrameworkButton;
    private Button submitButton;
    private Transform submissionChooseRoot;
    private SubmissionOptionScriptableObject selectedSubmissionOption;
    private Text incorporationCooldownText;
    private readonly Dictionary<Button, Color> submissionNormalColors = new Dictionary<Button, Color>();
    [SerializeField] private GameObject argumentPrefab;
    [SerializeField] private Transform argumentList;
    [SerializeField] private Transform continueArgumentList;
    [SerializeField] private List<SubmissionOptionScriptableObject> submissionOptions = new List<SubmissionOptionScriptableObject>();
    public readonly List<PaperInfoScriptableObject> createdPapers = new List<PaperInfoScriptableObject>();
    public PaperInfoScriptableObject currentResearchPaper;

    public AllLefButton allLefButton;

    private void Awake()
    {
        Transform canvas = transform.parent;
        chooseCoreArgument = FindPanel(canvas, "ChooseCoreArgument");
        chooseArgument = FindPanel(canvas, "chooseArgument");
        writeThesisMain = FindPanel(canvas, "WriteThesisMain");
        submitApplication = FindPanel(canvas, "SubmitApplication");
        continueArgument = FindPanel(canvas, "ContinueArgument");
        argumentState = FindPanel(canvas, "ArgumentState");
        peerReview = FindPanel(canvas, "peerReview");
        Transform peerReviewViewport = FindChild(peerReview == null ? null : peerReview.transform, "Viewport");
        peerReviewContent = FindChild(peerReviewViewport, "Content");
        reviewArgumentPrefab = LoadReviewArgumentPrefab();
        roundTimer = FindObjectOfType<RoundTimer>();
        Transform incorporateButton = FindChild(writeThesisMain == null ? null : writeThesisMain.transform, "IncorporateTheViewpoint");
        incorporateViewpointButton = incorporateButton == null ? null : incorporateButton.GetComponent<Button>();
        Transform literatureButton = FindChild(writeThesisMain == null ? null : writeThesisMain.transform, "LiteratureSearch");
        literatureSearchButton = literatureButton == null ? null : literatureButton.GetComponent<Button>();
        Transform finalizeButton = FindChild(writeThesisMain == null ? null : writeThesisMain.transform, "FinalizeTheFramework");
        finalizeFrameworkButton = finalizeButton == null ? null : finalizeButton.GetComponent<Button>();
        submissionChooseRoot = FindChild(submitApplication == null ? null : submitApplication.transform, "choose");
        Transform submitButtonTransform = FindChild(submitApplication == null ? null : submitApplication.transform, "SubmitButton (Legacy)");
        submitButton = submitButtonTransform == null ? null : submitButtonTransform.GetComponent<Button>();
        Transform cooldownTextTransform = FindChild(incorporateButton, "Text (Legacy) (6)");
        incorporationCooldownText = cooldownTextTransform == null ? null : cooldownTextTransform.GetComponent<Text>();
        if (roundTimer != null)
        {
            roundTimer.RoundAdvanced += RefreshCoreArgumentOptions;
            roundTimer.RoundAdvanced += AdvancePaperFusionCooldowns;
            roundTimer.RoundAdvanced += RefreshSubmissionControls;
            roundTimer.RoundAdvanced += OnRoundAdvancedSavePapers;
        }
        argumentList = ResolveArgumentList(submitApplication == null ? null : submitApplication.transform);
        continueArgumentList = ResolveArgumentList(continueArgument == null ? null : continueArgument.transform);
        argumentPrefab = argumentPrefab == null ? LoadArgumentPrefab() : argumentPrefab;

        AddListener("CoreArgument", ShowChooseArgument);
        AddListener(chooseArgument == null ? null : chooseArgument.transform, "NewButton", ShowChooseCoreArgument);
        AddPaperCreationListener("Choose1");
        AddPaperCreationListener("Choose2");
        AddPaperCreationListener("Choose3");
        AddPaperCreationListener("Choose4");
        AddListener(chooseArgument == null ? null : chooseArgument.transform, "continueButton", ShowContinueArgument);
        AddListener("WriteThesis", ShowWriteThesis);
        AddListener(writeThesisMain == null ? null : writeThesisMain.transform, "LiteratureSearch", UseLiteratureSearch);
        AddListener(writeThesisMain == null ? null : writeThesisMain.transform, "IncorporateTheViewpoint", IncorporateViewpoint);
        AddListener(writeThesisMain == null ? null : writeThesisMain.transform, "FinalizeTheFramework", FinalizeFramework);
        BindSubmissionOptions();
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitCurrentPaper);
        }
        AddListener("SubmitApplication", ShowSubmitApplication);
        AddListener("PeerReview", ShowPeerReview);
    }

    private void Start()
    {
        SetActive(chooseCoreArgument, false);
        SetActive(chooseArgument, false);
        SetActive(writeThesisMain, false);
        SetActive(submitApplication, false);
        SetActive(continueArgument, false);
        SetActive(peerReview, false);

        // ── 从存档恢复论文列表 ──────────────────────────────────────
        if (SaveManager.Instance != null && SaveManager.Instance.Data != null
            && SaveManager.Instance.Data.papers != null
            && SaveManager.Instance.Data.papers.Count > 0)
        {
            RestorePapersFromSave(SaveManager.Instance.Data.papers);
        }

        RefreshCoreArgumentOptions();
        RefreshIncorporateViewpointButton();
        RefreshLiteratureSearchButton();
        RefreshFinalizeFrameworkButton();
        RefreshSubmissionControls();
    }

    private void Update()
    {
        RefreshIncorporateViewpointButton();
        RefreshLiteratureSearchButton();
        RefreshFinalizeFrameworkButton();
        RefreshSubmissionControls();
    }

    private void OnDestroy()
    {
        if (roundTimer != null)
        {
            roundTimer.RoundAdvanced -= RefreshCoreArgumentOptions;
            roundTimer.RoundAdvanced -= AdvancePaperFusionCooldowns;
            roundTimer.RoundAdvanced -= RefreshSubmissionControls;
            roundTimer.RoundAdvanced -= OnRoundAdvancedSavePapers;
        }
    }

    // ── 存档：每次回合推进时保存论文 ──────────────────────────────
    private void OnRoundAdvancedSavePapers()
    {
        SavePapers();
    }

    private void SavePapers()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SavePapers(createdPapers);
    }

    // ── 存档：从 SaveData.papers 恢复论文 ────────────────────────
    private void RestorePapersFromSave(List<PaperSaveData> savedPapers)
    {
        createdPapers.Clear();
        currentResearchPaper = null;

        foreach (PaperSaveData sd in savedPapers)
        {
            if (sd == null) continue;
            PaperInfoScriptableObject paper = ScriptableObject.CreateInstance<PaperInfoScriptableObject>();
            paper.paperInfo = sd.ToPaperInfo();
            createdPapers.Add(paper);
        }

        foreach (PaperInfoScriptableObject p in createdPapers)
        {
            if (p.paperInfo.submissionStatus != SubmissionStatus.Accepted
                && p.paperInfo.submissionStatus != SubmissionStatus.Submitted)
            {
                currentResearchPaper = p;
                break;
            }
        }

        if (currentResearchPaper == null && createdPapers.Count > 0)
            currentResearchPaper = createdPapers[createdPapers.Count - 1];

        RefreshArgumentLists();
        UpdateArgumentState();
        Debug.Log("[PaperWorkspaceNavigator] Restored " + createdPapers.Count + " papers from save.");
    }

    private void AdvancePaperFusionCooldowns()
    {
        foreach (PaperInfoScriptableObject paper in createdPapers)
        {
            if (paper == null || paper.paperInfo == null) continue;
            paper.paperInfo.fusionCooldownRounds = Mathf.Max(0, paper.paperInfo.fusionCooldownRounds - 1);
            if (paper.paperInfo.submissionStatus == SubmissionStatus.Submitted)
                paper.paperInfo.remainingRounds = Mathf.Max(0, paper.paperInfo.remainingRounds - 1);
        }
        RefreshArgumentLists();
        RefreshIncorporateViewpointButton();
        RefreshLiteratureSearchButton();
        RefreshFinalizeFrameworkButton();
        RefreshSubmissionControls();
        if (peerReview != null && peerReview.activeSelf)
            RefreshPeerReviewList();
    }

    private void ShowChooseArgument()    { ShowOnly(chooseArgument, false, 0); }
    private void ShowChooseCoreArgument(){ ShowOnly(chooseCoreArgument, true, 0); }

    private void RefreshCoreArgumentOptions()
    {
        List<int> viewpoints = new List<int>();
        for (int i = 1; i <= 20; i++) viewpoints.Add(i);
        for (int i = viewpoints.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            int temp = viewpoints[i]; viewpoints[i] = viewpoints[swapIndex]; viewpoints[swapIndex] = temp;
        }
        for (int i = 0; i < 4; i++)
        {
            Transform button = FindChild(chooseCoreArgument == null ? null : chooseCoreArgument.transform, "Choose" + (i + 1));
            Text optionText = button == null ? null : button.GetComponentInChildren<Text>(true);
            if (optionText != null) optionText.text = "观点" + viewpoints[i];
        }
    }

    private void ShowWriteThesis()
    {
        ShowOnly(writeThesisMain, true, 1);
        RefreshIncorporateViewpointButton();
        RefreshLiteratureSearchButton();
        RefreshFinalizeFrameworkButton();
    }

    private void RefreshIncorporateViewpointButton()
    {
        if (incorporateViewpointButton == null) return;
        bool canUse = CanIncorporateViewpoint();
        incorporateViewpointButton.interactable = canUse;
        if (incorporationCooldownText != null)
        {
            float inspiration = roundTimer != null && roundTimer.State != null ? roundTimer.State.Inspiration : 0f;
            int fusionCooldown = currentResearchPaper != null && currentResearchPaper.paperInfo != null
                ? currentResearchPaper.paperInfo.fusionCooldownRounds : 0;
            incorporationCooldownText.text = inspiration < 1f ? "灵感不足" : "融合冷却回合：" + fusionCooldown;
            incorporationCooldownText.gameObject.SetActive(!canUse);
        }
        ColorBlock colors = incorporateViewpointButton.colors;
        colors.disabledColor = Color.gray;
        incorporateViewpointButton.colors = colors;
    }

    private bool CanIncorporateViewpoint()
    {
        return roundTimer != null && roundTimer.State != null &&
               roundTimer.State.Inspiration >= 1f &&
               currentResearchPaper != null && currentResearchPaper.paperInfo != null &&
               currentResearchPaper.paperInfo.submissionStatus != SubmissionStatus.Submitted &&
               currentResearchPaper.paperInfo.fusionCooldownRounds == 0;
    }

    private void IncorporateViewpoint()
    {
        if (!CanIncorporateViewpoint()) { RefreshIncorporateViewpointButton(); return; }
        PaperInfo info = currentResearchPaper.paperInfo;
        info.logicDegree = Mathf.Min(100f, info.logicDegree + 2f);
        info.dataRigor = Mathf.Min(100f, info.dataRigor + 2f);
        info.viewpointInnovation = Mathf.Min(100f, info.viewpointInnovation + 1f);
        info.complexity = Mathf.Min(100f, info.complexity + Random.Range(0f, 10f));
        info.fusionCooldownRounds += 2;
        info.incorporatedViewpointCount++;
        roundTimer.State.Inspiration -= 1f;
        UpdateArgumentState();
        RefreshArgumentLists();
        RefreshIncorporateViewpointButton();
        RefreshFinalizeFrameworkButton();
        SavePapers();
    }

    private void RefreshFinalizeFrameworkButton()
    {
        if (finalizeFrameworkButton == null) return;
        bool canFinalize = currentResearchPaper != null && currentResearchPaper.paperInfo != null &&
                           currentResearchPaper.paperInfo.submissionStatus != SubmissionStatus.Submitted &&
                           currentResearchPaper.paperInfo.incorporatedViewpointCount >= 1 &&
                           currentResearchPaper.paperInfo.fusionCooldownRounds == 0;
        finalizeFrameworkButton.interactable = canFinalize;
        ColorBlock colors = finalizeFrameworkButton.colors;
        colors.disabledColor = Color.gray;
        finalizeFrameworkButton.colors = colors;
    }

    private void UseLiteratureSearch()
    {
        if (roundTimer == null || currentResearchPaper == null || currentResearchPaper.paperInfo == null) return;
        if (currentResearchPaper.paperInfo.submissionStatus == SubmissionStatus.Submitted) { RefreshLiteratureSearchButton(); return; }
        if (!roundTimer.TryUseLiteratureSearch()) return;
        PaperInfo info = currentResearchPaper.paperInfo;
        info.logicDegree = Mathf.Min(100f, info.logicDegree + 2f);
        info.dataRigor = Mathf.Min(100f, info.dataRigor + 2f);
        info.viewpointInnovation = Mathf.Min(100f, info.viewpointInnovation + 1f);
        roundTimer.State.Inspiration += 0.4f;
        UpdateArgumentState();
        RefreshArgumentLists();
        SavePapers();
    }

    private void RefreshLiteratureSearchButton()
    {
        if (literatureSearchButton == null) return;
        bool canUse = currentResearchPaper != null && currentResearchPaper.paperInfo != null &&
                      currentResearchPaper.paperInfo.submissionStatus != SubmissionStatus.Submitted;
        literatureSearchButton.interactable = canUse;
        ColorBlock colors = literatureSearchButton.colors;
        colors.disabledColor = Color.gray;
        literatureSearchButton.colors = colors;
    }

    private void ShowSubmitApplication()
    {
        RefreshArgumentLists();
        ShowOnly(submitApplication, false, 2);
        RefreshSubmissionControls();
    }

    private void BindSubmissionOptions()
    {
        if (submissionChooseRoot == null) return;
        for (int i = 0; i < submissionChooseRoot.childCount; i++)
        {
            int optionIndex = i;
            Transform btnTransform = submissionChooseRoot.GetChild(i);
            Button button = btnTransform.GetComponent<Button>();
            if (button == null) continue;
            submissionNormalColors[button] = button.colors.normalColor;
            button.onClick.AddListener(() => SelectSubmissionOption(optionIndex));
        }
    }

    private void SelectSubmissionOption(int index)
    {
        if (index < 0 || index >= submissionOptions.Count) return;
        selectedSubmissionOption = submissionOptions[index];
        RefreshSubmissionControls();
    }

    private void SubmitCurrentPaper()
    {
        if (currentResearchPaper == null || currentResearchPaper.paperInfo == null) return;
        if (selectedSubmissionOption == null || !IsSubmissionOptionAvailable(selectedSubmissionOption)) return;
        PaperInfo info = currentResearchPaper.paperInfo;
        info.submissionStatus = SubmissionStatus.Submitted;
        info.submissionLevel = selectedSubmissionOption.journalLevel;
        info.submissionRound = roundTimer != null && roundTimer.State != null ? roundTimer.State.CurrRound : 0;
        info.expectedReviewRound = info.submissionRound + selectedSubmissionOption.reviewDurationRounds;
        info.remainingRounds = selectedSubmissionOption.reviewDurationRounds;
        UpdateArgumentState();
        RefreshArgumentLists();
        RefreshSubmissionControls();
        SavePapers();
    }

    private bool IsSubmissionOptionAvailable(SubmissionOptionScriptableObject option)
    {
        if (option == null || currentResearchPaper == null || currentResearchPaper.paperInfo == null
            || roundTimer == null || roundTimer.State == null) return false;
        PaperInfo info = currentResearchPaper.paperInfo;
        int round = roundTimer.State.CurrRound;
        bool canSubmit = info.submissionStatus == SubmissionStatus.ConstructionCompleted ||
                         info.submissionStatus == SubmissionStatus.Rejected;
        return canSubmit && round >= option.openRound && round < option.deadlineRound &&
               info.logicDegree >= option.minimumLogic &&
               info.dataRigor >= option.minimumRigor &&
               info.viewpointInnovation >= option.minimumInnovation;
    }

    private void RefreshSubmissionControls()
    {
        if (submissionChooseRoot == null && submitApplication != null)
            submissionChooseRoot = FindChild(submitApplication.transform, "choose");
        if (submissionChooseRoot != null)
        {
            for (int i = 0; i < submissionChooseRoot.childCount; i++)
            {
                Transform btnTransform = submissionChooseRoot.GetChild(i);
                Button button = btnTransform.GetComponent<Button>();
                if (button == null) continue;
                SubmissionOptionScriptableObject option = i < submissionOptions.Count ? submissionOptions[i] : null;
                bool available = IsSubmissionOptionAvailable(option);
                button.interactable = available;
                ColorBlock colors = button.colors;
                Color baseColor = submissionNormalColors.TryGetValue(button, out Color originalColor) ? originalColor : Color.white;
                colors.normalColor = option != null && option == selectedSubmissionOption
                    ? Color.Lerp(baseColor, Color.yellow, 0.5f) : baseColor;
                colors.disabledColor = Color.gray;
                button.colors = colors;
            }
        }
        if (submitButton != null)
            submitButton.interactable = selectedSubmissionOption != null && IsSubmissionOptionAvailable(selectedSubmissionOption);
    }

    private void FinalizeFramework()
    {
        if (currentResearchPaper == null || currentResearchPaper.paperInfo == null) return;
        if (currentResearchPaper.paperInfo.submissionStatus == SubmissionStatus.Submitted) { RefreshFinalizeFrameworkButton(); return; }
        currentResearchPaper.paperInfo.submissionStatus = SubmissionStatus.ConstructionCompleted;
        UpdateArgumentState();
        ShowSubmitApplication();
        SavePapers();
    }

    private void ShowContinueArgument()
    {
        RefreshArgumentLists();
        ShowOnly(continueArgument, false, 0);
    }

    private void AddPaperCreationListener(string buttonName)
    {
        Transform buttonTransform = FindChild(chooseCoreArgument == null ? null : chooseCoreArgument.transform, buttonName);
        Button button = buttonTransform == null ? null : buttonTransform.GetComponent<Button>();
        if (button == null) { Debug.LogError("PaperWorkspaceNavigator could not find button " + buttonName + ".", this); return; }
        button.onClick.AddListener(() => CreatePaper(buttonTransform));
    }

    private void CreatePaper(Transform buttonTransform)
    {
        Text paperNameText = buttonTransform.GetComponentInChildren<Text>(true);
        string baseName = paperNameText == null ? string.Empty : paperNameText.text;
        PaperInfoScriptableObject paper = ScriptableObject.CreateInstance<PaperInfoScriptableObject>();
        paper.paperInfo = new PaperInfo { paperName = GetUniquePaperName(baseName) };
        createdPapers.Add(paper);
        currentResearchPaper = paper;
        selectedSubmissionOption = null;
        UpdateArgumentState();
        RefreshArgumentLists();
        RefreshFinalizeFrameworkButton();
        ShowWriteThesis();
        SavePapers();
    }

    private void RefreshArgumentLists()
    {
        RefreshArgumentList(argumentList, "SubmitApplication");
        RefreshArgumentList(continueArgumentList, "ContinueArgument");
    }

    private void RefreshArgumentList(Transform list, string panelName)
    {
        if (list == null) { Debug.LogError("PaperWorkspaceNavigator could not find " + panelName + " content.", this); return; }
        for (int i = list.childCount - 1; i >= 0; i--) Destroy(list.GetChild(i).gameObject);
        if (argumentPrefab == null) { Debug.LogError("PaperWorkspaceNavigator: missing Argument prefab.", this); return; }
        foreach (PaperInfoScriptableObject paper in createdPapers)
        {
            if (paper == null || paper.paperInfo == null) continue;
            GameObject argument = Instantiate(argumentPrefab, list);
            argument.name = "Argument - " + paper.paperInfo.paperName;
            UpdateArgumentEntry(argument.transform, paper);
            BindArgumentSelection(argument, paper, panelName == "ContinueArgument");
        }
    }

    private void BindArgumentSelection(GameObject argument, PaperInfoScriptableObject paper, bool openWriteThesis)
    {
        Button button = argument.GetComponent<Button>();
        if (button == null) { Debug.LogError("PaperWorkspaceNavigator: Argument.prefab has no Button.", argument); return; }
        SubmissionStatus status = paper.paperInfo.submissionStatus;
        button.interactable = status != SubmissionStatus.Submitted && status != SubmissionStatus.Accepted;
        ColorBlock colors = button.colors;
        colors.disabledColor = Color.gray;
        button.colors = colors;
        button.onClick.AddListener(() => SelectResearchPaper(paper, openWriteThesis));
    }

    private void SelectResearchPaper(PaperInfoScriptableObject paper, bool openWriteThesis = false)
    {
        if (paper == null || paper.paperInfo == null) return;
        currentResearchPaper = paper;
        selectedSubmissionOption = null;
        UpdateArgumentState();
        RefreshArgumentLists();
        RefreshSubmissionControls();
        if (openWriteThesis) ShowWriteThesis();
    }

    private void UpdateArgumentEntry(Transform entry, PaperInfoScriptableObject paper)
    {
        PaperInfo info = paper.paperInfo;
        UpdateSubmissionSuccessRate(info);
        string displayName = info.paperName;
        if (paper == currentResearchPaper) displayName += " 已选择";
        SetText(entry, "NameText", displayName);
        SetText(entry, "stateText", GetSubmissionStatusText(info.submissionStatus));
        SetEntryMetric(entry, "LogicalCoherence", info.logicDegree);
        SetEntryMetric(entry, "DataRigor", info.dataRigor);
        SetEntryMetric(entry, "Innovativeness", info.viewpointInnovation);
        SetEntryMetric(entry, "AcceptanceRate", info.submissionSuccessRate);
    }

    private void UpdateSubmissionSuccessRate(PaperInfo info)
    {
        info.logicDegree = Mathf.Clamp(info.logicDegree, 0f, 100f);
        info.dataRigor = Mathf.Clamp(info.dataRigor, 0f, 100f);
        info.viewpointInnovation = Mathf.Clamp(info.viewpointInnovation, 0f, 100f);
        info.complexity = Mathf.Clamp(info.complexity, 0f, 100f);
        float average = Mathf.Floor((info.logicDegree + info.dataRigor + info.viewpointInnovation) / 3f);
        info.submissionSuccessRate = Mathf.Clamp(average - info.complexity, 0f, 100f);
    }

    private string GetSubmissionStatusText(SubmissionStatus status)
    {
        switch (status)
        {
            case SubmissionStatus.ConstructionCompleted: return "构筑完成";
            case SubmissionStatus.Submitted:             return "已投稿";
            case SubmissionStatus.Accepted:              return "已接受";
            case SubmissionStatus.Rejected:              return "已拒稿";
            default:                                     return "未投稿";
        }
    }

    private void SetEntryMetric(Transform entry, string metricName, float value)
    {
        Transform metric = FindChild(entry, metricName);
        Transform fillTransform = FindChild(metric, "Image (1)");
        RectTransform rectTransform = metric.GetComponent<RectTransform>();
        rectTransform.DOShakeScale(0.5f, 0.03f, 10, 90f, false);
        Image fillImage = fillTransform == null ? null : fillTransform.GetComponent<Image>();
        if (fillImage != null) fillImage.fillAmount = Mathf.Clamp01(value / 100f);
        Transform valueTextTransform = FindChild(fillTransform, "Text (Legacy)");
        Text valueText = valueTextTransform == null ? null : valueTextTransform.GetComponent<Text>();
        if (valueText != null) valueText.text = value.ToString("0.#");
    }

    private GameObject LoadArgumentPrefab()
    {
        GameObject loaded = Resources.Load<GameObject>("Prefab/Argument");
        if (loaded != null) return loaded;
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Argument.prefab");
#else
        return null;
#endif
    }

    private string GetUniquePaperName(string baseName)
    {
        int count = 1; string candidate = baseName;
        while (IsPaperNameTaken(candidate)) { candidate = baseName + " (" + count + ")"; count++; }
        return candidate;
    }

    private bool IsPaperNameTaken(string name)
    {
        foreach (PaperInfoScriptableObject paper in createdPapers)
            if (paper != null && paper.paperInfo != null && paper.paperInfo.paperName == name) return true;
        return false;
    }

    private Transform ResolveArgumentList(Transform panelTransform)
    {
        if (panelTransform == null) return null;
        Transform bg = FindChild(panelTransform, "BGArgumentList");
        Transform scroll = FindChild(bg, "ArgumentList");
        Transform viewport = FindChild(scroll, "Viewport");
        return FindChild(viewport, "Content");
    }

    private void UpdateArgumentState()
    {
        if (argumentState == null || currentResearchPaper == null || currentResearchPaper.paperInfo == null) return;
        PaperInfo info = currentResearchPaper.paperInfo;
        UpdateSubmissionSuccessRate(info);
        SetText(argumentState.transform, "NameText", info.paperName);
        SetText(argumentState.transform, "stateText", GetSubmissionStatusText(info.submissionStatus));
        SetEntryMetric(argumentState.transform, "LogicalCoherence", info.logicDegree);
        SetEntryMetric(argumentState.transform, "DataRigor", info.dataRigor);
        SetEntryMetric(argumentState.transform, "Innovativeness", info.viewpointInnovation);
        SetEntryMetric(argumentState.transform, "AcceptanceRate", info.submissionSuccessRate);
    }

    private void SetText(Transform root, string childName, string value)
    {
        Transform child = FindChild(root, childName);
        Text text = child == null ? null : child.GetComponent<Text>();
        if (text != null) text.text = value;
    }

    private void ShowOnly(GameObject target, bool showArgumentState, int allLefIndex)
    {
        SetActive(chooseCoreArgument, false);
        SetActive(chooseArgument, false);
        SetActive(writeThesisMain, false);
        SetActive(submitApplication, false);
        SetActive(continueArgument, false);
        SetActive(peerReview, false);
        SetActive(argumentState, showArgumentState);
        SetActive(target, true);
        if (allLefButton != null) allLefButton.LefButtonChoose(allLefIndex);
    }

    private void ShowPeerReview()
    {
        ShowOnly(peerReview, false, 3);
        RefreshPeerReviewList();
    }

    private void RefreshPeerReviewList()
    {
        if (peerReviewContent == null || reviewArgumentPrefab == null) return;
        for (int i = peerReviewContent.childCount - 1; i >= 0; i--)
            Destroy(peerReviewContent.GetChild(i).gameObject);
        foreach (PaperInfoScriptableObject paper in createdPapers)
        {
            if (paper == null || paper.paperInfo == null) continue;
            SubmissionStatus status = paper.paperInfo.submissionStatus;
            if (status == SubmissionStatus.NotSubmitted ||
                status == SubmissionStatus.ConstructionCompleted ||
                status == SubmissionStatus.Rejected) continue;
            GameObject entry = Instantiate(reviewArgumentPrefab, peerReviewContent);
            UpdateReviewArgumentEntry(entry.transform, paper.paperInfo);
            Button btn = entry.GetComponent<Button>();
            if (btn == null) continue;
            ColorBlock colors = btn.colors;
            colors.disabledColor = Color.gray;
            btn.colors = colors;
            if (status == SubmissionStatus.Accepted)
            {
                btn.interactable = false;
            }
            else if (status == SubmissionStatus.Submitted && paper.paperInfo.remainingRounds <= 0)
            {
                btn.interactable = true;
                PaperInfoScriptableObject captured = paper;
                btn.onClick.AddListener(() => ResolveReview(captured));
            }
            else
            {
                btn.interactable = false;
            }
        }
    }

    private void ResolveReview(PaperInfoScriptableObject paper)
    {
        if (paper == null || paper.paperInfo == null) return;
        if (paper.paperInfo.submissionStatus != SubmissionStatus.Submitted) return;
        UpdateSubmissionSuccessRate(paper.paperInfo);
        float roll = Random.Range(0f, 100f);
        paper.paperInfo.submissionStatus = roll < paper.paperInfo.submissionSuccessRate
            ? SubmissionStatus.Accepted : SubmissionStatus.Rejected;
        RefreshArgumentLists();
        RefreshPeerReviewList();
        UpdateArgumentState();
        SavePapers();
    }

    private void UpdateReviewArgumentEntry(Transform entry, PaperInfo info)
    {
        SetText(entry, "NameText", info.paperName);
        SetText(entry, "stateText", GetSubmissionStatusText(info.submissionStatus));
        SetText(entry, "TargetJournal", "投稿期刊 " + info.submissionLevel);
        SetText(entry, "ReviewRound", "投稿回合 " + info.submissionRound);
        SetText(entry, "ExpectedNumberofRounds", "预计审回回合 " + info.expectedReviewRound);
        SetText(entry, "RemainingRounds", "剩余回合 " + info.remainingRounds);
    }

    private GameObject LoadReviewArgumentPrefab()
    {
        GameObject loaded = Resources.Load<GameObject>("Prefab/ReviewArgument");
        if (loaded != null) return loaded;
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/ReviewArgument.prefab");
#else
        return null;
#endif
    }

    private void AddListener(string buttonName, UnityEngine.Events.UnityAction action)
    {
        AddListener(transform, buttonName, action);
    }

    private void AddListener(Transform root, string buttonName, UnityEngine.Events.UnityAction action)
    {
        Transform buttonTransform = FindChild(root, buttonName);
        Button button = buttonTransform == null ? null : buttonTransform.GetComponent<Button>();
        if (button == null) { Debug.LogError("PaperWorkspaceNavigator could not find button " + buttonName + ".", this); return; }
        button.onClick.AddListener(action);
    }

    private Transform FindChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName)) return null;
        foreach (Transform child in root)
        {
            if (child.name == childName) return child;
            Transform nested = FindChild(child, childName);
            if (nested != null) return nested;
        }
        return null;
    }

    private GameObject FindPanel(Transform canvas, string panelName)
    {
        Transform panel = canvas.Find(panelName);
        if (panel == null) { Debug.LogError("PaperWorkspaceNavigator could not find panel " + panelName + ".", this); return null; }
        return panel.gameObject;
    }

    private void SetActive(GameObject panel, bool isActive)
    {
        if (panel != null) panel.SetActive(isActive);
    }
}

