using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrakeRequester {
    private PlayerShipModel shipModel;
    private PlayerShipController shipController;

    public BrakeRequester(PlayerShipModel shipModel, PlayerShipController shipController) {
        this.shipModel = shipModel;
        this.shipController = shipController;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.brakeInput) {
            shipController.requestDirection(20, shipModel.selfRigidBody.velocity.normalized);
            shipController.requestMagnitude(20, slowShip());
        }
    }

    private float slowShip() {
        if (shipModel.selfRigidBody.velocity.magnitude < shipModel.accelerationForce() * .005f)
            return 0;
        else
            return shipModel.selfRigidBody.velocity.magnitude + (shipModel.accelerationForce() * -0.8f * Time.fixedDeltaTime);
    }
}
