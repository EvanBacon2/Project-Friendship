using System.Collections.Generic;
using UnityEngine;

/*
 * Moves player ship in the direction indicated by player input
 */
public class MoveSystem : RequestSystem<ShipState> {
    private RECSShipbody rb;
    private Vector3 newVelocity = new Vector3();

    public override void OnStateReceived(object sender, ShipState state) {
        rb = state.rigidbody;

        if (state.isAccelerating) {
            (Vector3, ForceMode) force = (new Vector3(state.horizontalMove, state.verticalMove, 0).normalized * rb.LinearAcceleration.value, ForceMode.Force);

            rb.Force.mutate(PriorityAlias.Move, (List<(Vector3, ForceMode)> forces) => { 
                forces.Add(force); 
                return forces;
            });

            newVelocity.x = rb.Velocity.value.x + force.Item1.x * Time.fixedDeltaTime;
            newVelocity.y = rb.Velocity.value.y + force.Item1.y * Time.fixedDeltaTime;

            if (newVelocity.magnitude > rb.LinearMax.value) {
                float diff = newVelocity.magnitude - rb.Velocity.value.magnitude;
                float ratio = Mathf.Abs(newVelocity.x) / (Mathf.Abs(newVelocity.x) + Mathf.Abs(newVelocity.y));
                
                Vector3 opp = new Vector3(newVelocity.x, newVelocity.y, 0).normalized * -diff / Time.fixedDeltaTime;
                (Vector3, ForceMode) oppForce = (opp, ForceMode.Force);
                
                rb.Force.mutate(PriorityAlias.Move, (List<(Vector3, ForceMode)> forces) => { 
                    forces.Add(oppForce); 
                    return forces;
                });
            }
        }
    }
}
