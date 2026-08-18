using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour {

    #region Animation Parameters
    private const string idleParameter = "Idle";
    private const string speedParameter = "Speed";
    private const string jumpParameter = "Jump";
    private const string isRunningParameter = "IsRunning";
    private const string isSprintingParameter = "IsSprinting";
    private const string groundParameter = "Grounded";
    private const string fallingParameter = "Falling";
    private const string attackParameter = "Attack";
    private const string shootParameter = "Shoot";
    #endregion
    #region References & Variables
    [Header("Current State")]
    [SerializeField] private PlayerState currentState = PlayerState.Movement;

    [Header("Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody rb;

    [Header("Cinemachine")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float topClamp = 70f, bottomClamp = -30f;
    private const float lookThreshold = 0.1f;

    [Header("Movement")]
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField, Range(1, 25)] private float moveSpeed = 5f;
    [SerializeField] private float lookSpeed = 10f;
    [HideInInspector] public Vector3 platformVelocity;
    [HideInInspector] public Quaternion platformRotationDelta = Quaternion.identity;
    [HideInInspector] public bool isRidingPlatform = false;

    [Header("Ground Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isGrounded = true;

    [Header("Jump Settings")]
    [SerializeField, Range(5, 25)] private float jumpForce = 12f;
    [SerializeField, Range(1f, 5f)] private float riseGravityMultiplier = 1.5f;
    [SerializeField, Range(3f, 10f)] private float fallGravityMultiplier = 4.5f;
    [SerializeField, Range(0.1f, 5f)] private float jumpCooldown = 0.1f;
    [SerializeField] private float lastHopTime = -10f;
    [SerializeField, Range(0.1f, 0.5f)] private float coyoteTimeDuration = 0.15f;
    private float coyoteTimeCounter;
    private float jumpTimeoutDelta = 0f;
    private const float jumpTimeout = 0.15f;
    [SerializeField] private bool canJump = true;
    private bool isHoldingJump = false;

    [Header("Input Buffer Settings")]
    [SerializeField] private float bufferWindow = 0.2f;
    private BufferedAction bufferedAction = BufferedAction.None;
    private float bufferTimestamp;

    private Vector2 look, move;
    private float pitch, yaw;

    private bool isRunning, isSprinting;
    #endregion
    private void Start() {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    #region Updates
    private void Update() {

        CheckGroundAndCoyote();
        ProcessInputBuffer();

        switch (currentState) {
            case PlayerState.Movement:
                break;
            case PlayerState.Melee: 
                break;
            case PlayerState.Shooting: 
                break;
        }
    }
    private void FixedUpdate() { Move(); }
    private void LateUpdate() { Look(); }
    #endregion
    #region Core
    private void CheckGroundAndCoyote() {

        if (jumpTimeoutDelta > 0f) {

            jumpTimeoutDelta -= Time.deltaTime;
            isGrounded = false;
            coyoteTimeCounter = 0;
            return;
        }
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        //anim.SetBool(groundParameter, isGrounded);

        if (isGrounded) {
            coyoteTimeCounter = coyoteTimeDuration;
        } else {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }
    private void BufferInput(BufferedAction action) {

        bufferedAction = action;
        bufferTimestamp = Time.time;
    }
    private void ProcessInputBuffer() {

        if (bufferedAction == BufferedAction.None) return;

        if (Time.time - bufferTimestamp > bufferWindow) {
            bufferedAction = BufferedAction.None;
            return;
        }
        if (bufferedAction == BufferedAction.Jump && currentState == PlayerState.Movement) {

            if (coyoteTimeCounter > 0f && canJump) {
                Jump();
                bufferedAction = BufferedAction.None;
            }
        } else if (bufferedAction == BufferedAction.Attack) {

            if (currentState == PlayerState.Movement || currentState == PlayerState.Melee) {
                MeleeAttack();
                bufferedAction = BufferedAction.None;
            }
        } else if (bufferedAction == BufferedAction.Shoot) {

            if (currentState == PlayerState.Movement || currentState == PlayerState.Shooting) {
                Shoot();
                bufferedAction = BufferedAction.None;
            }
        }
    }
    #endregion
    #region Execute Actions
    private void Jump() {
        Debug.Log("Entered Jump");
        if (!isGrounded || !canJump) return;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        
        canJump = false;
        isGrounded = false;
        coyoteTimeCounter = 0f;
        jumpTimeoutDelta = jumpTimeout;

        anim.SetTrigger(jumpParameter);
        
        StartCoroutine(ResetJumpCoroutine());
    }
    public void Hop() {
        
        if (Time.time - lastHopTime < 0.5f) return;
        
        lastHopTime = Time.time;
        Jump();
    }
    public void ForcePlatformHop() {

        rb.velocity = new(rb.velocity.x, 0f, rb.velocity.z);
        platformVelocity = Vector3.zero;
        rb.AddForce(Vector3.up * (jumpForce * 1.2f), ForceMode.Impulse);

        canJump = false;
        isGrounded = false;
        coyoteTimeCounter = 0f;
        jumpTimeoutDelta = jumpTimeout;

        anim.SetTrigger(jumpParameter);
        StartCoroutine(ResetJumpCoroutine());
    }
    private void MeleeAttack() {

        SwitchState(PlayerState.Shooting);
        anim.SetTrigger(attackParameter);
        StartCoroutine(ReturnToMovement(0.3f));
    }
    private void Shoot() {

        SwitchState(PlayerState.Shooting);
        anim.SetTrigger(shootParameter);
        StartCoroutine(ReturnToMovement(0.3f));
    }

    private void SwitchState(PlayerState newState) {

        if (currentState == newState) return;
        currentState = newState;

        switch(newState) {
            case PlayerState.Movement:
                break;
            case PlayerState.Melee:
                break;
            case PlayerState.Shooting:
                break;
        }
    }
    private IEnumerator ReturnToMovement(float delay) {
        yield return new WaitForSeconds(delay);

        if (currentState != PlayerState.Movement) SwitchState(PlayerState.Movement);
    }
    private IEnumerator ResetJumpCoroutine() {

        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }
    #endregion
    #region Camera & Movement Look
    private void Look() {

        if (look.sqrMagnitude >= lookThreshold) {

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

        bool hasMovementInput = move.sqrMagnitude > 0.01f;

        float speedModifier = 1f;
        if (currentState == PlayerState.Melee) speedModifier = 0.2f;
        if (currentState == PlayerState.Shooting) speedModifier = 0.5f;
        SwitchState(PlayerState.Movement);

        float targetSpeed = hasMovementInput ? (isRunning ? moveSpeed * 1.5f : moveSpeed) * move.magnitude * speedModifier : 0f;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * 10f);

        if (!hasMovementInput && currentSpeed < 0.05f) currentSpeed = 0f;

        Vector3 forward = cameraTarget.forward;
        Vector3 right = cameraTarget.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * move.y + right * move.x).normalized;
        Vector3 currentVelocity = rb.velocity;
        float targetYVelocity = currentVelocity.y;

        if (!isGrounded) {

            if (rb.velocity.y > 0) {

                if (isHoldingJump) {

                    targetYVelocity += Physics.gravity.y * (riseGravityMultiplier - 1) * Time.fixedDeltaTime;
                } else {

                    targetYVelocity += Physics.gravity.y * (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
                }
            }else if (rb.velocity.y <= 0) {

                targetYVelocity += Physics.gravity.y * (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
        }
        if (hasMovementInput) {

            Vector3 baseVelocity = new(moveDirection.x * currentSpeed, targetYVelocity, moveDirection.z * currentSpeed);
            rb.velocity = baseVelocity + platformVelocity;

            if (moveDirection.sqrMagnitude > 0.01f) {

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                Quaternion normalLook = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);

                if (isRidingPlatform) {

                    Vector3 platformForward = platformRotationDelta * Vector3.forward;
                    platformForward.y = 0f;

                    if (platformForward.sqrMagnitude > 0.01f) {
                        float platformYaw = Quaternion.LookRotation(platformForward).eulerAngles.y;
                        transform.Rotate(0f, platformYaw * Time.fixedDeltaTime, 0f, Space.World);
                    }
                } else {
                    rb.MoveRotation(normalLook);
                }
            }
        } else {
            
            Vector3 stoppedHorizontalVelocity = Vector3.Lerp(new Vector3(currentVelocity.x, 0f, currentVelocity.z), Vector3.zero, Time.fixedDeltaTime * 15f);
            Vector3 baseVelocity = new(stoppedHorizontalVelocity.x, targetYVelocity, stoppedHorizontalVelocity.z);
            rb.velocity = baseVelocity + platformVelocity;

            if (isRidingPlatform) {
                Vector3 platformForward = platformRotationDelta * Vector3.forward;
                platformForward.y = 0f;
                if (platformForward.sqrMagnitude > 0.01f) {
                    rb.MoveRotation(platformRotationDelta * transform.rotation);
                }
            }

            anim.SetTrigger(idleParameter);
        }
        anim.SetFloat(speedParameter, currentSpeed);
    }
    #endregion
    #region Input Actions
    private void OnMove(InputValue value) { move = value.Get<Vector2>(); }
    private void OnRun(InputValue value) { isRunning = value.isPressed; }
    private void OnSprint(InputValue value) { isSprinting = value.isPressed; }
    private void OnLook(InputValue value) { look = value.Get<Vector2>(); }
    private void OnJump(InputValue value) {
        Debug.Log("Enter Jump Input");
        if (value.isPressed) {
            Debug.Log("Enter Jump");
            isHoldingJump = true;
            BufferInput(BufferedAction.Jump); 
        } else {
            isHoldingJump = false;
        }
    }
    private void OnAttack(InputValue value) {
        Debug.Log("Enter Attack Input");
        if (value.isPressed) {
            Debug.Log("Enter Attack");
            BufferInput(BufferedAction.Attack);
        }
    }
    private void OnShoot(InputValue value) {
        Debug.Log("Enter Shoot Input");
        if (value.isPressed) {
            Debug.Log("Enter Shoot");
            BufferInput(BufferedAction.Shoot);
        }
    }
    #endregion
    private void OnDrawGizmosSelected() {
        if (groundCheck == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}