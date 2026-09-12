using System.IO;
using UnityEngine;

/// <summary>
/// 存档管理器（单例）。
/// 负责 JSON 的读写，不依赖场景中具体的游戏对象。
///
/// 存档路径：Application.persistentDataPath/save.json
///
/// 策略：
///   - 游戏运行中所有写操作只更新内存 Data，不写磁盘。
///   - 退出游戏时（OnApplicationQuit）统一写一次磁盘。
///   - 如需手动强制写盘，调用 ForceSave()。
/// </summary>
public class SaveManager : MonoBehaviour
{
    // ── 单例 ──────────────────────────────────────────────────────
    public static SaveManager Instance { get; private set; }

    private const string FileName = "save.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    // ── 当前存档（内存） ──────────────────────────────────────────
    public SaveData Data { get; private set; }

    // ── 自动引导：若场景中没有 SaveManager，则自动创建 ──────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoBootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("SaveManager");
        go.AddComponent<SaveManager>();
        // Awake 会在 AddComponent 时同步执行，Instance 已被赋值
    }

    // ── 生命周期 ──────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 启动时读取存档，供其他脚本的 Awake/Start 使用
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>退出游戏时统一写盘。</summary>
    private void OnApplicationQuit()
    {
        WriteToFile();
    }

    // ── 读取 ──────────────────────────────────────────────────────
    /// <summary>
    /// 尝试读取存档。
    /// 若文件不存在或解析失败，则自动初始化一份新存档（只在内存，退出时才写盘）。
    /// </summary>
    public SaveData Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath, System.Text.Encoding.UTF8);
                SaveData loaded = JsonUtility.FromJson<SaveData>(json);
                if (loaded != null)
                {
                    Data = loaded;
                    Debug.Log("[SaveManager] 读取存档成功：" + SavePath);
                    return Data;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[SaveManager] 读取存档失败，将初始化新存档。原因：" + e.Message);
            }
        }
        else
        {
            Debug.Log("[SaveManager] 未找到存档，初始化新存档。");
        }

        Data = CreateDefault();
        return Data;
    }

    // ── 内存更新（不写磁盘） ──────────────────────────────────────
    /// <summary>把 MainState 更新到内存 Data（不写磁盘）。</summary>
    public void SaveState(MainState state)
    {
        if (Data == null) Data = CreateDefault();
        Data.currentRound         = state.CurrRound;
        Data.remainingTimeSeconds = state.RemainingTimeSeconds;
        Data.stamina              = state.Strength;
        Data.inspiration          = state.Inspiration;
    }

    /// <summary>把论文列表更新到内存 Data（不写磁盘）。</summary>
    public void SavePapers(System.Collections.Generic.List<PaperInfoScriptableObject> papers)
    {
        if (Data == null) Data = CreateDefault();
        Data.papers.Clear();
        foreach (var p in papers)
            if (p != null)
                Data.papers.Add(PaperSaveData.FromPaperInfo(p.paperInfo));
    }

    // ── 写磁盘 ────────────────────────────────────────────────────
    /// <summary>立即将内存 Data 序列化写入磁盘（一般只在退出时调用）。</summary>
    public void ForceSave()
    {
        WriteToFile();
    }
    /// <summary>
    /// 重置存档：用默认数据覆盖内存并立即写盘。
    /// </summary>
    public void ResetSave()
    {
        Data = CreateDefault();
        WriteToFile();
        Debug.Log("[SaveManager] 存档已重置。");
    }


    private void WriteToFile()
    {
        if (Data == null) Data = CreateDefault();
        try
        {
            string json = JsonUtility.ToJson(Data, prettyPrint: true);
            File.WriteAllText(SavePath, json, System.Text.Encoding.UTF8);
            Debug.Log("[SaveManager] 存档已写入磁盘：" + SavePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SaveManager] 写盘失败：" + e.Message);
        }
    }

    // ── 默认存档 ──────────────────────────────────────────────────
    private SaveData CreateDefault()
    {
        return new SaveData
        {
            currentRound         = 1,
            remainingTimeSeconds = 8 * 60f,
            stamina              = 100,
            inspiration          = 0f,
        };
    }
}

