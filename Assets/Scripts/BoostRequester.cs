using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostRequester {
    private PlayerShipModel shipModel;
    private PlayerShipController shipController;

    public BoostRequester(PlayerShipModel shipModel, PlayerShipController shipController) {
        this.shipModel = shipModel;
        this.shipController = shipController;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.boostInput) {
            shipController.requestBoost(10);
        }
    }

    private float slowShip() {
        if (shipModel.selfRigidBody.velocity.magnitude < shipModel.accelerationForce() * .005f)
            return 0;
        else
            return shipModel.selfRigidBody.velocity.magnitude + (shipModel.accelerationForce() * -0.8f * Time.fixedDeltaTime);
    }
}
