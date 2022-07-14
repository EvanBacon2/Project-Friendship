using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController {
    PlayerShipModel shipModel;

    private int accelerationPriority;
    private int maxSpeedPriority;
    private int directionPriority;
    private int magnitudePriority;
    private int rotationPriority;

    private float priorityAcceleration;
    private float priorityMaxSpeed;
    private Vector3 priorityDirection;
    private float priorityMagnitude;
    private Quaternion priorityRotation;

    public PlayerShipController(PlayerShipModel shipModel) {
        this.shipModel = shipModel;

        accelerationPriority = 0;
        maxSpeedPriority = 0;
        directionPriority = 0;
        magnitudePriority = 0;
        rotationPriority = 0;
    }

    public void executeRequests() {
        float oldMag = shipModel.selfRigidBody.velocity.magnitude;

        if (accelerationPriority > 0)
            shipModel.acceleration = priorityAcceleration;

        if (maxSpeedPriority > 0)
            shipModel.maxSpeed = priorityMaxSpeed;

        if (directionPriority > 0)
            shipModel.selfRigidBody.velocity = priorityDirection;

        if (magnitudePriority > 0) 
            shipModel.selfRigidBody.velocity = shipModel.selfRigidBody.velocity.normalized * priorityMagnitude;
        else
            shipModel.selfRigidBody.velocity = shipModel.selfRigidBody.velocity.normalized * oldMag;

        if (rotationPriority > 0)
            shipModel.selfTransform.rotation = priorityRotation;

        accelerationPriority = 0;
        maxSpeedPriority = 0;
        directionPriority = 0;
        magnitudePriority = 0;
        rotationPriority = 0;
    }

    public void requestAcceleration(int priority, float acceleration) {
        if (priority > accelerationPriority) {
            accelerationPriority = priority;
            priorityAcceleration = acceleration;
        }
    }

    public void requestMaxSpeed(int priority, float maxSpeed) {
        if (priority > maxSpeedPriority) {
            maxSpeedPriority = priority;
            priorityMaxSpeed = maxSpeed;
        }
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
}
