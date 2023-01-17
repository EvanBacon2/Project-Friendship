using UnityEngine;
using Request;

public class MoveRequest : RequestSystem<ShipState> {
    private ShipModel model;

    public MoveRequest(ShipModel model) {
        this.model = model;
    }

    public override void OnStateReceived(object sender, ShipState args) {
        if (args.isAccelerating) {
            (Vector3, ForceMode) force = (new Vector3(args.horizontalInput, args.verticalInput, 0).normalized * args.playerShip.acceleration, ForceMode.Force);
            model.Force.takeRequest(new SetRequest<(Vector3, ForceMode)>(this, RequestClass.Move, force));
            //args.shipController.setRequest(this, RequestClass.Move, PlayerShipProperties.Force, force);
        }
    }
}
