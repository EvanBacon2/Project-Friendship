using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController {
    PlayerShipModel shipModel;

    private Dictionary<string, object> priorityRequests;
    private HashSet<string> blockedRequests;
    private Dictionary<string, int> requestPriorities;

    /*private int accelerationPriority;
    private int maxSpeedPriority;
    private int forcePriority;
    private int magnitudePriority;
    private int rotationPriority;

    private bool blockAcceleration;
    private bool blockMaxSpeed;
    private bool blockForce;

    private float priorityAcceleration;
    private float priorityMaxSpeed;
    private Vector3 priorityForce;
    private ForceMode priorityForceMode;
    private float priorityMagnitude;
    private Quaternion priorityRotation;*/

    public PlayerShipController(PlayerShipModel shipModel) {
        this.shipModel = shipModel;

        priorityRequests = new Dictionary<string, object>();
        blockedRequests = new HashSet<string>();
        requestPriorities = new Dictionary<string, int>();

        /*accelerationPriority = Request.NoRequest;
        maxSpeedPriority = Request.NoRequest;
        forcePriority = Request.NoRequest;
        magnitudePriority = Request.NoRequest;
        rotationPriority = Request.NoRequest;*/
    }

    public void executeRequests() {
        /*if (accelerationPriority != Request.NoRequest && !blockAcceleration)
            shipModel.acceleration = priorityAcceleration;

        if (maxSpeedPriority != Request.NoRequest && !blockMaxSpeed)
            shipModel.maxSpeed = priorityMaxSpeed;

        if (rotationPriority != Request.NoRequest)
            shipModel.rotation = priorityRotation;

        if (forcePriority != Request.NoRequest && !blockForce)
            shipModel.addForce(priorityForce, priorityForceMode);

        if (magnitudePriority != Request.NoRequest)
            shipModel.magnitude = priorityMagnitude;

        accelerationPriority = Request.NoRequest;
        maxSpeedPriority = Request.NoRequest;
        forcePriority = Request.NoRequest;
        magnitudePriority = Request.NoRequest;
        rotationPriority = Request.NoRequest;

        blockAcceleration = false;
        blockMaxSpeed = false;
        blockForce = false;*/

        foreach (KeyValuePair<string, object> entry in priorityRequests) {
            if (entry.Key == PlayerShipProperties.Force) 
                shipModel.addForce(((Vector3, ForceMode))entry.Value);
            else 
                shipModel.GetType().GetProperty(entry.Key).SetValue(shipModel, entry.Value);
        }

        priorityRequests.Clear();
        blockedRequests.Clear();
        requestPriorities.Clear();
    }

    public void makeRequest<T>(string property, int priorityID, T request) {
        int priority = PlayerShipRequestPriorities.getPriority(property, priorityID);
        if (!priorityRequests.ContainsKey(property) || priority > requestPriorities[property]) {
            priorityRequests[property] = request;
            requestPriorities[property] = priority;
            blockedRequests.Remove(property);
        }
    }

    public void blockRequest(string property, int priorityID) {
        int priority = PlayerShipRequestPriorities.getPriority(property, priorityID);
        if (!priorityRequests.ContainsKey(property) || priority > requestPriorities[property]) {
            requestPriorities[property] = priority;
            priorityRequests.Remove(property);
            blockedRequests.Add(property);
        }
    }

    /*public void requestAcceleration(int requestID, float acceleration) {
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
    }*/
}

/**
 * Todo
 *  - Find way to cut down on ShipController code.(Generics?)
 *  - add ability for requests to know if they were chosen.
 *  - finish implementing BoostRequest.
 */
