using System.Collections.Generic;
using UnityEngine;

public class BoostRequest : Request {
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
        boostCooldown = .4f;
        boostAccelerationMod = 1.2f;
        boostMaxSpeedMod = 3;
        boostMagnitude = PlayerShipModel.baseMaxSpeed * 2;
        boostLevel = 0;
        maxBoostLevel = 3;
        lastBoostTime = float.MinValue;
        boostTime = .15f;
        coastTime = .5f;
        coastStart = float.MaxValue;
        resetBoost = false;
    }

    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (!args.isAccelerating && lastInputArgs != null && lastInputArgs.isAccelerating && args.time >= lastBoostTime + boostTime) 
            coastStart = args.time;

        lastInputArgs = args;

        if (resetBoost) {
            if (args.shipModel.velocity.magnitude > PlayerShipModel.baseMaxSpeed) {
                args.shipController.makeRequest(this, RequestType.BoostReset, PlayerShipProperties.Magnitude, args.shipModel.velocity.magnitude - brakeStep); 
                args.shipController.blockRequest(this, RequestType.BoostReset, PlayerShipProperties.Force);
            } else {
                args.shipController.makeRequest(this, RequestType.BoostReset, PlayerShipProperties.Acceleration, PlayerShipModel.baseAcceleration);
                args.shipController.makeRequest(this, RequestType.BoostReset, PlayerShipProperties.MaxSpeed, PlayerShipModel.baseMaxSpeed);
                boostLevel = 0;
                resetBoost = false;
            }
        } else {
            if (args.time < lastBoostTime + boostTime) {
                float x = (boostTime - (args.time - lastBoostTime)) / boostTime;
                float boostMagMod = x > .25f ? .25f + (.75f * ((1 - x) / .75f)) : (Mathf.Log10(x * 4) / 3f) + .6f;
                args.shipController.makeRequest(this, RequestType.Boost, PlayerShipProperties.Force, (boostVelocity * boostMagMod, ForceMode.VelocityChange));
                coastStart = float.MaxValue;
            }
            else if (coastStart == float.MaxValue && !args.isAccelerating) {
                coastStart = args.time;
            }

            if (boostReady(args.time) && args.isAccelerating && args.boostInput) {
                boostFrame = true;
                if (boostLevel < maxBoostLevel) {
                    args.shipController.makeRequest(this, RequestType.Boost, PlayerShipProperties.Acceleration, args.shipModel.acceleration * boostAccelerationMod);
                    args.shipController.makeRequest(this, RequestType.Boost, PlayerShipProperties.MaxSpeed, args.shipModel.maxSpeed + boostMaxSpeedMod);
                }

                boostVelocity = new Vector3(args.horizontalInput, args.verticalInput).normalized * boostMagnitude;
                args.shipController.makeRequest(this, RequestType.Boost, PlayerShipProperties.Force, (boostVelocity * .25f, ForceMode.VelocityChange));
            } else if (!args.isAccelerating && args.time > coastStart + coastTime && boostLevel > 0) {
                brakeStep = (args.shipModel.velocity.magnitude - PlayerShipModel.baseMaxSpeed) / 30;
                resetBoost = true;
            }
        }
    }

    public override void onRequestExecuted(HashSet<string> executedProperties) {
        base.onRequestExecuted(executedProperties);
        if (boostFrame) {
            if (executedProperties.Contains(PlayerShipProperties.Force))
                lastBoostTime = lastInputArgs.time;
            if (executedProperties.Contains(PlayerShipProperties.MaxSpeed) && executedProperties.Contains(PlayerShipProperties.Acceleration))
                boostLevel += 1;
            boostFrame = false;
        }
	}

	private bool boostReady(float time) {
        return time >= lastBoostTime + boostTime + boostCooldown;
    }
}
