using UnityEngine;
using System.Collections;

namespace Lowscope.ScriptableObjectUpdater
{
    public class AwakeTick : Ticker
    {
        public void Awake()
        {
            DispatchTick();
        }
    }
}