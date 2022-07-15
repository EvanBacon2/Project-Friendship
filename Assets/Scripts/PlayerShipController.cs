using UnityEngine;

public class PlayerShipController {
    PlayerShipModel shipModel;

    private int accelerationPriority;
    private int maxSpeedPriority;
    private int forcePriority;
    private int directionPriority;
    private int magnitudePriority;
    private int rotationPriority;

    private bool blockAcceleration;
    private bool blockMaxSpeed;
    private bool blockDirection;
    private bool blockForce;

    private float priorityAcceleration;
    private float priorityMaxSpeed;
    private Vector3 priorityForce;
    private ForceMode priorityForceMode;
    private Vector3 priorityDirection;
    private float priorityMagnitude;
    private Quaternion priorityRotation;

    public PlayerShipController(PlayerShipModel shipModel) {
        this.shipModel = shipModel;

        accelerationPriority = Request.NoRequest;
        maxSpeedPriority = Request.NoRequest;
        forcePriority = Request.NoRequest;
        directionPriority = Request.NoRequest;
        magnitudePriority = Request.NoRequest;
        rotationPriority = Request.NoRequest;
    }

    public void executeRequests() {
        if (accelerationPriority != Request.NoRequest && !blockAcceleration)
            shipModel.acceleration = priorityAcceleration;

        if (maxSpeedPriority != Request.NoRequest && !blockMaxSpeed)
            shipModel.maxSpeed = priorityMaxSpeed;

        if (rotationPriority != Request.NoRequest)
            shipModel.rotation = priorityRotation;

        if (directionPriority != Request.NoRequest && magnitudePriority != Request.NoRequest && !blockDirection)
            shipModel.velocity = priorityDirection.normalized * priorityMagnitude;
        else if (directionPriority != Request.NoRequest && !blockDirection)
            shipModel.velocity = priorityDirection.normalized * shipModel.velocity.magnitude;
        else if (magnitudePriority != Request.NoRequest)
            shipModel.velocity = shipModel.velocity.normalized * priorityMagnitude;

        if (forcePriority != Request.NoRequest && !blockForce)
            shipModel.addForce(priorityForce, priorityForceMode);

        accelerationPriority = Request.NoRequest;
        maxSpeedPriority = Request.NoRequest;
        forcePriority = Request.NoRequest;
        directionPriority = Request.NoRequest;
        magnitudePriority = Request.NoRequest;
        rotationPriority = Request.NoRequest;

        blockAcceleration = false;
        blockMaxSpeed = false;
        blockDirection = false;
        blockForce = false;
    }

    public void requestAcceleration(int requestID, float acceleration) {
        if (PlayerShipRequestPriority.accelerationPriority(requestID) > accelerationPriority) {
            accelerationPriority = PlayerShipRequestPriority.accelerationPriority(requestID);
            priorityAcceleration = acceleration;
            blockAcceleration = false;
        }
    }

    public void requestAccelerationBlock(int requestID) {
        if (PlayerShipRequestPriority.accelerationPriority(requestID) > accelerationPriority) {
            accelerationPriority = PlayerShipRequestPriority.accelerationPriority(requestID);
            blockAcceleration = true;
        }
    }

    public void requestMaxSpeed(int requestID, float maxSpeed) {
        if (PlayerShipRequestPriority.maxSpeedPriority(requestID) > maxSpeedPriority) {
            maxSpeedPriority = PlayerShipRequestPriority.maxSpeedPriority(requestID);
            priorityMaxSpeed = maxSpeed;
        }
    }

    public void requestMaxSpeedBlock(int requestID) {
        if (PlayerShipRequestPriority.maxSpeedPriority(requestID) > maxSpeedPriority) {
            maxSpeedPriority = PlayerShipRequestPriority.maxSpeedPriority(requestID);
            blockMaxSpeed = true;
        }
    }

    public void requestForce(int requestID, Vector3 force, ForceMode mode) {
        if (PlayerShipRequestPriority.forcePriority(requestID) > forcePriority) {
            forcePriority = PlayerShipRequestPriority.forcePriority(requestID);
            priorityForce = force;
            priorityForceMode = mode;
        }
    }

    public void requestforceBlock(int requestID) {
        if (PlayerShipRequestPriority.forcePriority(requestID) > forcePriority) {
            forcePriority = PlayerShipRequestPriority.forcePriority(requestID);
            blockForce = true;
        }
    }

    public void requestDirection(int requestID, Vector3 direction) {
        if (PlayerShipRequestPriority.directionPriority(requestID) > directionPriority) {
            directionPriority = PlayerShipRequestPriority.directionPriority(requestID);
            priorityDirection = direction;
        }
    }

    public void requestDirectionBlock(int requestID) {
        if (PlayerShipRequestPriority.directionPriority(requestID) > directionPriority) {
            directionPriority = PlayerShipRequestPriority.directionPriority(requestID);
            blockDirection = true;
        }
    }

    public void requestMagnitude(int requestID, float magnitude) {
        if (PlayerShipRequestPriority.magnitudePriority(requestID) > magnitudePriority) {
            magnitudePriority = PlayerShipRequestPriority.magnitudePriority(requestID);
            priorityMagnitude = magnitude;
        }
    }

    public void requestRotation(int requestID, Quaternion rotation) {
        if (PlayerShipRequestPriority.rotationPriority(requestID) > rotationPriority) {
            rotationPriority = PlayerShipRequestPriority.rotationPriority(requestID);
            priorityRotation = rotation;
        }
    }
}

/**
 * Todo
 *  - add ability for requests to know if they were chosen.
 *  - finish implementing BoostRequest.
 */
