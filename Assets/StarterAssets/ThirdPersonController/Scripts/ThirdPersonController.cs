using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.XR;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
  public enum PlayerMovementState
  {
    FreeExplore,
    CombatStrafe,
    Sneak,
    Grappled
  }

  [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
  [RequireComponent(typeof(PlayerInput))]
#endif
  public class ThirdPersonController : MonoBehaviour
  {
    [Header("Player")]
    [Tooltip("Movement Restrictions")]
    public PlayerMovementState currentState = PlayerMovementState.FreeExplore;

    [Tooltip("Base speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("Sneak speed of the character in m/s")]
    public float SneakSpeed = 1.0f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Camera movement speend")]
    public float mouseSpeedMultiplier = 1.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    [Header("Audio Configuration")]
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
    public NoiseMaker noiseMaker;

    [Header("Survival Integration")]
    public SurvivalStats survivalStats;
    public float sprintStaminaCost = 15f; // Drains per second
    public float jumpStaminaCost = 20f;   // One-time chunk

    [Header("Combat Settings")]
    public AttackController attackController;
    public UnityEvent onGrappleBreak;
    private float grappleBreakTimeLimit = 0f;

    [Header("Sneak Capsule Adjustments")]
    public float normalHeight = 1.8f;
    public float normalCenterY = 0.9f;
    public float sneakHeight = 1.2f;
    public float sneakCenterY = 0.6f;

    [Header("Animation Drift Corrections")]
    [Tooltip("Adjust these to fix diagonal root motion from Mixamo animations")]
    public float forwardDrift = 0f;
    public float backwardDrift = 0f;
    public float leftStrafeDrift = 0f;
    public float rightStrafeDrift = 0f;
    // Tracks the absolute mathematical direction the player WANTS to go
    private Vector3 _currentMovementDir;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // Stores movement velocity from animation root motion for use in C# jump kinematics
    private Vector3 _lockedAirVelocity;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Animator _animator;
    private CharacterController _controller;
    private StarterAssetsInputs _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    private bool IsCurrentDeviceMouse
    {
      get
      {
#if ENABLE_INPUT_SYSTEM
        return _playerInput.currentControlScheme == "KeyboardMouse";
#else
        return false;
#endif
      }
    }


    public void ResetAnimationsToIdle()
    {
      // 1. Zero out the movement math
      _speed = 0f;
      _animationBlend = 0f;

      // 2. Clear the physical inputs so the player doesn't lurch forward when unpaused
      if (_input != null)
      {
        _input.move = Vector2.zero;
      }

      // 3. Force the Animator to the Idle state immediately
      if (_hasAnimator)
      {
        _animator.SetFloat(_animIDSpeed, 0f);
        _animator.SetFloat(_animIDMotionSpeed, 0f);
        _animator.SetFloat("MoveX", 0f);
        _animator.SetFloat("MoveZ", 0f);
      }
    }

    private void Awake()
    {
      // get a reference to our main camera
      if (_mainCamera == null)
      {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
      }
    }

    private void Start()
    {
      _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

      _hasAnimator = TryGetComponent(out _animator);
      _controller = GetComponent<CharacterController>();
      _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
      _playerInput = GetComponent<PlayerInput>();
#else
      Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

      AssignAnimationIDs();

      if (noiseMaker == null) noiseMaker = GetComponent<NoiseMaker>();
      if (attackController == null) attackController = GetComponent<AttackController>();

      // reset our timeouts on start
      _jumpTimeoutDelta = JumpTimeout;
      _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
      _hasAnimator = TryGetComponent(out _animator);

      switch (currentState)
      {
        case PlayerMovementState.FreeExplore:
          ApplyGravity();
          Jump();
          GroundedCheck();
          FreeMove();
          break;
        case PlayerMovementState.CombatStrafe:
          ApplyGravity();
          GroundedCheck();
          CombatStrafe();
          break;
        case PlayerMovementState.Sneak:
          ApplyGravity();
          GroundedCheck();
          SneakMove();
          break;
        case PlayerMovementState.Grappled:
          ApplyGravity();
          GroundedCheck();
          GrappleCheck();
          break;
      }
    }

    public void ChangeState(PlayerMovementState newState)
    {
      if (currentState == newState) return;

      // Prevents changing state until grapple is broken using SetState
      if (currentState == PlayerMovementState.Grappled) return;

      SetState(newState);
    }

    public void SetState(PlayerMovementState state)
    {
      currentState = state;
      switch (state)
      {
        case PlayerMovementState.FreeExplore:
          _animator.SetBool("IsCombat", false);
          _animator.SetBool("IsSneaking", false);
          // Restore full height
          _controller.height = normalHeight;
          _controller.center = new Vector3(0, normalCenterY, 0);
          break;
        case PlayerMovementState.CombatStrafe:
          _animator.SetBool("IsCombat", true);
          _animator.SetBool("IsSneaking", false);
          // Restore full height
          _controller.height = normalHeight;
          _controller.center = new Vector3(0, normalCenterY, 0);
          break;
        case PlayerMovementState.Sneak:
          _animator.SetBool("IsCombat", false);
          _animator.SetBool("IsSneaking", true);
          // Shrink the capsule
          _controller.height = sneakHeight;
          _controller.center = new Vector3(0, sneakCenterY, 0);
          break;
        case PlayerMovementState.Grappled:
          // Restore full height
          _controller.height = normalHeight;
          _controller.center = new Vector3(0, normalCenterY, 0);
          break;
        default:
          _animator.SetBool("IsCombat", false);
          _animator.SetBool("IsSneaking", false);
          break;
      }
    }

    private void LateUpdate()
    {
      CameraRotation();
    }

    private void AssignAnimationIDs()
    {
      _animIDSpeed = Animator.StringToHash("Speed");
      _animIDGrounded = Animator.StringToHash("Grounded");
      _animIDJump = Animator.StringToHash("Jump");
      _animIDFreeFall = Animator.StringToHash("FreeFall");
      _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
      // set sphere position, with offset
      Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
          transform.position.z);
      Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
          QueryTriggerInteraction.Ignore);

      // update animator if using character
      if (_hasAnimator)
      {
        _animator.SetBool(_animIDGrounded, Grounded);
      }
    }

    private void CameraRotation()
    {
      // if there is an input and camera position is not fixed
      if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
      {
        //Don't multiply mouse input by Time.deltaTime;
        float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f * mouseSpeedMultiplier : Time.deltaTime;

        _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
        _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
      }

      // clamp our rotations so our values are limited 360 degrees
      _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
      _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

      // Cinemachine will follow this target
      CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
          _cinemachineTargetYaw, 0.0f);
    }

    private void CombatStrafe()
    {
      // set target speed based on move speed, sprint speed and if sprint is pressed
      float targetSpeed = MoveSpeed;

      // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

      // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
      // if there is no input, set the target speed to 0
      if (_input.move == Vector2.zero) targetSpeed = 0.0f;

      // a reference to the players current horizontal velocity
      float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

      float speedOffset = 0.1f;
      float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

      // accelerate or decelerate to target speed
      if (currentHorizontalSpeed < targetSpeed - speedOffset ||
          currentHorizontalSpeed > targetSpeed + speedOffset)
      {
        // creates curved result rather than a linear one giving a more organic speed change
        // note T in Lerp is clamped, so we don't need to clamp our speed
        _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
            Time.deltaTime * SpeedChangeRate);

        // round speed to 3 decimal places
        _speed = Mathf.Round(_speed * 1000f) / 1000f;
      }
      else
      {
        _speed = targetSpeed;
      }

      _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
      if (_animationBlend < 0.01f) _animationBlend = 0f;

      // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
      float currentSmoothTime = RotationSmoothTime;

      // COMBAT MODE: Always lock rotation strictly to the camera
      _targetRotation = _mainCamera.transform.eulerAngles.y;
      float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, currentSmoothTime);
      transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

      _currentMovementDir = (transform.right * _input.move.x + transform.forward * _input.move.y).normalized;

      // SAFETY CHECK: If the player isn't pressing any keys, zero out the direction.
      // This prevents "Idle Wobble" in animations from slowly sliding the character!
      if (_input.move == Vector2.zero)
      {
        _currentMovementDir = Vector3.zero;
      }

      if (_hasAnimator)
      {
        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);

        // Send raw input to the strafe tree, and toggle the state
        // Fetch the current blend tree values
        float currentX = _animator.GetFloat("MoveX");
        float currentZ = _animator.GetFloat("MoveZ");

        // MoveTowards is linear. It hits exactly 0, killing "Blend Tree Ghosting"!
        // The "5f" is the transition speed. Higher = snappier, Lower = smoother.
        currentX = Mathf.MoveTowards(currentX, _input.move.x, Time.deltaTime * 5f);
        currentZ = Mathf.MoveTowards(currentZ, _input.move.y, Time.deltaTime * 5f);

        _animator.SetFloat("MoveX", currentX);
        _animator.SetFloat("MoveZ", currentZ);

      }
    }

    private void FreeMove()
    {
      // Default to assuming we have enough energy to sprint
      bool hasEnergyToSprint = true;

      if (_input.sprint && survivalStats != null)
      {
        // Try to drain the stamina. If they hit 0, this returns false!
        hasEnergyToSprint = survivalStats.UseStamina(sprintStaminaCost * Time.deltaTime);
      }

      // set target speed based on move speed, sprint speed and if sprint is pressed
      float targetSpeed = (_input.sprint && hasEnergyToSprint) ? SprintSpeed : MoveSpeed;

      // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

      // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
      // if there is no input, set the target speed to 0
      if (_input.move == Vector2.zero) targetSpeed = 0.0f;

      // a reference to the players current horizontal velocity
      float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

      float speedOffset = 0.1f;
      float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

      // accelerate or decelerate to target speed
      if (currentHorizontalSpeed < targetSpeed - speedOffset ||
          currentHorizontalSpeed > targetSpeed + speedOffset)
      {
        // creates curved result rather than a linear one giving a more organic speed change
        // note T in Lerp is clamped, so we don't need to clamp our speed
        _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
            Time.deltaTime * SpeedChangeRate);

        // round speed to 3 decimal places
        _speed = Mathf.Round(_speed * 1000f) / 1000f;
      }
      else
      {
        _speed = targetSpeed;
      }

      _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
      if (_animationBlend < 0.01f) _animationBlend = 0f;

      // normalise input direction
      Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

      // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
      // if there is a move input rotate player when the player is moving
      // COMBAT INJECTION: Cleanly separate the logic so SmoothDamp is only called once!
      float currentSmoothTime = RotationSmoothTime;
      if (_input.move != Vector2.zero)
      {
        // NORMAL MODE: Free-look rotation based on input direction
        _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, currentSmoothTime);
        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
      }


      // Calculate normal Starter Assets forward movement
      _currentMovementDir = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

      // SAFETY CHECK: If the player isn't pressing any keys, zero out the direction.
      // This prevents "Idle Wobble" in animations from slowly sliding the character!
      if (_input.move == Vector2.zero)
      {
        _currentMovementDir = Vector3.zero;
      }

      // update animator if using character
      if (_hasAnimator)
      {
        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
      }
    }

    private void SneakMove()
    {
      // 1. Hard-cap the target speed (no sprinting allowed)
      float targetSpeed = SneakSpeed;
      if (_input.move == Vector2.zero) targetSpeed = 0.0f;

      float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
      float speedOffset = 0.1f;
      float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

      if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
      {
        _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
        _speed = Mathf.Round(_speed * 1000f) / 1000f;
      }
      else
      {
        _speed = targetSpeed;
      }

      _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
      if (_animationBlend < 0.01f) _animationBlend = 0f;

      // 2. Rotate the player to face the input direction
      Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

      if (_input.move != Vector2.zero)
      {
        _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
      }

      _currentMovementDir = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

      if (_input.move == Vector2.zero)
      {
        _currentMovementDir = Vector3.zero;
      }

      // 3. Update the Animator
      if (_hasAnimator)
      {
        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
      }
    }

    private void ApplyGravity()
    {
      if (Grounded)
      {
        // reset the fall timeout timer
        _fallTimeoutDelta = FallTimeout;

        // update animator if using character
        if (_hasAnimator)
        {
          _animator.SetBool(_animIDJump, false);
          _animator.SetBool(_animIDFreeFall, false);
        }

        // stop our velocity dropping infinitely when grounded
        if (_verticalVelocity < 0.0f)
        {
          _verticalVelocity = -2f;
        }

        // jump timeout
        if (_jumpTimeoutDelta >= 0.0f)
        {
          _jumpTimeoutDelta -= Time.deltaTime;
        }
      }
      else
      {
        // reset the jump timeout timer
        _jumpTimeoutDelta = JumpTimeout;

        // fall timeout
        if (_fallTimeoutDelta >= 0.0f)
        {
          _fallTimeoutDelta -= Time.deltaTime;
        }
        else
        {
          // update animator if using character
          if (_hasAnimator)
          {
            _animator.SetBool(_animIDFreeFall, true);
          }
        }

        // if we are not grounded, do not jump
        _input.jump = false;
      }

      // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
      if (_verticalVelocity < _terminalVelocity)
      {
        _verticalVelocity += Gravity * Time.deltaTime;
      }
    }

    private void Jump()
    {
      // Jump
      if (_input.jump && _jumpTimeoutDelta <= 0.0f)
      {
        // THE INJECTION: Ask the stats script if we can afford the jump
        if (survivalStats != null && survivalStats.UseStamina(jumpStaminaCost))
        {
          // the square root of H * -2 * G = how much velocity to reach desired height
          _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

          // update animator if using character
          if (_hasAnimator)
          {
            _animator.SetBool(_animIDJump, true);
          }
          _input.jump = false;
        }
        else
        {
          // If they are too tired, consume the input so they don't auto-jump later
          _input.jump = false;
        }
      }
    }

    public void StartGrapple(float breakTime)
    {
      SetState(PlayerMovementState.Grappled);
      grappleBreakTimeLimit = Time.time + breakTime;
    }

    private void GrappleCheck()
    {
      // Cancel all inputs besides block
      _input.jump = false;
      _input.crouch = false;
      _input.lightAttack = false;
      _input.heavyAttack = false;
      _input.specialAttack = false;

      // Slow the player
      _speed = 0f;
      _animationBlend = Mathf.Lerp(_animationBlend, _speed, Time.deltaTime * SpeedChangeRate);
      if (_hasAnimator)
      {
        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, 0f);
      }
      
      if (_animationBlend < 0.01f) _animationBlend = 0f;

      // Check for grab break
      if (_input.block && Time.time <= grappleBreakTimeLimit && survivalStats.UseStamina(attackController.breakGrabStamina))
      {
        SetState(PlayerMovementState.FreeExplore);

        // Invoke event
        onGrappleBreak?.Invoke();

        // Consume the input
        _input.block = false;
      }
      else
      {
        _input.block = false;
      }

      
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
      if (lfAngle < -360f) lfAngle += 360f;
      if (lfAngle > 360f) lfAngle -= 360f;
      return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
      Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
      Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

      if (Grounded) Gizmos.color = transparentGreen;
      else Gizmos.color = transparentRed;

      // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
      Gizmos.DrawSphere(
          new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
          GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
      if (animationEvent.animatorClipInfo.weight > 0.5f)
      {
        // Dynamically calculate the noise radius based on our FSM state!
        float currentNoiseRadius = 20f;

        if (currentState == PlayerMovementState.Sneak)
        {
          currentNoiseRadius = 5f; // Sneaking is very quiet
        }
        else if (_input.sprint && _speed > MoveSpeed + 0.5f)
        {
          currentNoiseRadius = 40f; // Sprinting is extremely loud
        }

        if (noiseMaker != null) noiseMaker.MakeNoise(currentNoiseRadius);

        // Optional: Also reduce the audio volume of the footstep clip itself when sneaking
        float currentVolume = (currentState == PlayerMovementState.Sneak) ? FootstepAudioVolume * 0.3f : FootstepAudioVolume;

        if (FootstepAudioClips.Length > 0)
        {
          var index = Random.Range(0, FootstepAudioClips.Length);
          AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), currentVolume);
        }
      }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
      if (animationEvent.animatorClipInfo.weight > 0.5f)
      {
        AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
      }
    }

    void OnAnimatorMove()
    {
      if (_hasAnimator)
      {
        Vector3 step;

        if (Grounded)
        {
          // 1. GROUNDED: Let the animations drive the car
          step = _animator.deltaPosition;

          // (Your existing drift correction math goes here!)

          // 2. THE SNAPSHOT: Constantly record our true, physical forward momentum.
          if (Time.deltaTime > 0.001f)
          {
            _lockedAirVelocity = step / Time.deltaTime;
            _lockedAirVelocity.y = 0; // We only want horizontal momentum!
            _lockedAirVelocity *= 0.4f;
          }
        }
        else
        {
          // 3. AIRBORNE: Coast using the locked snapshot velocity!
          // We also add a tiny bit of steering so the player can slightly adjust their landing
          Vector3 airSteering = _currentMovementDir * 2.0f;

          step = (_lockedAirVelocity + airSteering) * Time.deltaTime;
        }

        // 4. Always apply gravity
        step.y = _verticalVelocity * Time.deltaTime;

        // 5. The Firewall
        if (float.IsNaN(step.x) || float.IsInfinity(step.x) ||
            float.IsNaN(step.y) || float.IsInfinity(step.y) ||
            float.IsNaN(step.z) || float.IsInfinity(step.z))
        {
          step = Vector3.zero;
        }

        // 6. Move safely
        _controller.Move(step);
      }
    }
  }
}