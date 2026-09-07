using UnityEngine;

public class TurretProjectile : MonoBehaviour {
    
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 4f;
    private Vector3 moveDirection;

    public void Setup(Vector3 direction) {
        moveDirection = direction;
        // Face the trajectory vector
        transform.forward = direction;
        Destroy(gameObject, lifetime);
    }
    private void Update() {
        transform.position += speed * Time.deltaTime * moveDirection;
    }
    private void OnTriggerEnter(Collider other) {

        if (other.CompareTag("Player")) {

            HealthSystem healthSystem = other.GetComponent<HealthSystem>();
            var playerHealth = healthSystem;
            if (playerHealth != null) {
                playerHealth.OnDamage(1f);
                Debug.Log("I damaged the player!");
            }

            Destroy(gameObject);
        } else if ((1 << other.gameObject.layer & LayerMask.GetMask("Default", "Obstacles")) != 0) {

            Destroy(gameObject);
        }
    }
}