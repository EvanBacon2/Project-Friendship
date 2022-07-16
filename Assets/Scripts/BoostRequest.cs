using System.Collections.Generic;
using UnityEngine;

public class BoostRequest : Request {
    public override RequestType type { get { return RequestType.Boost; } }
    private PlayerShipController shipController;

    PlayerInputArgs lastInputArgs;

    private float boostCooldown;
    private float boostAccelerationMod;
    private int boostMaxSpeedMod;
    private int boostLevel;
    private int maxBoostLevel;
    private float lastBoostTime;

    public BoostRequest(PlayerShipController shipController) {
        this.shipController = shipController;

        boostCooldown = .2f;
        boostAccelerationMod = 2.0f;
        boostMaxSpeedMod = 25;
        boostLevel = 0;
        maxBoostLevel = 3;
        lastBoostTime = float.MinValue;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        lastInputArgs = args;
        Debug.Log("boostlevel " + boostLevel);
        if (boostReady(args.time) && args.isAccelerating && args.boostInput && boostLevel < maxBoostLevel) {
            shipController.makeRequest(this, PlayerShipProperties.Acceleration, args.shipModel.acceleration * boostAccelerationMod);
            shipController.makeRequest(this, PlayerShipProperties.MaxSpeed, args.shipModel.maxSpeed + (boostLevel + 1) * boostMaxSpeedMod);
            shipController.makeRequest(this, PlayerShipProperties.Force, (new Vector3(args.horizontalInput, args.verticalInput) * args.shipModel.acceleration * 20, ForceMode.Force));

            //boostLevel += 1;
            //lastBoostTime = lastInputArgs.time;
        } else if (!args.isAccelerating && boostLevel > 0) {
            shipController.makeRequest(this, PlayerShipProperties.Acceleration, PlayerShipModel.baseAcceleration);
            shipController.makeRequest(this, PlayerShipProperties.MaxSpeed, PlayerShipModel.baseMaxSpeed);

            //boostLevel = 0;
        }
    }

	public override void onRequestExecuted(List<string> executedProperties) {
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
