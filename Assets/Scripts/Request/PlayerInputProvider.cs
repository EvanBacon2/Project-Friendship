using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputProvider : MonoBehaviour {
    private RECSRigidBody rigidBody;

    public const float BASE_ACCELERATION = 40;
    public const float BASE_MAXSPEED = 25;

    private Vector3 lookInput;
    private float horizontalInput;
    private float verticalInput;
    private bool brakeInput;
    private bool boostInput;

    public event EventHandler<ShipState> StateReceived;

    public List<RequestSystem<ShipState>> requestSystems;

    void Start() {
        rigidBody = new RECSRigidBody(GetComponent<Rigidbody>(), 
                new PlayerShipRequestReference(), 
                BASE_ACCELERATION, 
                BASE_MAXSPEED);

        requestSystems = new() {
            new BoostRequest(rigidBody),
            new BrakeRequest(rigidBody),
            new LookAtMouseRequest(rigidBody),
            new MoveRequest(rigidBody)
        };

        foreach (RequestSystem<ShipState> system in requestSystems) {
            StateReceived += system.OnStateReceived;
        }
    }

    void Update() {
        lookInput = Input.mousePosition;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        brakeInput = Input.GetKey(KeyCode.LeftShift);
        boostInput = !boostInput ? Input.GetKeyDown(KeyCode.Space) : boostInput;
    }

    void FixedUpdate() {
        OnStateReceived();
        rigidBody.executeRequests();
        boostInput = false;
    }

    protected virtual void OnStateReceived() {
            StateReceived?.Invoke(this, new ShipState() { 
                time = Time.time,
                rigidBody = rigidBody,
                position = rigidBody.Position,
                lookInput = lookInput,
                horizontalInput = horizontalInput, 
                verticalInput = verticalInput,
                brakeInput = brakeInput,
                boostInput = boostInput,
                isAccelerating = horizontalInput != 0 || verticalInput != 0,
                BASE_ACCELERATION = BASE_ACCELERATION,
                BASE_MAXSPEED = BASE_MAXSPEED
            });
    }
}

public class ShipState : EventArgs {
    public float time { get; set; }
    public RECSRigidBody rigidBody { get; set; }
    public Vector3 position { get; set; }
    public Vector3 lookInput { get; set; }
    public float horizontalInput { get; set; }
    public float verticalInput { get; set; }
    public bool brakeInput { get; set; }
    public bool boostInput { get; set; }
    public bool isAccelerating { get; set; }
    public float BASE_ACCELERATION { get; set; }
    public float BASE_MAXSPEED { get; set; }
}

/**
 * On each frame
 *  - record player input
 *  - publish a player input event to all subscribers
 *  - for each subscriber, create property request(s) based on the event, and send the request(s) to the controller
 *  - execute the highest priority request for each property.
 */
