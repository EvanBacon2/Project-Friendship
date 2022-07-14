using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate float MagnitudeEvent();
public interface MagnitudeObserver {
    public event MagnitudeEvent magnitudeEvent;
    public int magnitudePriority { get; set; }
    public bool checkMagnitudeEvent();
    public MagnitudeEvent getMagnitudeEvent();
}
