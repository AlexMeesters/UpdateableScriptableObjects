using UnityEngine;
using System.Collections;

namespace Lowscope.ScriptableObjectUpdater
{
    public class StartTick : Ticker
    {
        private void Start()
        {
            DispatchTick();
        }
    }
}