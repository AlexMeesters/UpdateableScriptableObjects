using UnityEngine;
using System.Collections;

namespace Lowscope.ScriptableObjectUpdater
{
    public class UpdateTicker : Ticker
    {
        private void Update()
        {
            DispatchTick();
        }
    }
}