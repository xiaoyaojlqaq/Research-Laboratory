# Research Laboratory

一款以学术科研为主题的回合制模拟游戏。玩家扮演研究员，在有限的时间与体力内完成论文撰写、投稿与同行评审，争取将论文发表在高级别期刊上。

---

## 目录

1. [打开方式](#打开方式)
2. [操作流程](#操作流程)
3. [配置位置](#配置位置)
4. [存档位置](#存档位置)
5. [已知问题](#已知问题)
6. [资源来源](#资源来源)
---

## 打开方式

**平台要求：** Windows 10 / 11（x64）

1. 解压发布包，确保以下文件与文件夹保存在**同一目录**下，不要单独移动任何文件：
   ```
   Research Laboratory.exe
   Research Laboratory_Data/
   MonoBleedingEdge/
   UnityCrashHandler64.exe
   UnityPlayer.dll
   ```
2. 双击 **`Research Laboratory.exe`** 启动游戏。
3. 首次启动会弹出 Unity 启动配置对话框，可在此选择分辨率与画质；也可直接点击 **Play** 使用默认设置进入游戏。
4. 游戏内亦可通过右上角 **设置（⚙）→ 分辨率下拉框** 随时切换分辨率（2560×1600 / 2560×1440 / 1920×1080，均为全屏）。

> **注意：** 请勿将 `Research Laboratory.exe` 移出所在目录单独运行，否则游戏将因找不到 `_Data` 文件夹而无法启动。

---

## 操作流程

### 主界面

游戏启动后进入 **主场景（MainScene）**。屏幕顶部显示当前回合、剩余时间、体力和灵感四项状态。

| 按钮 | 功能 |
|------|------|
| **实验台（Lab Bench）** | 保留入口，当前版本暂未实装 |
| **论文工作台（Paper Workspace）** | 进入论文撰写与投稿的核心工作区 |
| **设置（⚙）** | 打开分辨率设置与存档管理面板 |

> 点击主界面按钮时会播放入场过渡动画（Timeline）。动画播放期间所有按钮暂时禁用，动画结束后自动恢复。

---

### 论文工作台

进入工作台后，左侧边栏提供以下功能页签：

#### 1. 选择核心论点（Core Argument）
- 点击 **核心论点** 进入论点选择面板。
- 从 **4 个随机论点**（选择 1–4）中选择一个，自动创建一篇新论文并命名。
- 已完成构建的论文可通过 **继续已有论文** 继续编辑。

#### 2. 撰写论文（Write Thesis）
- 点击 **撰写论文** 进入撰写面板，可对当前选中的论文执行以下操作：

| 操作 | 体力消耗 | 时间消耗 | 说明 |
|------|---------|---------|------|
| **查文献（Literature Search）** | 10 | 60 秒 | 提升论文属性；体力或时间不足时强制推进回合 |
| **融入观点（Incorporate Viewpoint）** | — | — | 将其他论文的观点融合进当前论文，有冷却回合限制 |
| **完成构筑（Finalize Framework）** | — | — | 将论文状态标记为"构建完成"，解锁投稿功能 |

#### 3. 投稿（Submit Application）
- 定稿后可在投稿面板选择目标期刊（顶级 / 一般 / 普通 等级别），点击 **投稿** 完成投稿。
- 投稿要求满足期刊最低逻辑度、数据严谨度、观点创新度门槛和可投稿期限。

#### 4. 同行评审（Peer Review）
- 已投稿且审稿轮次倒计时归零后，论文进入可处理状态。
- 点击对应论文按钮随机结算审稿结果（接受 / 拒绝），结果由论文属性与期刊成功率共同决定。

---

### 回合推进

- 每回合默认时长 **8 分钟**。
- 倒计时归零**或**体力归零时，自动触发回合切换动画并进入下一回合。
- 回合推进时：体力恢复至满值（100）、计时器重置、所有论文的审稿剩余回合 -1。

---

### 设置面板

通过主界面右上角设置按钮打开 `设置`：

| 功能 | 说明 |
|------|------|
| 分辨率切换 | 2560×1600 / 2560×1440 / 1920×1080（全屏） |
| 重置存档 | 清空所有进度并重新加载场景 |
| 退出游戏 | 调用 `结束方法` |

---

## 配置位置

### Inspector 可调参数

| 脚本 | 挂载对象 | 可配置字段 | 说明 |
|------|---------|-----------|------|
| `RoundTimer` | `MainCanvas` 子对象 | `roundDurationSeconds` | 每回合秒数，默认 480 |
| `RoundTimer` | 同上 | `blockInputDuringTimeline` | 回合切换动画期间是否锁定输入 |
| `RoundTimer` | 同上 | `changeTurnDirector` | 指向 `ChangeTurn` 的 PlayableDirector |
| `MainButtonAnima` | 各主按钮 | `scaleFactor` | 悬停放大比例 |
| `LefButton` | 左侧边栏按钮 | `scaleFactor` | 悬停放大比例 |
| `ButtonSoundManager` | `ButtonSoundManager` | `clickClip` | 按钮音效 AudioClip（默认 `Assets/Audio/按钮.mp3`） |
| `PaperWorkspaceNavigator` | `PaperWorkspaceCanvas` 子对象 | `submissionOptions` | 投稿期刊选项列表（ScriptableObject） |
| `SettingButton` | `SettingsCanvas/Panel` | `ResolutionDropdown` | 分辨率下拉框引用 |

### ScriptableObject 数据资产

| 类型 | 菜单路径 | 说明 |
|------|---------|------|
| `PaperInfoScriptableObject` | **Research Laboratory / Paper Info** | 单篇论文数据模板 |
| `SubmissionOptionScriptableObject` | **Research Laboratory / Submission Option** | 期刊投稿条件配置（等级、审稿轮次、各属性门槛、成功率） |

> 在 Project 窗口右键 → **Create → Research Laboratory** 可新建上述资产。  
> 现有期刊配置资产位于 `Assets/` 目录下，可直接在 Inspector 中修改各字段。

### 音频配置

- 按钮音效文件：`Assets/Audio/按钮.mp3`
- 若需替换音效，将新 AudioClip 赋值给场景中 `ButtonSoundManager` 对象的 `clickClip` 字段即可。

---

## 存档位置

存档由 `SaveManager`（单例，`DontDestroyOnLoad`）统一管理。

### 文件路径

```
C:\Users\<用户名>\AppData\LocalLow\DefaultCompany\Research Laboratory\save.json
```

> `Application.persistentDataPath` 在 Windows 上固定为此路径。

### 存档格式（JSON）

```json
{
    "currentRound": 1,
    "remainingTimeSeconds": 480.0,
    "stamina": 100,
    "inspiration": 0.0,
    "papers": [
        {
            "paperName": "示例论文",
            "logicDegree": 60.0,
            "dataRigor": 50.0,
            "viewpointInnovation": 40.0,
            "complexity": 30.0,
            "submissionStatus": 0,
            "submissionLevel": "C",
            "submissionRound": 0,
            "expectedReviewRound": 2,
            "remainingRounds": 2,
            "submissionSuccessRate": 75.0,
            "fusionCooldownRounds": 0,
            "incorporatedViewpointCount": 0
        }
    ]
}
```

### 读写策略

| 时机 | 行为 |
|------|------|
| 游戏启动 | 自动读取 `save.json`；文件不存在则初始化默认存档 |
| 回合推进 | 自动将主状态（回合、时间、体力、灵感）写入内存 |
| 投稿/评审操作 | 自动将论文列表写入内存 |
| 退出游戏 | `OnApplicationQuit` 统一将内存数据序列化写入磁盘 |
| 手动重置 | 设置面板 → **重置存档**，立即清空并写盘后重载场景 |

> **注意：** 游戏运行中途如果通过 Editor 强制停止（Stop），`OnApplicationQuit` **不会**被触发，本局未写盘的进度将丢失。如需强制保存，可在代码中调用 `SaveManager.Instance.ForceSave()`。

---

## 已知问题

| # | 问题描述 | 影响范围 | 临时规避方案 |
|---|---------|---------|-------------|
| 1 | **Editor 停止不写盘**：在 Unity Editor 中点击 Stop 按钮时，`OnApplicationQuit` 不触发，当局进度不会持久化。 | 开发调试 | 测试时手动调用 `SaveManager.Instance.ForceSave()`，或改用打包版本测试存档逻辑。 |
| 2 | **PaperWorkspace 入场动画只触发一次**：`MainSceneStartDirector` 检测到 `PlayableDirector.time > 0` 后不再重复播放，重新点击 Paper Workspace 按钮无效果。 | 主界面 | 重载场景可重置动画状态。 |
| 3 | **同行评审列表动态按钮无音效**：`peerReview` 面板中通过 `Instantiate` 生成的 `ReviewArgument` 条目按钮，不在场景初始化时注册，故 `ButtonSoundManager` 的 onClick 持久化监听不覆盖这些按钮。 | 评审面板 | 在 `RefreshPeerReviewList()` 中手动添加 `ButtonSoundManager.Instance?.PlayClick()` 调用，或为 ReviewArgument.prefab 添加 `MainButtonAnima` 组件。 |
| 4 | **`peerReviewAnima` 脚本未实现**：`OnEnable` 方法体中的动画逻辑已注释掉，同行评审面板无入场动画。 | 评审面板 | 暂无，待后续实现 `DOAnchorPosY` 动画。 |
| 5 | **实验台（Lab Bench）按钮功能未开发**：主界面 `LabBench` 按钮的 onClick 事件已注册音效，但无任何游戏逻辑回调。 | 主界面 | 暂无，功能待规划实现。 |
| 6 | **分辨率切换仅支持全屏模式**：`SettingButton.SetResolution()` 始终以 `fullscreen: true` 调用 `Screen.SetResolution`，无法切换为窗口模式。 | 设置面板 | 如需窗口化运行，可在 Player Settings 中修改启动分辨率，或修改脚本传入 `false`。 |
| 7 | **ButtonSoundManager 跨场景重复创建风险**：若在新场景中仍存在名为 `ButtonSoundManager` 的 GameObject，单例 `Awake` 会销毁后来者，但新场景中的按钮 onClick 持久监听将引用已销毁对象（引用变为 `null`），导致音效静默。 | 多场景扩展 | 目前项目为单场景，不受影响；若后续添加新场景，需在新场景中重新绑定或改用全局事件总线方案。 |

## 资源来源
- 开发中所用到的美术素材资源均来自所给美术素材经TuanjieAI处理而来。
- 音频资源均来自https://www.aigei.com/?by=history&from=kkframenew 免费资源。