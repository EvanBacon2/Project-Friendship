using UnityEngine;

public class BrakeRequest : RequestSystem {
    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.brakeInput) {
            args.shipController.setRequest(this, RequestClass.Brake, PlayerShipProperties.Magnitude, slowShip(args.shipModel));
            args.shipController.blockRequest(this, RequestClass.Brake, PlayerShipProperties.Acceleration);
            args.shipController.blockRequest(this, RequestClass.Brake, PlayerShipProperties.MaxSpeed);
            args.shipController.blockRequest(this, RequestClass.Brake, PlayerShipProperties.Force);
        }
    }

    private float slowShip(PlayerShipModel shipModel) {
        if (shipModel.velocity.magnitude <= PlayerShipModel.baseAcceleration * .02f)
            return 0;
        else
            return shipModel.velocity.magnitude + (PlayerShipModel.baseAcceleration * -1f * Time.fixedDeltaTime);
    }
}
