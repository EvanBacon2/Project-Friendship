using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void RotationEvent();
public interface RotationObserver {
    public event RotationEvent rotationEvent;
    public int rotationPriority { get; set; }
    public bool checkRotationEvent();
    public RotationEvent getRotationEvent();
}
