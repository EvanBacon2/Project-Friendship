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

    public void Start() {
        boostCooldown = .2f;
        boostAccelerationMod = 2.0f;
        boostMaxSpeedMod = 25;
        boostLevel = 0;
        maxBoostLevel = 3;
        lastBoostTime = float.MinValue;
    }

    public override void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        lastInputArgs = args;
        
        if (boostReady(args.time) && args.isAccelerating && args.boostInput && boostLevel < maxBoostLevel) {
            args.shipController.makeRequest(this, PlayerShipProperties.Acceleration, args.shipModel.acceleration * boostAccelerationMod);
            args.shipController.makeRequest(this, PlayerShipProperties.MaxSpeed, args.shipModel.maxSpeed + (boostLevel + 1) * boostMaxSpeedMod);
            args.shipController.makeRequest(this, PlayerShipProperties.Force, (new Vector3(args.horizontalInput, args.verticalInput) * args.shipModel.acceleration * 20, ForceMode.Force));
        } else if (!args.isAccelerating && boostLevel > 0) {
            args.shipController.makeRequest(this, PlayerShipProperties.Acceleration, PlayerShipModel.baseAcceleration);
            args.shipController.makeRequest(this, PlayerShipProperties.MaxSpeed, PlayerShipModel.baseMaxSpeed);
        }
    }

	public override void onRequestExecuted(HashSet<string> executedProperties) {
		base.onRequestExecuted(executedProperties);
        if (lastInputArgs.isAccelerating) {
            boostLevel += 1;
            lastBoostTime = lastInputArgs.time;
        } else {
            boostLevel = 0;
        }
	}

	private bool boostReady(float time) {
        return time - lastBoostTime >= boostCooldown;
    }
}
