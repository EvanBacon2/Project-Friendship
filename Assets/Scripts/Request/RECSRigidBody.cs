using System;
using System.Collections.Generic;
using UnityEngine;

public class RECSRigidbody : MonoBehaviour {
    public Rigidbody rb;
    public float acceleration;
    public float maxSpeed;
    public float maxRotation;

    private float _baseAcceleration;
    private float _baseMaxSpeed;
    public float BASE_ACCELERATION { get { return _baseAcceleration; } }
    public float BASE_MAXSPEED { get { return _baseMaxSpeed; } }

    public RigidbodyReference reference;
    
    private Dictionary<RequestSender, HashSet<Guid>> senders;

    private ManagedAnyRequestable<Vector3> _velocity;
    private ManagedAnyRequestable<Vector3> _angularVelocity;
    private ManagedAnyRequestableValue<float> _acceleration;
    private ManagedAnyRequestableValue<float> _maxSpeed;
    private ManagedAnyRequestableValue<float> _maxRotation;
    private ManagedAnyRequestable<List<(Vector3, ForceMode)>> _force;
    private ManagedAnyRequestable<List<(Vector3, ForceMode)>> _torque;
    private ManagedAnyRequestable<float> _magnitude;
    private ManagedAnyRequestable<Quaternion> _rotation;
    private ManagedAnyRequestable<Vector3> _position;

    public static Vector2 impendingVelocity = Vector2.zero;//what the velocity of the ship will be after FixedUpdate has run
    private (Vector3, ForceMode) impendingForce = (Vector3.zero, ForceMode.Force);
 
    public IManagedAnyRequest<Vector3> Velocity {
        get { return _velocity; }
    }
    public IManagedAnyRequest<Vector3> AngularVelocity {
        get { return _angularVelocity; }
    }
    public IManagedAnyRequest<float> Acceleration { 
        get { return _acceleration; }
    }
    public IManagedAnyRequest<float> MaxSpeed { 
        get { return _maxSpeed; }
    }
    public IManagedAnyRequest<float> MaxRotation {
        get { return _maxRotation; }
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
    public IManagedAnyRequest<Quaternion> Rotation {
        get { return _rotation; }
    }
    public IManagedAnyRequest<Vector3> Position {
        get { return _position; }
    }

    void Start() {
        this._baseAcceleration = acceleration;
        this._baseMaxSpeed = maxSpeed;
        this.reference = new PlayerShipRequestReference();
        this.senders = new();
        
        AnyRequestPool<Vector3> velPool = new();
        _velocity = new ManagedAnyRequestable<Vector3>(
            () => { return rb.velocity; },
            (Vector3 v) => { rb.velocity = v; },
            reference.Velocity,
            new IncreasingPriority(() => { velPool.reset(); }),
            velPool
        );

        AnyRequestPool<Vector3> angVelPool = new();
        _angularVelocity = new ManagedAnyRequestable<Vector3>(
            () => { return rb.angularVelocity; },
            (Vector3 v) => { rb.angularVelocity = v; },
            reference.AngularVelocity,
            new IncreasingPriority(() => { angVelPool.reset(); }),
            angVelPool
        );

        AnyRequestPool<float> accPool = new();
        _acceleration = new ManagedAnyRequestableValue<float>(
            acceleration, 
            reference.Acceleration, 
            new IncreasingPriority(() => { accPool.reset(); }), 
            accPool
        );

        AnyRequestPool<float> maxSpeedPool = new();    
        _maxSpeed = new ManagedAnyRequestableValue<float>(
            maxSpeed, 
            reference.MaxSpeed, 
            new IncreasingPriority(() => { maxSpeedPool.reset(); }), 
            maxSpeedPool
        );

        AnyRequestPool<float> maxRotationPool = new();
        _maxRotation = new ManagedAnyRequestableValue<float>(
            maxRotation,
            reference.MaxRotation,
            new IncreasingPriority(() => { maxRotationPool.reset(); }),
            maxRotationPool
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

        AnyRequestPool<float> magPool = new();
        _magnitude = new ManagedAnyRequestable<float>(
            () => { return rb.velocity.magnitude; }, 
            (float m) => { rb.velocity = rb.velocity.normalized * m; },
            reference.Magnitude,
            new IncreasingPriority(() => { magPool.reset(); }),
            magPool
        );

        AnyRequestPool<Quaternion> rotPool = new();
        _rotation = new ManagedAnyRequestable<Quaternion>(
            () => { return rb.transform.rotation; }, 
            (Quaternion r) => { rb.transform.rotation = r; },
            reference.Rotation,
            new IncreasingPriority(() => { rotPool.reset(); }),
            rotPool
        );

        AnyRequestPool<Vector3> posPool = new();
        _position = new ManagedAnyRequestable<Vector3>(
            () => { return rb.transform.position; },
            (Vector3 p) => { rb.transform.position = p; },
            reference.Position,
            new IncreasingPriority(() => { posPool.reset(); }),
            posPool
        );
    }

    public void executeRequests() {
        notifySenders();

        _acceleration.executeRequests();
        _maxSpeed.executeRequests();
        _force.executeRequests();
        _torque.executeRequests();
        _magnitude.executeRequests();
        _rotation.executeRequests();
        _position.executeRequests();

        onExecuted();
    }

    public void setReference(RigidbodyReference reference) {
        _acceleration.setReference(reference.Acceleration);
        _maxSpeed.setReference(reference.MaxSpeed);
        _force.setReference(reference.Force);
        _torque.setReference(reference.Torque);
        _magnitude.setReference(reference.Magnitude);
        _rotation.setReference(reference.Rotation);
        _position.setReference(reference.Position);
    }

    private void notifySenders() {
        _acceleration.addSendersTo(senders);
        _maxSpeed.addSendersTo(senders);
        _force.addSendersTo(senders);
        _torque.addSendersTo(senders);
        _magnitude.addSendersTo(senders);
        _rotation.addSendersTo(senders);
        _position.addSendersTo(senders);

        foreach(RequestSender sender in senders.Keys) {
            Debug.Log(sender);
            sender.onRequestsExecuted(senders[sender]);
        }

        senders.Clear();
    }

    private void calcNextVelocity() {
        if (impendingForce.Item2 == ForceMode.Force || impendingForce.Item2 == ForceMode.Impulse) 
            impendingForce.Item1 /= rb.mass;
        
        if (impendingForce.Item2 == ForceMode.Force || impendingForce.Item2 == ForceMode.Acceleration)
            impendingForce.Item1 *= Time.fixedDeltaTime;

        if (impendingForce.Item2 == ForceMode.VelocityChange)
            impendingVelocity = impendingForce.Item1;
        else
            impendingVelocity = rb.velocity + impendingForce.Item1;
    }

    private void onExecuted() {
        if (rb.velocity.magnitude > MaxSpeed.value)
            rb.velocity = rb.velocity.normalized * MaxSpeed.value;

        //if (rb.angularVelocity.z > MaxRotation.value * Mathf.Deg2Rad)
          //  rb.angularVelocity = new Vector3(rb.angularVelocity.x, rb.angularVelocity.y, MaxRotation.value);

        calcNextVelocity();
    }
}
