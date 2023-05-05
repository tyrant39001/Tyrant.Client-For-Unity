using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Tyrant.ComponentsForUnity
{
    [RequireComponent(typeof(Text))]
    public class FPS : MonoBehaviour
    {
        [Tooltip("更新频率，单位：秒")]
        public float refreshRate = 1.0f;

        private float time = 0;
        void Update()
        {
            time += Time.unscaledDeltaTime;
            if (time >= refreshRate)
            {
                time -= refreshRate;
                var text = GetComponent<Text>();
                if (text != null)
                    text.text = (1.0f / Time.unscaledDeltaTime).ToString();
            }
        }
    }
}