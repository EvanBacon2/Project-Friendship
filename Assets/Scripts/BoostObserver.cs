using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void BoostEvent();

public interface BoostObserver {
    public event BoostEvent boostEvent;
    public int boostPriority { get; set; }
    public bool checkBoostEvent();
    public BoostEvent getBoostEvent();
}
