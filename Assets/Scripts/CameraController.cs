using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 15f;

    [Header("Panning Settings")]
    [SerializeField] private float panSpeed = 5f;
    [SerializeField] private float dragSpeed = 0.1f;

    private Camera cam;
    private Vector3 dragOrigin;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleZoom();
        HandlePanning();
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            cam.orthographicSize -= scrollInput * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    void HandlePanning()
    {
        // Keyboard Panning (AWSD)
        Vector3 move = new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"),
            0) * panSpeed * Time.deltaTime;
        transform.position += move;

        // Mouse Drag Panning (Right Click)
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position += difference * dragSpeed;
        }
    }
}
