using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSens = 1f;
    public Transform playerBody;

    float xRotation = 0f;
    float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get Input
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSens;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSens;

        yRotation += mouseX;
        xRotation -= mouseY;

        // Clamp camera rotation
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate Camera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Rotate player along its y axis
        playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);

    }
}
