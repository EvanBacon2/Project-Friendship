using System;
using System.Collections.Generic;

using UnityEngine;

public class BrakeRequest : RequestSystem<ShipState> {
    private RECSRigidBody model;

    public BrakeRequest(RECSRigidBody model) {
        this.model = model;
    }

    public void OnStateReceived(object sender, ShipState state) {
        if (state.brakeInput) {
            model.Magnitude.set(RequestClass.Brake, slowShip(state));
            model.Acceleration.block(RequestClass.Brake);
            model.MaxSpeed.block(RequestClass.Brake);
            model.Force.block(RequestClass.Brake);
        }
    }

    public void onRequestsExecuted(HashSet<Guid> executedRequests) {
    
    }

    private float slowShip(ShipState state) {
        if (model.Magnitude.value <= state.BASE_ACCELERATION * .02f)
            return 0;
        else
            return model.Magnitude.value + (state.BASE_ACCELERATION * -1f * Time.fixedDeltaTime);
    }
}
