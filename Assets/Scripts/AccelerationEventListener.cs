using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationEventListener : DirectionObserver, MagnitudeObserver {
    private PlayerShipModel shipModel;

    public event DirectionEvent directionEvent;
    public event MagnitudeEvent magnitudeEvent;

    public int directionPriority { get; set; }
    public int magnitudePriority { get; set; }

    public AccelerationEventListener(PlayerShipModel shipModel) {
        this.shipModel = shipModel; 

        directionPriority = 10;
        magnitudePriority = 10;
    }

    public bool checkDirectionEvent() {
        return shipModel.isAccelerating();
    }

    public DirectionEvent getDirectionEvent() {
        directionEvent = changeDirection;
        return directionEvent;
    }

    public bool checkMagnitudeEvent() {
        return shipModel.isAccelerating();
    }

    public MagnitudeEvent getMagnitudeEvent() {
        MagnitudeEvent de = changeMagnitude;
        return de;
    }

    public Vector3 changeDirection() {
        Vector3 movement = new Vector3(shipModel.horizontalInput, shipModel.verticalInput, 0).normalized;
        Vector3 newVelocity = shipModel.selfRigidBody.velocity + movement * shipModel.accelerationForce() * Time.fixedDeltaTime;
        return newVelocity.normalized;
    }

    public float changeMagnitude() {
        Vector3 movement = new Vector3(shipModel.horizontalInput, shipModel.verticalInput, 0).normalized;
        Vector3 newVelocity = shipModel.selfRigidBody.velocity + movement * shipModel.accelerationForce() * Time.fixedDeltaTime;
        
        if (newVelocity.magnitude > shipModel.speedLimit())
            newVelocity = newVelocity.normalized * shipModel.speedLimit();
        return newVelocity.magnitude;
    }
}
