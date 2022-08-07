using System.Collections.Generic;
using UnityEngine;

public class BoostRequest : Request {
    PlayerInputArgs lastInputArgs;

    [SerializeField] private int boostLevel;
    private int maxBoostLevel;

    private float boostCooldown;
    private float lastBoostTime;
    private float boostTime;

    private float boostAccelerationMod;
    private int boostMaxSpeedMod;

    private float coastTime;
    private float coastStart;
    private bool resetBoost;
    private float brakeStep;

    public void Start() {
        boostCooldown = .2f;
        boostAccelerationMod = 2.0f;
        boostMaxSpeedMod = 1;
        boostLevel = 0;
        maxBoostLevel = 3;
        lastBoostTime = float.MinValue;
        boostTime = .4f;
        coastTime = .5f;
        coastStart = float.MaxValue;
        resetBoost = false;
    }

    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (!args.isAccelerating && lastInputArgs != null && lastInputArgs.isAccelerating) 
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
            if (args.time < lastBoostTime + boostTime)
                args.shipController.makeRequest(this, RequestType.Boost, PlayerShipProperties.Force, (new Vector3(args.horizontalInput, args.verticalInput) * args.shipModel.velocity.magnitude * 3, ForceMode.VelocityChange));

            if (boostReady(args.time) && args.isAccelerating && args.boostInput && boostLevel < maxBoostLevel) {
                //args.shipController.makeRequest(this, PlayerShipProperties.Acceleration, args.shipModel.acceleration * boostAccelerationMod);
                args.shipController.makeRequest(this, RequestType.Boost, PlayerShipProperties.MaxSpeed, args.shipModel.maxSpeed + boostMaxSpeedMod);
                args.shipController.makeRequest(this, RequestType.Boost, PlayerShipProperties.Force, (new Vector3(args.horizontalInput, args.verticalInput) * args.shipModel.velocity.magnitude * 3, ForceMode.VelocityChange));
            } else if (!args.isAccelerating && args.time - coastStart > coastTime && boostLevel > 0) {
                brakeStep = (args.shipModel.velocity.magnitude - PlayerShipModel.baseMaxSpeed) / 30;
                resetBoost = true;
            }
        }
    }

	public override void onRequestExecuted(HashSet<string> executedProperties) {
		base.onRequestExecuted(executedProperties);
        if (executedProperties.Contains(PlayerShipProperties.Force) && executedProperties.Contains(PlayerShipProperties.MaxSpeed)) {
            boostLevel += 1;
            lastBoostTime = lastInputArgs.time;
        }
        
        /*if (executedProperties.Contains(PlayerShipProperties.Magnitude)) {
            if (lastInputArgs.shipModel.velocity.magnitude <= PlayerShipModel.baseMaxSpeed) 
                boostLevel = 0;
        } */
	}

	private bool boostReady(float time) {
        return time - lastBoostTime >= boostCooldown;
    }
}
