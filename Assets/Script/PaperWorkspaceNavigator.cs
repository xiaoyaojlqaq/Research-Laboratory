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
    [SerializeField] private GameObject argumentPrefab;
    [SerializeField] private Transform argumentList;
    [SerializeField] private Transform continueArgumentList;
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
        if (roundTimer != null)
        {
            roundTimer.RoundAdvanced += RefreshCoreArgumentOptions;
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
    }

    private void Update()
    {
        RefreshIncorporateViewpointButton();
    }

    private void OnDestroy()
    {
        if (roundTimer != null)
        {
            roundTimer.RoundAdvanced -= RefreshCoreArgumentOptions;
        }
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
    }

    private void RefreshIncorporateViewpointButton()
    {
        if (incorporateViewpointButton == null)
        {
            return;
        }

        bool canUse = roundTimer != null && roundTimer.State != null &&
                      roundTimer.State.Inspiration >= 1f &&
                      currentResearchPaper != null && currentResearchPaper.paperInfo != null &&
                      currentResearchPaper.paperInfo.fusionCooldownRounds == 0;
        incorporateViewpointButton.interactable = canUse;

        ColorBlock colors = incorporateViewpointButton.colors;
        colors.disabledColor = Color.gray;
        incorporateViewpointButton.colors = colors;
    }

    private void UseLiteratureSearch()
    {
        if (roundTimer == null || currentResearchPaper == null || currentResearchPaper.paperInfo == null)
        {
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

    private void ShowSubmitApplication()
    {
        RefreshArgumentLists();
        ShowOnly(submitApplication, false);
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
        UpdateArgumentState();
        RefreshArgumentLists();
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
            BindArgumentSelection(argument, paper);
        }
    }

    private void BindArgumentSelection(GameObject argument, PaperInfoScriptableObject paper)
    {
        Button button = argument.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("PaperWorkspaceNavigator could not find the Button on Argument.prefab.", argument);
            return;
        }

        button.onClick.AddListener(() => SelectResearchPaper(paper));
    }

    private void SelectResearchPaper(PaperInfoScriptableObject paper)
    {
        if (paper == null || paper.paperInfo == null)
        {
            return;
        }

        currentResearchPaper = paper;
        UpdateArgumentState();
        RefreshArgumentLists();
    }

    private void UpdateArgumentEntry(Transform entry, PaperInfoScriptableObject paper)
    {
        PaperInfo info = paper.paperInfo;
        string displayName = info.paperName;
        if (paper == currentResearchPaper)
        {
            displayName += " 已选择";
        }

        SetText(entry, "NameText", displayName);
        SetEntryMetric(entry, "LogicalCoherence", info.logicDegree);
        SetEntryMetric(entry, "DataRigor", info.dataRigor);
        SetEntryMetric(entry, "Innovativeness", info.viewpointInnovation);
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
        SetText("Name", info.paperName);
        SetFillAmount("LogicalCoherence", info.logicDegree);
        SetFillAmount("DataRigor", info.dataRigor);
        SetFillAmount("Innovativeness", info.viewpointInnovation);
        SetFillAmount("Heterogeneity", info.complexity);
        SetFillAmount("AcceptanceRate", info.submissionSuccessRate);
        SetText("state", info.submissionStatus.ToString());
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
