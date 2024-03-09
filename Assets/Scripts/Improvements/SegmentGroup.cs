using System;
using System.Collections.Generic;
using UnityEngine;

public class SegmentGroup : MonoBehaviour {
    public bool Simulated;
    public bool SpeedClamped;

    public double SubLinearDrag { get { return _subLinearDrag; } }
    public double SubAngularDrag { get { return _subAngularDrag; } }

    //base
    public int Count;
    public int ActiveSegments;
    private bool[] _active;

    private Vector2d[] _p1;
    private Vector2d[] _p2;

    private Vector2d[] _position;
    private Vector2d[] _orientation;//should be a unit vector

    private double[] _length;
    private double[] _halfLength;

    //simulated
    private Vector2d[] _previousPosition;
    private Vector2d[] _velocity;

    private Vector2d[] _previousOrientation;
    private double[] _angulerVelocity;//radians

    private double[] _mass;
    private double[] _inverseMass;

    private double[] _inertia;
    private double[] _inverseInertia;

    public double LinearDrag;
    public double AngularDrag;

    private double _subLinearDrag;
    private double _subAngularDrag;

    private SegmentSimulator _simulator;

    //speedClamped
    public double MaxSpeedBase;
    public double MaxSpeedScale;

    public EventHandler Step;
    public EventHandler SubStep;
    public EventHandler LateStep;

    private void handleSimulationEvent(object sender, SimulationState state) {
        if (Simulated)
            state.handle(this);
    }

    void Start() {
        //base 
        this._active = new bool[Count];
        this.ActiveSegments = Count;

        this._p1 = new Vector2d[Count];
        this._p2 = new Vector2d[Count];

        this._position = new Vector2d[Count];
        this._orientation = new Vector2d[Count];

        this._length = new double[Count];
        this._halfLength = new double[Count];

        //simulated
        this._previousPosition = new Vector2d[Count];
        this._velocity = new Vector2d[Count];
        
        this._previousOrientation = new Vector2d[Count];
        this._angulerVelocity = new double[Count];

        this._mass = new double[Count];
        this._inverseMass = new double[Count];

        this._inertia = new double[Count];
        this._inverseInertia = new double[Count];

        this._subLinearDrag = 1;

        this._subAngularDrag = 1;

        this._simulator = GameObject.FindWithTag("SegmentManager").GetComponent<SegmentSimulator>();

        this._simulator.Step += (object sender, EventArgs e) => { this.Step.Invoke(this, new()); };
        this._simulator.SubStep += (object sender, EventArgs e) => { this.SubStep.Invoke(this, new()); };
        this._simulator.LateStep += (object sender, EventArgs e) => { this.LateStep.Invoke(this, new()); };

        this._simulator.Simulate += handleSimulationEvent;
        this._simulator.AdjustVelocities += handleSimulationEvent;
        this._simulator.SolveVelocities += handleSimulationEvent;
        this._simulator.Drag += handleSimulationEvent;

        //speedClamped
        this.MaxSpeedBase = double.PositiveInfinity;
        this.MaxSpeedScale = 1;

        populate();
    }

    private void populate() {
        for (int i = 0; i < Count; i++) {
            this._active[i] = true;
            
            this._p1[i] = Vector2d.zero;
            this._p2[i] = Vector2d.zero;

            this._position[i] = Vector2d.zero;
            this._previousPosition[i] = Vector2d.zero;
            this._velocity[i] = Vector2d.zero;

            this._orientation[i] = Vector2d.zero;
            this._previousOrientation[i] = Vector2d.zero;

            this._mass[i] = 1;
            this._inverseMass[i] = 1;

            this._inertia[i] = 1;
            this._inverseInertia[i] = 1;
        }
    }

    #region Getters/Setters

    //base

    public bool Active(int i) {
        return _active[i];
    }

    public void setActive(int i, bool active) {
        if (_active[i] == active)
            return;

        ActiveSegments += active ? 1 : -1;
        _active[i] = active;
    }

    public Vector2d P1(int i) {
        return _p1[i];
    }

    public void setP1(int i, double x, double y) {
        _position[i].x = x + _orientation[i].x * _halfLength[i];
        _position[i].y = y + _orientation[i].y * _halfLength[i];
        _p1[i].x = x;
        _p1[i].y = y;
        _p2[i].x = x + _orientation[i].x * _length[i];
        _p2[i].y = y + _orientation[i].y * _length[i];
    }

    public Vector2d P2(int i) {
        return _p2[i];
    }

