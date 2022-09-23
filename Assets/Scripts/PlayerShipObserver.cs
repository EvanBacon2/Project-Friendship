using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipObserver {
    public delegate void directionEvent();
    public delegate float magnitudeEvent();
    public delegate Vector2 rotationEvent();
    public delegate void boostEvent();

    public int directionPriority = 0;
    public int magnitudePriority = 0;
    public int rotationPriority = 0;
    public int boostPriority = 0;

    public virtual bool checkDirectionEvent() {
        return false;
    }
    public virtual bool checkMagnitudeEvent() {
        return false;
    }
    public virtual bool checkRotationEvent() {
        return false;
    }
    public virtual bool checkBoostEvent() {
        return false;
    }
}
