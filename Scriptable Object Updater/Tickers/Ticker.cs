using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lowscope.ScriptableObjectUpdater
{
    public class Ticker : MonoBehaviour
    {
        public List<UpdateableEvent> Events = new List<UpdateableEvent>();

        protected void DispatchTick()
        {
            for (int i = 0; i < Events.Count; i++)
            {
                Events[i].Tick();
            }
        }
    }
}