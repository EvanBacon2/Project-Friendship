using System.Collections.Generic;
using UnityEngine;

public class BoostRequest : RequestSystem {
    private enum BoostState {
        BOOST_START,
        BOOST_ACTIVE,
        RESET_START,
        RESET_ACTIVE,
        INACTIVE
    }

    PlayerInputArgs lastInputArgs;

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

    public void Start() {
        boostLevel = 0;
        maxBoostLevel = 3;

        boostCooldown = .1f;
        boostAccelerationMod = 1.3f;
        boostMaxSpeedMod = 3;
        boostMagnitude = PlayerShipModel.baseMaxSpeed * 2;
        
        lastBoostTime = float.MinValue;
        boostTime = .25f;

        coastTime = .5f;
        coastStart = float.MaxValue;
        resetBoost = false;
    }

    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        switch (getState(args)) {
            case BoostState.BOOST_START:
                boostFrame = true;
                if (boostLevel < maxBoostLevel) {
                    args.shipController.setRequest(this, RequestClass.Boost, PlayerShipProperties.Acceleration, args.shipModel.acceleration * boostAccelerationMod);
                    args.shipController.setRequest(this, RequestClass.Boost, PlayerShipProperties.MaxSpeed, args.shipModel.maxSpeed + boostMaxSpeedMod);
                }

                boostVelocity = new Vector3(args.horizontalInput, args.verticalInput).normalized * boostMagnitude;
                args.shipController.setRequest(this, RequestClass.Boost, PlayerShipProperties.Force, (boostVelocity * .01f, ForceMode.VelocityChange));//try to remove
                break;
            case BoostState.BOOST_ACTIVE:
                float x = (boostTime - (args.time - lastBoostTime)) / boostTime;
                float boostMagMod = x > .85f ? 0f + (1f * ((1 - x) * (1 - x) / 1f)) : (Mathf.Log10(x * 4) / 3f) + .6f;
                args.shipController.setRequest(this, RequestClass.Boost, PlayerShipProperties.Force, (boostVelocity * boostMagMod, ForceMode.VelocityChange));
                coastStart = float.MaxValue;
                break;
            case BoostState.RESET_START:
                brakeStep = (args.shipModel.velocity.magnitude - PlayerShipModel.baseMaxSpeed) / 80;
                resetBoost = true;
                break;
            case BoostState.RESET_ACTIVE:
                if (args.shipModel.velocity.magnitude > PlayerShipModel.baseMaxSpeed) {
                    args.shipController.setRequest(this, RequestClass.BoostReset, PlayerShipProperties.Magnitude, args.shipModel.velocity.magnitude - brakeStep);
                    //args.shipController.blockRequest(this, RequestType.BoostReset, PlayerShipProperties.Force);
                }
                else {
                    args.shipController.setRequest(this, RequestClass.BoostReset, PlayerShipProperties.Acceleration, PlayerShipModel.baseAcceleration);
                    args.shipController.setRequest(this, RequestClass.BoostReset, PlayerShipProperties.MaxSpeed, PlayerShipModel.baseMaxSpeed);
                    boostLevel = 0;
                    resetBoost = false;
                }
                break;
            case BoostState.INACTIVE:
                if (!args.isAccelerating && ((lastInputArgs?.isAccelerating ?? false) || coastStart == float.MaxValue))
                    coastStart = args.time;
                else if (args.isAccelerating)
                    coastStart = float.MaxValue;
                break;
        }

        lastInputArgs = args;
    }

    public override void onRequestExecuted(HashSet<string> executedProperties) {
        base.onRequestExecuted(executedProperties);
        if (boostFrame) {
            lastBoostTime = lastInputArgs.time;
            if (executedProperties.Contains(PlayerShipProperties.MaxSpeed) && executedProperties.Contains(PlayerShipProperties.Acceleration))
                boostLevel += 1;
            boostFrame = false;
        }
	}

    private BoostState getState(PlayerInputArgs args) {
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
