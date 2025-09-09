using UnityEngine;
using Slax.Schedule;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MyGame.Time
{
    public class APDrivenScheduleManager : TickObserver
    {
        public ScheduleEventCheckAssociationSO EventCheckAssociationData;
        public bool HasInBetweenTickEvents = false;

        protected override void OnEnable()
        {
            // 不调用 base，避免订阅原 TimeManager
            APTimeManager.OnDateTimeChangedAP += CheckEventOnTick;
            if (HasInBetweenTickEvents)
                APTimeManager.OnInBetweenTickFiredAP += FireInBetweenTick;
        }

        protected override void OnDisable()
        {
            APTimeManager.OnDateTimeChangedAP -= CheckEventOnTick;
            if (HasInBetweenTickEvents)
                APTimeManager.OnInBetweenTickFiredAP -= FireInBetweenTick;
        }

        protected override void CheckEventOnTick(DateTime date)
        {
            List<ScheduleEvent> eventsToStart = _scheduleEvents.GetEventsForTimestamp(date.GetTimestamp());
            if (eventsToStart.Count == 0) return;

            foreach (var ev in eventsToStart)
            {
                Debug.Log($"[ScheduleEvent Fired] ID: {ev.ID}, DateTime: {date.DateToString()} {date.TimeToString()}");
            }

            if (EventCheckAssociationData != null)
            {
                List<ScheduleEvent> filtered = EventCheckAssociationData.RunChecksAndGetPassedEvents(eventsToStart);
                ScheduleManager.OnScheduleEvents.Invoke(filtered);
            }
            else
            {
                ScheduleManager.OnScheduleEvents.Invoke(eventsToStart);
            }
        }

        protected override void FireInBetweenTick()
        {
            if (HasInBetweenTickEvents)
                ScheduleManager.OnInBetweenTickFired.Invoke();
        }
    }
}
