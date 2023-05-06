using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Tyrant.ComponentsForUnity
{
    [RequireComponent(typeof(Text))]
    public class DebugText : MonoBehaviour
    {
        private static List<DebugUnit> debugUnitsList = new List<DebugUnit>();

        public static DebugUnit RequestDebugUnit()
        {
            DebugUnit debugUnit = new DebugUnit();
            debugUnitsList.Add(debugUnit);
            return debugUnit;
        }

        void Update()
        {
            var text = GetComponent<Text>();
            if (text != null)
            {
                text.raycastTarget = false;
                text.text = "";
                foreach (var debugUnit in debugUnitsList)
                    text.text += debugUnit.Text + Environment.NewLine;
            }
        }
    }

    public class DebugUnit
    {
        public string Text { get; set; }

        internal DebugUnit() { }
    }
}