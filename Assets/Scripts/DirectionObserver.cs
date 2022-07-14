using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate Vector3 DirectionEvent();
public interface DirectionObserver {
    public event DirectionEvent directionEvent;
    public int directionPriority { get; set; }
    public bool checkDirectionEvent();
    public DirectionEvent getDirectionEvent();
}
