using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerRopeSystem : RequestSystem<PlayerRopeState> {
    private bool tight;
    private bool winchDone;
    private float ropeAngle;

    private int tightenFrames;

    private float rotateRate;

    private Vector2d hookPoint = Vector2d.zero;

    private TightenRope tighty = new TightenRope();

    public override void OnStateReceived(object sender, PlayerRopeState state) {
        if (state.mode && state.rope.extended) {
            if (state.rope.mode == RopeMode.FLEXIBLE) {
                this.tight = true;

                /*for (int i = 0; i < state.rope.segments.Length; i++) {
                    state.rope.setAngleConstraint(360, i);
                }

                state.rope.hookPoint.x = state.rope.segments[0].p2.x;
                state.rope.hookPoint.y = state.rope.segments[0].p2.y;
                state.rope.constrainHook = true;

                ropeAngle = Mathf.Atan2((float)state.rope.segments[0].p2.y - state.rope.anchor.rb.transform.position.y, 
                                    (float)state.rope.segments[0].p2.x - state.rope.anchor.rb.transform.position.x) * Mathf.Rad2Deg - 90;
                if (ropeAngle < 0) 
                    ropeAngle += 360;    

                state.rope.angulerDrag = .995;
		        state.rope.maxSpeed = 30;        

                tightenRope(state.rope);

                tightenFrames = Mathf.CeilToInt((float)tightLength / tightenRate);

                float diff = ropeAngle - state.rope.anchor.rb.Rotation.pendingValue().eulerAngles.z;
                if (diff > 180)
                    diff -= 360;

                if (diff < -180)
                    diff += 360;
                rotateRate = Mathf.Abs(diff) * 1.5f / tightenFrames;

                tighten(state.rope);
                state.rope.anchor.mass = double.PositiveInfinity;
                state.rope.anchor.inertia = double.PositiveInfinity;*/

                tighty.execute(state);
                //state.rope.stiff();
            } else 
                state.rope.flexible();
        }

        if (tight)
            tight = tighty.execute(state);
            //tighten(state.rope);

        /*if (winchDone && state.rope.winchOffset == 0) {
            state.rope.anchor.mass = 1;
            state.rope.anchor.inertia = .05;
            winchDone = false;
            
            state.rope.stiff();
            /*for (int i = 0; i < state.rope.segments.Length; i++) {
                state.rope.setAngleConstraint(3, i);
            }
        }*/
    }

    float tightenRate = .65f;
    private double tightLength = 0;
    private double looseLength = 0;
    
    
    private void tighten(PlayerRope rope) {
        if (tightLength > -1/*tightenRate*/) {
                rope.winchOffset -= tightenRate;
                //tightLength -= tightenRate;

                rope.hookPoint.x += rope.anchor.rb.Velocity.pendingValue().x * Time.fixedDeltaTime;
                rope.hookPoint.y += rope.anchor.rb.Velocity.pendingValue().y * Time.fixedDeltaTime;

                rope.anchor.inertia = double.PositiveInfinity;
                rotateToRope(rope);

                tightenRope(rope);
            } else {
                rope.winchOffset -= tightLength;
                if (rope.baseExtention > 0)
                    rope.winchOffset -= rope.winchUnit * rope.baseExtention;
                looseLength = 0;
                tightLength = 0;
                //rope.stiff();
                tight = false;
                winchDone = true;
                rope.constrainHook = false;
                rope.anchor.inertia = .05;

                rope.anchor.mass = 1;
                rope.anchor.inertia = .05;
                winchDone = false;
                rope.stiff();
            }
    }

    private Vector2d hook2Base = Vector2d.zero;
    private void tightenRope(PlayerRope rope) {
        hook2Base.x = rope.segments[0].p2.x - rope.anchor.attachPoint.x;
        hook2Base.y = rope.segments[0].p2.y - rope.anchor.attachPoint.y;

        looseLength = (rope.baseExtention + rope.activeSegments - 1) * rope.segmentLength;
        tightLength = looseLength - hook2Base.magnitude;
        //Debug.Log("tight " + tightLength + " loose " + looseLength);
    }

    private void rotateToRope(PlayerRope rope) {
        float rbAngV = rope.anchor.rb.AngularVelocity.value.z;
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
