/*
NPC对话触发器
  - 检测Player接近距离（可调节范围）
  - 按T键触发对话
  - 支持UI提示显示
*/
using UnityEngine;
using PixelCrushers.DialogueSystem;
using MyGame.Time;
using System; // 引入APTimeManager

public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("NPC设置")]
    public string npcName;
    public string conversationTitle;

    [Header("触发设置")]
    public float interactionRange;
    public bool isInRange = false;
    private bool isConversationActive = false;

    [Header("UI提示")]
    public GameObject interactionPrompt;

    [Header("AP设置")]
    public int apCost = 1;

    private Transform player;

    // 本触发器生成并管理的 token（当前对话的唯一标识）
    private string _currentToken = null;

    void Start()
    {
        // 查找Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"NPC {npcName}: 未找到标签为'Player'的对象");
        }

        // 检查对话管理器
        if (DialogueManager.instance == null)
        {
            Debug.LogWarning($"NPC {npcName}: 场景中未找到DialogueManager");
        }

        // 初始化UI提示
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        //Debug.Log("NPCDialogueTrigger Update running, player=" + (player != null));

        if (player == null || isConversationActive) return;

        // 检查距离
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionRange && !isInRange)
        {
            // 进入交互范围
            OnEnterRange();
        }
        else if (distance > interactionRange && isInRange)
        {
            // 离开交互范围
            OnExitRange();
        }

        // 检查交互输入
        if (isInRange && Input.GetKeyDown(KeyCode.T))
        {
            StartConversation();
        }
    }

    void OnEnterRange()
    {
        isInRange = true;
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }

        Debug.Log($"可以与 {npcName} 对话，按T键开始对话");
    }

    void OnExitRange()
    {
        isInRange = false;
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    public void StartConversation()
    {
        if (isConversationActive || string.IsNullOrEmpty(conversationTitle)) return;

        // 1) 生成 token 并注册（保证唯一）
        _currentToken = $"conversation:{npcName}:{Guid.NewGuid()}";
        if (PlayerActionStateManager.Instance != null)
        {
            PlayerActionStateManager.Instance.StartAction(_currentToken);
        }
        else
        {
            Debug.LogWarning("PlayerActionStateManager 未找到，仍继续，但可能会导致状态无法正确管理");
        }

        // 2) 尝试消耗 AP
        string reason;
        var apManager = FindObjectOfType<APTimeManager>(); // 兼容你的实现（非单例）
        //if (apManager == null)
        //{
        //    Debug.LogWarning("APTimeManager 未找到，继续对话但不会消耗行动点");
        //    // 启动对话，但依然需要监听结束以清理 token
        //    StartConversation_Internal();
        //    return;
        //}

        if (!apManager.TryConsumeAP(apCost, out reason))
        {
            // AP 不足：撤销 token 并提示玩家
            if (PlayerActionStateManager.Instance != null) PlayerActionStateManager.Instance.EndAction(_currentToken);
            _currentToken = null;
            ShowUIMessage(reason);
            return;
        }


        // 检查对话管理器是否存在
        if (DialogueManager.instance == null)
        {
            Debug.LogError("Dialogue Manager未找到，无法开始对话");
            isConversationActive = false;

            //回滚token
            if (PlayerActionStateManager.Instance != null && _currentToken != null)
            {
                // 回滚 token（虽然 AP 已扣了）
                PlayerActionStateManager.Instance.EndAction(_currentToken);
                _currentToken = null;
            }
            return;
        }

        isConversationActive = true;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        // 确定要开始的对话
        string conversation = GetConversationToStart();

        // 启动对话
        DialogueManager.StartConversation(conversation, transform, player);

        // 监听对话结束事件
        DialogueManager.instance.conversationEnded += OnConversationEnded;

        Debug.Log($"开始与 {npcName} 的对话: {conversation}");
    }

    string GetConversationToStart()
    {
        // 获取对话条件管理器
        //NPCDialogueConditions conditions = GetComponent<NPCDialogueConditions>();

        // if (conditions != null)
        // {
        //     return conditions.GetCurrentConversation();
        // }

        // 默认返回基础对话
        return conversationTitle;
    }

    void OnConversationEnded(Transform actor)
    {
        if (actor == transform)
        {
            // 恢复提示状态（如果玩家依然在范围内）
            isConversationActive = false;
            if (isInRange && interactionPrompt != null) interactionPrompt.SetActive(true);

            // 清理本触发器生成的 token（若存在）
            if (!string.IsNullOrEmpty(_currentToken) && PlayerActionStateManager.Instance != null)
            {
                PlayerActionStateManager.Instance.EndAction(_currentToken);
            }
            _currentToken = null;

            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.conversationEnded -= OnConversationEnded;
            }
            Debug.Log($"与 {npcName} 的对话结束");
        }
    }

    void OnDrawGizmosSelected()
    {
        // 在场景视图中显示交互范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    private void ShowUIMessage(string msg)
    {
        Debug.Log($"[UI] {msg}");
        // TODO: 替换成项目内真实的 UI 提示方法（弹窗/吐司）
    }
}