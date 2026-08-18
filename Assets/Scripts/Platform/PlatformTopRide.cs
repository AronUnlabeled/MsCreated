using UnityEngine;

public class PlatformTopRide : MonoBehaviour {
    private ThirdPersonController activePlayer;
    private Rigidbody playerRb;

    private Vector3 lastPlatformPosition;
    private Quaternion lastPlatformRotation;

    private void Start() {
        lastPlatformPosition = transform.parent.position;
        lastPlatformRotation = transform.parent.rotation;
    }

    private void FixedUpdate() {
        // Smoothly track moving and rotating physics if the player is riding on top
        if (activePlayer != null && playerRb != null) {
            Vector3 linearVelocity = (transform.parent.position - lastPlatformPosition) / Time.fixedDeltaTime;
            Quaternion rotationDelta = transform.parent.rotation * Quaternion.Inverse(lastPlatformRotation);

            Vector3 playerRelativePos = playerRb.position - transform.parent.position;
            Vector3 rotationalVelocity = ((rotationDelta * playerRelativePos) - playerRelativePos) / Time.fixedDeltaTime;

            activePlayer.platformVelocity = linearVelocity + rotationalVelocity;
            activePlayer.platformRotationDelta = rotationDelta;
            activePlayer.isRidingPlatform = true;
        }

        lastPlatformPosition = transform.parent.position;
        lastPlatformRotation = transform.parent.rotation;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            activePlayer = other.GetComponent<ThirdPersonController>();
            playerRb = other.GetComponent<Rigidbody>();

            // Standing on top clears out any platform velocity forces so they don't bounce
            if (activePlayer != null) {
                activePlayer.platformVelocity = Vector3.zero;
            }
            Debug.Log("Player is safely on top of the platform. No automatic hop.");
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && activePlayer != null) {
            Debug.Log("Player stepped off the top! Triggering exit hop.");
            activePlayer.ForcePlatformHop();

            // Reset movement states cleanly
            activePlayer.platformVelocity = Vector3.zero;
            activePlayer.platformRotationDelta = Quaternion.identity;
            activePlayer.isRidingPlatform = false;

            activePlayer = null;
            playerRb = null;
        }
    }
}
