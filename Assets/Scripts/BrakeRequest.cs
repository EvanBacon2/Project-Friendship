using UnityEngine;

public class BrakeRequest {
    private PlayerShipController shipController;

    public BrakeRequest(PlayerShipController shipController) {
        this.shipController = shipController;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.brakeInput) {
            shipController.makeRequest(PlayerShipProperties.Magnitude, Request.Brake, slowShip(args.shipModel));
            shipController.blockRequest(PlayerShipProperties.Acceleration, Request.Brake);
            shipController.blockRequest(PlayerShipProperties.MaxSpeed, Request.Brake);
            shipController.blockRequest(PlayerShipProperties.Force, Request.Brake);
        }
    }

    private float slowShip(PlayerShipModel shipModel) {
        if (shipModel.velocity.magnitude < shipModel.acceleration * .005f)
            return 0;
        else
            return shipModel.velocity.magnitude + (shipModel.acceleration * -0.8f * Time.fixedDeltaTime);
    }
}
