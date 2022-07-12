using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : MonoBehaviour {
    List<DirectionObserver> DirectionListeners;
    List<MagnitudeObserver> MagnitudeListeners;

    public PlayerShipController(PlayerShipModel shipModel) {
        AccelerationEventListener acc = new AccelerationEventListener(shipModel);
        DirectionListeners.Add(acc);
        MagnitudeListeners.Add(acc);
    }

    public void notifyListeners() {
        notifyDirectionListeners();
        notifyMagnitudeListeners();
    }

    public void notifyDirectionListeners() {
        if (DirectionListeners[0].checkDirectionEvent()) {
            DirectionEvent priorityEvent = DirectionListeners[0].getDirectionEvent();
            priorityEvent();
        }
    }

    public void notifyMagnitudeListeners() {
        if (DirectionListeners[0].checkDirectionEvent()) {
            DirectionEvent priorityEvent = DirectionListeners[0].getDirectionEvent();
            priorityEvent();
        }
    }

    /*void Start() {
        
    }

    void Update() {
        
    }

	private void FixedUpdate() {
        /*if (shipModel.brakeOn) {
            shipModel.slowShip();
        } else {
            if (shipModel.isAccelerating()) {
                if (shipModel.activateBoost)
                    shipModel.boostShip();
                shipModel.accelerateShip();
            }
        }

        if (!shipModel.isAccelerating())
            shipModel.shaveBoostSpeed();

        shipModel.checkSpeed();
        shipModel.rotateToMouse();
    }*/
}
