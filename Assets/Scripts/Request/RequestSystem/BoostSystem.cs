using System;
using System.Collections.Generic;
using UnityEngine;

public enum BoostState {
    BOOST_START,
    BOOST_ACTIVE,
    RESET_START,
    RESET_ACTIVE,
    INACTIVE
}

public class BoostSystem : RequestSystem<ShipState> {
    private RECSRigidbody rb;
    private BoostManager manager;

    private bool wasAccelerating;
    private float lastBoostRequestTime;
    private Vector3 boostVelocity;

    private int boostLevel;
    private float brakeStep;//Rate at which to slow down the provided rigidbody when reseting boostLevel
    public float lastBoostTime;
    public float coastStart;
    public bool resetBoost;

    private Guid BoostStartForce;
    private Guid BoostStartAcceleration;
    private Guid BoostStartMaxSpeed;

    public BoostSystem() {
        wasAccelerating = false;
        lastBoostRequestTime = 0;

        boostLevel = 0;
        lastBoostTime = float.MinValue;
        coastStart = float.MaxValue;
        resetBoost = false;

        BoostStartForce = Guid.Empty;
        BoostStartAcceleration = Guid.Empty;
        BoostStartMaxSpeed = Guid.Empty;
    }

    public override void OnStateReceived(object sender, ShipState state) {
        rb = state.rigidbody;
        manager = state.manager;

        switch (getState(state)) {
            case BoostState.BOOST_START:
                if (boostLevel < manager.maxBoostLevel) {
                    BoostStartAcceleration = rb.Acceleration.mutate(this, RequestClass.Boost, (float val) => { return val * manager.boostAccelerationMod; });
                    BoostStartMaxSpeed = rb.MaxSpeed.mutate(this, RequestClass.Boost, (float val) => { return val + manager.boostMaxSpeedMod; });
                }

                boostVelocity = new Vector3(state.horizontalMove, state.verticalMove).normalized * rb.BASE_MAXSPEED * 2;
                BoostStartForce = rb.Force.mutate(this, RequestClass.Boost, (List<(Vector3, ForceMode)> forces) => {
                    forces.Add((boostVelocity * .01f, ForceMode.VelocityChange));
                    return forces; 
                });
                break;
            case BoostState.BOOST_ACTIVE:
                float x = (manager.boostTime - (state.time - lastBoostTime)) / manager.boostTime;
                float boostMagMod = x > .85f ? 0f + (1f * ((1 - x) * (1 - x) / 1f)) : (Mathf.Log10(x * 4) / 3f) + .6f;
                rb.Force.mutate(RequestClass.Boost, (List<(Vector3, ForceMode)> forces) => {
                    forces.Add((boostVelocity * boostMagMod, ForceMode.VelocityChange));
                    return forces; 
                });
                coastStart = float.MaxValue;
                break;
            case BoostState.RESET_START:
                brakeStep = (rb.Magnitude.value - rb.BASE_MAXSPEED) / 80;
                resetBoost = true;
                break;
            case BoostState.RESET_ACTIVE:
                if (rb.Magnitude.value > rb.BASE_MAXSPEED) {
                    rb.Magnitude.mutate(RequestClass.BoostReset, (float val) => { return val - brakeStep; });
                    rb.Force.block(RequestClass.BoostReset);
                } else {
                    rb.Acceleration.set(RequestClass.BoostReset, rb.BASE_ACCELERATION);
                    rb.MaxSpeed.set(RequestClass.BoostReset, rb.BASE_MAXSPEED);
                    boostLevel = 0;
                    resetBoost = false;
                }
                break;
            case BoostState.INACTIVE:
                if (!state.isAccelerating && (wasAccelerating || coastStart == float.MaxValue))
                    coastStart = state.time;
                else if (state.isAccelerating)
                    coastStart = float.MaxValue;
                break;
        }

        wasAccelerating = state.isAccelerating;
        lastBoostRequestTime = state.time;
    }

    public override void onRequestsExecuted(HashSet<Guid> executedRequests) {
        if (executedRequests.Contains(BoostStartForce))  
            lastBoostTime = lastBoostRequestTime;

        if (executedRequests.Contains(BoostStartAcceleration) && executedRequests.Contains(BoostStartMaxSpeed)) 
            boostLevel += 1;   
	}

    private BoostState getState(ShipState args) {
        if (resetBoost)
            return BoostState.RESET_ACTIVE;
        else if (boostActive(args.time))
            return BoostState.BOOST_ACTIVE;
        else if (boostReady(args.time) && args.isAccelerating && args.boost)
            return BoostState.BOOST_START;
        else if (resetReady(args.time) && !args.isAccelerating && boostLevel > 0)
            return BoostState.RESET_START;
        else
            return BoostState.INACTIVE;
    }

	private bool boostReady(float time) {
        return time >= lastBoostTime + manager.boostTime + manager.boostCooldown;
    }

    private bool boostActive(float time) {
        return time < lastBoostTime + manager.boostTime;
    }

    private bool resetReady(float time) {
        return time > coastStart + manager.coastTime;
    }
}
