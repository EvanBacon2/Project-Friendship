using UnityEngine;

public class BoostRequest {
    private PlayerShipController shipController;

    private float boostCooldown;
    private float boostAccelerationMod;
    private int boostMaxSpeedMod;
    private int boostLevel;
    private int maxBoostLevel;
    private float lastBoostTime;

    public BoostRequest(PlayerShipController shipController) {
        this.shipController = shipController;

        this.boostCooldown = .2f;
        this.boostAccelerationMod = 2.0f;
        this.boostMaxSpeedMod = 25;
        this.boostLevel = 0;
        this.maxBoostLevel = 3;
        this.lastBoostTime = float.MinValue;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (boostReady(args.time) && args.isAccelerating && args.boostInput && boostLevel < maxBoostLevel) {
            shipController.requestAcceleration(Request.Boost, args.shipModel.acceleration * boostAccelerationMod);
            shipController.requestMaxSpeed(Request.Boost, args.shipModel.maxSpeed + (boostLevel + 1) * boostMaxSpeedMod);
            shipController.requestForce(Request.Boost, new Vector3(args.horizontalInput, args.verticalInput) * args.shipModel.acceleration * 20, ForceMode.Force);
            boostLevel += 1;
            lastBoostTime = args.time;
        } else if (!args.isAccelerating && boostLevel > 0) {
            shipController.requestAcceleration(Request.Boost, PlayerShipModel.baseAcceleration);
            shipController.requestMaxSpeed(Request.Boost, PlayerShipModel.baseMaxSpeed);
            boostLevel = 0;
        }
    }

    private bool boostReady(float time) {
        return time - lastBoostTime >= boostCooldown;
    }
}
