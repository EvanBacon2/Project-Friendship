using System;
using System.Collections.Generic;

using UnityEngine;

public class BrakeRequest : RequestSystem<ShipState> {
    private RECSShipbody rb;

    public override void OnStateReceived(object sender, ShipState state) {
        rb = state.rigidbody;

        if (state.brake) {
            rb.Magnitude.set(RequestClass.Brake, slowShip());
            rb.LinearAcceleration.block(RequestClass.Brake);
            rb.LinearMax.block(RequestClass.Brake);
            rb.Force.block(RequestClass.Brake);
        }
    }

    private float slowShip() {
        if (rb.Magnitude.value <= rb.BASE_LINEAR_ACCELERATION * .02f)
            return 0;
        else
            return rb.Magnitude.value + (rb.BASE_LINEAR_ACCELERATION * -1f * Time.fixedDeltaTime);
    }
}
