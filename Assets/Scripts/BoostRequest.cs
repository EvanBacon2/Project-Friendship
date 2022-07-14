using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoostRequest {
    private PlayerShipModel shipModel;
    private PlayerShipController shipController;

    private float boostCooldown;
    private float boostAccelerationMod;
    private int boostMaxSpeedMod;
    private int boostLevel;
    private int maxBoostLevel;
    private float lastBoostTime;

    public BoostRequest(PlayerShipModel shipModel, PlayerShipController shipController) {
        this.shipModel = shipModel;
        this.shipController = shipController;

        this.boostCooldown = 3;
        this.boostAccelerationMod = 2.0f;
        this.boostMaxSpeedMod = 25;
        this.boostLevel = 0;
        this.maxBoostLevel = 3;
        this.lastBoostTime = float.MinValue;
    }

    public void OnPlayerInputRecorded(object sender, PlayerInputArgs args) {
        if (boostReady(args.time) && args.isAccelerating && args.boostInput && boostLevel < maxBoostLevel) {
            shipController.requestAcceleration(10, shipModel.acceleration * boostAccelerationMod);
            shipController.requestMaxSpeed(10, shipModel.maxSpeed + (boostLevel + 1) * boostMaxSpeedMod);
            boostLevel += 1;
            lastBoostTime = args.time;
        }
    }

    private bool boostReady(float time) {
        return time - lastBoostTime >= boostCooldown;
    }
}
