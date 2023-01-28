using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * Defines a rigidbody that a segemnt may attach to.
 */
public class RopeAnchor {
    private RECSRigidBody anchor;
    private Segment anchorSegment;
    private int substeps;

    private List<Action<double, Vector2d>> interpolationCallbacks;

    private Vector2d interPosition = Vector2d.zero;
    private double interRotation = 0;
	private Vector2d positionStep = Vector2d.zero;
	private double rotationStep = 0;

    public readonly Vector2d anchorPosition = Vector2d.zero;
	public readonly Vector2d anchorOrientation = Vector2d.zero;
    private Vector2 offset;

    private double doublePI = 2 * System.Math.PI;
    private double PI = System.Math.PI;
    private double halfPI = .5 * System.Math.PI;

    public RopeAnchor(RECSRigidBody baseRB, double mass, double inertia, Vector2 offset, int substeps) {
        this.anchor = baseRB;
        this.offset = offset;
        this.substeps = substeps;

        this.anchorSegment = new Segment(new Vector2d(offset.x - baseRB.Position.x, offset.y - baseRB.Position.y), 
                                             Vector2d.up, 
                                             mass, 
                                             inertia, 
                                             offset.magnitude);

        addInterpolationCallback((interRotation, interPosition) => {
            anchorSegment.setOrientation(System.Math.Cos(interRotation), System.Math.Sin(interRotation));
            anchorSegment.setP1(interPosition.x, interPosition.y);
        });

        /*addInterpolationCallback((interRotation, interPosition) => { 
            anchorOrientation.x = System.Math.Cos(interRotation);
            anchorOrientation.y = System.Math.Sin(interRotation);
        });

        addInterpolationCallback((interRotation, interPosition) => {
            anchorPosition.x = interPosition.x + positionStep.x + anchorOrientation.x * offset;
            anchorPosition.y = interPosition.y + positionStep.y + anchorOrientation.y * offset;
        });*/
    }

    /*
     * Updates the rigidbody
     */
    public void updateAnchor() {
        //anchor.Position.set
        //anchor.Rotation.set
    }

    private Vector2d anchorDiff = Vector2d.zero;
	private Vector2d anchor_r = Vector2d.zero;

    /*
     * Constrains p1 of segment to the RopeAnchor
     */
    public void anchorConstraint(Segment segment, double angleLimit) {
        SegmentConstraint.distanceConstraint(anchorSegment, segment);
        SegmentConstraint.angleConstraint(anchorSegment, segment, angleLimit);

        //once all substeps are complete, make set request to update baseRB position/rotation

		/*anchorDiff.x = anchorPosition.x - segment.p1.x;
		anchorDiff.y = anchorPosition.y - segment.p1.y;
		anchor_r.x = segment.p1.x - segment.position.x;
        anchor_r.y = segment.p1.y - segment.position.y;
		
		double torque = Vector2d.cross(anchor_r, anchorDiff);
 
		segment.setP1(segment.p1.x + anchorDiff.x, segment.p1.y + anchorDiff.y);
		
		Segment.rotateOrientation(segment, torque);

        double angle = Vector2d.SignedAngle(anchorOrientation, segment.orientation);
        double limit = angleLimit;

        if (System.Math.Abs(angle) >= limit) {
            double diff = angle - (angle > 0 ? limit : -limit);
            Segment.rotateOrientation(segment, -diff);
        }*/
	}

    /*
     * Add a callback that is called everytime the anchors position is interpolated
     */
    public void addInterpolationCallback(Action<double, Vector2d> callback) {
        interpolationCallbacks.Add(callback);
    }

    /*
     * Updates the interpolation start and end points for a new physics step.
     */
    public void updateInterpolation() {
        double nextRotation = (anchor.Rotation.value.eulerAngles.z + 90) * Mathf.Deg2Rad;

        //When prev and next cross over the 0 degree mark angle goes from doublePI to zero and vice versa.  To ensure angle difference is accurate
        //prevRotation is adjusted so that both it and nextRotation are either > or < doublePI.    
        if (System.Math.Clamp(interRotation, doublePI, doublePI + halfPI) == interRotation && System.Math.Clamp(nextRotation, halfPI, PI) == nextRotation)
            interRotation -= doublePI;
        if (System.Math.Clamp(interRotation, halfPI, PI) == interRotation && System.Math.Clamp(nextRotation, doublePI, doublePI + halfPI) == nextRotation)
            interRotation += doublePI;

        rotationStep = (nextRotation - interRotation) / substeps;

        double nextPositionX = anchor.Position.x + RECSRigidBody.impendingVelocity.x * Time.fixedDeltaTime;
        double nextPositionY = anchor.Position.y + RECSRigidBody.impendingVelocity.y * Time.fixedDeltaTime;

        positionStep.x = (nextPositionX - interPosition.x) / substeps;
        positionStep.y = (nextPositionY - interPosition.y) / substeps;
    }

    /*
     * Advances positions by one substep
     */
    public void interpolatePositions() {
        interRotation += rotationStep;
        interPosition.x += positionStep.x;
        interPosition.y += positionStep.y;

        foreach (Action<double, Vector2d> callback in interpolationCallbacks) {
            callback(interRotation, interPosition);
        }
    }
}
