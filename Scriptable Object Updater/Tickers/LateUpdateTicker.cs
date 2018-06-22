using UnityEngine;
using System.Collections;

namespace Lowscope.ScriptableObjectUpdater
{
    public class LateUpdateTicker : Ticker
    {
        private void LateUpdate()
        {
            DispatchTick();
        }
    }
}