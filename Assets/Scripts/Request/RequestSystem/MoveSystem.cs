using UnityEngine;

public class MoveSystem : RequestSystem<ShipState> {
    private RECSRigidbody rb;

    public override void OnStateReceived(object sender, ShipState state) {
        rb = state.rigidbody;

        if (state.isAccelerating) {
            (Vector3, ForceMode) force = (new Vector3(state.horizontalMove, state.verticalMove, 0).normalized * rb.Acceleration.value, ForceMode.Force);
            rb.Force.set(RequestClass.Move, force);
        }
    }
}
