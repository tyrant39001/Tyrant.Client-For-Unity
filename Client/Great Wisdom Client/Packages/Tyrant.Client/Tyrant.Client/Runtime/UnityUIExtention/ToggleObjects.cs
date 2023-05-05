using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleObjects : MonoBehaviour
{
    public GameObject[] Activate;
    private Toggle toggle;
    //public GameObject[] Deactivate;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (toggle != null)         
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
            OnToggleValueChanged(toggle.isOn);
        }
    }

    void OnToggleValueChanged(bool check)
    {
        if (Activate != null)
        {
            foreach (var go in Activate)
            {
                if (go != null)
                    go.SetActive(check);
            }
        }
        //if (Deactivate != null)
        //{
        //    foreach (var go in Deactivate)
        //        go.SetActive(!check);
        //}
    }
}