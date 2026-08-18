using UnityEngine;

public class Platform : MonoBehaviour {

    [Header("Hop Settings")]
    public bool hopOnBoard = true;
    public bool hopOnLeave = true;

    private Rigidbody platformRb;
    private ThirdPersonController activePlayer;
    private Rigidbody playerRb;

    private Vector3 lastPlatformPosition;
    private Quaternion lastPlatformRotation;
    private bool playerWasOnPlatform = false;

    private void Start() {
        platformRb = GetComponent<Rigidbody>();
        lastPlatformPosition = transform.position;
        lastPlatformRotation = transform.rotation;
    }

    private void FixedUpdate() {
        bool playerIsOnPlatform = CheckIfPlayerIsOnTop();

        // 1. Handle Boarding
        if (playerIsOnPlatform && !playerWasOnPlatform) {
            if (activePlayer != null) {
                activePlayer.isRidingPlatform = true;
                if (hopOnBoard) activePlayer.ForcePlatformHop();
            }
        }
        // 2. Handle Leaving
        else if (!playerIsOnPlatform && playerWasOnPlatform) {
            if (activePlayer != null) {
                if (hopOnLeave) activePlayer.ForcePlatformHop();

                activePlayer.platformVelocity = Vector3.zero;
                activePlayer.platformRotationDelta = Quaternion.identity;
                activePlayer.isRidingPlatform = false;
            }
            activePlayer = null;
            playerRb = null;
        }

        // 3. Handle Active Riding
        if (playerIsOnPlatform && activePlayer != null && playerRb != null) {
            Vector3 linearVelocity = (transform.position - lastPlatformPosition) / Time.fixedDeltaTime;
            Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(lastPlatformRotation);

            Vector3 playerRelativePos = playerRb.position - transform.position;
            Vector3 rotationalVelocity = ((rotationDelta * playerRelativePos) - playerRelativePos) / Time.fixedDeltaTime;

            // Strip out weird extreme vertical velocity anomalies causing the Y-axis jump glitch
            if (Mathf.Abs(linearVelocity.y) > 20f) linearVelocity.y = 0f;

            activePlayer.platformVelocity = linearVelocity + rotationalVelocity;
            activePlayer.platformRotationDelta = rotationDelta;
        }

        playerWasOnPlatform = playerIsOnPlatform;
        lastPlatformPosition = transform.position;
        lastPlatformRotation = transform.rotation;
    }

    private bool CheckIfPlayerIsOnTop() {
        Vector3 boxCenter = transform.position + Vector3.up * 1.1f;
        Vector3 boxHalfExtents = new(transform.localScale.x * 0.48f, 0.6f, transform.localScale.z * 0.48f);

        Collider[] colliders = Physics.OverlapBox(boxCenter, boxHalfExtents, transform.rotation);
        foreach (var col in colliders) {
            if (col.CompareTag("Player")) {
                activePlayer = col.GetComponent<ThirdPersonController>();
                playerRb = col.GetComponent<Rigidbody>();
                return true;
            }
        }
        return false;
    }
}