using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputProvider : MonoBehaviour {
    private PlayerShipModel playerShip;

    private Vector3 lookInput;
    private float horizontalInput;
    private float verticalInput;
    private bool brakeInput;
    private bool boostInput;

    public event EventHandler<ShipState> StateReceived;

    public List<RequestSystem<ShipState>> requestSystems;

    void Start() {
        playerShip = new PlayerShipModel(GetComponent<Rigidbody>());
        
        requestSystems = new() {
            new BoostRequest(playerShip.model),
            new BrakeRequest(playerShip.model),
            new LookAtMouseRequest(playerShip.model),
            new MoveRequest(playerShip.model)
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
        playerShip.model.executeRequests();
        boostInput = false;
    }

    protected virtual void OnStateReceived() {
            StateReceived?.Invoke(this, new ShipState() { 
                time = Time.time,
                playerShip = playerShip,
                lookInput = lookInput,
                horizontalInput = horizontalInput, 
                verticalInput = verticalInput,
                brakeInput = brakeInput,
                boostInput = boostInput,
                isAccelerating = horizontalInput != 0 || verticalInput != 0 
            });
    }
}

public class ShipState : EventArgs {
    public float time { get; set; }
    public PlayerShipModel playerShip { get; set; }
    public Vector3 lookInput { get; set; }
    public float horizontalInput { get; set; }
    public float verticalInput { get; set; }
    public bool brakeInput { get; set; }
    public bool boostInput { get; set;  }
    public bool isAccelerating { get; set;  }
}

/**
 * On each frame
 *  - record player input
 *  - publish a player input event to all subscribers
 *  - for each subscriber, create property request(s) based on the event, and send the request(s) to the controller
 *  - execute the highest priority request for each property.
 */
