using UnityEngine;
using System.Collections;

public class Render3DToSprite : MonoBehaviour
{
    [SerializeField, Tooltip("3d资源, 要求直立于y轴，调整旋转使背部朝向屏幕")]
    private GameObject resource3d;
    [SerializeField]
    private LayerMask layer = 0;
    [SerializeField]
    private int textureSizeX = 100;
    [SerializeField]
    private int textureSizeY = 100;
    [SerializeField, Tooltip("转向速度, 小于或等于0为禁用转向，单位:角度/秒")]
    private float rotateSpeed = 45;
    [SerializeField]
    private string isWalkingAnimatorParameter = "IsWalking";

    //private Quaternion form, to;
    private void OnEnable()
    {
        Init3DRole();
    }

    private Camera camera3dRole;
    private float currentAngle = 0;
    private Vector3 role3DInstanceInitEuler;
    private GameObject role3DInstance = null;

    public GameObject Resource3d
    {
        get { return resource3d; }
        set
        {
            resource3d = value;
            Init3DRole();
        }
    }

    private bool isWalking;
    public bool IsWalking
    {
        get { return isWalking; }
        set
        {
            if (isWalking == value)
                return;
            isWalking = value;
            var anmimator = role3DInstance?.GetComponent<Animator>();
            if (anmimator != null && !string.IsNullOrEmpty(isWalkingAnimatorParameter))
                anmimator.SetBool(isWalkingAnimatorParameter, value);
        }
    }

    private void Init3DRole()
    {
        if (resource3d == null)
            return;
        //var rectTransform = GetComponent<RectTransform>();
        //if (rectTransform == null)
        //    throw new ApplicationException($"can not get {nameof(RectTransform)} for {nameof(GridBase2DRole)}");

        if (camera3dRole == null)
        {
            var gameObj = new GameObject("Camera3dRole");
            camera3dRole = gameObj.AddComponent<Camera>();
            camera3dRole.clearFlags = CameraClearFlags.SolidColor;
            var rt = RenderTexture.GetTemporary(textureSizeX, textureSizeY, 24);
            rt.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            camera3dRole.targetTexture = rt;

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            var texture2D = new Texture2D(rt.width, rt.height);
            texture2D.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            var sprite = Sprite.Create(texture2D, new Rect(0, 0, rt.width, rt.height), new Vector2(0.5f, 0.5f));
            sprite.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            spriteRenderer.sprite = sprite;
        }
        else
        {
            camera3dRole.targetTexture.width = textureSizeX;
            camera3dRole.targetTexture.height = textureSizeY;
        }
        camera3dRole.cullingMask = 1 << layer;

        if (role3DInstance != null)
            Destroy(role3DInstance);
        role3DInstance = Instantiate(resource3d, camera3dRole.transform, false);
        role3DInstanceInitEuler = role3DInstance.transform.eulerAngles;
        ChangeGameObjectLayer(role3DInstance.transform, layer);
    }

    private void ChangeGameObjectLayer(Transform parent, int layer)
    {
        parent.gameObject.layer = layer;
        foreach (Transform child in parent)
            ChangeGameObjectLayer(child, layer);
    }

    private void Update()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (camera3dRole != null && spriteRenderer != null)
        {
            var currentActiveRT = RenderTexture.active;
            RenderTexture.active = camera3dRole.targetTexture;
            var texture2D = spriteRenderer.sprite.texture;
            texture2D.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = currentActiveRT;
        }
    }

    private IEnumerator currentRotateCoroutine = null;
    /// <summary>
    /// 根据目标方向旋转3d资源实例
    /// </summary>
    /// <param name="targetDir">目标方向，可用目标点减起始点得到</param>
    /// <returns></returns>
    public void StartRotate(Vector2 targetDir)
    {
        StartCoroutine(StartRotateAsync(targetDir));
    }

    public IEnumerator StartRotateAsync(Vector2 targetDir)
    {
        StopRotate();
        currentRotateCoroutine = RotateCoroutine(targetDir);
        return currentRotateCoroutine;
    }

    public void StopRotate()
    {
        if (currentRotateCoroutine != null)
        {
            StopCoroutine(currentRotateCoroutine);
            currentRotateCoroutine = null;
        }
    }

    private IEnumerator RotateCoroutine(Vector2 targetDir)
    {
        if (rotateSpeed <= 0.0f)
            yield break;

        var currentAngleRad = currentAngle * Mathf.Deg2Rad;
        var currentDir = new Vector2(Mathf.Sin(currentAngleRad), Mathf.Cos(currentAngleRad));
        var rotateAngle = Vector2.Angle(currentDir, targetDir); // 需要旋转的角度
        bool isClockWise = Vector3.Cross(currentDir, targetDir).z < 0; // 是否是顺时针旋转
        var targetAngle = currentAngle + (isClockWise ? rotateAngle : -rotateAngle); // 旋转之后的角度
        bool isBreak = false;
        //UnityEngine.Debug.Log($"bbbbbbbbbb-currentAngle:{currentAngle} currentDir:{currentDir} targetDir{targetDir} rotateAngle:{rotateAngle} targetAngle:{targetAngle} isClockWise:{isClockWise}");
        while (true)
        {
            yield return null;
            var deltaAngle = Time.deltaTime * rotateSpeed;
            currentAngle += isClockWise ? deltaAngle : -deltaAngle;
            //UnityEngine.Debug.Log($"isClockWise:{isClockWise} currentAngle:{currentAngle}");
            if (isBreak = isClockWise ? currentAngle > targetAngle : currentAngle < targetAngle)
                currentAngle = targetAngle;
            //UnityEngine.Debug.Log($"deltaAngle:{(isClockWise ? deltaAngle : -deltaAngle)} currentAngle:{currentAngle}");
            role3DInstance.transform.eulerAngles = new Vector3(role3DInstanceInitEuler.x, role3DInstanceInitEuler.y + currentAngle, role3DInstanceInitEuler.z);
            if (isBreak)
                break;
        }
    }
}