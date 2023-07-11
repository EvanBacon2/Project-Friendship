using System;
using System.Collections.Generic;
using UnityEngine;

public class TightenRope {
    private bool active;

    float tightenRate = .7f;
    private double tightLength = 0;
    private double looseLength = 0;

    private bool endTighten = false;

    public bool execute(PlayerRope rope) {
        if (!active) {
            this.active = true;
            tightenRate /= rope.substeps;
            for (int i = 0; i < rope.segments.Length; i++) {
                rope.setAngleConstraint(360, i);
            }

            for (int i = 0; i < rope.activeSegments; i++) {
                rope.segments[i].inertia = .1;
            }
            rope.segments[0].mass = 1000000;
            rope.angulerDrag = .995;
            rope.maxSpeed = 30;        
            
            rope.anchor.mass = double.PositiveInfinity;
            rope.anchor.inertia = double.PositiveInfinity;
        }

        return tighten(rope);
    }

    private bool tighten(PlayerRope rope) {
        calcLengths(rope);

        if (tightLength > 0) {
            if (tightLength > tightenRate)
                rope.winchOffset -= tightenRate;
            else
                rope.winchOffset -= tightLength;

            rope.segments[0].velocity.x *= .997;
            rope.segments[0].velocity.y *= .997;
            //rotateToRope(rope);
    
            return true;
        } else if (endTighten) {
            //if (rope.baseExtention > 0)
            //        rope.winchOffset -= rope.winchUnit * rope.baseExtention;
            rope.segments[0].mass = 1;
            for (int i = 0; i < rope.activeSegments; i++) {
                rope.segments[i].inertia = 1;
            }

            rope.tighten = false;
            rope.anchor.mass = 1;
            rope.anchor.inertia = .05;
            rope.tightEnd = true;
            rope.stiff();
            tightenRate *= rope.substeps;
            this.active = false;

            endTighten = false;

            return false;
        } else {
            endTighten = true;
            return true;
        }
    }

    private Vector2d hook2Base = Vector2d.zero;
    private Vector2d segGap = Vector2d.zero;
    private void calcLengths(PlayerRope rope) {
        hook2Base.x = rope.segments[0].p2.x - rope.anchor.attachPoint.x;
        hook2Base.y = rope.segments[0].p2.y - rope.anchor.attachPoint.y;

        looseLength = (rope.baseExtention + rope.activeSegments) * rope.segmentLength;
        tightLength = looseLength - hook2Base.magnitude;
        //Debug.Log("tight: " + hook2Base.magnitude + " loose: " + looseLength);
    }

    public void rotateToRope(PlayerRope rope) {
        float rbAngV = rope.anchor.rb.AngularVelocity.pendingValue().z;
        float rbAngA = rope.anchor.rb.AngularAcceleration.pendingValue();
        float rbAngM = rope.anchor.rb.AngularMax.pendingValue();

        float ropeAngle = Mathf.Atan2((float)rope.segments[0].p2.y - rope.anchor.rb.transform.position.y, 
                                (float)rope.segments[0].p2.x - rope.anchor.rb.transform.position.x) * Mathf.Rad2Deg - 90;
        float shipAngle = rope.anchor.rb.Rotation.pendingValue().eulerAngles.z + rbAngV * Time.fixedDeltaTime;
        float diff = ropeAngle - shipAngle;

        if (diff > 180)
            diff -= 360;

        if (diff < -180)
            diff += 360;
        
        float targetVelocity = Mathf.Clamp(diff * 25, -rbAngM, rbAngM);
        float currentVelocity = rbAngV * Mathf.Rad2Deg;
        float velocityChange = targetVelocity - currentVelocity;
        
        velocityChange = Mathf.Clamp(velocityChange, -rbAngA, rbAngA) * Mathf.Deg2Rad;

        List<(Vector3, ForceMode)> torques = new List<(Vector3, ForceMode)>();
        torques.Add((new Vector3(0, 0, velocityChange), ForceMode.VelocityChange));

        rope.anchor.rb.Torque.set(PriorityAlias.Rope, torques);
    }
}
