using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltWeapon : MonoBehaviour {
    [SerializeField] private float smooth;
    [SerializeField] private float multiplier;
    [SerializeField] private float addRotation;

    private void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * multiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * multiplier;

        // calculate target rotation
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        Quaternion xRotation = Quaternion.Euler(addRotation, 0f, 0f);
        targetRotation *= xRotation;

        // rotate 
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
    }
}