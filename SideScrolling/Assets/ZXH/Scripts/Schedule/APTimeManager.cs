using UnityEngine;
using UnityEngine.Events;
using Slax.Schedule; // 使用 Schedule Master 的 DateTime / DayConfiguration 等类型

namespace MyGame.Time
{
    /// <summary>
    /// AP 驱动的时间推进器（按阶段推进：早/中/晚/夜）
    /// 它会广播自己的 OnDateTimeChangedAP 事件，供派生的 ScheduleManager 订阅
    /// </summary>
    public class APTimeManager : MonoBehaviour
    {
        public int maxAPPerDay = 4;
        public int currentAP;
        public TimeConfigurationSO timeConfiguration; // 可选：用来读取 DayConfiguration / 初始日期等

        public enum Phase { Morning = 0, Midday = 1, Evening = 2, Night = 3 }
        public Phase currentPhase = Phase.Morning;

        // 对外事件（AP 驱动的“Tick”）
        public static event UnityAction<Slax.Schedule.DateTime> OnDateTimeChangedAP = delegate { };
        public static event UnityAction OnInBetweenTickFiredAP = delegate { };

        // 内部保存的日期时间（类似 TimeManager 的 _dateTime）
        private Slax.Schedule.DateTime _dateTime;

        void Awake()
        {
            // 初始化 currentAP 和 _dateTime（若有外部 timeConfig，使用它；否则默认）
            currentAP = maxAPPerDay;
            if (timeConfiguration != null)
            {
                // 读取初始时间配置（示例：使用 config 的 values）
                var dc = timeConfiguration.DayConfiguration;
                // 这里取配置的 season/year/date/hour/minutes（可扩展）
                _dateTime = new Slax.Schedule.DateTime(1, (int)timeConfiguration.Season, timeConfiguration.Year, timeConfiguration.Hour, timeConfiguration.Minutes, dc);
            }
            else
            {
                // 默认示例
                var defaultDC = new Slax.Schedule.DayConfiguration { MorningStartHour = 8, AfternoonStartHour = 12, NightStartHour = 20 };
                _dateTime = new Slax.Schedule.DateTime(1, 0, 1, 8, 0, defaultDC);
            }
        }

        /// <summary>
        /// 玩家执行一个会消耗行动点的操作时调用（UI/逻辑处调用）
        /// </summary>
        public void ConsumeAP(int cost = 1)
        {
            currentAP -= cost;
            if (currentAP <= 0)
            {
                // AP 用尽 -> 推进一个阶段（或多阶段，见 AdvanceBySteps）
                AdvancePhase();
                // 重置 AP（或按你游戏逻辑设为 0，直到下一日恢复）
                currentAP = maxAPPerDay;
            }
            else
            {
                // 可选：触发“in-between”回调用于 UI 刷新
                OnInBetweenTickFiredAP?.Invoke();
            }
        }

        /// <summary>
        /// 进入下一个阶段（步骤可扩展）
        /// </summary>
        public void AdvancePhase(int steps = 1)
        {
            for (int i = 0; i < steps; i++)
            {
                currentPhase = (Phase)(((int)currentPhase + 1) % 4);

                // 根据 currentPhase 生成合适的小时（示例映射）
                int hour = PhaseToHour(currentPhase);
                // 使用当前的 dayConfiguration（来自 timeConfiguration 或 默认）
                Slax.Schedule.DayConfiguration dc = timeConfiguration != null ? timeConfiguration.DayConfiguration : new Slax.Schedule.DayConfiguration { MorningStartHour = 8, AfternoonStartHour = 12, NightStartHour = 20 };

                // 生成新的 DateTime（如果从 Night -> Morning，则 advance day）
                if ((int)currentPhase == (int)Phase.Morning)
                {
                    // 跨日：年的日期＋1 或使用 DateTime.SetNewDay 的逻辑（可复用 TimeManager 的方法）
                    // 这里简单示例增加一天（项目中应使用更完整的日历逻辑）
                    // 注意 Slax.Schedule.DateTime 是 struct，没有 SetDate public API exposed in this simplified snippet.
                }

                _dateTime = new Slax.Schedule.DateTime(_dateTime.Date + ((int)currentPhase == 0 ? 1 : 0), (int)_dateTime.Season, _dateTime.Year, hour, 0, dc);

                // 广播 AP 驱动的时间变化（供 APDrivenScheduleManager 订阅）
                OnDateTimeChangedAP?.Invoke(_dateTime);
            }
        }

        private int PhaseToHour(Phase p)
        {
            if (timeConfiguration != null)
            {
                var dc = timeConfiguration.DayConfiguration;
                switch (p)
                {
                    case Phase.Morning: return dc.MorningStartHour;
                    case Phase.Midday: return dc.AfternoonStartHour;
                    case Phase.Evening: return Mathf.Max(dc.AfternoonStartHour + 4, dc.AfternoonStartHour); // 自定义逻辑
                    case Phase.Night: return dc.NightStartHour;
                }
            }
            // fallback
            return p == Phase.Morning ? 8 : p == Phase.Midday ? 12 : p == Phase.Evening ? 16 : 20;
        }

        // 公开 API：手动推进多个阶段（例如一次行动跨越多阶段）
        public void AdvanceBySteps(int steps) => AdvancePhase(steps);

        // 存档关键数据（示例：用 JSON 或 PlayerPrefs）
        public string GetSaveStateJson()
        {
            var state = new { currentAP, phase = (int)currentPhase, date = _dateTime.Date, year = _dateTime.Year };
            return JsonUtility.ToJson(state);
        }

        public void LoadFromJson(string json)
        {
            // 简化加载，实际项目请做校验
            var state = JsonUtility.FromJson<APTimeSaveState>(json);
            currentAP = state.currentAP;
            currentPhase = (Phase)state.phase;
            // 恢复 _dateTime 需要更完整的字段 —— 这里只是示例
        }

        [System.Serializable]
        private class APTimeSaveState { public int currentAP; public int phase; public int date; public int year; }
    }
}
