using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

/// <summary>
/// 挂载在 MainSceneStartDirector 对象上。
/// 等 TimeLine 播放完毕后，才启用 MainCanvas/PaperWorkspace 和 MainCanvas/LabBench 按钮。
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class MainSceneStartDirector : MonoBehaviour
{
    private PlayableDirector director;
    private Button paperWorkspaceButton;
    private Button labBenchButton;
    private bool buttonsEnabled = false;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();

        paperWorkspaceButton = FindButtonInMainCanvas("PaperWorkspace");
        labBenchButton = FindButtonInMainCanvas("LabBench");

        SetButtonsInteractable(false);

        // 绑定 PaperWorkspace 按钮：点击时播放对应 TimeLine
        if (paperWorkspaceButton != null)
            paperWorkspaceButton.onClick.AddListener(OnPaperWorkspaceClicked);
    }

    private void OnDestroy() { }

    private void Update()
    {
        if (buttonsEnabled) return;

        // 在 TimeLine 结束前 0.5 秒启用按钮
        if (director.state == UnityEngine.Playables.PlayState.Playing &&
            director.time >= director.duration - 0.5)
        {
            SetButtonsInteractable(true);
            buttonsEnabled = true;
        }
    }




private void OnPaperWorkspaceClicked()
    {
        GameObject dirObj = GameObject.Find("PaperWorkspaceStartDirector");
        if (dirObj == null)
        {
            Debug.LogError("MainSceneStartDirector：找不到 PaperWorkspaceStartDirector。", this);
            return;
        }

        PlayableDirector pd = dirObj.GetComponent<PlayableDirector>();
        if (pd == null) return;

        // 正在播放中或已播放过（time > 0），不重复播放
        if (pd.state == PlayState.Playing || pd.time > 0) return;

        // 如果 PaperWorkspaceCanvas 处于非激活状态，先激活它
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.gameObject.name == "PaperWorkspaceCanvas" && !c.gameObject.activeInHierarchy)
            {
                c.gameObject.SetActive(true);
                break;
            }
        }

        pd.time = 0;
        pd.Play();
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (paperWorkspaceButton != null)
            paperWorkspaceButton.interactable = interactable;
        if (labBenchButton != null)
            labBenchButton.interactable = interactable;
    }

    /// <summary>
    /// 在场景中查找 MainCanvas 下指定名称的直接子对象上的 Button 组件。
    /// </summary>
    private Button FindButtonInMainCanvas(string buttonName)
    {
        GameObject mainCanvas = GameObject.Find("MainCanvas");
        if (mainCanvas == null)
        {
            Debug.LogError("MainSceneStartDirector：找不到 MainCanvas。", this);
            return null;
        }

        Transform t = mainCanvas.transform.Find(buttonName);
        if (t == null)
        {
            Debug.LogError($"MainSceneStartDirector：在 MainCanvas 下找不到 {buttonName}。", this);
            return null;
        }

        Button btn = t.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError($"MainSceneStartDirector：{buttonName} 上没有 Button 组件。", this);
        }
        return btn;
    }
}
