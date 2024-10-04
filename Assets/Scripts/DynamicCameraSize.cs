using UnityEngine;

public class DynamicCameraSize : MonoBehaviour
{
    public Vector2 referenceResolution = new Vector2(4, 4);
    public float deciderRatio = 1.0f;
    private Camera _camera;

    void Start()
    {
        _camera = GetComponent<Camera>();
        AdjustCameraSize();
    }

    void FixedUpdate()
    {
        AdjustCameraSize();
    }
    private void AdjustCameraSize()
    {
        float targetWidth = referenceResolution.x;
        float targetHeight = referenceResolution.y;
        float aspectRatio = (float)Screen.width / Screen.height;

        if (aspectRatio >= deciderRatio)
        {
            _camera.orthographicSize = targetHeight / 2;
        }
        else
        {
            _camera.orthographicSize = (targetWidth / 2) / aspectRatio;
        }
    }
}