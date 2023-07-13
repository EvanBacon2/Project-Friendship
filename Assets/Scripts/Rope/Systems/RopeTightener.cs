using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rope))]
[RequireComponent(typeof(Anchor))]
[RequireComponent(typeof(RopeExtender))]
[RequireComponent(typeof(PlayerRopeNoIn))]
public class RopeTightener : MonoBehaviour, RopeBehaviour {
    public bool active;
    public float tightenRate = .7f;

    private Rope _rope;
    private Anchor _anchor;
    private RopeExtender _extender;
    private PlayerRopeNoIn _playerRope;

    private Action action;

    void Awake() {
        this._rope = GetComponent<Rope>();
        this._anchor = GetComponent<Anchor>();
        this._extender = GetComponent<RopeExtender>();
        this._playerRope = GetComponent<PlayerRopeNoIn>();
    }

    void Start() {
        action = tightenStart;
    }

    public void OnUpdate() {
        if (active)
            rotateToRope();
    }

    public void OnSubUpdate() {
        if (active)
            action();
    }

    private void tightenStart() {
        tightenRate /= _rope.substeps;
        for (int i = 0; i < _rope.segments.Length; i++) {
            _rope.setAngleConstraint(360, i);
        }

        _rope.segments[0].mass = 1000000;
        _rope.angulerDrag = .995;
        _rope.maxSpeed = 30;        
        
        _anchor.mass = double.PositiveInfinity;
        _anchor.inertia = double.PositiveInfinity;

        action = tighten;
    }

    private void tighten() {
        double slackLength = calcLengths();

        if (slackLength > 0) {
            if (slackLength > tightenRate)
                _extender.winchOffset -= tightenRate;
            else
                _extender.winchOffset -= slackLength;

            _rope.segments[0].velocity.x *= .997;
            _rope.segments[0].velocity.y *= .997;
        } else {
            _rope.segments[0].mass = 1;

            _anchor.mass = 1;
            _anchor.inertia = .05;
            _rope.tightEnd = true;
            _playerRope.stiff();
            tightenRate *= _rope.substeps;
            active = false;

            action = tightenStart;
        } 
    }

    private Vector2d hook2Base = Vector2d.zero;
    private Vector2d segGap = Vector2d.zero;
    private double calcLengths() {
        hook2Base.x = _rope.segments[0].p2.x - _anchor.attachPoint.x;
        hook2Base.y = _rope.segments[0].p2.y - _anchor.attachPoint.y;

        double looseLength = (_extender.baseExtention + _rope.activeSegments) * _playerRope.segmentLength;
        return looseLength - hook2Base.magnitude;
        //Debug.Log("tight: " + hook2Base.magnitude + " loose: " + looseLength);
    }

    public void rotateToRope() {
        float rbAngV = _anchor.rb.AngularVelocity.pendingValue().z;
        float rbAngA = _anchor.rb.AngularAcceleration.pendingValue();
        float rbAngM = _anchor.rb.AngularMax.pendingValue();

        float ropeAngle = Mathf.Atan2((float)_rope.segments[0].p2.y - _anchor.rb.transform.position.y, 
                                (float)_rope.segments[0].p2.x - _anchor.rb.transform.position.x) * Mathf.Rad2Deg - 90;
        float shipAngle = _anchor.rb.Rotation.pendingValue().eulerAngles.z + rbAngV * Time.fixedDeltaTime;
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

        _anchor.rb.Torque.set(PriorityAlias.Rope, torques);
    }
}
