using System;
using Unity.Cinemachine;
using UnityEngine;

public class OldCameraController : MonoBehaviour
{
    public CinemachineCamera virtualCamera;
    public float panSpeed = 10f;
    public float minZoom = 1f;
    public float maxZoom = 10f;
    
    
    private bool isPanning = false;
    private Vector2 currentPanPosition;
    private float mouseScroll;
    
    private void Start()
    {
        InputReader.Singleton.OnCameraPanning += IsPanning;
        InputReader.Singleton.OnUpdateMousePosition += OnMousePositionUpdate;
        InputReader.Singleton.OnMouseZoom += OnMouseScroll;
    }

    private void OnDestroy()
    {
        InputReader.Singleton.OnCameraPanning -= IsPanning;
        InputReader.Singleton.OnUpdateMousePosition -= OnMousePositionUpdate;
        InputReader.Singleton.OnMouseZoom -= OnMouseScroll;
    }

    void Update()
    {
        if (isPanning)
        {
            if (Vector2.Distance(currentPanPosition, new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)) > 0.1f)
            {
                Vector3 pan = new Vector3(InputReader.Singleton.MouseDelta.normalized.x, InputReader.Singleton.MouseDelta.normalized.y , 0);
                virtualCamera.transform.position += pan * (panSpeed * Time.deltaTime); 
            }
        }
        
        virtualCamera.Lens.OrthographicSize = Mathf.Clamp(virtualCamera.Lens.OrthographicSize - mouseScroll, minZoom, maxZoom);
    }

    void IsPanning(bool _isPanning)
    {
        isPanning = _isPanning;
    }

    void OnMousePositionUpdate(Vector2 _mousePosition)
    {
        currentPanPosition = _mousePosition;
    }

    void OnMouseScroll(float _mouseScroll)
    {
        mouseScroll = _mouseScroll;
    }
}
