using System;
using System.Collections.Generic;
using UnityEngine;

public class RECSRigidbody : MonoBehaviour {
    public Rigidbody rb;

    protected RigidbodyReference reference;
    protected Dictionary<RequestSender, HashSet<Guid>> senders;

    protected ManagedAnyRequestable<Vector3> _position;
    public ManagedAnyRequestable<Quaternion> _rotation;
    private ManagedAnyRequestable<Vector3> _velocity;
    private ManagedAnyRequestable<Vector3> _angularVelocity;
    private ManagedAnyRequestable<List<(Vector3, ForceMode)>> _force;
    private ManagedAnyRequestable<List<(Vector3, ForceMode)>> _torque;

    private List<(Vector3, ForceMode)> collisionForces;
 
    public IManagedRequestPort<Vector3> Position {
        get { return _position; }
    }
    public IManagedRequestPort<Quaternion> Rotation {
        get { return _rotation; }
    }
    public IManagedRequestPort<Vector3> Velocity {
        get { return _velocity; }
    }
    public IManagedRequestPort<Vector3> AngularVelocity {
        get { return _angularVelocity; }
    }
    public IManagedRequestPort<List<(Vector3, ForceMode)>> Force {
        get { return _force; }
    }
    public IManagedRequestPort<List<(Vector3, ForceMode)>> Torque {
        get { return _torque; }
    }

    void Start() {
        this.senders = new();
        this.collisionForces = new();

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
            () => { return rb.velocity; /*new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);*/ },
            (Vector3 v) => { rb.velocity = v; },
            reference.Velocity,
            new IncreasingPriority(() => { velocityPool.reset(); }),
            velocityPool
        );

        AnyRequestPool<Vector3> angularVelocityPool = new();
        _angularVelocity = new ManagedAnyRequestable<Vector3>(
            () => { return rb.angularVelocity; /*new Vector3(rb.angularVelocity.x, rb.angularVelocity.y, rb.angularVelocity.z);*/ },
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
    }

    protected virtual void start() {}

    public void executeRequests() {
        notifySenders();
        _executeRequests();
    }

    protected virtual void _executeRequests() {
        _velocity.executeRequests();
        _angularVelocity.executeRequests();
        _force.executeRequests();
        _torque.executeRequests();
        _position.executeRequests();
        _rotation.executeRequests();

        collisionForces.Clear();
    }

    protected void setReference(RigidbodyReference reference) {
        this.reference = reference;

        _position.setReference(reference.Position);
        _rotation.setReference(reference.Rotation);
        _velocity.setReference(reference.Velocity);
        _angularVelocity.setReference(reference.AngularVelocity);
        _force.setReference(reference.Force);
        _torque.setReference(reference.Torque);
    }

    private void notifySenders() {
        _notifySenders();

        foreach(RequestSender sender in senders.Keys) {
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
    }

    /*
     * Calculates what this rigidbody's velocity will be after all forces have been applied to it
     */
    public Vector3 calcPendingVelocity(Vector3 baseVelocity) {
        foreach((Vector3, ForceMode) force in Force.pendingValue()) {
            baseVelocity = addForce(baseVelocity, force);
        }

        foreach((Vector3, ForceMode) force in collisionForces) {
            baseVelocity = addForce(baseVelocity, force);
        }

        return baseVelocity;
    }

    /*
     * Calculates what this rigidbody's angular velocity will be after all torques have been applied to it
     */
    public Vector3 calcPendingAngularVelocity(Vector3 baseAngularVelocity) {
        foreach((Vector3, ForceMode) torque in Torque.pendingValue()) {
            baseAngularVelocity = addForce(baseAngularVelocity, torque);
        }

        return baseAngularVelocity;
    }

    private Vector3 addForce(Vector3 velocity, (Vector3, ForceMode) force) {
        float forceX = force.Item1.x;
        float forceY = force.Item1.y;
        float forceZ = force.Item1.z;

        if (force.Item2 == ForceMode.Force || force.Item2 == ForceMode.Impulse) {
            forceX /= rb.mass;
            forceY /= rb.mass;
            forceZ /= rb.mass;
        }
        
        if (force.Item2 == ForceMode.Force || force.Item2 == ForceMode.Acceleration) {
            forceX *= Time.fixedDeltaTime;
            forceY *= Time.fixedDeltaTime;
            forceZ *= Time.fixedDeltaTime;
        }

        velocity.x += forceX;
        velocity.y += forceY;
        velocity.z += forceZ;

        return velocity;
    }

    void OnCollisionEnter(Collision other) {
        collisionForces.Add((other.impulse, ForceMode.Impulse));
    }
}
