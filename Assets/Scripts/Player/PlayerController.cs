using UnityEngine;
using RaftSurvival.Core;

namespace RaftSurvival.Player
{
    /// <summary>
    /// Handles player movement on the raft/land (walking, running, jumping)
    /// and basic swim-surface movement when in water. Reads input from a
    /// mobile virtual joystick (via public SetMoveInput) and drives the
    /// Animator's Speed/IsGrounded/Jump parameters for walking/swim animations.
    ///
    /// Designed to work with a Mixamo-rigged humanoid model + Animator
    /// Controller (see Asset Requirement note below).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform cameraRig;
        [SerializeField] private PlayerSwimState swimState;

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float swimSpeed = 2.5f;
        [SerializeField] private float turnSmoothTime = 0.1f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float jumpHeight = 1.2f;

        private CharacterController controller;
        private Vector2 moveInput; // set externally by UI joystick
        private bool runInput;
        private bool jumpRequested;
        private float turnSmoothVelocity;
        private float verticalVelocity;

        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int AnimJump = Animator.StringToHash("Jump");

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            bool isSwimming = swimState != null && swimState.IsSwimming;

            if (isSwimming)
            {
                HandleSwimMovement();
            }
            else
            {
                HandleGroundMovement();
            }
        }

        /// <summary>
        /// Called by the on-screen virtual joystick UI element every frame
        /// with a normalized -1..1 vector.
        /// </summary>
        public void SetMoveInput(Vector2 input) => moveInput = Vector2.ClampMagnitude(input, 1f);

        public void SetRunInput(bool isRunning) => runInput = isRunning;

        public void RequestJump() => jumpRequested = true;

        private void HandleGroundMovement()
        {
            Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            float speed = 0f;

            if (inputDir.magnitude >= 0.1f && cameraRig != null)
            {
                float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraRig.eulerAngles.y;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                speed = runInput ? runSpeed : walkSpeed;
                controller.Move(moveDir.normalized * speed * Time.deltaTime);
            }

            // Gravity + grounded check.
            bool grounded = controller.isGrounded;
            if (grounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (jumpRequested && grounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                animator?.SetTrigger(AnimJump);
            }
            jumpRequested = false;

            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

            if (animator != null)
            {
                animator.SetFloat(AnimSpeed, speed);
                animator.SetBool(AnimGrounded, grounded);
            }
        }

        private void HandleSwimMovement()
        {
            Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            if (inputDir.magnitude >= 0.1f && cameraRig != null)
            {
                float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraRig.eulerAngles.y;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDir.normalized * swimSpeed * Time.deltaTime);

                animato
