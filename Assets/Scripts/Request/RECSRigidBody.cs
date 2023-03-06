using System;
using System.Collections.Generic;
using UnityEngine;

public class RECSRigidbody : MonoBehaviour {
    public Rigidbody rb;

    protected RigidbodyReference reference;
    private Dictionary<RequestSender, HashSet<Guid>> senders;

    protected ManagedAnyRequestable<Vector3> _position;
    public ManagedAnyRequestable<Quaternion> _rotation;
    private ManagedAnyRequestable<Vector3> _velocity;
    private ManagedAnyRequestable<Vector3> _angularVelocity;
    private ManagedAnyRequestable<List<(Vector3, ForceMode)>> _force;
    private ManagedAnyRequestable<List<(Vector3, ForceMode)>> _torque;
    private ManagedAnyRequestable<float> _magnitude;
 
    public IManagedAnyRequest<Vector3> Position {
        get { return _position; }
    }
    public IManagedAnyRequest<Quaternion> Rotation {
        get { return _rotation; }
    }
    public IManagedAnyRequest<Vector3> Velocity {
        get { return _velocity; }
    }
    public IManagedAnyRequest<Vector3> AngularVelocity {
        get { return _angularVelocity; }
    }
    public IManagedAnyRequest<List<(Vector3, ForceMode)>> Force {
        get { return _force; }
    }
    public IManagedAnyRequest<List<(Vector3, ForceMode)>> Torque {
        get { return _torque; }
    }
    public IManagedAnyRequest<float> Magnitude {
        get { return _magnitude; }
    }

    void Start() {
        this.senders = new();

        start();

        AnyRequestPool<Vector3> positionPool = new();
        _position = new ManagedAnyRequestable<Vector3>(
            () => { return rb.transform.position; }, 
            (Vector3 p) => { rb.transform.position = p; }, 
            reference.Position, 
            new IncreasingPriority(() => { positionPool.reset(); }), 
            positionPool
        );

        AnyRequestPool<Quaternion> rotationPool = new();
        _rotation = new ManagedAnyRequestable<Quaternion>(
            () => { return rb.transform.rotation; }, 
            (Quaternion r) => { rb.transform.rotation = r; },
            reference.Rotation,
            new IncreasingPriority(() => { rotationPool.reset(); }),
            rotationPool
        );
        
        AnyRequestPool<Vector3> velocityPool = new();
        _velocity = new ManagedAnyRequestable<Vector3>(
            () => { return rb.velocity; },
            (Vector3 v) => { rb.velocity = v; },
            reference.Velocity,
            new IncreasingPriority(() => { velocityPool.reset(); }),
            velocityPool
        );

        AnyRequestPool<Vector3> angularVelocityPool = new();
        _angularVelocity = new ManagedAnyRequestable<Vector3>(
            () => { return rb.angularVelocity; },
            (Vector3 v) => { rb.angularVelocity = v; },
            reference.AngularVelocity,
            new IncreasingPriority(() => { angularVelocityPool.reset(); }),
            angularVelocityPool
        );

        AnyRequestPool<List<(Vector3, ForceMode)>> forcePool = new();
        _force = new ManagedAnyRequestable<List<(Vector3, ForceMode)>>(
            () => { return new List<(Vector3, ForceMode)>(); }, 
            (List<(Vector3, ForceMode)> forces) => { 
                foreach((Vector3, ForceMode) force in forces) {
                    rb.AddForce(force.Item1, force.Item2);
                }
            },
            reference.Force,
            new IncreasingPriority(() => { forcePool.reset(); }),
            forcePool
        );

        AnyRequestPool<List<(Vector3, ForceMode)>> torquePool = new();
        _torque = new ManagedAnyRequestable<List<(Vector3, ForceMode)>>(
            () => { return new List<(Vector3, ForceMode)>(); },
            (List<(Vector3, ForceMode)> torques) => {
                foreach((Vector3, ForceMode) torque in torques) {
                    rb.AddTorque(torque.Item1, torque.Item2);
                }
            },
            reference.Torque,
            new IncreasingPriority(() => { torquePool.reset(); }),
            torquePool
        );

        AnyRequestPool<float> magnitudePool = new();
        _magnitude = new ManagedAnyRequestable<float>(
            () => { return rb.velocity.magnitude; }, 
            (float m) => { rb.velocity = rb.velocity.normalized * m; },
            reference.Magnitude,
            new IncreasingPriority(() => { magnitudePool.reset(); }),
            magnitudePool
        );
    }

    protected virtual void start() {}

    public void executeRequests() {
        notifySenders();
        _executeRequests();
    }

    protected virtual void _executeRequests() {
        //_velocity.executeRequests();
        //_angularVelocity.executeRequests();
        _force.executeRequests();
        _torque.executeRequests();
        _magnitude.executeRequests();
        _position.executeRequests();
        _rotation.executeRequests();
    }

    protected void setReference(RigidbodyReference reference) {
        this.reference = reference;

        _position.setReference(reference.Position);
        _rotation.setReference(reference.Rotation);
        _velocity.setReference(reference.Velocity);
        _angularVelocity.setReference(reference.AngularVelocity);
        _force.setReference(reference.Force);
        _torque.setReference(reference.Torque);
        _magnitude.setReference(reference.Magnitude);
    }

    private void notifySenders() {
        _notifySenders();

        foreach(RequestSender sender in senders.Keys) {
            Debug.Log(sender);
            sender.onRequestsExecuted(senders[sender]);
        }

        senders.Clear();
    }

    protected virtual void _notifySenders() {
        _position.addSendersTo(senders);
        _rotation.addSendersTo(senders);
        _velocity.addSendersTo(senders);
        _angularVelocity.addSendersTo(senders);
        _force.addSendersTo(senders);
        _torque.addSendersTo(senders);
        _magnitude.addSendersTo(senders);
    }
}
