using System;
using System.Collections.Generic;
using UnityEngine;

public class Anchor : MonoBehaviour, RopeBehaviour {
	public RECSRigidbody rb;
    public Segment anchorSegment;
	public Segment attachSegment;
	public ExtendableRope rope;
	private double angleLimit;
	private int substeps;

	protected Vector2d _offset;
	public Vector2d offset { get { return _offset; } }

	public Vector2d position { get { return anchorSegment.p1; } }
	public Vector2d orientation { get { return anchorSegment.orientation; } }
	public Vector2d attachPoint { get { return anchorSegment.p2; } }
    public double mass { 
        get { return anchorSegment.mass; }
        set { anchorSegment.mass = value; }
    }

    public double inertia {
        get { return anchorSegment.inertia; }
        set { anchorSegment.inertia = value; }
    }

	public double velocityCorrection;

	public Vector2d positionStep;
	public double rotationStep;

	private Vector3 pendingVelocity = Vector3.zero;
	
	void Start() {
		anchorSegment = new Segment(Vector2d.zero, Vector2d.up,
									double.PositiveInfinity, double.PositiveInfinity, .65);
		this.substeps = rope.substeps;

		this._offset = Vector2d.zero;
		this.positionStep = Vector2d.zero;
		this.rotationStep = 0;
	}

	public void setAttachSegment(int index) {
		this.attachSegment = index >= 0 ? rope.segments[index] : null;	
	}

	public void setAngleLimit(double angleLimit) {
		this.angleLimit = angleLimit;
	}

    private Vector2d rbOrientation = Vector2d.zero;

	public void setOffset(double offsetX, double offsetY) {
        _offset.x = offsetX;
        _offset.y = offsetY;

        double rotation = Vector2d.SignedAngle(Vector2d.up, anchorSegment.orientation);
        Vector2d.rotateOrientation(_offset, rotation);
        anchorSegment.stretchP2(anchorSegment.p1.x + _offset.x, anchorSegment.p1.y + _offset.y); 
    } 

    Vector3 giz1 = new Vector3();
    Vector3 giz2 = new Vector3();

	public void OnUpdate() {
		updateInterpolation();
		correctVelocity();
	}

	public void OnSubUpdate() {
        setAttachSegment(rope.baseSegment);
		interpolatePositions();
        setOffset(0, .00000001 + rope.winchOffset);
	}

	public void ApplyConstraints() {
		if (attachSegment != null) {
			SegmentConstraint.dostanceConstraint(anchorSegment, attachSegment);
			SegmentConstraint.angleConstraint(anchorSegment, attachSegment, angleLimit * rope.baseExtention);
		} 
	}

	public void OnUpdateLate() {
        updateAnchor();

        giz1.x = (float)anchorSegment.p1.x;
        giz1.y = (float)anchorSegment.p1.y;
        giz2.x = (float)anchorSegment.p2.x;
        giz2.y = (float)anchorSegment.p2.y;

        Debug.DrawLine(giz1, giz2, Color.red, Time.fixedDeltaTime);
	}

    protected Vector2d linearDiff = Vector2d.zero;
	protected double angulerDiff = 0;

    public void applyDrag() {
        if (attachSegment != null) {
            linearDiff.x = (attachSegment.velocity.x - positionStep.x / rope.h) * rope.subLinearDrag;
            linearDiff.y = (attachSegment.velocity.y - positionStep.y / rope.h) * rope.subLinearDrag;
            angulerDiff = (attachSegment.angulerVelocity - rotationStep / rope.h) * rope.subAngulerDrag;
            
            attachSegment.velocity.x -= linearDiff.x;
            attachSegment.velocity.y -= linearDiff.y;
            attachSegment.angulerVelocity -= angulerDiff;
        }
    }

	private Vector2d nextOrientation = Vector2d.zero;

