using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.Rendering;


[RequireComponent(typeof(CharacterController))]
public class FPController : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("Sleep hier je Y-Bot in")]
    [SerializeField] Animator characterAnimator;

    [Header("Movement parameters")]
    public float MaxSpeed => SprintInput ? SprintSpeed : WalkSpeed;
    public float Acceleration = 15f;

    public bool Sprinting
    {
        get
        {
            return SprintInput && CurrentSpeed > 0.1f;
        }
    }

    [SerializeField] public float WalkSpeed = 2.5f;
    [SerializeField] public float SprintSpeed = 8f;

    
    [Space(15)]
    [Tooltip("jup height")]
    [SerializeField] float JumpHeight = 2f;


    [Header("Looking parameters")]
    public Vector2 LookSen = new Vector2(0.1f,0.1f);
    public float PitchLimit = 85f;
    [SerializeField] public float currentPitch = 0f;

    public float CurrentPitch
    {
        get => currentPitch;
        set
        {
            currentPitch = Mathf.Clamp(value, -PitchLimit, PitchLimit);
        }
    }
    [Header("Camera Parameters")]
    [SerializeField] float CameraNormalFOV = 60f;
    [SerializeField] float CameraSprintFOV = 80f;
    [SerializeField] float CameraFOVSmoothing = 1f;

    float TargetCameraFOV
    {
        get
        {
            return Sprinting ? CameraSprintFOV : CameraNormalFOV;
        }
    }
    [Header("Physics Parameters")]
    [SerializeField] float GravityScale = 2f;
    public float VerticalVelocity = 0f;
    public Vector3 CurrentVelocity { get; private set; }
    public float CurrentSpeed { get; private set; }
    public bool IsGrounded => characterController.isGrounded;


    [Header("input")]
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool SprintInput;

    [Header("Component")]
    [SerializeField] CinemachineCamera fpCamera;
    [SerializeField]CharacterController characterController;
    private void OnValidate()
    {
        if(characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
    }
    // Update is called once per frame
    void Update()
    {
        MoveUpdate();
        AnimationUpdate();
    }
    void LateUpdate()
    {
        LookUpdate();
        CameraUpdate();
    }
    public void TryJump()
    {
        if(IsGrounded == false)
        {
            return;
        }
        VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y*GravityScale);
        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger("Jump");
        }
    }

    void MoveUpdate()
    {
        Vector3 motion = transform.forward * MoveInput.y + transform.right * MoveInput.x;
        motion.y = 0f;
        motion.Normalize();


        if (motion.sqrMagnitude >= 0.1f)
        {
            // Als we input hebben: Versnellen naar MaxSpeed
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, motion * MaxSpeed, Acceleration * Time.deltaTime);
        }
        else 
        {
            // Als we GEEN input hebben: Vertragen naar 0
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, Vector3.zero, Acceleration * Time.deltaTime);
        }

        if (IsGrounded && VerticalVelocity < 0.01f)
        {
            VerticalVelocity = -3f;
        }
        else
        {
            VerticalVelocity += Physics.gravity.y * GravityScale * Time.deltaTime;
        }

        Vector3 fullVelocity = new Vector3(CurrentVelocity.x, VerticalVelocity, CurrentVelocity.z);

        CollisionFlags flags = characterController.Move(fullVelocity * Time.deltaTime);

        if ((flags & CollisionFlags.Above) != 0 && VerticalVelocity > 0.01f)
        {
            VerticalVelocity = 0f;
        }

        CurrentSpeed = CurrentVelocity.magnitude;
    }
    void LookUpdate()
    {
        Vector2 input = new Vector2(LookInput.x * LookSen.x, LookInput.y * LookSen.x);
        //benden kijken
        CurrentPitch -= input.y;
        fpCamera.transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

        // links rechts kijken
        transform.Rotate(Vector3.up * input.x);
    }
    void CameraUpdate()
    {
        float targetFOV = CameraNormalFOV;

        if (Sprinting)
        {
            float speedRatio = CurrentSpeed / SprintSpeed;
            targetFOV = Mathf.Lerp(CameraNormalFOV, CameraSprintFOV, speedRatio);
        }
        fpCamera.Lens.FieldOfView = Mathf.Lerp(fpCamera.Lens.FieldOfView, targetFOV, CameraFOVSmoothing * Time.deltaTime);

    }
    void AnimationUpdate()
    {
        if (characterAnimator == null) return;

        // Stuur waarden naar de Animator Controller
        characterAnimator.SetFloat("Speed", CurrentSpeed);
        characterAnimator.SetBool("IsGrounded", IsGrounded);
    }

}
