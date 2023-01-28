using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSegment {
    private Segment segment;

    public Vector2d p1 {
        get { return segment.p1; }
    }
    public Vector2d p2 {
        get { return segment.p2; }
    }

    public Vector2d position {
        get { return segment.position; }
    }
    public Vector2d previousPosition {
        get { return segment.previousPosition; }
        set { segment.previousPosition = value; }
    }
    public Vector2d velocity {
        get { return segment.velocity; }
        set { segment.velocity = value; }
    }

    public readonly double inverseMass;

    public Vector2d orientation {//should be a unit vector
        get { return segment.orientation; }
    }
    public Vector2d previousOrientation {
        get { return segment.previousOrientation; }
        set { segment.previousOrientation = value; }
    }
    public double angulerVelocity {//radians
        get { return angulerVelocity; }
        set { segment.angulerVelocity = value;}
    }
    public readonly double inverseInertia;

    public double length {
        get { return segment.length; }
        set { segment.length = value; }
    }
    private double halfLength;

    public UnitSegment(Vector2d position, Vector2d orientation, double length) {
        this.segment = new(position, orientation, 1, 1, length);
        this.inverseMass = 1;
        this.inverseInertia = 1;
        this.halfLength = segment.length / 2.0;
    }

    public void setPosition(double x, double y) {
        segment.setPosition(x, y);
    }

    public void setP1(double x, double y) {
        segment.setP1(x, y);
    }

    public void setP2(double x, double y) {
        segment.setP2(x, y);
    }

    public void setOrientation(double x, double y) {
        segment.setOrientation(x, y);
    }
}
