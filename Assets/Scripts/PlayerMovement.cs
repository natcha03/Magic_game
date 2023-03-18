using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")] [SerializeField]
    private float moveSpeed = 2f;

    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float walkRotateSpeed = 15f;
    [SerializeField] private float runRotateSpeed = 20f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Ground Check Settings")] [SerializeField]
    private Transform groundCheck;
    [SerializeField] private float jumpGravity = -30f; // New variable for jump gravity
    [SerializeField] private float groundDistance = 0f;
    [SerializeField] public LayerMask groundMask;

    [Header("Jump Settings")] [SerializeField]
    private float jumpForce = 9f;

    [SerializeField] private float jumpAcceleration = 0.5f;
    [SerializeField] private float jumpTime = 0.65f;

    [Header("Roll Settings")] [SerializeField]
    private float rollSpeed = 8.5f;

    [SerializeField] private float rollAcceleration = 1f;
    [SerializeField] private float rollTime = 0.5f;
    [SerializeField] private float gravityTransitionTime = 0.8f; // New field for gravity transition time
    private Animator animator;
    private CharacterController characterController;
    private Vector3 movementDirection;
    private Vector2 moveInput;

    private bool isRunning;
    private bool isJumping;
    private bool isRolling;
    public Transform cameraTransform;
    private float currentRollSpeed;
    private float currentJumpForce;
    private float jumpTimer;

    [Header("Roll Cooldown Settings")] [SerializeField]
    private float rollCooldown = 0.2f;
    
    private float rollCooldownTimer;
    private bool rollDirectionSet;
    private Vector3 rollDirection;
    private void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        rollCooldownTimer = 0f;
        currentRollSpeed = rollSpeed;
        currentJumpForce = jumpForce;
    }

    private void Update()
    {
        HandleJumping();
        HandleRolling();
        ApplyMovement();
        UpdateRollCooldownTimer();
        ApplyGravity();
        UpdateAnimation();
    }

    private void HandleJumping()
    {
        if (!isJumping || !characterController.isGrounded) return;

        if (jumpTimer <= jumpTime)
        {
            movementDirection.y = currentJumpForce;
            jumpTimer += Time.deltaTime;
            currentJumpForce += jumpAcceleration * Time.deltaTime;
        }
        else
        {
            isJumping = false;
            jumpTimer = 0f;
            currentJumpForce = jumpForce;
        }
    }
    private void UpdateRollCooldownTimer()
    {
        if (rollCooldownTimer > 0f)
        {
            rollCooldownTimer -= Time.deltaTime;
        }
    }
    
    private void HandleRolling()
    {
        if (!isRolling || rollCooldownTimer > 0f) return;

        if (!rollDirectionSet)
        {
            rollDirection = moveInput.magnitude > 0
                ? new Vector3(moveInput.x, 0f, moveInput.y).normalized
                : transform.forward;

            // Take camera orientation into account
            if (cameraTransform != null)
            {
                rollDirection = cameraTransform.TransformDirection(rollDirection);
                rollDirection.y = 0f;
                rollDirection.Normalize();
            }

            Quaternion targetRotation = Quaternion.LookRotation(rollDirection);
            transform.rotation = targetRotation;
            rollDirectionSet = true;
        }

        characterController.Move(rollDirection * currentRollSpeed * Time.deltaTime);
        currentRollSpeed += rollAcceleration * Time.deltaTime;

        rollTime -= Time.deltaTime;
        if (rollTime <= 0f)
        {
            isRolling = false;
            rollTime = 0.5f;
            currentRollSpeed = rollSpeed;
            rollDirection = Vector3.zero;
            rollDirectionSet = false;
        }
    }


    private void ApplyMovement()
    {
        if (isRolling) return;
        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && movementDirection.y < 0)
        {
            movementDirection.y = -2f;
        }

        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        // Take camera orientation into account
        if (cameraTransform != null)
        {
            moveDirection = cameraTransform.TransformDirection(moveDirection);
            moveDirection.y = 0f;
            moveDirection.Normalize();
        }

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            float rotationSpeed = isRunning ? runRotateSpeed : walkRotateSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        float movementSpeed = isRunning ? runSpeed : moveSpeed;
        characterController.Move(moveDirection * movementSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && movementDirection.y < 0)
        {
            movementDirection.y = -2f;
        }

        if (isJumping)
        {
            // Custom jump gravity for a few seconds
            if (jumpTimer < jumpTime)
            {
                movementDirection.y += jumpGravity * Time.deltaTime;
            }
            else
            {
                // Gradually transition back to normal gravity
                float t = (jumpTimer - jumpTime) / gravityTransitionTime; // Calculate the transition progress
                float currentGravity = Mathf.Lerp(jumpGravity, gravity, t); // Interpolate between jumpGravity and gravity
                movementDirection.y += currentGravity * Time.deltaTime;
            }

            jumpTimer += Time.deltaTime;
            currentJumpForce += jumpAcceleration * Time.deltaTime;
        }
        else
        {
            movementDirection.y += gravity * Time.deltaTime;
        }

        characterController.Move(movementDirection * Time.deltaTime);

        if (characterController.isGrounded)
        {
            isJumping = false;
            currentRollSpeed = rollSpeed;
            jumpTimer = 0f; // Reset jump timer when grounded
            currentJumpForce = jumpForce; // Reset jump force when grounded
        }
    }




    private void UpdateAnimation()
    {
        bool isMoving = moveInput.magnitude > 0;
        bool notRollingOrJumping = !isRolling && !isJumping;

        animator.SetBool("IsWalking", isMoving && !isRunning && notRollingOrJumping);
        animator.SetBool("IsRunning", isMoving && isRunning && notRollingOrJumping);
        animator.SetBool("IsIdle", !isMoving && notRollingOrJumping);
        animator.SetBool("IsJumping", isJumping && !isRolling);
        animator.SetBool("IsRolling", isRolling && !isJumping);

        if (!Keyboard.current.leftShiftKey.isPressed && isRunning && isMoving)
        {
            isRunning = false;
        }
    }
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnRun(InputValue value)
    {
        isRunning = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && characterController.isGrounded && !isRolling)
        {
            isJumping = true;
        }
    }

    public void OnRoll(InputValue value)
    {
        if (value.isPressed && !isRolling && !isJumping && rollCooldownTimer <= 0f)
        {
            isRolling = true;
            rollCooldownTimer = rollCooldown;
        }
    }


}
