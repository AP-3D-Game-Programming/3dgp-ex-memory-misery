using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))] // Voegt automatisch een AudioSource toe voor voetstappen
public class FPController : MonoBehaviour
{
    [Header("Status")]
    public bool isHidden = false;

    [Header("Animation")]
    [SerializeField] Animator characterAnimator;

    [Header("Movement parameters")]
    public float WalkSpeed = 2.5f;
    public float SprintSpeed = 8f;
    public float Acceleration = 15f;

    // Checkt nu ook of je nog stamina hebt
    public float MaxSpeed => IsSprintingAllowed ? SprintSpeed : WalkSpeed;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrain = 20f;
    public float staminaRegen = 10f;
    [SerializeField] private float currentStamina;

    private bool IsSprintingAllowed => SprintInput && currentStamina > 0;

    public bool Sprinting
    {
        get { return IsSprintingAllowed && CurrentSpeed > 0.1f; }
    }

    [Space(15)]
    [Tooltip("Jump height")]
    [SerializeField] float JumpHeight = 2f;

    [Header("Looking parameters")]
    public Vector2 LookSensitivity = new Vector2(2f, 2f);
    public float PitchLimit = 85f;

    [Range(0.0f, 0.1f)]
    public float LookSmoothing = 0.005f;

    private Vector2 currentLookInput;
    private Vector2 lookInputVelocity;
    [SerializeField] public float currentPitch = 0f;
    public float CurrentPitch
    {
        get => currentPitch;
        set { currentPitch = Mathf.Clamp(value, -PitchLimit, PitchLimit); }
    }
    private float currentYaw = 0f;

    [Header("Camera Parameters")]
    [SerializeField] float CameraNormalFOV = 60f;
    [SerializeField] float CameraSprintFOV = 80f;
    [SerializeField] float CameraFOVSmoothing = 1f;

    [Header("Physics Parameters")]
    [SerializeField] float GravityScale = 2f;
    public float VerticalVelocity = 0f;
    public Vector3 CurrentVelocity { get; private set; }
    public float CurrentSpeed { get; private set; }
    public bool IsGrounded => characterController.isGrounded;

    [Header("Input")]
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool SprintInput;

    [Header("Audio: Ademhaling")]
    public AudioSource breathSource;
    public AudioClip heavyBreathSound;
    [Range(0, 1)] public float maxBreathVolume = 0.8f;

    [Header("Audio: Voetstappen")]
    public AudioSource stepSource;
    public AudioClip[] footstepSounds;
    public float stepIntervalWalk = 0.5f;
    public float stepIntervalSprint = 0.3f;
    private float stepTimer;

    [Header("Component")]
    [SerializeField] CinemachineCamera fpCamera;
    [SerializeField] CharacterController characterController;

    private void OnValidate()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (stepSource == null)
            stepSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        currentStamina = maxStamina;

        if (breathSource != null && heavyBreathSound != null)
        {
            breathSource.clip = heavyBreathSound;
            breathSource.loop = true;
            breathSource.volume = 0;
            breathSource.Play();
        }
    }

    void Update()
    {
        HandleStamina();
        if (!isHidden)
        {
            MoveUpdate();
            AnimationUpdate();
            HandleFootsteps();
        }
    }

    void LateUpdate()
    {
        LookUpdate();
        CameraUpdate();
    }

    void HandleStamina()
    {
        if (SprintInput && CurrentSpeed > 0.1f && currentStamina > 0)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegen * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (breathSource != null)
        {
            float staminaPercentage = currentStamina / maxStamina;
            float targetVolume = 1 - staminaPercentage;

            if (staminaPercentage > 0.7f) targetVolume = 0;

            breathSource.volume = Mathf.Lerp(breathSource.volume, targetVolume * maxBreathVolume, Time.deltaTime * 2f);
        }
    }

    void HandleFootsteps()
    {
        if (!IsGrounded || CurrentSpeed < 0.1f) return;

        float interval = IsSprintingAllowed ? stepIntervalSprint : stepIntervalWalk;
        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0)
        {
            if (footstepSounds.Length > 0 && stepSource != null)
            {
                stepSource.pitch = Random.Range(0.9f, 1.1f); // Beetje variatie
                stepSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
            }
            stepTimer = interval;
        }
    }

    public void TryJump()
    {
        if (IsGrounded == false) return;
        VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y * GravityScale);
        if (characterAnimator != null) characterAnimator.SetTrigger("Jump");
    }

    void MoveUpdate()
    {
        if (isHidden)
        {
            CurrentVelocity = Vector3.zero;
            CurrentSpeed = 0f;
            return;
        }
        Vector3 motion = transform.forward * MoveInput.y + transform.right * MoveInput.x;
        motion.y = 0f;
        motion.Normalize();

        if (motion.sqrMagnitude >= 0.1f)
        {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, motion * MaxSpeed, Acceleration * Time.deltaTime);
        }
        else
        {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, Vector3.zero, Acceleration * Time.deltaTime);
        }

        if (IsGrounded && VerticalVelocity < 0.01f) VerticalVelocity = -3f;
        else VerticalVelocity += Physics.gravity.y * GravityScale * Time.deltaTime;

        Vector3 fullVelocity = new Vector3(CurrentVelocity.x, VerticalVelocity, CurrentVelocity.z);
        CollisionFlags flags = characterController.Move(fullVelocity * Time.deltaTime);

        if ((flags & CollisionFlags.Above) != 0 && VerticalVelocity > 0.01f) VerticalVelocity = 0f;

        CurrentSpeed = new Vector3(CurrentVelocity.x, 0, CurrentVelocity.z).magnitude;
    }

    void LookUpdate()
    {
        currentLookInput = Vector2.SmoothDamp(currentLookInput, LookInput, ref lookInputVelocity, LookSmoothing);
        Vector2 input = currentLookInput * LookSensitivity;

        currentPitch -= input.y;
        float pitchMin = isHidden ? -30f : -PitchLimit;
        float pitchMax = isHidden ? 30f : PitchLimit;
        currentPitch = Mathf.Clamp(currentPitch, pitchMin, pitchMax);

        if (isHidden)
        {
            currentYaw += input.x;
            currentYaw = Mathf.Clamp(currentYaw, -45f, 45f);
            if (fpCamera != null)
            {
                fpCamera.transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            }
        }
        else
        {
            // Normaal gedrag: Draai het hele lichaam
            currentYaw = Mathf.Lerp(currentYaw, 0f, Time.deltaTime * 10f);
            if (fpCamera != null)
            {
                fpCamera.transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            }
            transform.Rotate(Vector3.up * input.x);
        }
    }

    void CameraUpdate()
    {
        float targetFOV = IsSprintingAllowed && CurrentSpeed > 0.1f ? CameraSprintFOV : CameraNormalFOV;
        fpCamera.Lens.FieldOfView = Mathf.Lerp(fpCamera.Lens.FieldOfView, targetFOV, CameraFOVSmoothing * Time.deltaTime);
    }

    void AnimationUpdate()
    {

        if (characterAnimator == null) return;
        if (isHidden)
        {
            characterAnimator.SetFloat("Speed", 0f);
            characterAnimator.SetBool("IsGrounded", true);
        }
        else
        {
            characterAnimator.SetFloat("Speed", CurrentSpeed);
            characterAnimator.SetBool("IsGrounded", IsGrounded);
        }
    }

}