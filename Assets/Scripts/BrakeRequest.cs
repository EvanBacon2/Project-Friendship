using UnityEngine;

public class BrakeRequest : Request {
    public override RequestType type { get { return RequestType.Brake; } }

    private PlayerShipController shipController;

    public BrakeRequest(PlayerShipController shipController) {
        this.shipController = shipController;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.brakeInput) {
            shipController.makeRequest(this, PlayerShipProperties.Magnitude, slowShip(args.shipModel));
            shipController.blockRequest(this, PlayerShipProperties.Acceleration);
            shipController.blockRequest(this, PlayerShipProperties.MaxSpeed);
            shipController.blockRequest(this, PlayerShipProperties.Force);
        }
    }

    private float slowShip(PlayerShipModel shipModel) {
        if (shipModel.velocity.magnitude < shipModel.acceleration * .005f)
            return 0;
        else
            return shipModel.velocity.magnitude + (shipModel.acceleration * -0.8f * Time.fixedDeltaTime);
    }
}
