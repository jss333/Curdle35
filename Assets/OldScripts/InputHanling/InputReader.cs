using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, InputActions.IGameplayActions
{
    #region Singleton

    public static InputReader Singleton;
    
    private void Awake()
    {
        if (Singleton == null)
            Singleton = this;
        else
        {
            if (Singleton != this)
                Destroy(gameObject);
        }
    }

    #endregion
    
    InputActions inputActions;


    public Action<bool> OnCameraPanning;
    public Action<float> OnMouseZoom;
    public Action<Vector2> OnUpdateMousePosition;
    public Vector2 MouseDelta;
    
    
    void Start()
    {
        inputActions = new InputActions();
        
        inputActions.Gameplay.SetCallbacks(this);
        inputActions.Gameplay.Enable();
    }

    void Update()
    {
    }

    public void OnCameraPan(InputAction.CallbackContext context)
    {
        OnCameraPanning?.Invoke(context.ReadValueAsButton());
    }

    public void OnCameraPanningVector(InputAction.CallbackContext context)
    {
        MouseDelta = context.ReadValue<Vector2>();
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        OnUpdateMousePosition?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnCameraZoom(InputAction.CallbackContext context)
    {
        //Debug.Log($"OnCameraZoomIn: {context.ReadValue<Vector2>()}");
        OnMouseZoom?.Invoke(context.ReadValue<Vector2>().y);
    }
}
