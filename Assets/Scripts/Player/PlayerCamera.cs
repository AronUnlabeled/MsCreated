using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {
    
    [SerializeField] private float sensitivityX, sensitivityY;

    [SerializeField] private Transform orientation;

    private void Update() {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivityY * Time.deltaTime;
        orientation.Rotate(Vector3.up * mouseX);
        Vector3 currentRotation = transform.localEulerAngles;
        float desiredXRotation = currentRotation.x - mouseY;
        // Clamp the vertical rotation to prevent flipping
        if (desiredXRotation > 180) desiredXRotation -= 360; // Convert to -180 to 180 range
        desiredXRotation = Mathf.Clamp(desiredXRotation, -90, 90);
        transform.localEulerAngles = new Vector3(desiredXRotation, currentRotation.y, currentRotation.z);
    }
}