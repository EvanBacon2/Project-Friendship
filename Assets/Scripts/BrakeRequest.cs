using UnityEngine;

public class BrakeRequest {
    private PlayerShipController shipController;

    public BrakeRequest(PlayerShipController shipController) {
        this.shipController = shipController;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.brakeInput) {
            shipController.requestDirectionBlock(Request.Brake);
            shipController.requestMagnitude(Request.Brake, slowShip(args.shipModel));
            shipController.requestAccelerationBlock(Request.Brake);
            shipController.requestMaxSpeedBlock(Request.Brake);
            shipController.requestforceBlock(Request.Brake);
        }
    }

    private float slowShip(PlayerShipModel shipModel) {
        if (shipModel.velocity.magnitude < shipModel.acceleration * .005f)
            return 0;
        else
            return shipModel.velocity.magnitude + (shipModel.acceleration * -0.8f * Time.fixedDeltaTime);
    }
}
