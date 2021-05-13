using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Movement : MonoBehaviour //This script handles the movement of the player. 
{
    public GameObject mainView;
    public float movementSpeed = 5;
    public float mouseSensitivity = 100;
    public float jumpHeight = 10;
    public float gravityMult = 2;
    public float timeBetweenFootstepsInSeconds;
    public int startingRotation;
    public LayerMask groundLayers;
    public List<AudioClip> footSteps;
    
    [CanBeNull]
    public AudioSource audioSource;

    public int MaxAirSpeed = 30;
    
    private Rigidbody _body;
    private float _rotX;
    private float _rotY;
    private float _currentTime;

    private void Start() //After Awake(), before everything else
    {
        //Lock the cursor so you can't click outside the window
        Cursor.lockState = CursorLockMode.Locked;

        //Get the Rigidbody component, this implies physics-based movement
        _body = GetComponent<Rigidbody>();
        
        //Set starting rotation to orient the player correctly
        _rotY = startingRotation;
        
    }

    private void FixedUpdate() //Movement only happens on physics cycle each frame
    {
        WalkingHandler();
    }

    private void Update() //Looking around and jumping has to happen each frame
    {
        RotationHandler();
        //JumpingHandler(); //No jumping for english project
    }

    private void RotationHandler()
    {
        _rotX -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * 0.02f; //Get the rotation based on the movement of the mouse
        _rotY += Input.GetAxisRaw("Mouse X") * mouseSensitivity * 0.02f;

        // Clamp the X rotation
        if(_rotX < -90)
            _rotX = -90;
        else if(_rotX > 90)
            _rotX = 90;
        
        if (Input.GetAxis("Mouse X") != 0)
        {
            mainView.transform.localRotation = Quaternion.AngleAxis(_rotX, Vector3.right); //move camera up and down, independent of player
            transform.localRotation =
                Quaternion.Euler(0, _rotY, 0); //move player rotation left and right, which also moves camera
        }
        
    }

    private void WalkingHandler()
    {
        float h = Input.GetAxisRaw("Horizontal");//A and D or left and right arrow
        float v = Input.GetAxisRaw("Vertical");//W and S or up and down arrow
        
        Vector3 tempVect = new Vector3(h, 0, v); //translate the input to a vector
        tempVect = tempVect.normalized * movementSpeed * Time.deltaTime; //Normalize the vector (just direction), multiply it by speed and time to make it independent of framerate
        transform.Translate(tempVect);


        if (audioSource == null)
        {
            return;
        }
        
        if (h == 0 && v == 0)
        {
            _currentTime = Time.deltaTime + 0.1f;
            return;
        }

        _currentTime -= Time.deltaTime;
        if (_currentTime <= 0)
        {
            audioSource.PlayOneShot(footSteps[Random.Range(0, footSteps.Count - 1)]);

            _currentTime = timeBetweenFootstepsInSeconds;
        }
    }

    private void JumpingHandler()
    {
        var grounded = IsGrounded();
        var isEnabled = IsEnabled();
        Vector3 tempVect = new Vector3();
        
        if (Input.GetButtonDown("Jump") && grounded && isEnabled) 
        {
            tempVect = new Vector3(0, jumpHeight, 0);
            _body.AddForce(tempVect, ForceMode.Impulse); //Add a force up once to jump
        }
        else if(!grounded)
        {
            tempVect = new Vector3(0, -gravityMult, 0);
            _body.AddForce(tempVect); //Unity's gravity is weird, so we fake our own to add onto it
        }
        else
        {
            tempVect = Vector3.zero;
        }
        
    }
    
    private void AirMovement(Vector3 vector3)
    {
        // project the velocity onto the movevector
        Vector3 projVel = Vector3.Project(GetComponent<Rigidbody>().velocity, vector3);

        // check if the movevector is moving towards or away from the projected velocity
        bool isAway = Vector3.Dot(vector3, projVel) <= 0f;

        // only apply force if moving away from velocity or velocity is below MaxAirSpeed
        if (projVel.magnitude < MaxAirSpeed || isAway)
        {
            // calculate the ideal movement force
            Vector3 vc = vector3.normalized * 2;

            // cap it if it would accelerate beyond MaxAirSpeed directly.
            if (!isAway)
            {
                vc = Vector3.ClampMagnitude(vc, MaxAirSpeed - projVel.magnitude);
            }
            else
            {
                vc = Vector3.ClampMagnitude(vc, MaxAirSpeed + projVel.magnitude);
            }

            // Apply the force
            GetComponent<Rigidbody>().AddForce(vc, ForceMode.VelocityChange);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down,1.5f, groundLayers); //See if there is ground right below our feet
    }
    
    private bool IsEnabled()
    {
        Vector3 tempVec = transform.position;
        tempVec[1] -= .6f;
        
        if (Physics.Raycast(tempVec, Vector3.down, out var hit, 1.5f, groundLayers))
        {
            if(!hit.transform.CompareTag("Disabled"))
            {
                return true;
            }
        }

        return false;
    }
    
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = mainView.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainView.transform.localPosition = new Vector3(x, y + 0.5f, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainView.transform.localPosition = originalPos;
    }
}
