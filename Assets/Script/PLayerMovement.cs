using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PLayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    [SerializeField] private float WalkSpeed;
    [SerializeField] private float SprintSpeed;
    public float slidespeed;

 // expose MaxSlopAngle through property


    private float DesiredMoveSpeed;
    private float LastDesiredSpeed;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask WhatisGround;
    [SerializeField] private float groundDrag;
    public bool grounded;

    [Header("Jumpinmg")]
    [SerializeField] private float JumpForce;
    [SerializeField] private float JumpCoolDown;
    [SerializeField] private float AirMultiplier;
    bool readytoJump;

    [Header("Crouching")]
    [SerializeField] private float CrouchSpeed;
    [SerializeField] private float CrouchYScale;
    private float StartYScale;


    [Header("Slope Handling")]
    [SerializeField] private float MaxSlopAngle;
    private RaycastHit slopHit;
    private bool exitingSLop;

   


    //player orientation  
    [SerializeField] private Transform orientation;


    float HorizontalInput;
    float VerticalInput;

    Vector3 MoveDirection;

    Rigidbody Rigidbody;

  


    public MoveingState state;

    public enum MoveingState
    {
        Walking,
        Sprinting,
        Crouching,
        sliding,
        air
    }

    public bool sliding;

    //Get Component 
    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.freezeRotation = true;
        readytoJump = true;
        StartYScale = transform.localScale.y;
    }


    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Update()
    {
        // raycast to check  ground With Starting point, Ending point, Max Distance, LayerMask
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, WhatisGround);


        
        PlayerInputs();
        SpeedControl();
        StateHandler();


        //Change drag of rigidbody if player is in ground
        if (grounded)
        {
            Rigidbody.drag = groundDrag;
        }
        else
        {
            Rigidbody.drag = 0;
        }
    
    }


  

     private void StateHandler()
    {
        if (sliding)
        {
            state = MoveingState.sliding;

            if (OnSlop() && Rigidbody.velocity.y < 0.1f)
            {
                DesiredMoveSpeed = slidespeed;
            }
            else
            {
                DesiredMoveSpeed = SprintSpeed;
            }
        }


        else if (Input.GetKey(KeyCode.LeftShift) && grounded)
        {
            state = MoveingState.Sprinting;
            DesiredMoveSpeed = SprintSpeed;
        }
        else if (grounded)
        {
            state = MoveingState.Walking;
            DesiredMoveSpeed = WalkSpeed;
        }
        else if (Input.GetKey(KeyCode.C))
        {
            state = MoveingState.Crouching;
            moveSpeed = CrouchSpeed;
        }
        else
        {
            state = MoveingState.air;
         
        }

        if(Mathf.Abs(DesiredMoveSpeed- LastDesiredSpeed) > 8f && moveSpeed!=0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = DesiredMoveSpeed;
        }
        LastDesiredSpeed = DesiredMoveSpeed;


       

    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float Difference = Mathf.Abs(DesiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < Difference)
        {
            moveSpeed = Mathf.Lerp(startValue, DesiredMoveSpeed, time / Difference);
            time += Time.deltaTime;
            yield return null;
        }
        moveSpeed = DesiredMoveSpeed;
    }
    private void PlayerInputs()
    {
        HorizontalInput = Input.GetAxis("Horizontal");
        VerticalInput = Input.GetAxis("Vertical");

        //When to Jump
        if(Input.GetKey(KeyCode.Space) && readytoJump && grounded)
        {
            readytoJump = false;
            Jump();

            Invoke(nameof(ResetJump), JumpCoolDown);
        }

        //Start Crouch
        if (Input.GetKeyDown(KeyCode.C)){
            transform.localScale= new Vector3(transform.localScale.x,CrouchYScale,transform.localScale.z);
            Rigidbody.AddForce(Vector3.down *5f, ForceMode.Impulse);
            

        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            transform.localScale= new Vector3(transform.localScale.x,StartYScale,transform.localScale.z);
        }



    }

    private void MovePlayer()
    {
        MoveDirection= orientation.forward * VerticalInput + orientation.right * HorizontalInput;


        if (OnSlop() && !exitingSLop)
        {
            Rigidbody.AddForce(getSlopMoveDirection(MoveDirection)* moveSpeed*20f,ForceMode.Force );

            if (Rigidbody.velocity.y > 0)
            {
                Rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (grounded)
        {
            Rigidbody.AddForce(MoveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            Rigidbody.AddForce(MoveDirection.normalized * moveSpeed * 10f *AirMultiplier, ForceMode.Force);
        }
        Rigidbody.useGravity = !OnSlop();
    }

    // limit player speed
    private void SpeedControl()
    {
        if (OnSlop()&& !exitingSLop)
        {
            if (Rigidbody.velocity.magnitude > moveSpeed)
            {
                Rigidbody.velocity= Rigidbody.velocity.normalized* moveSpeed;
            }
        }

        else
        {
            //player speed store in flatvel
            Vector3 flatvel = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z);


            //check if it magnitude greater then speed
            if (flatvel.magnitude > moveSpeed)
            {
                //limit speed
                Vector3 limitedVel = flatvel.normalized * moveSpeed;
                Rigidbody.velocity = new Vector3(limitedVel.x, Rigidbody.velocity.y, limitedVel.z);
            }
        }


       
    }


    private void Jump()
    {
        exitingSLop = true;


        //rest y velocity
        Rigidbody.velocity = new Vector3(Rigidbody.velocity.x,0f,Rigidbody.velocity.z);
        Rigidbody.AddForce(transform.up* JumpForce, ForceMode.Impulse);

    }

    private void ResetJump()
    {
        readytoJump = true;

        exitingSLop = false;
    }

    public bool OnSlop()
    {
        if(Physics.Raycast(transform.position,Vector3.down, out slopHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopHit.normal);
            return angle < MaxSlopAngle && angle != 0;
        }


        return false;
    }

    public Vector3 getSlopMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopHit.normal).normalized;
    }



}
