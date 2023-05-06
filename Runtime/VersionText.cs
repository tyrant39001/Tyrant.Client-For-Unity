using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Tyrant.ComponentsForUnity
{
    [RequireComponent(typeof(Text))]
    public class VersionText : MonoBehaviour
    {
        void Awake()
        {
            var text = GetComponent<Text>();
            if (text != null)
                text.raycastTarget = false;
                text.text = Application.version;
        }
    }
}
