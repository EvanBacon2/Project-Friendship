using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TightenRope {
    private bool tight;
    private bool winchDone;
    private float ropeAngle;

    private Vector2d hookPoint = Vector2d.zero;

    float tightenRate = .65f;
    private double tightLength = 0;
    private double looseLength = 0;

    public bool execute(PlayerRopeState state) {
        tightenRate = .65f;

        if (tight)
            tighten(state.rope);
        else {
            this.tight = true;

            for (int i = 1; i < state.rope.segments.Length; i++) {
                state.rope.setAngleConstraint(360, i);
            }

            //state.rope.noAngle = true;

            state.rope.hookPoint.x = state.rope.segments[0].p2.x;
            state.rope.hookPoint.y = state.rope.segments[0].p2.y;
            //state.rope.constrainHook = true;
            state.rope.segments[0].mass = 1000000;

            ropeAngle = Mathf.Atan2((float)state.rope.segments[0].p2.y - state.rope.anchor.rb.transform.position.y, 
                                (float)state.rope.segments[0].p2.x - state.rope.anchor.rb.transform.position.x) * Mathf.Rad2Deg - 90;
            if (ropeAngle < 0) 
                ropeAngle += 360;    

            state.rope.angulerDrag = .995;
            state.rope.maxSpeed = 30;        

            tightenRope(state.rope);

            float diff = ropeAngle - state.rope.anchor.rb.Rotation.pendingValue().eulerAngles.z;
            if (diff > 180)
                diff -= 360;

            if (diff < -180)
                diff += 360;
            

            tighten(state.rope);
            state.rope.anchor.mass = double.PositiveInfinity;
            state.rope.anchor.inertia = double.PositiveInfinity;
        }

        return tight;
    }

    private void tighten(PlayerRope rope) {

        /*if (tightLength <= 2) {
            tightenRate /= 3;
        }*/

        if (tightLength > -1) {

            rope.winchOffset -= tightenRate;
            //tightLength -= tightenRate;
            Debug.Log(tightLength);
            //rope.hookPoint.x += rope.anchor.rb.Velocity.pendingValue().x * Time.fixedDeltaTime;
            //rope.hookPoint.y += rope.anchor.rb.Velocity.pendingValue().y * Time.fixedDeltaTime;

            rope.anchor.inertia = double.PositiveInfinity;
            calcRopeAngle(rope);
            rotateToRope(rope);

            tightenRope(rope);
        } else {
            /*rope.winchOffset -= tightLength;
            if (rope.baseExtention > 0)
                rope.winchOffset -= rope.winchUnit * rope.baseExtention;*/
            looseLength = 0;
            tightLength = 0;
            rope.segments[0].mass = 1;
            tight = false;
            //winchDone = true;
            rope.constrainHook = false;

            rope.anchor.mass = 1;
            rope.anchor.inertia = .05;
            rope.noAngle = false;
            rope.stiff();
        }
    }

    private Vector2d hook2Base = Vector2d.zero;
    private void tightenRope(PlayerRope rope) {
        hook2Base.x = rope.segments[0].p2.x - rope.anchor.attachPoint.x;
        hook2Base.y = rope.segments[0].p2.y - rope.anchor.attachPoint.y;

        looseLength = (rope.baseExtention + rope.activeSegments - 1) * rope.segmentLength;
        tightLength = looseLength - hook2Base.magnitude;
    }

    private void calcRopeAngle(PlayerRope rope) {
         ropeAngle = Mathf.Atan2((float)rope.segments[0].p2.y - rope.anchor.rb.transform.position.y, 
                                (float)rope.segments[0].p2.x - rope.anchor.rb.transform.position.x) * Mathf.Rad2Deg - 90;
    }

    private void rotateToRope(PlayerRope rope) {
        float rbAngV = rope.anchor.rb.AngularVelocity.pendingValue().z;
        float rbAngA = rope.anchor.rb.AngularAcceleration.pendingValue();
        float rbAngM = rope.anchor.rb.AngularMax.pendingValue();

        float shipAngle = rope.anchor.rb.Rotation.pendingValue().eulerAngles.z + rbAngV * Time.fixedDeltaTime;

        Debug.Log("ship: " + shipAngle + " rope: " + ropeAngle);

        float diff = ropeAngle - shipAngle;

        if (diff > 180)
            diff -= 360;

        if (diff < -180)
            diff += 360;

        float angSign = Mathf.Sign(diff);
        
        float targetVelocity = Mathf.Clamp(diff * 25, -rbAngM, rbAngM);
        //targetVelocity = Mathf.Clamp(targetVelocity, -diff, diff);
        float currentVelocity = rbAngV * Mathf.Rad2Deg;
        float velocityChange = targetVelocity - currentVelocity;
        
        velocityChange = Mathf.Clamp(velocityChange, -rbAngA, rbAngA) * Mathf.Deg2Rad;

        List<(Vector3, ForceMode)> torques = new List<(Vector3, ForceMode)>();
        torques.Add((new Vector3(0, 0, velocityChange), ForceMode.VelocityChange));

        rope.anchor.rb.Torque.set(PriorityAlias.Rope, torques);

        /*float max = Mathf.Min(30, Mathf.Abs(diff));

        max *= Mathf.Sign(diff);
        max += rope.anchor.rb.Rotation.pendingValue().eulerAngles.z;

        Quaternion newThing = Quaternion.AngleAxis(max, Vector3.forward);

        rope.anchor.rb.Torque.block(PriorityAlias.Rope);
        rope.anchor.rb.Rotation.set(PriorityAlias.Rope, newThing);*/
    }
}
