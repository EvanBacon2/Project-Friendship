using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputRecorder : MonoBehaviour {
    private PlayerShipModel shipModel;
    private PlayerShipController shipController;

    private Vector3 mouseInput;
    private Vector3 playerPos;
    private float horizontalInput;
    private float verticalInput;
    private bool brakeInput;
    private bool boostInput;

    public event EventHandler<PlayerInputArgs> PlayerInputRecorded;

    void Start() {
        shipModel = GetComponent<PlayerShipModel>();
        shipController = new PlayerShipController(shipModel);
        PlayerInputRecorded += new MoveRequest(shipModel, shipController).OnPlayerInputRecorded;
        PlayerInputRecorded += new BrakeRequest(shipModel, shipController).OnPlayerInputRecorded;
        PlayerInputRecorded += new LookAtMouseRequest(shipController).OnPlayerInputRecorded;
        PlayerInputRecorded += new BoostRequest(shipModel, shipController).OnPlayerInputRecorded;
    }

    void Update() {
        mouseInput = Input.mousePosition;
        playerPos = shipModel.position;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        brakeInput = Input.GetKey(KeyCode.LeftShift);
        boostInput = !boostInput ? Input.GetKeyDown(KeyCode.Space) : boostInput;
    }

    void FixedUpdate() {
        OnPlayerInputRecorded();
        shipController.executeRequests();
        boostInput = false;
    }

    protected virtual void OnPlayerInputRecorded() {
            PlayerInputRecorded?.Invoke(this, new PlayerInputArgs() { time = Time.time,
                                                                      mouseInput = mouseInput,
                                                                      playerPos = playerPos,
                                                                      horizontalInput = horizontalInput, 
                                                                      verticalInput = verticalInput,
                                                                      brakeInput = brakeInput,
                                                                      boostInput = boostInput,
                                                                      isAccelerating = horizontalInput != 0 || verticalInput != 0});;
    }
}

public class PlayerInputArgs : EventArgs {
    public float time { get; set; }
    public Vector3 mouseInput { get; set; }
    public Vector3 playerPos { get; set; }
    public float horizontalInput { get; set; }
    public float verticalInput { get; set; }
    public bool brakeInput { get; set; }
    public bool boostInput { get; set; }
    public bool isAccelerating { get; set; }
}

/**
 * On each frame
 *  - record player input
 *  - publish a player input event to all subscribers
 *  - for each subscriber, create a request(s) based on the event, and send the request to the controller
 *  - execute the highest priority requests.
 */
