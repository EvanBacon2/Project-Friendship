using System;
using System.Collections.Generic;
using UnityEngine;

public enum BoostState {
    BOOST_START,//boost input is received
    BOOST_ACTIVE,//being boosted
    BOOST_END,//boost ends
    RESET_START,//start slowing down to reach non boost max velocity
    RESET_ACTIVE,//slowing down to reach non boost max velocity
    INACTIVE//Not boosting/reseting, and no boost input is received
}

/*
 * Rapidly moves the PlayerShip in the direction it is currently moving for a short period of time
 */
public class BoostSystem : RequestSystem<ShipState> {
    private RECSShipbody rb;
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

    private BoostState boostState;

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

        boostState = getState(state);

        switch (boostState) {
            case BoostState.BOOST_START:
                if (boostLevel < manager.maxBoostLevel) {
                    BoostStartAcceleration = rb.LinearAcceleration.mutate(this, PriorityAlias.Boost, (float val) => { return val * manager.boostAccelerationMod; });
                    BoostStartMaxSpeed = rb.LinearMax.mutate(this, PriorityAlias.Boost, (float val) => { return val + manager.boostMaxSpeedMod; });
                }

                boostVelocity = new Vector3(state.horizontalMove, state.verticalMove).normalized * rb.BASE_LINEAR_MAX * 1;
                BoostStartForce = rb.Force.mutate(this, PriorityAlias.Boost, (List<(Vector3, ForceMode)> forces) => {
                    forces.Add((boostVelocity * .01f, ForceMode.VelocityChange));
                    return forces; 
                });
                break;
            case BoostState.BOOST_ACTIVE:
                float x = (manager.boostTime - (state.time - lastBoostTime)) / manager.boostTime;
                float boostMagMod = x > .85f ? 0f + (1f * ((1 - x) * (1 - x) / 1f)) : (Mathf.Log10(x * 4) / 3f) + .6f;
                rb.Force.mutate(PriorityAlias.Boost, (List<(Vector3, ForceMode)> forces) => {
                    forces.Add((boostVelocity * boostMagMod, ForceMode.VelocityChange));
                    return forces; 
                });
                coastStart = float.MaxValue;
                break;
            case BoostState.BOOST_END:
                float diff = rb.Velocity.value.magnitude - rb.LinearMax.value;
                float ratio = Mathf.Abs(rb.Velocity.value.x) / (Mathf.Abs(rb.Velocity.value.x) + Mathf.Abs(rb.Velocity.value.y));
                
                Vector3 opp = new Vector3(rb.Velocity.value.x, rb.Velocity.value.y, 0).normalized * -diff / Time.fixedDeltaTime;
                (Vector3, ForceMode) oppForce = (opp, ForceMode.Force);
                
                rb.Force.mutate(PriorityAlias.Move, (List<(Vector3, ForceMode)> forces) => { 
                    forces.Add(oppForce); 
                    return forces;
                });
                break;
            case BoostState.RESET_START:
                rb.LinearMax.set(PriorityAlias.BoostReset, rb.BASE_LINEAR_MAX);
                rb.LinearAcceleration.set(PriorityAlias.BoostReset, rb.BASE_LINEAR_ACCELERATION);
                brakeStep = (rb.Velocity.pendingValue().magnitude - rb.BASE_LINEAR_MAX) * .9f;
                resetBoost = true;
                break;
            case BoostState.RESET_ACTIVE:
                if (rb.Velocity.pendingValue().magnitude > rb.BASE_LINEAR_MAX) {
                    Vector3 pendingVelocity = rb.Velocity.pendingValue();
                    rb.calcPendingVelocity(pendingVelocity);
                    rb.Force.mutate(PriorityAlias.BoostReset, (List<(Vector3, ForceMode)> forces) => {
                        forces.Add((new Vector3(pendingVelocity.x, pendingVelocity.y, 0).normalized * -brakeStep, ForceMode.Force));
                        return forces;
                    });
                } else {
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
        else if (!boostActive(args.time) && boostState == BoostState.BOOST_ACTIVE)
            return BoostState.BOOST_END;
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
