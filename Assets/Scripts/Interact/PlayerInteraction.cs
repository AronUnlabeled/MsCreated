using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour {

    public static Action OnHide;
    public static Action<string> OnInteract;

    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField, Range(1.5f, 5f)] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private InputAction interact;

    [SerializeField] private IInteractable current;
    private Collider lastCheckedCollider;

    private void OnEnable() => interact.Enable();
    private void OnDisable() => interact.Disable();

    private void Start() {

        if (target == null) target = Camera.main.transform;
    }
    private void Update() {

        Ray ray = new(target.position, target.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer)) {

            if (hit.collider != lastCheckedCollider) {
                
                lastCheckedCollider = hit.collider;

                if (hit.collider.TryGetComponent<IInteractable>(out var interactable)) {

                    current = interactable;
                    OnInteract?.Invoke(current.InteractionPrompt);
                } 
                else { ClearCurrentTarget(); }
            }
        } else {

            if (lastCheckedCollider != null) ClearCurrentTarget();
        }
        if (interact.WasPressedThisFrame() && current != null) {

            current.Interact();

            ClearCurrentTarget();
        }
    }
    private void ClearCurrentTarget() {
        current = null;
        lastCheckedCollider = null;
        OnHide?.Invoke();
    }
}