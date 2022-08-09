using UnityEngine;

public class BrakeRequest : Request {
    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.brakeInput) {
            args.shipController.makeRequest(this, RequestType.Brake, PlayerShipProperties.Magnitude, slowShip(args.shipModel));
            args.shipController.blockRequest(this, RequestType.Brake, PlayerShipProperties.Acceleration);
            args.shipController.blockRequest(this, RequestType.Brake, PlayerShipProperties.MaxSpeed);
            args.shipController.blockRequest(this, RequestType.Brake, PlayerShipProperties.Force);
        }
    }

    private float slowShip(PlayerShipModel shipModel) {
        if (shipModel.velocity.magnitude < PlayerShipModel.baseAcceleration * .02f)
            return 0;
        else
            return shipModel.velocity.magnitude + (PlayerShipModel.baseAcceleration * -1f * Time.fixedDeltaTime);
    }
}
