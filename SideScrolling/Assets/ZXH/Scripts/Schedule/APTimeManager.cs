using UnityEngine;
using UnityEngine.Events;
using Slax.Schedule;

namespace MyGame.Time
{
    public class APTimeManager : MonoBehaviour
    {
        [Header("AP 配置")]
        public int maxAPPerDay = 4;
        public int currentAP;

        [Header("时间配置")]
        public TimeConfigurationSO timeConfiguration;

        public enum Phase { Morning = 0, Midday, Evening, Night }
        public Phase currentPhase = Phase.Morning;

        public static event UnityAction<DateTime> OnDateTimeChangedAP = delegate { };
        public static event UnityAction OnInBetweenTickFiredAP = delegate { };

        private DateTime _dateTime;

        // 标记是否已有一次 pending 的推进在等玩家空闲
        private bool _waitingForIdle = false;

        private void Awake()
        {
            currentAP = maxAPPerDay;
            if (timeConfiguration != null)
            {
                _dateTime = new DateTime(
                    timeConfiguration.Date,
                    (int)timeConfiguration.Season,
                    timeConfiguration.Year,
                    timeConfiguration.Hour,
                    0,
                    timeConfiguration.DayConfiguration
                );
            }
            else
            {
                var dc = new DayConfiguration { MorningStartHour = 8, AfternoonStartHour = 12, EveningStartHour = 16, NightStartHour = 20 };
                _dateTime = new DateTime(1, 0, 1, dc.MorningStartHour, 0, dc);
            }
        }

        private void Start()
        {
            Debug.Log($"[APTimeManager] Initialized: {_dateTime.DateToString()} {_dateTime.TimeToString()} | AP={currentAP}/{maxAPPerDay}");
            OnDateTimeChangedAP?.Invoke(_dateTime);
        }

        /// <summary>
        /// 行动的入口
        /// 尝试消耗 cost 点行动点
        /// 返回 true 表示被接受（已扣点或已安排）；false 表示被拒绝（reason 给出原因）
        /// 规则：
        /// - 如果 cost > currentAP -> 拒绝
        /// - 否则扣点，如果扣点后 currentAP>0 -> 触发 OnInBetweenTickFiredAP
        /// - 如果扣点后 currentAP<=0 -> 标记等待推进，只有当 PlayerActionStateManager 报 Idle 时才推进阶段
        /// </summary>
        public bool TryConsumeAP(int cost, out string reason)
        {
            reason = null;
            if (cost <= 0)
            {
                reason = "消耗必须为正整数";
                return false;
            }

            if (cost > currentAP)
            {
                reason = $"行动点不足（需要 {cost}，当前 {currentAP}）";
                return false;
            }

            // 扣点
            currentAP -= cost;
            Debug.Log($"[APTimeManager] 消耗 {cost} AP，剩余 {currentAP}/{maxAPPerDay}");

            if (currentAP > 0)
            {
                // 仍有剩余 -> 触发 in-between tick
                OnInBetweenTickFiredAP?.Invoke();
            }
            else
            {
                // AP 正好耗尽或负（按逻辑应该 =0），开始等待玩家空闲后推进一次阶段
                if (!_waitingForIdle)
                {
                    _waitingForIdle = true;
                    Debug.Log("[APTimeManager] AP 用尽，等待 PlayerActionStateManager 变为 Idle 后推进时间段。");

                    // 如果玩家现在已空闲，则直接推进；否则订阅 OnBecameIdle
                    if (PlayerActionStateManager.Instance != null && PlayerActionStateManager.Instance.CurrentState == PlayerActionStateManager.PlayerState.Idle)
                    {
                        DoPendingAdvance();
                    }
                    else
                    {
                        if (PlayerActionStateManager.Instance != null)
                        {
                            PlayerActionStateManager.Instance.OnBecameIdle += OnPlayerBecameIdle_Handler;
                        }
                        else
                        {
                            // 没有 Manager 的兜底：直接推进
                            Debug.LogWarning("[APTimeManager] 找不到 PlayerActionStateManager，直接推进时间段以避免停滞。");
                            DoPendingAdvance();
                        }
                    }
                }
                else
                {
                    Debug.Log("[APTimeManager] 已在等待推进中（忽略重复等待请求）");
                }
            }

            return true;
        }

        /// <summary>
        /// 监听角色转换到空闲状态
        /// </summary>
        private void OnPlayerBecameIdle_Handler()
        {
            // 取消订阅并推进
            if (PlayerActionStateManager.Instance != null)
                PlayerActionStateManager.Instance.OnBecameIdle -= OnPlayerBecameIdle_Handler;

            DoPendingAdvance();
        }

        /// <summary>
        /// 推进
        /// </summary>
        private void DoPendingAdvance()
        {
            if (!_waitingForIdle) return;
            _waitingForIdle = false;

            // 推进一个阶段（你的原逻辑）
            AdvancePhases(1);

            // 重置 AP（遵循你原有逻辑：消耗完后重置为 max）
            currentAP = maxAPPerDay;
            Debug.Log($"[APTimeManager] 推进完成，AP 重置为 {currentAP}/{maxAPPerDay}");
        }

        /// <summary>
        /// 逐阶段推进（保留你原有实现的日历/跨日逻辑）
        /// </summary>
        public void AdvancePhases(int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                currentPhase = (Phase)(((int)currentPhase + 1) % 4);

                int hour;
                if (timeConfiguration != null)
                {
                    var dc = timeConfiguration.DayConfiguration;
                    hour = currentPhase switch
                    {
                        Phase.Morning => dc.MorningStartHour,
                        Phase.Midday => dc.AfternoonStartHour,
                        Phase.Evening => dc.EveningStartHour,
                        Phase.Night => dc.NightStartHour,
                        _ => dc.MorningStartHour
                    };
                }
                else
                {
                    hour = currentPhase switch
                    {
                        Phase.Morning => 8,
                        Phase.Midday => 12,
                        Phase.Evening => 16,
                        Phase.Night => 20,
                        _ => 8
                    };
                }

                if (currentPhase == Phase.Morning)
                {
                    _dateTime.SetNewDay();
                }

                _dateTime = new DateTime(
                    _dateTime.Date,
                    (int)_dateTime.Season,
                    _dateTime.Year,
                    hour,
                    0,
                    _dateTime.DayConfiguration
                );

                Debug.Log($"[APTimeManager] Tick -> {_dateTime.DateToString()} {_dateTime.TimeToString()}");
                OnDateTimeChangedAP?.Invoke(_dateTime);
            }
        }

        // 可选：强制推进（调试用）
        public void ForceAdvanceOnePhase()
        {
            if (_waitingForIdle)
            {
                // 如果在等空闲就直接推进并取消等待
                if (PlayerActionStateManager.Instance != null)
                    PlayerActionStateManager.Instance.OnBecameIdle -= OnPlayerBecameIdle_Handler;
                _waitingForIdle = false;
            }

            AdvancePhases(1);
        }
    }
}
