using UnityEngine;

/// <summary>
/// This is a very simple player movement script that allows the player to move in 3D space using a CharacterController. 
/// It handles basic movement, jumping, and gravity. 
/// You can customize the speed, jump height, and gravity to fit the character.
/// </summary>

public class PlayerMovement : MonoBehaviour {

    [SerializeField] private CharacterController characterController;

    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    Vector3 velocity;
    bool isGrounded;

    void Update() {
        // Check if player is on the ground
        isGrounded = characterController.isGrounded;

        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f; // Small downward force to keep player grounded
        }

        // Get Input (WASD or Arrow Keys)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Calculate move direction relative to where the player is facing
        Vector3 move = transform.right * x + transform.forward * z;

        // Move the controller
        characterController.Move(move * speed * Time.deltaTime);

        // Handle Jumping
        if (Input.GetButtonDown("Jump") && isGrounded) {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply Gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}