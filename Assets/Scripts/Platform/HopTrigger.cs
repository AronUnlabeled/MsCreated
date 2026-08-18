using UnityEngine;

public class HopTrigger : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Player")) {

           ThirdPersonController player = other.GetComponent<ThirdPersonController>();

            if (player != null) {

                Debug.Log("Player hit the side of the platform! Hopping.");
                player.ForcePlatformHop();
            }
        }
    }
}