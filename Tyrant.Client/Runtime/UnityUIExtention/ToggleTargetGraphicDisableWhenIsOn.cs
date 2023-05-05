using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
[ExecuteInEditMode]
public class ToggleTargetGraphicDisableWhenIsOn : MonoBehaviour
{
    void OnEnable()
    {
        var toggle = GetComponent<Toggle>();
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(toggle.isOn);
        }
    }

    void OnDisable()
    {
        var toggle = GetComponent<Toggle>();
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(bool isOn)
    {
        var toggle = GetComponent<Toggle>();
        if (toggle != null)
            toggle.targetGraphic.enabled = !isOn;
    }
}
