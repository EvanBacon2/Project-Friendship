using UnityEngine;

public class MoveRequest : Request {
    public override RequestType type { get { return RequestType.Move; } }
    private PlayerShipController shipController;

    public void Start() {
        shipController = GetComponent<PlayerShipController>();
    }

    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.isAccelerating) {
            (Vector3, ForceMode) force = (new Vector3(args.horizontalInput, args.verticalInput, 0).normalized * args.shipModel.acceleration, ForceMode.Force);
            shipController.makeRequest(this, PlayerShipProperties.Force, force);
        }
    }
}
