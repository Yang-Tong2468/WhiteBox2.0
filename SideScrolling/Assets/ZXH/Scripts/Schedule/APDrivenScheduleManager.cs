using UnityEngine;
using Slax.Schedule;

namespace MyGame.Time
{
    /// <summary>
    /// 继承自 ScheduleManager，但改为订阅 APTimeManager 的事件
    /// 这样你可以保持 ScheduleMaster 的事件查找与广播逻辑不变
    /// </summary>
    public class APDrivenScheduleManager : ScheduleManager
    {
        protected override void OnEnable()
        {
            // 不调用 base.OnEnable()，避免它订阅原 TimeManager 的事件
            // base.OnEnable();

            // 订阅我们的 APTimeManager 事件
            APTimeManager.OnDateTimeChangedAP += CheckEventOnTick;
            APTimeManager.OnInBetweenTickFiredAP += FireInBetweenTick;
        }

        protected override void OnDisable()
        {
            APTimeManager.OnDateTimeChangedAP -= CheckEventOnTick;
            APTimeManager.OnInBetweenTickFiredAP -= FireInBetweenTick;
            // 不调用 base.OnDisable() 同样避免取消原订阅
            // base.OnDisable();
        }
    }
}
