using UnityEngine;
using System.Collections;
using System;

namespace Lowscope.ScriptableObjectUpdater
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UpdateScriptableObjectAttribute : Attribute
    {
        public EEventType eventType { get; set; }
        public float Delay { get; set; }
        public float tickDelay { get; set; }
        public int ExecutionOrder { get; set; }
    }
}
