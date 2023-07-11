using System;
using System.Collections.Generic;
using UnityEngine;

public class TightenRope {
    private bool active;

    float tightenRate = .7f;
    private double tightLength = 0;
    private double looseLength = 0;

    private bool endTighten = false;

    public bool execute(/*PlayerRope rope*/Rope rope, Anchor anchor, ExtendRopeNoIn extender, PlayerRopeNoIn playerRope) {//Rope, Anchor, Extender, PlayerRope
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
            
            anchor.mass = double.PositiveInfinity;
            anchor.inertia = double.PositiveInfinity;
        }

        return tighten(rope, extender, anchor, playerRope);
    }

    private bool tighten(/*PlayerRope rope*/Rope rope, ExtendRopeNoIn extender, Anchor anchor, PlayerRopeNoIn playerRope) {//Rope, Extender, Anchor, PlayerRope
        calcLengths(rope, extender, anchor, playerRope);

        if (tightLength > 0) {
            if (tightLength > tightenRate)
                extender.winchOffset -= tightenRate;
            else
                extender.winchOffset -= tightLength;

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

            playerRope.tighten = false;
            anchor.mass = 1;
            anchor.inertia = .05;
            rope.tightEnd = true;
            playerRope.stiff();
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
    private void calcLengths(Rope rope, ExtendRopeNoIn extender, Anchor anchor, PlayerRopeNoIn playerRope) {//Rope, Extender, Anchor
        hook2Base.x = rope.segments[0].p2.x - anchor.attachPoint.x;
        hook2Base.y = rope.segments[0].p2.y - anchor.attachPoint.y;

        looseLength = (extender.baseExtention + rope.activeSegments) * playerRope.segmentLength;
        tightLength = looseLength - hook2Base.magnitude;
        //Debug.Log("tight: " + hook2Base.magnitude + " loose: " + looseLength);
    }

    public void rotateToRope(Rope rope, Anchor anchor) {
        float rbAngV = anchor.rb.AngularVelocity.pendingValue().z;
        float rbAngA = anchor.rb.AngularAcceleration.pendingValue();
        float rbAngM = anchor.rb.AngularMax.pendingValue();

        float ropeAngle = Mathf.Atan2((float)rope.segments[0].p2.y - anchor.rb.transform.position.y, 
                                (float)rope.segments[0].p2.x - anchor.rb.transform.position.x) * Mathf.Rad2Deg - 90;
        float shipAngle = anchor.rb.Rotation.pendingValue().eulerAngles.z + rbAngV * Time.fixedDeltaTime;
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

        anchor.rb.Torque.set(PriorityAlias.Rope, torques);
    }
}
