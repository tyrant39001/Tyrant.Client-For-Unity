using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[SelectionBase]
public class ProgressBar : MonoBehaviour
{
    [SerializeField]
    private Image progressImage = null;
    [SerializeField]
    private float minValue = 0.0f;
    [SerializeField]
    [HideInInspector]
    private float value = 5.0f;
    [SerializeField]
    private float maxValue = 10.0f;
    [SerializeField]
    private ProgressBarType type = ProgressBarType.Horizontal;

    public Image ProgressImage
    {
        get { return progressImage; }
        set
        {
            progressImage = value;
            OnValidate();
        }
    }

    public float MinValue
    {
        get { return minValue; }
        set
        {
            minValue = value;
            OnValidate();
        }
    }

    public float Value
    {
        get { return Value; }
        set
        {
            this.value = value;
            OnValidate();
        }
    }

    public float MaxValue
    {
        get { return maxValue; }
        set
        {
            maxValue = value;
            OnValidate();
        }
    }

    public ProgressBarType Type
    {
        get { return type; }
        set
        {
            type = value;
            OnValidate();
        }
    }

    void OnValidate()
    {
        if (progressImage == null)
            return;
        progressImage.type = Image.Type.Filled;
        switch (type)
        {
            case ProgressBarType.Horizontal:
                progressImage.fillMethod = Image.FillMethod.Horizontal;
                break;
            case ProgressBarType.Vertical:
                progressImage.fillMethod = Image.FillMethod.Vertical;
                break;
            case ProgressBarType.Circle:
                progressImage.fillMethod = Image.FillMethod.Radial360;
                break;
        }
        progressImage.fillAmount = Mathf.Clamp01((value - minValue) / (maxValue - minValue));
    }
}

public enum ProgressBarType
{
    Horizontal,
    Vertical,
    Circle,
}