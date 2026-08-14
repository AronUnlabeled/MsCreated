using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour {

    private const string speedParameter = "Speed";
    private const string jumpParameter = "Jump";
    private const string isRunningParameter = "IsRunning";
    private const string isSprintingParameter = "IsSprinting";
    private const string groundParameter = "Grounded";
    private const string fallingParameter = "Falling";

    private const float lookThreshold = 0.1f;

    [Header("Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody rb;

    [Header("Cinemachine")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float topClamp = 70f, bottomClamp = -30f;

    [Header("Movement")]
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSpeed = 10f;

    [Header("Ground Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump Settings")]
    [SerializeField, Range(5, 10)] private float jumpForce = 7f;
    [SerializeField] private float jumpCooldown = 0.1f;
    private float jumpTimeoutDelta = 0f;
    private const float jumpTimeout = 0.15f;
    
    private Vector2 look, move;
    private float pitch, yaw;
    [SerializeField] private bool isGrounded = true; 
    [SerializeField] private bool canJump = true;
    private bool isRunning, isSprinting;

    private void Start() {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
    private void Update() {
        CheckGround();
    }
    private void LateUpdate() {
        Look();
    }
    private void FixedUpdate() {
        Move();
    }
    private void Jump() {
        Debug.Log("Entered Jump");
        if (!isGrounded || !canJump) return;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        
        canJump = false;
        isGrounded = false;

        jumpTimeoutDelta = jumpTimeout;

        anim.SetTrigger(jumpParameter);
        
        StartCoroutine(ResetJumpCoroutine());
    }
    private IEnumerator ResetJumpCoroutine() {

        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }
    private void CheckGround() {

        if (jumpTimeoutDelta > 0f) {

            jumpTimeoutDelta -= Time.deltaTime;
            return;
        }
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        
        //anim.SetBool(groundParameter, isGrounded);
    }
    private void Look() {
        
        if(look.sqrMagnitude >= lookThreshold) {
            
            float deltaTimeMultiplier = Time.deltaTime * lookSpeed;
            yaw += look.x * deltaTimeMultiplier;
            pitch -= look.y * deltaTimeMultiplier;
        }
        
        yaw = ClampAngle(yaw, float.MinValue, float.MaxValue);
        pitch = ClampAngle(pitch, bottomClamp, topClamp);

        cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
    private float ClampAngle(float angle, float min, float max) {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
    private void Move() {
        float targetSpeed = (isRunning ? moveSpeed * 1.5f : moveSpeed) * move.magnitude;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * 10f);

        Vector3 forward = cameraTarget.forward;
        Vector3 right = cameraTarget.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * move.y + right * move.x).normalized;

        if (moveDirection.sqrMagnitude > 0.01f) {

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), Time.fixedDeltaTime * 10f);

            Vector3 currentVelocity = rb.velocity;
            rb.velocity = new Vector3(moveDirection.x * currentSpeed, currentVelocity.y, moveDirection.z * currentSpeed);
        } else { 
            Vector3 currentVelocity = rb.velocity;
            rb.velocity = new Vector3(moveDirection.x * currentSpeed, currentVelocity.y, moveDirection.z * currentSpeed);
        }
        float normalizedSpeed = currentSpeed / (moveSpeed * 2);
        //anim for walking.
    }

    private void OnMove(InputValue value) { move = value.Get<Vector2>(); }
    private void OnJump(InputValue value) {
        Debug.Log("Enter Jump Input");
        if (value.isPressed) {
            Debug.Log("Enter Jump");
            Jump(); 
        } 
    }
    private void OnRun(InputValue value) { isRunning = value.isPressed; }
    private void OnSprint(InputValue value) { isSprinting = value.isPressed; }
    private void OnLook(InputValue value) { look = value.Get<Vector2>(); }
    private void OnDrawGizmosSelected() {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}