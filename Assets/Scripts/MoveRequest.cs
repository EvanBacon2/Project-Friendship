using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRequest  {
    private PlayerShipModel shipModel;
    private PlayerShipController shipController;

    public MoveRequest(PlayerShipModel shipModel, PlayerShipController shipController) {
        this.shipModel = shipModel;
        this.shipController = shipController;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.isAccelerating) {
            shipController.requestDirection(10, changeDirection(args.horizontalInput, args.verticalInput));
            shipController.requestMagnitude(10, changeMagnitude(args.horizontalInput, args.verticalInput));
        }
    }

    public Vector3 changeDirection(float hi, float vi) {
        Vector3 movement = new Vector3(hi, vi, 0).normalized;
        Vector3 newVelocity = shipModel.getVelocity() + movement * shipModel.acceleration * Time.fixedDeltaTime;
        return newVelocity.normalized;
    }

    public float changeMagnitude(float hi, float vi) {
        Vector3 movement = new Vector3(hi, vi, 0).normalized;
        Vector3 newVelocity = shipModel.getVelocity()+ movement * shipModel.acceleration* Time.fixedDeltaTime;
        
        if (newVelocity.magnitude > shipModel.maxSpeed)
            newVelocity = newVelocity.normalized * shipModel.maxSpeed;
        return newVelocity.magnitude;
    }
}
