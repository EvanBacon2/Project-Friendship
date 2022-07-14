using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController {
    private int directionPriority;
    private int magnitudePriority;
    private int rotationPriority;
    private int boostPriority;

    private Vector3 priorityDirection;
    private float priorityMagnitude;
    private Quaternion priorityRotation;

    PlayerShipModel shipModel;

    public PlayerShipController(PlayerShipModel shipModel) {
        this.shipModel = shipModel;
        directionPriority = 0;
        magnitudePriority = 0;
        rotationPriority = 0;
        boostPriority = 0;
    }

    public void executeRequests() {
        float oldMag = shipModel.selfRigidBody.velocity.magnitude;

        if (directionPriority > 0)
            shipModel.selfRigidBody.velocity = priorityDirection;

        if (magnitudePriority > 0) 
            shipModel.selfRigidBody.velocity = shipModel.selfRigidBody.velocity.normalized * priorityMagnitude;
        else
            shipModel.selfRigidBody.velocity = shipModel.selfRigidBody.velocity.normalized * oldMag;

        if (rotationPriority > 0)
            shipModel.selfTransform.rotation = priorityRotation;

        if (boostPriority > 0) {
            shipModel.boostShip();
        }

        directionPriority = 0;
        magnitudePriority = 0;
        rotationPriority = 0;
        boostPriority = 0;
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
        if (priority > rotationPriority) {
            rotationPriority = priority;
            priorityRotation = rotation;
        }
    }
    public void requestBoost(int priority) {
        if (priority > boostPriority) {
            boostPriority = priority;
        }
    }

    /*void Start() {
        
    }

    void Update() {
        
    }*/

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

        shipModel.checkSpeed();*/
        //shipModel.rotateToMouse();
    }
}
