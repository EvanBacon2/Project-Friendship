using System;
using System.Collections.Generic;
using UnityEngine;

public class RECSRigidbody : MonoBehaviour {
    public Rigidbody rb;
    public float acceleration;
    public float maxSpeed;

    private float _baseAcceleration;
    private float _baseMaxSpeed;
    public float BASE_ACCELERATION { get { return _baseAcceleration; } }
    public float BASE_MAXSPEED { get { return _baseMaxSpeed; } }

    public RigidbodyReference reference;
    
    private Dictionary<RequestSender, HashSet<Guid>> senders;

    private ManagedAnyRequestable<Vector3> _velocity;
    private ManagedAnyRequestableValue<float> _acceleration;
    private ManagedAnyRequestableValue<float> _maxSpeed;
    private ManagedAnyRequestable<(Vector3, ForceMode)> _force;
    private ManagedAnyRequestable<float> _magnitude;
    private ManagedAnyRequestable<Quaternion> _rotation;
    private ManagedAnyRequestable<Vector3> _position;

    public static Vector2 impendingVelocity = Vector2.zero;//what the velocity of the ship will be after FixedUpdate has run
    private (Vector3, ForceMode) impendingForce = (Vector3.zero, ForceMode.Force);
 
    public IManagedAnyRequest<Vector3> Velocity {
        get { return _velocity; }
    }
    public IManagedAnyRequest<float> Acceleration { 
        get { return _acceleration; }
    }
    public IManagedAnyRequest<float> MaxSpeed { 
        get { return _maxSpeed; }
    }
    public IManagedAnyRequest<(Vector3, ForceMode)> Force {
        get { return _force; }
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
            (Vector3 v) => {rb.velocity = v; },
            reference.Velocity,
            new IncreasingPriority(() => { velPool.reset(); }),
            velPool
        );

        AnyRequestPool<float> accPool = new();
        _acceleration = new ManagedAnyRequestableValue<float>(
            acceleration, 
            reference.Acceleration, 
            new IncreasingPriority(() => { accPool.reset(); }), 
            accPool
        );

        AnyRequestPool<float> maxPool = new();    
        _maxSpeed = new ManagedAnyRequestableValue<float>(
            maxSpeed, 
            reference.MaxSpeed, 
            new IncreasingPriority(() => { maxPool.reset(); }), 
            maxPool
        );

        AnyRequestPool<(Vector3, ForceMode)> forcePool = new();
        _force = new ManagedAnyRequestable<(Vector3, ForceMode)>(
            () => { return (Vector3.zero, ForceMode.Force); }, 
            ((Vector3, ForceMode) v) => { 
                rb.AddForce(v.Item1, v.Item2);

                //if (v.Item2 == ForceMode.Force || v.Item2 == ForceMode.Acceleration)
                //    v.Item1 *= Time.fixedDeltaTime;
                //forceAcceleration = v.Item1;
                impendingForce = v;
            },
            reference.Force,
            new IncreasingPriority(() => { forcePool.reset(); }),
            forcePool
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
        _magnitude.executeRequests();
        _rotation.executeRequests();
        _position.executeRequests();

        onExecuted();
    }

    public void setReference(RigidbodyReference reference) {
        _acceleration.setReference(reference.Acceleration);
        _maxSpeed.setReference(reference.MaxSpeed);
        _force.setReference(reference.Force);
        _magnitude.setReference(reference.Magnitude);
        _rotation.setReference(reference.Rotation);
        _position.setReference(reference.Position);
    }

    private void notifySenders() {
        _acceleration.addSendersTo(senders);
        _maxSpeed.addSendersTo(senders);
        _force.addSendersTo(senders);
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

        calcNextVelocity();
    }
}