	/*
     * Updates the interpolation start and end points for a new physics step.
     */
    private void updateInterpolation() {
		calcPendingVelocity();

        double anchorRotation = (rb.Rotation.pendingValue().eulerAngles.z + 90) * Mathf.Deg2Rad;
        nextOrientation.x = Math.Cos(anchorRotation);
        nextOrientation.y = Math.Sin(anchorRotation);

        rotationStep = Vector2d.SignedAngle(anchorSegment.orientation, nextOrientation) / rope.substeps;

        double nextPositionX = rb.Position.pendingValue().x + pendingVelocity.x * Time.fixedDeltaTime;
        double nextPositionY = rb.Position.pendingValue().y + pendingVelocity.y * Time.fixedDeltaTime;
        positionStep.x = (nextPositionX - anchorSegment.p1.x) / rope.substeps;
        positionStep.y = (nextPositionY - anchorSegment.p1.y) / rope.substeps;
    }

	private void calcPendingVelocity() {
        Rigidbody rigid = GetComponent<Rigidbody>();
        pendingVelocity.x = rb.Velocity.pendingValue().x;
        pendingVelocity.y = rb.Velocity.pendingValue().y;
        
        foreach((Vector3, ForceMode) force in rb.Force.pendingValue()) {
            float forceX = force.Item1.x;
            float forceY = force.Item1.y;

            if (force.Item2 == ForceMode.Force || force.Item2 == ForceMode.Impulse) {
                forceX /= rigid.mass;
                forceY /= rigid.mass;
            }
            
            if (force.Item2 == ForceMode.Force || force.Item2 == ForceMode.Acceleration) {
                forceX *= Time.fixedDeltaTime;
                forceY *= Time.fixedDeltaTime;
            }

            pendingVelocity.x += forceX;
            pendingVelocity.y += forceY;
        }

        double mag = pendingVelocity.magnitude;
        if (mag > rb.MaxSpeed.pendingValue())
            pendingVelocity = pendingVelocity.normalized * (rb.MaxSpeed.pendingValue() + .8f);
    }

	private void correctVelocity() {
        for (int i = rope.baseSegment; i >= 0; i--) {
            rope.segments[i].setPosition(rope.segments[i].position.x + rb.Velocity.value.x * Time.fixedDeltaTime * velocityCorrection, 
                                         rope.segments[i].position.y + rb.Velocity.value.y * Time.fixedDeltaTime * velocityCorrection);
        }
    }

	/*
     * Advances positions by one substep
     */
    private void interpolatePositions() {
        Segment.rotateAroundP1(anchorSegment, rotationStep);
        anchorSegment.setP1(anchorSegment.p1.x + positionStep.x, anchorSegment.p1.y + positionStep.y);
    }

    private Vector3 look = new Vector3();
    private Quaternion rot = new Quaternion();
    private Vector3 anchorVC = new Vector3();

    /*
     * Converts the position change of anchorSegment into a Force that, when applied to the anchor's rigidbody,
     * will result in the same position change
     */
    private void updateAnchor() {
        look.x = (float)anchorSegment.orientation.x;
        look.y = (float)anchorSegment.orientation.y;
        rot.SetLookRotation(look, Vector3.forward);

        float pendingPosX = rb.Position.pendingValue().x + pendingVelocity.x * Time.fixedDeltaTime;
        float pendingPosY = rb.Position.pendingValue().y + pendingVelocity.y * Time.fixedDeltaTime;

        anchorVC.x = ((float)anchorSegment.p1.x - pendingPosX) / Time.fixedDeltaTime;
        anchorVC.y = ((float)anchorSegment.p1.y - pendingPosY) / Time.fixedDeltaTime; 

        rb.Force.mutate(rb.Force.priorityClass, (List<(Vector3, ForceMode)> forces) => {
            forces.Add((anchorVC, ForceMode.VelocityChange));
            return forces;
        });

        //rb.Rotation.set(RequestClass.Rope, rot);
    }

	void OnValidate() {
		velocityCorrection = System.Math.Clamp(velocityCorrection, 0, 1);
	}
}
