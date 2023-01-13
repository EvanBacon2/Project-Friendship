using UnityEngine;

public class MoveRequest : RequestSystem {
    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.isAccelerating) {
            (Vector3, ForceMode) force = (new Vector3(args.horizontalInput, args.verticalInput, 0).normalized * args.shipModel.acceleration, ForceMode.Force);
            args.shipController.setRequest(this, RequestClass.Move, PlayerShipProperties.Force, force);
        }
    }
}
