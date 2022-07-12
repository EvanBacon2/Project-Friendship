using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController {
    private int directionPriority;
    private int magnitudePriority;
    private int rotationPriority;
    private int boostLevelPriority;

    private Vector3 priorityDirection;
    private float priorityMagnitude;
    private Quaternion priorityRotation;
    private int priorityBoostLevel;

    PlayerShipModel shipModel;

    public PlayerShipController(PlayerShipModel shipModel) {
        this.shipModel = shipModel;
        directionPriority = 0;
        magnitudePriority = 0;
    }

    public void executeRequests() {
        float oldMag = shipModel.selfRigidBody.velocity.magnitude;
        if (directionPriority > 0)
            shipModel.selfRigidBody.velocity = priorityDirection;
        if (magnitudePriority > 0) 
            shipModel.selfRigidBody.velocity = shipModel.selfRigidBody.velocity.normalized * priorityMagnitude;
        else
            shipModel.selfRigidBody.velocity = shipModel.selfRigidBody.velocity.normalized * oldMag;

        //shipModel.direction = priorityDirection
        //shipModel.magnitude = priorityMagnitude
        //shipModel.rotation = priorityRotation
        //shipModel.boostLevel = priorityBoostlevel

        directionPriority = 0;
        magnitudePriority = 0;
    }

    public void requestDirection(int priority, Vector3 direction) {
        if (priority > directionPriority) {
            directionPriority = priority;
            priorityDirection = direction;
        }
    }
    public void requestMagnitude(int priority, float magnitude) {
        if (priority > magnitudePriority) {
            magnitudePriority = priority;
            priorityMagnitude = magnitude;
        }
    }
    public void requestRotation(int priority, Quaternion rotation) {
        if (priority > rotationPriority)
            priorityRotation = rotation;
    }
    public void requestBoostLevel(int priority, int boostLevel) {
        if (priority > boostLevelPriority)
            priorityBoostLevel = boostLevel;
    }

    /*void Start() {
        
    }

    void Update() {
        
    }

	private void FixedUpdate() {
        /*if (shipModel.brakeOn) {
            shipModel.slowShip();
        } else {
            if (shipModel.isAccelerating()) {
                if (shipModel.activateBoost)
                    shipModel.boostShip();
                shipModel.accelerateShip();
            }
        }

        if (!shipModel.isAccelerating())
            shipModel.shaveBoostSpeed();

        shipModel.checkSpeed();
        shipModel.rotateToMouse();
    }*/
}
