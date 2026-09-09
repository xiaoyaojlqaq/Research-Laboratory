using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    private void Awake()
    {
        Transform canvas = GetComponentInParent<Canvas>().transform;
        chooseCoreArgument = FindPanel(canvas, "ChooseCoreArgument");
        chooseArgument = FindPanel(canvas, "chooseArgument");
        writeThesisMain = FindPanel(canvas, "WriteThesisMain");
        submitApplication = FindPanel(canvas, "SubmitApplication");
        continueArgument = FindPanel(canvas, "ContinueArgument");
        argumentState = FindPanel(canvas, "ArgumentState");
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
    }

    private void Start()
    {
        SetActive(chooseCoreArgument, false);
        SetActive(chooseArgument, false);
        SetActive(writeThesisMain, false);
        SetActive(submitApplication, false);
        SetActive(continueArgument, false);
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
        }
    }

    private void AdvancePaperFusionCooldowns()
    {
        foreach (PaperInfoScriptableObject paper in createdPapers)
        {
            if (paper != null && paper.paperInfo != null)
            {
                paper.paperInfo.fusionCooldownRounds = Mathf.Max(0, paper.paperInfo.fusionCooldownRounds - 1);
            }
        }

        RefreshArgumentLists();
        RefreshIncorporateViewpointButton();
        RefreshLiteratureSearchButton();
        RefreshFinalizeFrameworkButton();
        RefreshSubmissionControls();
    }

    private void ShowChooseArgument()
    {
        ShowOnly(chooseArgument, false);
    }

    private void ShowChooseCoreArgument()
    {
        ShowOnly(chooseCoreArgument, true);
    }

    private void RefreshCoreArgumentOptions()
    {
        List<int> viewpoints = new List<int>();
        for (int i = 1; i <= 20; i++)
        {
            viewpoints.Add(i);
        }

        for (int i = viewpoints.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            int temp = viewpoints[i];
            viewpoints[i] = viewpoints[swapIndex];
            viewpoints[swapIndex] = temp;
        }

        for (int i = 0; i < 4; i++)
        {
            Transform button = FindChild(chooseCoreArgument == null ? null : chooseCoreArgument.transform, "Choose" + (i + 1));
            Text optionText = button == null ? null : button.GetComponentInChildren<Text>(true);
            if (optionText != null)
            {
                optionText.text = "观点" + viewpoints[i];
            }
        }
    }

    private void ShowWriteThesis()
    {
        ShowOnly(writeThesisMain, true);
        RefreshIncorporateViewpointButton();
        RefreshLiteratureSearchButton();
        RefreshFinalizeFrameworkButton();
    }

    private void RefreshIncorporateViewpointButton()
    {
        if (incorporateViewpointButton == null)
        {
            return;
        }

        bool canUse = CanIncorporateViewpoint();
        incorporateViewpointButton.interactable = canUse;

        if (incorporationCooldownText != null)
        {
            bool hasCurrentPaper = currentResearchPaper != null && currentResearchPaper.paperInfo != null;
            float inspiration = roundTimer != null && roundTimer.State != null
                ? roundTimer.State.Inspiration
                : 0f;
            int fusionCooldown = hasCurrentPaper ? currentResearchPaper.paperInfo.fusionCooldownRounds : 0;
            if (inspiration < 1f)
            {
                incorporationCooldownText.text = "灵感不足";
            }
            else
            {
                incorporationCooldownText.text = "融合冷却回合：" + fusionCooldown;
            }
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
        if (!CanIncorporateViewpoint())
        {
            RefreshIncorporateViewpointButton();
            return;
        }

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
    }

    private void RefreshFinalizeFrameworkButton()
    {
        if (finalizeFrameworkButton == null)
        {
            return;
        }

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
        if (roundTimer == null || currentResearchPaper == null || currentResearchPaper.paperInfo == null)
        {
            return;
        }

        if (currentResearchPaper.paperInfo.submissionStatus == SubmissionStatus.Submitted)
        {
            RefreshLiteratureSearchButton();
            return;
        }

        if (!roundTimer.TryUseLiteratureSearch())
        {
            return;
        }

        PaperInfo info = currentResearchPaper.paperInfo;
        info.logicDegree += 2f;
        info.dataRigor += 2f;
        info.viewpointInnovation += 1f;
        roundTimer.State.Inspiration += 0.4f;
        UpdateArgumentState();
        RefreshArgumentLists();
    }

    private void RefreshLiteratureSearchButton()
    {
        if (literatureSearchButton == null)
        {
            return;
        }

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
        ShowOnly(submitApplication, false);
    }

    private void BindSubmissionOptions()
    {
        if (submissionChooseRoot == null)
        {
            return;
        }

        for (int i = 0; i < submissionChooseRoot.childCount; i++)
        {
            int optionIndex = i;
            Button button = submissionChooseRoot.GetChild(i).GetComponent<Button>();
            if (button != null)
            {
                if (!submissionNormalColors.ContainsKey(button))
                {
                    submissionNormalColors.Add(button, button.colors.normalColor);
                }
                button.onClick.AddListener(() => SelectSubmissionOption(optionIndex));
            }
        }
    }

    private void SelectSubmissionOption(int optionIndex)
    {
        RefreshSubmissionControls();
        if (optionIndex < 0 || optionIndex >= submissionOptions.Count || !IsSubmissionOptionAvailable(submissionOptions[optionIndex]))
        {
            return;
        }

        selectedSubmissionOption = submissionOptions[optionIndex];
        RefreshSubmissionControls();
    }

    private void SubmitCurrentPaper()
    {
        if (currentResearchPaper == null || currentResearchPaper.paperInfo == null || selectedSubmissionOption == null ||
            !IsSubmissionOptionAvailable(selectedSubmissionOption))
        {
            return;
        }

        PaperInfo info = currentResearchPaper.paperInfo;
        info.submissionStatus = SubmissionStatus.Submitted;
        info.submissionLevel = selectedSubmissionOption.journalLevel;
        info.submissionRound = roundTimer.State.CurrRound;
        info.expectedReviewRound = info.submissionRound + selectedSubmissionOption.reviewDurationRounds;
        info.remainingRounds = selectedSubmissionOption.reviewDurationRounds;
        UpdateArgumentState();
        RefreshArgumentLists();
        RefreshSubmissionControls();
    }

    private bool IsSubmissionOptionAvailable(SubmissionOptionScriptableObject option)
    {
        if (option == null || currentResearchPaper == null || currentResearchPaper.paperInfo == null || roundTimer == null || roundTimer.State == null)
        {
            return false;
        }

        PaperInfo info = currentResearchPaper.paperInfo;
        int round = roundTimer.State.CurrRound;
        return info.submissionStatus == SubmissionStatus.ConstructionCompleted &&
               round >= option.openRound && round <= option.deadlineRound &&
               info.logicDegree >= option.minimumLogic &&
               info.dataRigor >= option.minimumRigor &&
               info.viewpointInnovation >= option.minimumInnovation;
    }

    private void RefreshSubmissionControls()
    {
        if (submissionChooseRoot != null)
        {
            for (int i = 0; i < submissionChooseRoot.childCount; i++)
            {
                Button button = submissionChooseRoot.GetChild(i).GetComponent<Button>();
                if (button == null) continue;
                SubmissionOptionScriptableObject option = i < submissionOptions.Count ? submissionOptions[i] : null;
                bool available = IsSubmissionOptionAvailable(option);
                button.interactable = available;
                ColorBlock colors = button.colors;
                Color baseColor = submissionNormalColors.TryGetValue(button, out Color originalColor)
                    ? originalColor
                    : Color.white;
                colors.normalColor = option != null && option == selectedSubmissionOption
                    ? Color.Lerp(baseColor, Color.yellow, 0.5f)
                    : baseColor;
                colors.disabledColor = Color.gray;
                button.colors = colors;
            }
        }

        if (submitButton != null)
        {
            submitButton.interactable = selectedSubmissionOption != null && IsSubmissionOptionAvailable(selectedSubmissionOption);
        }
    }

    private void FinalizeFramework()
    {
        if (currentResearchPaper == null || currentResearchPaper.paperInfo == null)
        {
            return;
        }

        if (currentResearchPaper.paperInfo.submissionStatus == SubmissionStatus.Submitted)
        {
            RefreshFinalizeFrameworkButton();
            return;
        }

        currentResearchPaper.paperInfo.submissionStatus = SubmissionStatus.ConstructionCompleted;
        UpdateArgumentState();
        ShowSubmitApplication();
    }

    private void ShowContinueArgument()
    {
        RefreshArgumentLists();
        ShowOnly(continueArgument, false);
    }

    private void AddPaperCreationListener(string buttonName)
    {
        Transform buttonTransform = FindChild(chooseCoreArgument == null ? null : chooseCoreArgument.transform, buttonName);
        Button button = buttonTransform == null ? null : buttonTransform.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("PaperWorkspaceNavigator could not find button " + buttonName + ".", this);
            return;
        }

        button.onClick.AddListener(() => CreatePaper(buttonTransform));
    }

    private void CreatePaper(Transform buttonTransform)
    {
        Text paperNameText = buttonTransform.GetComponentInChildren<Text>(true);
        string baseName = paperNameText == null ? string.Empty : paperNameText.text;
        PaperInfoScriptableObject paper = ScriptableObject.CreateInstance<PaperInfoScriptableObject>();
        paper.paperInfo = new PaperInfo
        {
            paperName = GetUniquePaperName(baseName)
        };

        createdPapers.Add(paper);
        currentResearchPaper = paper;
        selectedSubmissionOption = null;
        UpdateArgumentState();
        RefreshArgumentLists();
        RefreshFinalizeFrameworkButton();
        ShowWriteThesis();
    }

    private void RefreshArgumentLists()
    {
        RefreshArgumentList(argumentList, "SubmitApplication");
        RefreshArgumentList(continueArgumentList, "ContinueArgument");
    }

    private void RefreshArgumentList(Transform list, string panelName)
    {
        if (list == null)
        {
            Debug.LogError("PaperWorkspaceNavigator could not find " + panelName + "/BGArgumentList/ArgumentList/Viewport/Content.", this);
            return;
        }

        for (int i = list.childCount - 1; i >= 0; i--)
        {
            Destroy(list.GetChild(i).gameObject);
        }

        if (argumentPrefab == null)
        {
            Debug.LogError("PaperWorkspaceNavigator could not load Prefab/Argument.prefab.", this);
            return;
        }

        foreach (PaperInfoScriptableObject paper in createdPapers)
        {
            if (paper == null || paper.paperInfo == null)
            {
                continue;
            }

            GameObject argument = Instantiate(argumentPrefab, list);
            argument.name = "Argument - " + paper.paperInfo.paperName;
            UpdateArgumentEntry(argument.transform, paper);
            BindArgumentSelection(argument, paper, panelName == "ContinueArgument");
        }
    }

    private void BindArgumentSelection(GameObject argument, PaperInfoScriptableObject paper, bool openWriteThesis)
    {
        Button button = argument.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("PaperWorkspaceNavigator could not find the Button on Argument.prefab.", argument);
            return;
        }

        button.interactable = paper.paperInfo.submissionStatus != SubmissionStatus.Submitted;
        button.onClick.AddListener(() => SelectResearchPaper(paper, openWriteThesis));
    }

    private void SelectResearchPaper(PaperInfoScriptableObject paper, bool openWriteThesis = false)
    {
        if (paper == null || paper.paperInfo == null)
        {
            return;
        }

        currentResearchPaper = paper;
        selectedSubmissionOption = null;
        UpdateArgumentState();
        RefreshArgumentLists();
        if (openWriteThesis)
        {
            ShowWriteThesis();
        }
    }

    private void UpdateArgumentEntry(Transform entry, PaperInfoScriptableObject paper)
    {
        PaperInfo info = paper.paperInfo;
        UpdateSubmissionSuccessRate(info);
        string displayName = info.paperName;
        if (paper == currentResearchPaper)
        {
            displayName += " 已选择";
        }

        SetText(entry, "NameText", displayName);
        SetText(entry, "stateText", GetSubmissionStatusText(info.submissionStatus));
        SetEntryMetric(entry, "LogicalCoherence", info.logicDegree);
        SetEntryMetric(entry, "DataRigor", info.dataRigor);
        SetEntryMetric(entry, "Innovativeness", info.viewpointInnovation);
        SetEntryMetric(entry, "AcceptanceRate", info.submissionSuccessRate);
    }

    private void UpdateSubmissionSuccessRate(PaperInfo info)
    {
        info.submissionSuccessRate = Mathf.Floor((info.logicDegree + info.dataRigor + info.viewpointInnovation) / 3f);
    }

    private string GetSubmissionStatusText(SubmissionStatus status)
    {
        switch (status)
        {
            case SubmissionStatus.ConstructionCompleted:
                return "构筑完成";
            case SubmissionStatus.Submitted:
                return "已投稿";
            case SubmissionStatus.Accepted:
                return "已接受";
            case SubmissionStatus.Rejected:
                return "已拒稿";
            default:
                return "未投稿";
        }
    }

    private void SetEntryMetric(Transform entry, string metricName, float value)
    {
        Transform metric = FindChild(entry, metricName);
        Transform fillTransform = FindChild(metric, "Image (1)");
        Image fillImage = fillTransform == null ? null : fillTransform.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(value / 100f);
        }

        Transform valueTextTransform = FindChild(fillTransform, "Text (Legacy)");
        Text valueText = valueTextTransform == null ? null : valueTextTransform.GetComponent<Text>();
        if (valueText != null)
        {
            valueText.text = value.ToString("0.#");
        }
    }

    private GameObject LoadArgumentPrefab()
    {
        GameObject loaded = Resources.Load<GameObject>("Prefab/Argument");
        if (loaded != null)
        {
            return loaded;
        }

#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Argument.prefab");
#else
        return null;
#endif
    }

    private Transform CreateArgumentList(Transform parent)
    {
        GameObject listObject = new GameObject("ArgumentList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        RectTransform rect = listObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = listObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        return rect;
    }

    private Transform ResolveArgumentList(Transform submitRoot)
    {
        if (submitRoot == null)
        {
            return null;
        }

        Transform bgArgumentList = FindChild(submitRoot, "BGArgumentList");
        Transform argumentListRoot = FindChild(bgArgumentList, "ArgumentList");
        Transform viewportContent = FindChild(FindChild(argumentListRoot, "Viewport"), "Content");
        if (viewportContent != null)
        {
            return viewportContent;
        }

        Transform content = FindChild(argumentListRoot, "Content");
        if (content != null)
        {
            return content;
        }

        Transform explicitList = FindChild(submitRoot, "ArgumentList");
        if (explicitList != null)
        {
            return explicitList;
        }

        Transform scrollView = FindChild(submitRoot, "Scroll View");
        Transform viewport = FindChild(scrollView, "Viewport");
        Transform legacyContent = FindChild(viewport, "Content");
        if (legacyContent != null)
        {
            return legacyContent;
        }

        return CreateArgumentList(submitRoot);
    }

    private void UpdateArgumentState()
    {
        if (currentResearchPaper == null || currentResearchPaper.paperInfo == null)
        {
            return;
        }

        PaperInfo info = currentResearchPaper.paperInfo;
        UpdateSubmissionSuccessRate(info);
        SetText("Name", info.paperName);
        SetFillAmount("LogicalCoherence", info.logicDegree);
        SetFillAmount("DataRigor", info.dataRigor);
        SetFillAmount("Innovativeness", info.viewpointInnovation);
        SetFillAmount("Heterogeneity", info.complexity);
        SetFillAmount("AcceptanceRate", info.submissionSuccessRate);
        SetText("state", GetSubmissionStatusText(info.submissionStatus));
    }

    private void SetFillAmount(string objectName, float value)
    {
        Transform target = FindChild(argumentState == null ? null : argumentState.transform, objectName);
        Transform fillTransform = FindChild(target, "Image (1)");
        Image fillImage = fillTransform == null ? null : fillTransform.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(value / 100f);
        }
        else
        {
            Debug.LogError("PaperWorkspaceNavigator could not find fill image for " + objectName + ".", this);
        }
    }

    private void SetText(string objectName, string value)
    {
        SetText(argumentState == null ? null : argumentState.transform, objectName, value);
    }

    private void SetText(Transform root, string objectName, string value)
    {
        Transform target = FindChild(root, objectName);
        Text text = target == null ? null : target.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = value;
        }
        else
        {
            Debug.LogError("PaperWorkspaceNavigator could not find text object " + objectName + ".", this);
        }
    }

    private string GetUniquePaperName(string baseName)
    {
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "Paper";
        }

        string candidate = baseName;
        int suffix = 2;
        while (PaperNameExists(candidate))
        {
            candidate = baseName + " (" + suffix + ")";
            suffix++;
        }

        return candidate;
    }

    private bool PaperNameExists(string paperName)
    {
        foreach (PaperInfoScriptableObject paper in createdPapers)
        {
            if (paper != null && paper.paperInfo != null && paper.paperInfo.paperName == paperName)
            {
                return true;
            }
        }

        return false;
    }

    private void ShowOnly(GameObject target, bool showArgumentState)
    {
        SetActive(chooseArgument, target == chooseArgument);
        SetActive(chooseCoreArgument, target == chooseCoreArgument);
        SetActive(writeThesisMain, target == writeThesisMain);
        SetActive(submitApplication, target == submitApplication);
        SetActive(continueArgument, target == continueArgument);
        SetActive(argumentState, showArgumentState);
    }

    private void AddListener(string buttonName, UnityEngine.Events.UnityAction action)
    {
        AddListener(transform, buttonName, action);
    }

    private void AddListener(Transform root, string buttonName, UnityEngine.Events.UnityAction action)
    {
        Transform buttonTransform = FindChild(root, buttonName);
        Button button = buttonTransform == null ? null : buttonTransform.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("PaperWorkspaceNavigator could not find button " + buttonName + ".", this);
            return;
        }

        button.onClick.AddListener(action);
    }

    private Transform FindChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
        {
            return null;
        }

        foreach (Transform child in root)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform nested = FindChild(child, childName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private GameObject FindPanel(Transform canvas, string panelName)
    {
        Transform panel = canvas.Find(panelName);
        if (panel == null)
        {
            Debug.LogError("PaperWorkspaceNavigator could not find panel " + panelName + ".", this);
            return null;
        }

        return panel.gameObject;
    }

    private void SetActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }
}
