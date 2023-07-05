using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TightenRope {
    private bool active;

    float tightenRate = .7f;
    private double tightLength = 0;
    private double looseLength = 0;

    public bool execute(PlayerRopeState state) {
        if (!active) {
            this.active = true;

            for (int i = 1; i < state.rope.segments.Length; i++) {
                state.rope.setAngleConstraint(360, i);
            }
            state.rope.segments[0].mass = 1000000;
            state.rope.angulerDrag = .995;
            state.rope.maxSpeed = 30;        
            
            state.rope.anchor.mass = double.PositiveInfinity;
            state.rope.anchor.inertia = double.PositiveInfinity;
        }

        return tighten(state.rope);
    }

    private bool tighten(PlayerRope rope) {
        calcLengths(rope);

        if (tightLength > -.1) {
            rope.winchOffset -= tightenRate;
            rope.segments[0].velocity.x *= .95;
            rope.segments[0].velocity.y *= .95;
            rotateToRope(rope);
    
            return true;
        } else {
            if (rope.baseExtention > 0)
                    rope.winchOffset -= rope.winchUnit * rope.baseExtention;
            rope.segments[0].mass = 1;

            rope.anchor.mass = 1;
            rope.anchor.inertia = .05;
            rope.stiff();
            this.active = false;

            return false;
        }
    }

    private Vector2d hook2Base = Vector2d.zero;
    private void calcLengths(PlayerRope rope) {
        hook2Base.x = rope.segments[0].p2.x - rope.anchor.attachPoint.x;
        hook2Base.y = rope.segments[0].p2.y - rope.anchor.attachPoint.y;

        looseLength = (rope.baseExtention + rope.activeSegments - 1) * rope.segmentLength;
        tightLength = looseLength - hook2Base.magnitude;
    }

    private void rotateToRope(PlayerRope rope) {
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
