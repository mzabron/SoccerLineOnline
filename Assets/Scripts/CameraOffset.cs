using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraOffset : MonoBehaviour
{
    [Range(-1f, 1f)]
    public float verticalOffset = 0.05f;

    [Header("Field Fitting")]
    public bool autoFitToField = true;
    public int fieldWidth = 9;
    public int fieldHeight = 11;

    private Camera cam;
    private bool hasInitialized = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void OnValidate()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        // Only apply in edit mode when values change
        if (!Application.isPlaying && autoFitToField)
        {
            FitPerspectiveCameraToField();
        }
    }

    void OnEnable()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        hasInitialized = false;
    }

    void Start()
    {
        // Initialize camera positioning at Start to ensure proper timing
        if (Application.isPlaying && autoFitToField && !hasInitialized)
        {
            InitializeCamera();
        }
    }

    void LateUpdate()
    {
        if (Application.isPlaying)
        {
            if (cam == null)
                cam = GetComponent<Camera>();

            // Ensure camera is initialized on first frame
            if (!hasInitialized && autoFitToField)
            {
                InitializeCamera();
            }

            ApplyOffset();
        }
    }

    private void InitializeCamera()
    {
        if (cam == null) return;

        FitPerspectiveCameraToField();
        hasInitialized = true;
    }

    void ApplyOffset()
    {
        if (cam == null) return;

        Matrix4x4 p = cam.projectionMatrix;
        p[1, 2] = verticalOffset;
        cam.projectionMatrix = p;
    }

    public void FitPerspectiveCameraToField()
    {
        if (cam == null) return;

        float fieldWidthFloat = fieldWidth;
        float fieldHeightFloat = fieldHeight;

        Vector3 center = new Vector3((fieldWidth - 1) / 2f, 0, (fieldHeight - 1) / 2f);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        float aspect = (float)Screen.width / Screen.height;
        float fovRad = cam.fieldOfView * Mathf.Deg2Rad;

        float halfFieldWidth = fieldWidthFloat / 2f;
        float halfFieldHeight = fieldHeightFloat / 2f;

        float distanceForHeight = halfFieldHeight / Mathf.Tan(fovRad / 2f);
        float distanceForWidth = halfFieldWidth / (Mathf.Tan(fovRad / 2f) * aspect);

        float requiredDistance = Mathf.Max(distanceForHeight, distanceForWidth);
        cam.transform.position = center + new Vector3(0, requiredDistance, 0);

        Debug.Log($"Camera positioned at: {cam.transform.position}, looking at center: {center}");
    }

    void OnDisable()
    {
        if (cam != null)
            cam.ResetProjectionMatrix();
    }

    // Public method to manually trigger camera fitting (useful for debugging)
    [ContextMenu("Fit Camera to Field")]
    public void ManualFitCamera()
    {
        if (cam == null)
            cam = GetComponent<Camera>();
        FitPerspectiveCameraToField();
    }
}