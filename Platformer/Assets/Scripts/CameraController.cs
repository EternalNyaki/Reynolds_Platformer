using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Tilemap tilemap;
    public Vector2 offset;
    public Camera followCamera;
    public float smoothing;

    private Vector2 _viewportHalfSize;

    private float _leftBoundary;
    private float _rightBoundary;
    private float _bottomBoundary;

    private Vector3 _shakeOffset;

    // Start is called before the first frame update
    void Start()
    {
        tilemap.CompressBounds();
        CalculateBounds();
    }

    private void CalculateBounds()
    {
        _viewportHalfSize = new(followCamera.aspect * followCamera.orthographicSize, followCamera.orthographicSize);

        _leftBoundary = tilemap.transform.position.x + tilemap.cellBounds.min.x + _viewportHalfSize.x;
        _rightBoundary = tilemap.transform.position.x + tilemap.cellBounds.max.x - _viewportHalfSize.x;
        _bottomBoundary = tilemap.transform.position.y + tilemap.cellBounds.min.y + _viewportHalfSize.y;
    }

    private void LateUpdate()
    {
        Vector3 desiredPosition = target.position + new Vector3(offset.x, offset.y, transform.position.z) + _shakeOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, 1 - Mathf.Exp(-smoothing * Time.deltaTime));

        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, _leftBoundary, _rightBoundary);
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, _bottomBoundary, smoothedPosition.y);

        transform.position = smoothedPosition;
    }

    public void Shake(float intensity, float duration)
    {
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            _shakeOffset = Random.insideUnitCircle * intensity;
            elapsed += Time.deltaTime;
            yield return null;
        }
        _shakeOffset = Vector3.zero;
    }
}
