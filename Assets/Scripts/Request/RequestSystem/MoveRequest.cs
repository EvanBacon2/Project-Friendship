using System;
using System.Collections.Generic;

using UnityEngine;

public class MoveRequest : RequestSystem<ShipState> {
    private RECSRigidBody model;

    public MoveRequest(RECSRigidBody model) {
        this.model = model;
    }

    public void OnStateReceived(object sender, ShipState args) {
        if (args.isAccelerating) {
            (Vector3, ForceMode) force = (new Vector3(args.horizontalInput, args.verticalInput, 0).normalized * model.Acceleration.value, ForceMode.Force);
            model.Force.set(RequestClass.Move, force);
        }
    }

    public void onRequestsExecuted(HashSet<Guid> executedRequests) {
    
    }
}
