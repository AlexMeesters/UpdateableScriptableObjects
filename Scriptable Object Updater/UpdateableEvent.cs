using UnityEngine;
using System.Collections;
using System;

namespace Lowscope.ScriptableObjectUpdater
{
    [System.Serializable]
    public class UpdateableEvent
    {
        public UpdateableEvent(Action action, float updateRate, float delay, int executionOrder, InitializeableAssetContainer assetContainer)
        {
            this.tickableEvent = action;
            this.updateRate = updateRate;
            this.startDelay = delay;
            this.executionOrder = executionOrder;
            this.assetContainer = assetContainer;
        }

        private Action tickableEvent;

        private float updateRate;
        private float startDelay;
        private bool utilizingDelay;

        private float t;

        private InitializeableAssetContainer assetContainer;

        private int executionOrder;
        public int ExecutionOrder { get { return executionOrder; } }

        public void Tick()
        {
            if (startDelay != 0)
            {
                if (!utilizingDelay)
                {
                    utilizingDelay = true;
                    assetContainer.StartCoroutine(TickDelayed());
                }
                return;
            }

            if (t <= 0)
            {
                tickableEvent.Invoke();

                t = updateRate;
            }
            else
            {
                t -= Time.deltaTime;
            }
        }

        private IEnumerator TickDelayed()
        {
            yield return new WaitForSeconds(startDelay);
            tickableEvent.Invoke();
            startDelay = 0;
        }
    }
}