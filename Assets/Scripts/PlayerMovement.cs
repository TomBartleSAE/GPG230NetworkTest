using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private MainControls controls;
    private InputAction move;
    private InputAction jump;

    public Rigidbody rb;
    private Vector3 moveDirection;
    public float moveSpeed = 10f;
    public float jumpForce = 100f;
    
    private void Awake()
    {
        controls = new MainControls();
    }

    private void OnEnable()
    {
        move = controls.InGame.Move;
        move.Enable();
        move.performed += Move;
        move.canceled += Move;

        jump = controls.InGame.Jump;
        jump.Enable();
        jump.performed += Jump;
    }

    private void OnDisable()
    {
        move.Disable();
    }

    private void Move(InputAction.CallbackContext obj)
    {
        // Converts 2D movement input into 3D vector
        moveDirection = new Vector3(obj.ReadValue<Vector2>().x, 0, obj.ReadValue<Vector2>().y);
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        rb.AddForce(moveDirection * moveSpeed, ForceMode.VelocityChange);
    }
}
