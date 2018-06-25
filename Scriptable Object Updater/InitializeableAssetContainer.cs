using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

namespace Lowscope.ScriptableObjectUpdater
{
    [System.Serializable]
    public class InitializeableAssetContainer : MonoBehaviour
    {
        [System.Serializable]
        public class EventData
        {
            public UnityEvent serializedEvent = new UnityEvent();
            public EEventType eventType;
            public float updateRate;
            public float delay;
            public int executionOrder;

            public Action GetSerializedEventDelegate(int eventIndex)
            {
                UnityEngine.Object target = serializedEvent.GetPersistentTarget(eventIndex);
                System.Reflection.MethodInfo targetMethod = target.GetType().GetMethod(serializedEvent.GetPersistentMethodName(eventIndex));

#if NET_4_6
                return targetMethod.CreateDelegate(typeof(Action), target) as Action;
#else
                return Delegate.CreateDelegate(typeof(Action), target, serializedEvent.GetPersistentMethodName(eventIndex)) as Action;
#endif
            }
        }

        // Unity Events are more costly than C# delegates
        // So we only use the events for serialization purposes.

        [SerializeField]
        private List<EventData> eventData = new List<EventData>();

        private AwakeTick awakeTicker;
        private StartTick startTicker;
        private UpdateTicker updateTicker;
        private FixedUpdateTicker fixedUpdateTicker;
        private LateUpdateTicker lateUpdateTicker;

        public void AddEvent(EventData data)
        {
            eventData.Add(data);
        }

        public void InitializeEvents(bool forceAwake)
        {
            for (int i = 0; i < eventData.Count; i++)
            {
                AssignEventData(eventData[i]);
            }

            OrderEventExecutions();

            if (forceAwake && awakeTicker != null)
            {
                awakeTicker.Awake();
            }
        }

        private void OrderEventExecutions()
        {
            if (awakeTicker != null)
            {
                awakeTicker.Events = awakeTicker.Events.OrderBy(u => u.ExecutionOrder).ToList();
            }

            if (startTicker != null)
            {
                startTicker.Events = startTicker.Events.OrderBy(u => u.ExecutionOrder).ToList();
            }

            if (updateTicker != null)
            {
                updateTicker.Events = updateTicker.Events.OrderBy(u => u.ExecutionOrder).ToList();
            }

            if (fixedUpdateTicker != null)
            {
                fixedUpdateTicker.Events = fixedUpdateTicker.Events.OrderBy(u => u.ExecutionOrder).ToList();
            }

            if (lateUpdateTicker != null)
            {
                lateUpdateTicker.Events = lateUpdateTicker.Events.OrderBy(u => u.ExecutionOrder).ToList();
            }
        }

        private void AssignEventData(EventData data)
        {
            int eventCount = data.serializedEvent.GetPersistentEventCount();

            for (int i = 0; i < eventCount; i++)
            {
                UpdateableEvent updateEventContainer = new UpdateableEvent
                    (
                    data.GetSerializedEventDelegate(i) as System.Action,
                    data.updateRate,
                    data.delay,
                    data.executionOrder,
                    this
                    );

                switch (data.eventType)
                {
                    case EEventType.Awake:

                        if (awakeTicker == null)
                        {
                            awakeTicker = gameObject.AddComponent<AwakeTick>();
                        }

                        AddEventToContainer(awakeTicker.Events, updateEventContainer);
                        awakeTicker.Awake();

                        break;
                    case EEventType.Start:

                        if (startTicker == null)
                        {
                            startTicker = gameObject.AddComponent<StartTick>();
                        }

                        AddEventToContainer(startTicker.Events, updateEventContainer);

                        break;
                    case EEventType.Update:

                        if (updateTicker == null)
                        {
                            updateTicker = gameObject.AddComponent<UpdateTicker>();
                        }

                        AddEventToContainer(updateTicker.Events, updateEventContainer);

                        break;
                    case EEventType.FixedUpdate:

                        if (fixedUpdateTicker == null)
                        {
                            fixedUpdateTicker = gameObject.AddComponent<FixedUpdateTicker>();
                        }

                        AddEventToContainer(fixedUpdateTicker.Events, updateEventContainer);

                        break;
                    case EEventType.LateUpdate:

                        if (lateUpdateTicker == null)
                        {
                            lateUpdateTicker = gameObject.AddComponent<LateUpdateTicker>();
                        }

                        AddEventToContainer(lateUpdateTicker.Events, updateEventContainer);

                        break;
                    default:
                        break;
                }
            }
        }

        private static void AddEventToContainer(List<UpdateableEvent> target, UpdateableEvent updateEventContainer)
        {
            if (updateEventContainer == null)
            {
                return;
            }

            if (target == null)
            {
                target = new List<UpdateableEvent>();
            }

            target.Add(updateEventContainer);
        }

        private void Awake()
        {
            InitializeEvents(false);
        }
    }

}
