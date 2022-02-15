using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerMovement : NetworkBehaviour
{
    private MainControls controls;
    private InputAction move;
    private InputAction jump;

    public Rigidbody rb;
    private NetworkVariable<Vector3> MoveDirection;
    public float moveSpeed = 2f;
    public float jumpForce = 20f;

    private NetworkVariable<Color> MeshColour;
    public MeshRenderer meshRenderer;
    
    private void Awake()
    {
        controls = new MainControls();
    }

    private void OnEnable()
    {
        move = controls.InGame.Move;
        move.Enable();
        move.performed += MoveInput;
        move.canceled += MoveInput;

        jump = controls.InGame.Jump;
        jump.Enable();
        jump.started += JumpInput;
    }

    private void OnDisable()
    {
        move.Disable();
    }

    private void MoveInput(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            // Converts 2D movement input into 3D vector
            Vector3 newMoveDirection = new Vector3(obj.ReadValue<Vector2>().x, 0, obj.ReadValue<Vector2>().y);
            
            if (NetworkManager.Singleton.IsServer)
            {
                // Player hosting the server can move as normal
                MoveDirection.Value = newMoveDirection;
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                // Players ona client must request to move on the server
                RequestMoveServerRpc(newMoveDirection);
            }
        }
    }

    [ServerRpc]
    private void RequestMoveServerRpc(Vector3 newMoveDirection, ServerRpcParams rpcParams = default)
    {
        MoveDirection.Value = newMoveDirection;
    }

    private void JumpInput(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Jump();
                
                Color randomColour = GetRandomColour();
                meshRenderer.material.color = randomColour;
                MeshColour.Value = randomColour;
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                RequestJumpServerRpc();
            }
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    [ServerRpc]
    private void RequestJumpServerRpc(ServerRpcParams rpcParams = default)
    {
        Jump();
        MeshColour.Value = GetRandomColour();
    }

    private Color GetRandomColour()
    {
        return new Color(Random.value, Random.value, Random.value, 1f);
    }

    private void FixedUpdate()
    {
        rb.AddForce(MoveDirection.Value * moveSpeed, ForceMode.VelocityChange);
        meshRenderer.material.color = MeshColour.Value;
    }
}
