using System.Collections.Generic;
using UnityEngine;

public class MoveSystem : RequestSystem<ShipState> {
    private RECSShipbody rb;

    public override void OnStateReceived(object sender, ShipState state) {
        rb = state.rigidbody;

        if (state.isAccelerating) {
            (Vector3, ForceMode) force = (new Vector3(state.horizontalMove, state.verticalMove, 0).normalized * rb.LinearAcceleration.value, ForceMode.Force);
            rb.Force.mutate(RequestClass.Move, (List<(Vector3, ForceMode)> forces) => { 
                forces.Add(force); 
                return forces;
            });
        }
    }
}
