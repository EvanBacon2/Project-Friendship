using System;
using System.Collections.Generic;
using UnityEngine;
using Request;

public class BoostRequest : RequestSystem<ShipState> {
    private enum BoostState {
        BOOST_START,
        BOOST_ACTIVE,
        RESET_START,
        RESET_ACTIVE,
        INACTIVE
    }

    private ShipState lastShipState;
    private ShipModel model;

    [SerializeField] private int boostLevel;
    private int maxBoostLevel;

    private float boostCooldown;
    [SerializeField] private float lastBoostTime;
    private float boostTime;
    private bool boostFrame;

    private float boostAccelerationMod;
    private int boostMaxSpeedMod;
    private Vector3 boostVelocity;
    private float boostMagnitude;

    private float coastTime;
    [SerializeField] private float coastStart;
    [SerializeField] private bool resetBoost;
    private float brakeStep;

    private Guid BoostStartAcceleration;
    private Guid BoostStartMaxSpeed;

    public BoostRequest(ShipModel model) {
        this.model = model;

        boostLevel = 0;
        maxBoostLevel = 3;

        boostCooldown = .1f;
        boostAccelerationMod = 1.3f;
        boostMaxSpeedMod = 3;
        boostMagnitude = PlayerShipModel.BASE_MAXSPEED * 2;
        
        lastBoostTime = float.MinValue;
        boostTime = .25f;

        coastTime = .5f;
        coastStart = float.MaxValue;
        resetBoost = false;

        BoostStartAcceleration = Guid.Empty;
        BoostStartMaxSpeed = Guid.Empty;
    }

    public override void OnStateReceived(object sender, ShipState args) {
        switch (getState(args)) {
            case BoostState.BOOST_START:
                boostFrame = true;
                if (boostLevel < maxBoostLevel) {
                    BoostStartAcceleration = model.Acceleration.takeRequest(new MutateRequest<float>(this, RequestClass.Boost, 
                        (float val) => { return val * boostAccelerationMod; }));
                    BoostStartMaxSpeed = model.MaxSpeed.takeRequest(new MutateRequest<float>(this, RequestClass.Boost, 
                        (float val) => { return val + boostMaxSpeedMod; }));
                    //args.shipController.setRequest(this, RequestClass.Boost, PlayerShipProperties.Acceleration, args.shipModel.acceleration * boostAccelerationMod);
                    //args.shipController.setRequest(this, RequestClass.Boost, PlayerShipProperties.MaxSpeed, args.shipModel.maxSpeed + boostMaxSpeedMod);
                }

                boostVelocity = new Vector3(args.horizontalInput, args.verticalInput).normalized * boostMagnitude;
                model.Force.takeRequest(new SetRequest<(Vector3, ForceMode)>(this, RequestClass.Boost, 
                    (boostVelocity * .01f, ForceMode.VelocityChange)));
                //args.shipController.setRequest(this, RequestClass.Boost, PlayerShipProperties.Force, (boostVelocity * .01f, ForceMode.VelocityChange));//try to remove
                break;
            case BoostState.BOOST_ACTIVE:
                float x = (boostTime - (args.time - lastBoostTime)) / boostTime;
                float boostMagMod = x > .85f ? 0f + (1f * ((1 - x) * (1 - x) / 1f)) : (Mathf.Log10(x * 4) / 3f) + .6f;
                model.Force.takeRequest(new SetRequest<(Vector3, ForceMode)>(this, RequestClass.Boost, (boostVelocity * boostMagMod, ForceMode.VelocityChange)));
                //args.shipController.setRequest(this, RequestClass.Boost, PlayerShipProperties.Force, (boostVelocity * boostMagMod, ForceMode.VelocityChange));
                coastStart = float.MaxValue;
                break;
            case BoostState.RESET_START:
                brakeStep = (args.playerShip.magnitude - PlayerShipModel.BASE_MAXSPEED) / 80;
                resetBoost = true;
                break;
            case BoostState.RESET_ACTIVE:
                if (args.playerShip.magnitude > PlayerShipModel.BASE_MAXSPEED) {
                    model.Magnitude.takeRequest(new MutateRequest<float>(this, RequestClass.BoostReset, (float val) => { return val - brakeStep; }));
                    //args.shipController.setRequest(this, RequestClass.BoostReset, PlayerShipProperties.Magnitude, args.shipModel.velocity.magnitude - brakeStep);
                    //args.shipController.blockRequest(this, RequestType.BoostReset, PlayerShipProperties.Force);
                }
                else {
                    model.Acceleration.takeRequest(new SetRequest<float>(this, RequestClass.BoostReset, PlayerShipModel.BASE_ACCELERATION));
                    model.MaxSpeed.takeRequest(new SetRequest<float>(this, RequestClass.Boost, PlayerShipModel.BASE_MAXSPEED));
                    //args.shipController.setRequest(this, RequestClass.BoostReset, PlayerShipProperties.Acceleration, PlayerShipModel.baseAcceleration);
                    //args.shipController.setRequest(this, RequestClass.BoostReset, PlayerShipProperties.MaxSpeed, PlayerShipModel.baseMaxSpeed);
                    boostLevel = 0;
                    resetBoost = false;
                }
                break;
            case BoostState.INACTIVE:
                if (!args.isAccelerating && ((lastShipState?.isAccelerating ?? false) || coastStart == float.MaxValue))
                    coastStart = args.time;
                else if (args.isAccelerating)
                    coastStart = float.MaxValue;
                break;
        }

        lastShipState = args;
    }

    public override void onRequestsExecuted(HashSet<Guid> executedRequests) {
        base.onRequestsExecuted(executedRequests);
        if (boostFrame) {
            lastBoostTime = lastShipState.time;
            if (executedRequests.Contains(BoostStartAcceleration) && executedRequests.Contains(BoostStartMaxSpeed))
                boostLevel += 1;
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
