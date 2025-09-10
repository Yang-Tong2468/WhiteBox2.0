/*
NPC对话触发器
  - 检测Player接近距离（可调节范围）
  - 按T键触发对话
  - 支持UI提示显示
*/
using UnityEngine;
using PixelCrushers.DialogueSystem;
using MyGame.Time; // 引入APTimeManager

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

    private Transform player;

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

        isConversationActive = true;

        // 确定要开始的对话
        string conversation = GetConversationToStart();

        // 检查对话管理器是否存在
        if (DialogueManager.instance == null)
        {
            Debug.LogError("Dialogue Manager未找到，无法开始对话");
            isConversationActive = false;
            return;
        }

        // 启动对话
        DialogueManager.StartConversation(conversation, transform, player);

        // 消耗一个行动点
        var apManager = FindObjectOfType<APTimeManager>();
        if (apManager != null)
        {
            apManager.ConsumeAP();
        }
        else
        {
            Debug.LogWarning("未找到APTimeManager，无法消耗行动点");
        }

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
            isConversationActive = false;
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
}