    public void setP2(int i, double x, double y) {
        _position[i].x = x - _orientation[i].x * _halfLength[i];
        _position[i].y = y - _orientation[i].y * _halfLength[i];
        _p1[i].x = x - _orientation[i].x * _length[i];
        _p1[i].y = y - _orientation[i].y * _length[i];
        _p2[i].x = x;
        _p2[i].y = y;
    }

    public Vector2d Position(int i ) {
        return _position[i];
    }

    public void setPosition(int i, double x, double y) {
        _position[i].x = x;
        _position[i].y = y;
        _p1[i].x = x - _orientation[i].x * _length[i];
        _p1[i].y = y - _orientation[i].y * _length[i];
        _p2[i].x = x + _orientation[i].x * _length[i];
        _p2[i].y = y + _orientation[i].y * _length[i];
    }

    public Vector2d Orientation(int i) {
        return _orientation[i];
    }

    public void setOrientation(int i, double x, double y) {
        _orientation[i].x = x;
        _orientation[i].y = y;
        _orientation[i].normalize();
        _p1[i].x = _position[i].x - _orientation[i].x * _halfLength[i];
        _p1[i].y = _position[i].y - _orientation[i].y * _halfLength[i];
        _p2[i].x = _position[i].x + _orientation[i].x * _halfLength[i];
        _p2[i].y = _position[i].y + _orientation[i].y * _halfLength[i];
    }

    public double Length(int i) {
        return _length[i];
    } 

    public double HalfLength(int i) {
        return _halfLength[i];
    }

    //simulated

    public Vector2d PreviousPosition(int i) {
        return _previousPosition[i];
    }

    public Vector2d Velocity(int i) {
        return _velocity[i];
    }    

    public Vector2d PreviousOrientation(int i) {
        return _previousOrientation[i];
    }

    public double AngularVelocity(int i) {
        return _angulerVelocity[i];
    }

    public void SetAngularVelocity(int i, double angularVelocity) {
        _angulerVelocity[i] = angularVelocity;
    }

    public double Mass(int i ) {
        return _mass[i];
    }

    public void SetMass(int i, double mass) {
        _mass[i] = mass;
        _inverseMass[i] = mass == 0 ? double.PositiveInfinity : 1 / mass;
    }

    public double InverseMass(int i) {
        return _inverseMass[i];
    }

    public void SetInverseMass(int i, double inverseMass) {
        _inverseMass[i] = inverseMass;
        _mass[i] = inverseMass == 0 ? double.PositiveInfinity : 1 / inverseMass;
    }
    
    public double Inertia(int i) {
        return _inertia[i];
    }

    public void SetInertia(int i, double inertia) {
        _inertia[i] = inertia;
        _inverseInertia[i] = inertia == 0 ? double.PositiveInfinity : 1 / inertia;
    }

    public double InverseInertia(int i) {
        return _inverseInertia[i];
    }

    public void setInverseInertia(int i, double inverseInertia) {
        _inverseInertia[i] = inverseInertia;
        _inertia[i] = inverseInertia == 0 ? double.PositiveInfinity : 1 / inverseInertia;
    }

    #endregion


    #region Special Methods

    /*
     * Set's p2 position to the given coordinates without changing p1's position
     */
    public void StretchP2(int i, double x, double y) {
        _p2[i].x = x;
        _p2[i].y = y;
        _orientation[i].x = _p2[i].x - _p1[i].x;
        _orientation[i].y = _p2[i].y - _p1[i].y;
        _length[i] = _orientation[i].magnitude;
        _halfLength[i] = _length[i] / 2;
        _orientation[i].normalize();
        _position[i].x = _p1[i].x + _orientation[i].x * _halfLength[i];
        _position[i].y = _p1[i].y + _orientation[i].y * _halfLength[i];
    }

    private static Vector2d real = Vector2d.zero;
	private static Vector2d complex = Vector2d.zero;

    /*
     * Rotates segment at index i by the rotation r given in radians
	 */
    public void Rotate(int i, double r) {
		double cosR = System.Math.Cos(r);
		double sinR = System.Math.Sin(r);

		real.x = cosR * _orientation[i].x;
		real.y = cosR * _orientation[i].y;
		complex.x = sinR * _orientation[i].x;
		complex.y = sinR * _orientation[i].y;
		setOrientation(i, real.x - complex.y, real.y + complex.x);
	}

    private static Vector2d oldP1 = Vector2d.zero;

    public void RotateAroundP1(int i, double r) {
        oldP1.x = _p1[i].x;
        oldP1.y = _p1[i].y;
        Rotate(i, r);
        setP1(i, oldP1.x, oldP1.y);
    }

    #endregion
}
