using UnityEngine;

public class MoveRequest  {
    private PlayerShipController shipController;

    public MoveRequest(PlayerShipController shipController) {
        this.shipController = shipController;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (args.isAccelerating) 
            shipController.requestForce(Request.Move, 
                new Vector3(args.horizontalInput, args.verticalInput, 0).normalized * args.shipModel.acceleration, ForceMode.Force);
    }
}
