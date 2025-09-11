using System;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

/// <summary>
/// 全局单例：跟踪玩家/角色当前是否有未完成的“行动”
/// 使用 token 模式：StartAction(token) / EndAction(token)
/// 自动接收 Dialogue System 的 OnConversationStart/End 消息（如果这个对象是参与者）
/// 也会订阅 DialogueManager.instance.conversationEnded 作为备用
/// </summary>
public class PlayerActionStateManager : MonoBehaviour
{
    public static PlayerActionStateManager Instance { get; private set; }

    public enum PlayerState { Idle, Busy }

    // token 集合：用于记录当前有哪些未完成行为
    private readonly HashSet<string> _activeTokens = new HashSet<string>();

    // 方便从 actor 找到对应的 conversation token
    private readonly Dictionary<Transform, List<string>> _actorTokens = new Dictionary<Transform, List<string>>();

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    // 事件：状态改变、从 Busy -> Idle 的特殊事件
    public event Action<PlayerState> OnStateChanged;
    public event Action OnBecameIdle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationEnded += OnConversationEnded_Global;
        }
    }

    private void OnDisable()
    {
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationEnded -= OnConversationEnded_Global;
        }
    }

    #region Public API

    /// <summary>
    /// 注册一个行动 token，返回 true 表示成功（如果 token 已存在返回 false）
    /// token 要保证唯一，eg： "dialogue:player:GUID" 或 "quest:questId:nodeId"
    /// </summary>
    public bool StartAction(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        var added = _activeTokens.Add(token);
        UpdateStateIfNeeded();
        return added;
    }

    /// <summary>
    /// 注销一个行动 token，返回 true 表示成功（若不存在返回 false）
    /// </summary>
    public bool EndAction(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        var removed = _activeTokens.Remove(token);
        UpdateStateIfNeeded();
        return removed;
    }

    /// <summary>
    /// 清空所有 token（例如在读档或场景切换时调用以重置状态）
    /// </summary>
    public void ClearAllActions()
    {
        _activeTokens.Clear();
        _actorTokens.Clear();
        UpdateStateIfNeeded();
    }

    #endregion

    #region Dialogue System hooks (Message-based & global)

    /// <summary>
    /// Message hook：Dialogue System 会向参与者发送 OnConversationStart(Transform actor)
    /// 如果这个脚本挂到 player 或对话触发器上并且被调用，会生成 token 并把 token 与 actor 关联
    /// </summary>
    public void OnConversationStart(Transform actor)
    {
        if (actor == null) return;

        string token = $"conversation:{actor.name}:{Guid.NewGuid()}";
        StartAction(token);

        // 记录 actor->token 映射，方便 End 时移除
        if (!_actorTokens.TryGetValue(actor, out var list))
        {
            list = new List<string>();
            _actorTokens[actor] = list;
        }

        list.Add(token);
        Debug.Log($"[PlayerActionStateManager] OnConversationStart: created token {token} for actor {actor.name}");
    }

    /// <summary>
    /// Message hook：Dialogue System 会发送 OnConversationEnd(Transform actor)
    /// 把与 actor 关联的 token 全部移除（因为同一 actor 可能有多个 token）
    /// </summary>
    public void OnConversationEnd(Transform actor)
    {
        if (actor == null) return;

        if (_actorTokens.TryGetValue(actor, out var list))
        {
            foreach (var t in list)
            {
                EndAction(t);
                Debug.Log($"[PlayerActionStateManager] OnConversationEnd: removed token {t} for actor {actor.name}");
            }
            _actorTokens.Remove(actor);
        }
        else
        {
            // 兜底：若没有映射（例如 token 由触发器生成但未记录），也尝试移除 tokens 前缀匹配
            var tokens = new List<string>(_activeTokens);
            foreach (var t in tokens)
            {
                if (t.StartsWith($"conversation:{actor.name}:"))
                {
                    EndAction(t);
                    Debug.Log($"[PlayerActionStateManager] OnConversationEnd fallback removed {t}");
                }
            }
        }
    }

    /// <summary>
    /// 全局订阅到 DialogueManager.instance.conversationEnded 的处理（备用/补充）
    /// </summary>
    private void OnConversationEnded_Global(Transform actor)
    {
        // 这里与 OnConversationEnd 执行相同移除逻辑
        OnConversationEnd(actor);
    }

    #endregion

    #region Quest Machine / Node callbacks (在 QM 的节点 Actions 中调用)

    // 在 Quest Machine 节点里可以添加 UnityEvent -> 目标 GameObject -> 这些方法
    public void OnQuestNodeTrue(string questNodeId)
    {
        StartAction($"quest:{questNodeId}");
    }

    public void OnQuestNodeFalse(string questNodeId)
    {
        EndAction($"quest:{questNodeId}");
    }

    #endregion

    private void UpdateStateIfNeeded()
    {
        var newState = _activeTokens.Count == 0 ? PlayerState.Idle : PlayerState.Busy;
        if (newState != CurrentState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
            if (newState == PlayerState.Idle)
            {
                OnBecameIdle?.Invoke();
            }
            Debug.Log($"[PlayerActionStateManager] State changed -> {CurrentState}. ActiveTokens={_activeTokens.Count}");
        }
    }
}
