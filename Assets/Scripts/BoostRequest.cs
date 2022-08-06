using System.Collections.Generic;
using UnityEngine;

public class BoostRequest : Request {
    public override RequestType type { get { return RequestType.Boost; } }
    PlayerInputArgs lastInputArgs;

    [SerializeField] private int boostLevel;
    private int maxBoostLevel;

    private float boostCooldown;
    private float lastBoostTime;

    private float boostAccelerationMod;
    private int boostMaxSpeedMod;

    private bool accelerating;
    private float coastTime;
    private float coastStart;
    private bool resetBoost;
    private float brakeStep;

    public void Start() {
        boostCooldown = .2f;
        boostAccelerationMod = 2.0f;
        boostMaxSpeedMod = 15;
        boostLevel = 0;
        maxBoostLevel = 3;
        lastBoostTime = float.MinValue;
        coastTime = .2f;
        coastStart = float.MaxValue;
        resetBoost = false;
    }

    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        lastInputArgs = args;

        if (args.isAccelerating)
            accelerating = true;
        else if (accelerating) {
            accelerating = false;
            coastStart = args.time;
        }

        if (resetBoost) {
            if (args.shipModel.velocity.magnitude > PlayerShipModel.baseMaxSpeed) {
                args.shipController.makeRequest(this, PlayerShipProperties.Magnitude, args.shipModel.velocity.magnitude - brakeStep); //+ (args.shipModel.acceleration * -0.6f * Time.fixedDeltaTime));
                args.shipController.blockRequest(this, PlayerShipProperties.Force);
            } else {
                args.shipController.makeRequest(this, PlayerShipProperties.Acceleration, PlayerShipModel.baseAcceleration);
                args.shipController.makeRequest(this, PlayerShipProperties.MaxSpeed, PlayerShipModel.baseMaxSpeed);
                resetBoost = false;
            }
        } else {
            if (boostReady(args.time) && args.isAccelerating && args.boostInput && boostLevel < maxBoostLevel) {
                //args.shipController.makeRequest(this, PlayerShipProperties.Acceleration, args.shipModel.acceleration * boostAccelerationMod);
                args.shipController.makeRequest(this, PlayerShipProperties.MaxSpeed, args.shipModel.maxSpeed + boostMaxSpeedMod);
                args.shipController.makeRequest(this, PlayerShipProperties.Force, (new Vector3(args.horizontalInput, args.verticalInput) * args.shipModel.acceleration * 80, ForceMode.Acceleration));
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
        
        if (executedProperties.Contains(PlayerShipProperties.Magnitude)) {
            if (lastInputArgs.shipModel.velocity.magnitude <= PlayerShipModel.baseMaxSpeed) 
                boostLevel = 0;
        } 
	}

	private bool boostReady(float time) {
        return time - lastBoostTime >= boostCooldown;
    }
}
