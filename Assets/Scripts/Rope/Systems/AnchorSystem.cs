using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * Defines a system that controls an Anchor that a rope can attach to
 */
public class AnchorSystem : RequestSystem<AnchorState> {
    /*private List<Action<Vector2d, Vector2d>> interpolationCallbacks;

	private Vector2d positionStep = Vector2d.zero;
	private double rotationStep = 0;
    private Vector3 pendingVelocity = Vector3.zero;*/

    //private Rope rope;
    //private Anchor anchor;
    //private (Vector3, ForceMode) pendingForce;
    //private Vector3 anchorVelocity;
    //private Segment attachSegment;
    //private double angleLimit;
    //public double correction;

    public override void OnStateReceived(object sender, AnchorState state) {
        //this.rope = state.rope;
        //this.pendingForce = state.pendingForce;
        //this.anchorVelocity = state.anchorVelocity;     
        //this.attachSegment = state.attachSegment; 
        //this.angleLimit = state.angleLimit;
        //state.anchor.setAttachSegment(state.attachSegment);
        state.anchor.setAngleLimit(state.angleLimit);
        //state.anchor.setOffset(state.OffsetX, state.OffsetY);
    }

    //public override void OnUpdate() {
        //calcPendingVelocity();
        //anchor.updateInterpolation(rope.substeps);
        //anchorVelocityCorrection();
    //}

    //public override void OnSubUpdate() {
        //anchor.interpolatePositions();
    //}

    //public override void ApplyConstraints() {
        //anchorConstraint(attachSegment, angleLimit);
    //}

    //public override void OnUpdateLate() {
        //anchor.updateAnchor();
    //}

    /*private void calcPendingVelocity() {
        if (pendingForce.Item2 == ForceMode.Force || pendingForce.Item2 == ForceMode.Impulse) 
            pendingForce.Item1 /= (float)anchor.anchorSegment.mass;
        
        if (pendingForce.Item2 == ForceMode.Force || pendingForce.Item2 == ForceMode.Acceleration)
            pendingForce.Item1 *= Time.fixedDeltaTime;

        pendingVelocity.x = pendingForce.Item1.x;
        pendingVelocity.y = pendingForce.Item1.y;

        if (pendingForce.Item2 != ForceMode.VelocityChange) {
            pendingVelocity.x += anchorVelocity.x;
            pendingVelocity.y += anchorVelocity.y;
        }
    }

    private Vector2d nextOrientation = Vector2d.zero;*/

    /*
     * Updates the interpolation start and end points for a new physics step.
     */
    /*private void updateInterpolation() {
        double anchorRotation = (anchor.anchor.Rotation.value.eulerAngles.z + 90) * Mathf.Deg2Rad;
        nextOrientation.x = Math.Cos(anchorRotation); 
        nextOrientation.y = Math.Sin(anchorRotation);

        rotationStep = Vector2d.SignedAngle(anchor.anchorSegment.orientation, nextOrientation) / rope.substeps;

        double nextPositionX = anchor.anchor.Position.value.x + pendingVelocity.x * Time.fixedDeltaTime;
        double nextPositionY = anchor.anchor.Position.value.y + pendingVelocity.y * Time.fixedDeltaTime;

        positionStep.x = (nextPositionX - anchor.anchorSegment.p1.x) / rope.substeps;
        positionStep.y = (nextPositionY - anchor.anchorSegment.p1.y) / rope.substeps;
    }*/

    /*public void anchorVelocityCorrection() {//move to anchor
        for (int i = rope.baseSegment; i >= 0; i--) {
            rope.segments[i].setPosition(rope.segments[i].position.x + anchor.rb.Velocity.value.x * Time.fixedDeltaTime * correction, 
                                         rope.segments[i].position.y + anchor.rb.Velocity.value.y * Time.fixedDeltaTime * correction);
        }
    }*/

    /*
     * Advances positions by one substep
     */
    /*public void interpolatePositions() {
        Segment.rotateOrientation(anchor.anchorSegment, rotationStep);
        anchor.anchorSegment.setP1(anchor.anchorSegment.p1.x + positionStep.x, anchor.anchorSegment.p1.y + positionStep.y);

        foreach (Action<Vector2d, Vector2d> callback in interpolationCallbacks) {
            callback(anchor.anchorSegment.orientation, anchor.anchorSegment.p1);
        }
    }*/

    /*
     * Constrains p1 of segment to the RopeAnchor
     */
    /*public void anchorConstraint(Segment segment, double angleLimit) {
        SegmentConstraint.distanceConstraint(anchor.anchorSegment, segment);
        SegmentConstraint.angleConstraint(anchor.anchorSegment, segment, angleLimit);
	}

    /*private Vector3 pos = new Vector3();
    private Vector3 look = new Vector3();
    private Quaternion rot = new Quaternion();
    private Vector3 newVelocity = new Vector3();
    private Vector3 newAcceleration = new Vector3();
    private Vector3 force = new Vector3();*/

    /*
     * Converts the position change of anchorSegment into a Force that, when applied to the anchor's rigidbody,
     * will result in the same position change
     */
    /*public void updateAnchor() {
        look.x = (float)anchor.anchorSegment.orientation.x;
        look.y = (float)anchor.anchorSegment.orientation.y;

        rot.SetLookRotation(look, Vector3.forward);

        newVelocity.Set(((float)anchor.anchorSegment.position.x - anchor.anchor.Position.value.x) * Time.fixedDeltaTime, 
                        ((float)anchor.anchorSegment.position.y - anchor.anchor.Position.value.y) * Time.fixedDeltaTime, 0);
        
        newAcceleration.Set((newVelocity.x - (float)pendingVelocity.x) * Time.fixedDeltaTime, 
                            (newVelocity.y - (float)pendingVelocity.y) * Time.fixedDeltaTime, 0);

        force.x = newAcceleration.x * (float)anchor.anchorSegment.mass;
		force.y = newAcceleration.y * (float)anchor.anchorSegment.mass;

        anchor.anchor.Force.set(RequestClass.Rope, (force, ForceMode.Force));
        anchor.anchor.Rotation.set(RequestClass.Rope, rot);
    }*/
}
