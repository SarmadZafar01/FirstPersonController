using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class Sliding : MonoBehaviour


{

    [Header("Reference")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PLayerMovement playerMovmentscript;


    [Header("Sliding")]
    [SerializeField] private float maxSlideTime; // sliding time
    [SerializeField] private float slideForce; 
    private float sliderTimer;

    public float slideYscale;  // player scale while sliding
    private float StartYscale; // currect scale



    float horizontal;
    float vertical;


    private void Start()
    {
        rb= GetComponent<Rigidbody>();
        playerMovmentscript = GetComponent<PLayerMovement>();

        StartYscale = playerObj.localScale.y;
    }

    private void Update()
    {


        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        
        if (Input.GetKeyDown(KeyCode.LeftControl) && (horizontal != 0 || vertical != 0))
        {
            StartSlide();
        }

        if (Input.GetKeyUp(KeyCode.LeftControl) && playerMovmentscript.sliding) {
            StopSliding();
        }
    }


    private void FixedUpdate()
    {
        if (playerMovmentscript.sliding)
        {
            MovementSliding();
        }
    }

    private void StartSlide()
    {
        playerMovmentscript.sliding = true;

        //Player scale down for sliding
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYscale, playerObj.localScale.z);

        //Down Force Apply
        rb.AddForce(Vector3.down * 5f , ForceMode.Impulse);


        sliderTimer = maxSlideTime;
    }

    private void StopSliding()
    {
        playerMovmentscript.sliding = false;
        // Player scale to its orginal position
        playerObj.localScale = new Vector3(playerObj.localScale.x,StartYscale,playerObj.localScale.z);
    }


    private void MovementSliding()
    {
        //orientation calculatino
        Vector3 inputDirewcdtion = orientation.forward * vertical + orientation.right * horizontal;
        //Normal slide
        if(!playerMovmentscript.OnSlop() || rb.velocity.y > -0.1f)
        {
            //force apply
            rb.AddForce(inputDirewcdtion.normalized * slideForce, ForceMode.Force);

            //timer --
            sliderTimer -= Time.deltaTime;
        }
        else
        {
            rb.AddForce(playerMovmentscript.getSlopMoveDirection(inputDirewcdtion) * slideForce, ForceMode.Force);
            
        }

     

        //condition to stop sliding
        if(sliderTimer <= 0)

            StopSliding();
        
    }

}
