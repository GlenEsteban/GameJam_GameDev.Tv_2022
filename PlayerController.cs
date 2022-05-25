using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] bool isCurrentCharacter = false;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float turnSmoothTime = 0.2f;

    CharacterManager characterManager;
    AIBehaviour followingBehaviour;
    Rigidbody thisRigidBody;
    Vector2 moveInput;
    Vector3 playerVelocity;
    float turnSmoothVelocity;


    void Start()
    {
        characterManager = FindObjectOfType<CharacterManager>();
        if (GetComponent<AIBehaviour>() != null)
        {
            followingBehaviour = GetComponent<AIBehaviour>();
        }
        thisRigidBody = GetComponent<Rigidbody>();
    }
    void Update()
    {
        MoveCharacter();
    }

    private void MoveCharacter()
    {
        if (GetComponent<AIBehaviour>() != null && followingBehaviour.GetIsStaying()) {return;}

        {
            playerVelocity = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            thisRigidBody.velocity += playerVelocity * moveSpeed;
        }

        if (playerVelocity.magnitude >= Mathf.Epsilon)
        {
            float targetAngle = Mathf.Atan2(playerVelocity.x, playerVelocity.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    void OnMove(InputValue value) 
    {
        moveInput = value.Get<Vector2>();
    }

    void OnSwitchRight(InputValue value)
    {
        if (value.isPressed)
        {
            characterManager.SwitchRight();
        }
    }
    void OnSwitchLeft(InputValue value)
    {
        if (value.isPressed)
        {
            characterManager.SwitchLeft();
        }
    }
    public void SetToCurrentCharacter(bool value)
    { 
        isCurrentCharacter = value;
    }

    public void DeactivateControls()
    {
        GetComponent<PlayerInput>().DeactivateInput();
    }
    public void ActivateControls()
    {
        GetComponent<PlayerInput>().ActivateInput();
    }
}