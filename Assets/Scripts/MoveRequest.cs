using UnityEngine;

public class MoveRequest : Request {
    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.isAccelerating) {
            (Vector3, ForceMode) force = (new Vector3(args.horizontalInput, args.verticalInput, 0).normalized * args.shipModel.acceleration, ForceMode.Force);
            args.shipController.makeRequest(this, RequestType.Move, PlayerShipProperties.Force, force);
        }
    }
}
