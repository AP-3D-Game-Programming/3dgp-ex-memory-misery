using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;


[RequireComponent(typeof(FPController))]

public class FPPlayer : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] FPController FPController;

     void OnMove(InputValue value)
    {
        FPController.MoveInput = value.Get<Vector2>();
    }
     void OnLook(InputValue value)
    {
        FPController.LookInput = value.Get<Vector2>();

    }
    void OnSprint(InputValue value)
    {
        FPController.SprintInput = value.isPressed;

    }
    void OnJump(InputValue value)
    {
        if(value.isPressed)
        {
            FPController.TryJump();
        }
    }
    private void OnValidate()
    {
        if (FPController == null) FPController = GetComponent<FPController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
