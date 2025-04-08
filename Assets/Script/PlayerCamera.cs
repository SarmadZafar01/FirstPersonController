using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Sentivity Camera")]
    [SerializeField] private float SensX;
    [SerializeField] private float SensY;

    [SerializeField] private Transform orientation;

    // Camera Rotation
    private float xRotation;
    private float yRotation;

    // Axis for Mouse
    private float MouseX;
    private float MouseY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update()
    {
        MouseX = Input.GetAxis("Mouse X") * Time.deltaTime * SensX;
        MouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * SensY;


        yRotation += MouseX;


        xRotation -= MouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation= Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);


    }
}
