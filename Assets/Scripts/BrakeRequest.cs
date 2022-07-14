using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrakeRequest {
    private PlayerShipModel shipModel;
    private PlayerShipController shipController;

    public BrakeRequest(PlayerShipModel shipModel, PlayerShipController shipController) {
        this.shipModel = shipModel;
        this.shipController = shipController;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.brakeInput) {
            shipController.requestDirection(20, shipModel.getVelocity().normalized);
            shipController.requestMagnitude(20, slowShip());
        }
    }

    private float slowShip() {
        if (shipModel.getVelocity().magnitude < shipModel.acceleration * .005f)
            return 0;
        else
            return shipModel.getVelocity().magnitude + (shipModel.acceleration * -0.8f * Time.fixedDeltaTime);
    }
}
