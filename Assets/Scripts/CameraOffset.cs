using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraOffset : MonoBehaviour
{
    [Range(-1f, 1f)]
    public float verticalOffset = 0.1f;

    private Camera cam;

    void OnValidate()
    {
        cam = GetComponent<Camera>();
        ApplyOffset();
    }

    void ApplyOffset()
    {
        if (cam == null) return;

        Matrix4x4 p = cam.projectionMatrix;
        p[1, 2] = verticalOffset;
        cam.projectionMatrix = p;
    }

    void OnDisable()
    {
        if (cam != null)
            cam.ResetProjectionMatrix();
    }
}