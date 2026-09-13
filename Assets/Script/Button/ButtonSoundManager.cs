using UnityEngine;

/// <summary>
/// 全局按钮音效管理器（单例）。
/// 挂载在任意常驻 GameObject 上，各按钮脚本通过 ButtonSoundManager.Instance.PlayClick() 播放音效。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ButtonSoundManager : MonoBehaviour
{
    public static ButtonSoundManager Instance { get; private set; }

    [Tooltip("按钮点击音效 AudioClip（Assets/Audio/按钮.mp3）")]
    public AudioClip clickClip;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // 如果 Inspector 未赋值则尝试自动加载
        if (clickClip == null)
            clickClip = Resources.Load<AudioClip>("Audio/按钮");
    }

    /// <summary>播放一次按钮点击音效。</summary>
    public void PlayClick()
    {
        if (clickClip != null)
            audioSource.PlayOneShot(clickClip);
    }
}
