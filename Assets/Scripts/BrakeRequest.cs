using UnityEngine;
using Request;

public class BrakeRequest : RequestSystem {
    private ShipModel model;
    private BlockRequest blockAcceleration;
    private BlockRequest blockMaxSpeed;
    private BlockRequest blockForce;

    public BrakeRequest(ShipModel model) {
        this.model = model;
        blockAcceleration = new BlockRequest(this, RequestClass.Brake);
        blockMaxSpeed = new BlockRequest(this, RequestClass.Brake);
        blockForce = new BlockRequest(this, RequestClass.Brake);
    }

    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.brakeInput) {
            //args.shipController.setRequest(this, RequestClass.Brake, PlayerShipProperties.Magnitude, slowShip(args.shipModel));
            //args.shipController.blockRequest(this, RequestClass.Brake, PlayerShipProperties.Acceleration);
            //args.shipController.blockRequest(this, RequestClass.Brake, PlayerShipProperties.MaxSpeed);
            //args.shipController.blockRequest(this, RequestClass.Brake, PlayerShipProperties.Force);
            model.Magnitude.takeRequest(new SetRequest<float>(this, RequestClass.Brake, slowShip(args.shipModel)));
            model.Acceleration.takeRequest(blockAcceleration);
            model.MaxSpeed.takeRequest(blockMaxSpeed);
            model.Force.takeRequest(blockForce);
        }
    }

    private float slowShip(PlayerShipModel shipModel) {
        if (shipModel.velocity.magnitude <= PlayerShipModel.baseAcceleration * .02f)
            return 0;
        else
            return shipModel.velocity.magnitude + (PlayerShipModel.baseAcceleration * -1f * Time.fixedDeltaTime);
    }
}
