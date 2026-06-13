using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New InputObject", menuName = "InputObject")]
public class InputObject : ScriptableObject, PlayerControls.IMovementActions {

    public event UnityAction<Vector2> Movement = delegate { };
    public event UnityAction<Vector2, bool> Look = delegate { };
    public event UnityAction EnableMouseCameraControls = delegate { };
    public event UnityAction DisableMouseCameraControls = delegate { };

    PlayerControls playerControls;

    private const string MOUSE = "Mouse";

    public Vector3 MoveDirection => playerControls.Movement.Move.ReadValue<Vector2>();

    private void OnEnable() {
        if (playerControls == null) {
            playerControls = new();
            playerControls.Movement.SetCallbacks(this);
        }
    }
    private void OnDisable() {
        playerControls.Movement.Disable();
        playerControls.UI.Disable();
    }

    public void EnablePlayerInput() => playerControls.Enable();

    public void OnJump(InputAction.CallbackContext context) {
        throw new System.NotImplementedException();
    }

    public void OnMove(InputAction.CallbackContext context) {
        throw new System.NotImplementedException();
    }

    public void OnSprint(InputAction.CallbackContext context) {
        throw new System.NotImplementedException();
    }

    public void OnLook(InputAction.CallbackContext context) {
        throw new System.NotImplementedException();
    }

    public void OnFire(InputAction.CallbackContext context) {
        throw new System.NotImplementedException();
    }

    public void OnMouseCameraControl(InputAction.CallbackContext context) {
        throw new System.NotImplementedException();
    }

    public void OnRun(InputAction.CallbackContext context) {
        throw new System.NotImplementedException();
    }
}