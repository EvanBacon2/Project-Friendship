using UnityEngine;
using Request;

public class BrakeRequest : RequestSystem<ShipState> {
    private ShipModel model;
    private BlockRequest block;

    public BrakeRequest(ShipModel model) {
        this.model = model;
      
        block = new BlockRequest(this, RequestClass.Brake);
    }

    public override void OnStateReceived(object sender, ShipState args) {
        if (args.brakeInput) {
            //args.shipController.setRequest(this, RequestClass.Brake, PlayerShipProperties.Magnitude, slowShip(args.shipModel));
            //args.shipController.blockRequest(this, RequestClass.Brake, PlayerShipProperties.Acceleration);
            //args.shipController.blockRequest(this, RequestClass.Brake, PlayerShipProperties.MaxSpeed);
            //args.shipController.blockRequest(this, RequestClass.Brake, PlayerShipProperties.Force);
            model.Magnitude.takeRequest(new SetRequest<float>(this, RequestClass.Brake, slowShip(args.playerShip)));
            model.Acceleration.takeRequest(block);
            model.MaxSpeed.takeRequest(block);
            model.Force.takeRequest(block);
        }
    }

    private float slowShip(PlayerShipModel playerShip) {
        if (playerShip.magnitude <= PlayerShipModel.BASE_ACCELERATION * .02f)
            return 0;
        else
            return playerShip.magnitude + (PlayerShipModel.BASE_ACCELERATION * -1f * Time.fixedDeltaTime);
    }
}
