using UnityEngine;
using System.Collections;

namespace Lowscope.ScriptableObjectUpdater
{
    public class FixedUpdateTicker : Ticker
    {
        private void FixedUpdate()
        {
            DispatchTick();
        }
    }
}
