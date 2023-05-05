using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

namespace Tyrant.ComponentsForUnity
{
    public class DeactiveGameObject : MonoBehaviour
    {
        void DeactiveAttachedGameObject()
        {
            gameObject.SetActive(false);
        }
    }
}