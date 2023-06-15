using System;
using System.Collections.Generic;

using UnityEngine;

public class BrakeRequest : RequestSystem<ShipState> {
    private RECSShipbody rb;
    float brakeRate; 

    public override void OnStateReceived(object sender, ShipState state) {
        rb = state.rigidbody;
        brakeRate = rb.BASE_LINEAR_ACCELERATION * -1f;

        Vector3 pendingVelocity = rb.Velocity.pendingValue();
        rb.calcPendingVelocity(pendingVelocity);

        if (state.brake) {
            rb.Force.mutate(PriorityAlias.Brake, (List<(Vector3, ForceMode)> forces) => { 
                forces.Add((new Vector3(pendingVelocity.x, pendingVelocity.y, 0).normalized * brakeRate, ForceMode.Force));
                return forces;
            });
            rb.LinearAcceleration.block(PriorityAlias.Brake);
            rb.LinearMax.block(PriorityAlias.Brake);
        }
    }
}
