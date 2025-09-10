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

        public TimeConfigurationSO timeConfiguration;

        public enum Phase { Morning = 0, Midday, Evening, Night }
        public Phase currentPhase = Phase.Morning;

        public static event UnityAction<DateTime> OnDateTimeChangedAP = delegate { };
        public static event UnityAction OnInBetweenTickFiredAP = delegate { };

        private DateTime _dateTime;

        void Awake()
        {
            currentAP = maxAPPerDay;
            if (timeConfiguration != null)
            {
                _dateTime = new DateTime(
                    timeConfiguration.Date,
                    (int)timeConfiguration.Season,
                    timeConfiguration.Year,
                    timeConfiguration.Hour,
                    //timeConfiguration.Minutes
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
            Debug.Log($"[APTimeManager] Tick to {_dateTime.DateToString()} {_dateTime.TimeToString()}");
            // 初始通知
            OnDateTimeChangedAP?.Invoke(_dateTime);
        }


        [ContextMenu("Consume 1 AP (Debug)")]
        private void ContextConsumeAP()
        {
            ConsumeAP();
        }

        public void ConsumeAP(int cost = 1)
        {
            currentAP -= cost;
            if (currentAP <= 0)
            {
                AdvancePhases(1);
                currentAP = maxAPPerDay;
            }
            else
            {
                OnInBetweenTickFiredAP?.Invoke();
            }
        }

        public void AdvancePhases(int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                currentPhase = (Phase)(((int)currentPhase + 1) % 4);

                int hour = currentPhase switch
                {
                    Phase.Morning => timeConfiguration.DayConfiguration.MorningStartHour,
                    Phase.Midday => timeConfiguration.DayConfiguration.AfternoonStartHour,
                    Phase.Evening => timeConfiguration.DayConfiguration.EveningStartHour,
                    Phase.Night => timeConfiguration.DayConfiguration.NightStartHour,
                    _ => timeConfiguration.DayConfiguration.MorningStartHour
                };

                if (currentPhase == Phase.Morning)
                {
                    var status = _dateTime.SetNewDay();
                    // SetNewDay handles date/season/year transitions
                }
                _dateTime = new DateTime(
                    _dateTime.Date,
                    (int)_dateTime.Season,
                    _dateTime.Year,
                    hour,
                    0,
                    _dateTime.DayConfiguration
                );

                Debug.Log($"[APTimeManager] Tick to {_dateTime.DateToString()} {_dateTime.TimeToString()}");
                OnDateTimeChangedAP?.Invoke(_dateTime);
            }
        }
        //private int PhaseToHour(Phase p)
        //{
        //    if (timeConfiguration != null)
        //    {
        //        var dc = timeConfiguration.DayConfiguration;
        //        switch (p)
        //        {
        //            case Phase.Morning: return dc.MorningStartHour;
        //            case Phase.Midday: return dc.AfternoonStartHour;
        //            case Phase.Evening: return Mathf.Max(dc.AfternoonStartHour + 4, dc.AfternoonStartHour); // 自定义逻辑
        //            case Phase.Night: return dc.NightStartHour;
        //        }
        //    }
        //    // fallback
        //    return p == Phase.Morning ? 8 : p == Phase.Midday ? 12 : p == Phase.Evening ? 16 : 20;
        //}

        // 手动推进多个阶段（如一次行动跨越多阶段）
        public void AdvanceBySteps(int steps) => AdvancePhases(steps);

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
            // 恢复 _dateTime 需要更完整的字段
        }

        [System.Serializable]
        private class APTimeSaveState { public int currentAP; public int phase; public int date; public int year; }
    }
}
