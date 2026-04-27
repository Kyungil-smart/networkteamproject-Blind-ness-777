using System;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class TempPlayerCtr : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    private Vector2 moveInput;

    private Vector3 _dir;

    private PlayerInput _playerInput; 
    private InputActionMap _mainActionMap;
    private InputAction _move;
    
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        // 소유자 전용 입력 바인딩 등 초기화
        moveInput = Vector2.zero;
        _playerInput = GetComponent<PlayerInput>();
        _mainActionMap = _playerInput.actions.FindActionMap("Player");
        _move = _mainActionMap.FindAction("Move");

        _move.performed += OnMove;
        _move.canceled += OnMove;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _move.performed -= OnMove;
        _move.canceled -= OnMove;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        float h = moveInput.x;
        float v = moveInput.y;
        _dir = new Vector3(h, 0f, v) * _moveSpeed * Time.deltaTime;
        transform.position += _dir;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>(); 
    }
}
