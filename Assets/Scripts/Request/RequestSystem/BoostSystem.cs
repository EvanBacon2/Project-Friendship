using System;
using System.Collections.Generic;
using UnityEngine;

public class BoostRequest : RequestSystem<ShipState> {
    private enum BoostState {
        BOOST_START,
        BOOST_ACTIVE,
        RESET_START,
        RESET_ACTIVE,
        INACTIVE
    }

    private RECSRigidBody model;
    private bool wasAccelerating;
    private float lastBoostRequestTime;

    [SerializeField] private int boostLevel;
    private int maxBoostLevel;

    private float boostCooldown;
    [SerializeField] private float lastBoostTime;
    private float boostTime;
    private bool boostFrame;

    private float boostAccelerationMod;
    private int boostMaxSpeedMod;
    private Vector3 boostVelocity;

    private float coastTime;
    [SerializeField] private float coastStart;
    [SerializeField] private bool resetBoost;
    private float brakeStep;

    private Guid BoostStartAcceleration;
    private Guid BoostStartMaxSpeed;

    public BoostRequest(RECSRigidBody model) {
        this.model = model;

        wasAccelerating = false;
        lastBoostRequestTime = 0;

        boostLevel = 0;
        maxBoostLevel = 3;

        boostCooldown = .1f;
        boostAccelerationMod = 1.3f;
        boostMaxSpeedMod = 3;
        
        lastBoostTime = float.MinValue;
        boostTime = .25f;

        coastTime = .5f;
        coastStart = float.MaxValue;
        resetBoost = false;

        BoostStartAcceleration = Guid.Empty;
        BoostStartMaxSpeed = Guid.Empty;
    }

    public void OnStateReceived(object sender, ShipState state) {
        switch (getState(state)) {
            case BoostState.BOOST_START:
                boostFrame = true;
                if (boostLevel < maxBoostLevel) {
                    BoostStartAcceleration = model.Acceleration.mutate(this, RequestClass.Boost, (float val) => { return val * boostAccelerationMod; });
                    BoostStartMaxSpeed = model.MaxSpeed.mutate(this, RequestClass.Boost, (float val) => { return val + boostMaxSpeedMod; });
                }

                boostVelocity = new Vector3(state.horizontalInput, state.verticalInput).normalized * state.BASE_MAXSPEED * 2;
                model.Force.set(RequestClass.Boost, (boostVelocity * .01f, ForceMode.VelocityChange));
                break;
            case BoostState.BOOST_ACTIVE:
                float x = (boostTime - (state.time - lastBoostTime)) / boostTime;
                float boostMagMod = x > .85f ? 0f + (1f * ((1 - x) * (1 - x) / 1f)) : (Mathf.Log10(x * 4) / 3f) + .6f;
                model.Force.set(RequestClass.Boost, (boostVelocity * boostMagMod, ForceMode.VelocityChange));
                coastStart = float.MaxValue;
                break;
            case BoostState.RESET_START:
                brakeStep = (model.Magnitude.value - state.BASE_MAXSPEED) / 80;
                resetBoost = true;
                break;
            case BoostState.RESET_ACTIVE:
                if (model.Magnitude.value > state.BASE_MAXSPEED) {
                    model.Magnitude.mutate(RequestClass.BoostReset, (float val) => { return val - brakeStep; });
                    model.Force.block(RequestClass.BoostReset);
                } else {
                    model.Acceleration.set(RequestClass.BoostReset, state.BASE_ACCELERATION);
                    model.MaxSpeed.set(RequestClass.BoostReset, state.BASE_MAXSPEED);
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

    public void onRequestsExecuted(HashSet<Guid> executedRequests) {
        if (boostFrame) {
            if (executedRequests.Contains(BoostStartAcceleration) && executedRequests.Contains(BoostStartMaxSpeed)) 
                boostLevel += 1;
            
            lastBoostTime = lastBoostRequestTime;
            boostFrame = false;
        }
	}

    private BoostState getState(ShipState args) {
        if (resetBoost)
            return BoostState.RESET_ACTIVE;
        else if (boostActive(args.time))
            return BoostState.BOOST_ACTIVE;
        else if (boostReady(args.time) && args.isAccelerating && args.boostInput)
            return BoostState.BOOST_START;
        else if (resetReady(args.time) && !args.isAccelerating && boostLevel > 0)
            return BoostState.RESET_START;
        else
            return BoostState.INACTIVE;
    }

	private bool boostReady(float time) {
        return time >= lastBoostTime + boostTime + boostCooldown;
    }

    private bool boostActive(float time) {
        return time < lastBoostTime + boostTime;
    }

    private bool resetReady(float time) {
        return time > coastStart + coastTime;
    }
}
