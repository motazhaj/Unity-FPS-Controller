using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPos;
    public float camSmooth = 0.001f;
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        Vector3 smoothPosition = Vector3.SmoothDamp(transform.position, cameraPos.position,ref velocity, camSmooth);
        transform.position = smoothPosition;


    }
}
