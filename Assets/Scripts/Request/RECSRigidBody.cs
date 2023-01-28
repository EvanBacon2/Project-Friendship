using System;
using System.Collections.Generic;
using UnityEngine;

public class RECSRigidBody {
    private Rigidbody rb;
    private Dictionary<RequestSender, HashSet<Guid>> senders;

    private ManagedAnyRequestableValue<float> _acceleration;
    private ManagedAnyRequestableValue<float> _maxSpeed;
    private ManagedAnyRequestable<(Vector3, ForceMode)> _force;
    private ManagedAnyRequestable<float> _magnitude;
    private ManagedAnyRequestable<Quaternion> _rotation;
    private ManagedAnyRequestable<Vector3> _position;

    public static Vector2 impendingVelocity = Vector2.zero;//what the velocity of the ship will be after FixedUpdate has run
    private Vector2 forceAcceleration = Vector2.zero;
 
    public IAnyRequest<float> Acceleration { 
        get { return _acceleration; }
    }
    public IAnyRequest<float> MaxSpeed { 
        get { return _maxSpeed; }
    }
    public IAnyRequest<(Vector3, ForceMode)> Force {
        get { return _force; }
    }
    public IAnyRequest<float> Magnitude {
        get { return _magnitude; }
    }
    public IAnyRequest<Quaternion> Rotation {
        get { return _rotation; }
    }

    public Vector3 Position {
        get { return rb.position; }
    }

    public RECSRigidBody(Rigidbody rb, ShipReference reference, float acceleration = 0, float maxSpeed = 0) {
        this.rb = rb;
        this.senders = new();
        
        AnyRequestPool<float> accPool = new();
        _acceleration = new ManagedAnyRequestableValue<float>(
            acceleration, 
            reference.Acceleration, 
            new IncreasingPriority(-1, () => { accPool.reset(); }), 
            accPool
        );

        AnyRequestPool<float> maxPool = new();    
        _maxSpeed = new ManagedAnyRequestableValue<float>(
            maxSpeed, 
            reference.MaxSpeed, 
            new IncreasingPriority(-1, () => { maxPool.reset(); }), 
            maxPool
        );

        AnyRequestPool<(Vector3, ForceMode)> forcePool = new();
        _force = new ManagedAnyRequestable<(Vector3, ForceMode)>(
            () => { return (Vector3.zero, ForceMode.Force); }, 
            ((Vector3, ForceMode) v) => { 
                rb.AddForce(v.Item1, v.Item2);

                if (v.Item2 == ForceMode.Force || v.Item2 == ForceMode.Acceleration)
                    v.Item1 *= Time.fixedDeltaTime;
                forceAcceleration = v.Item1;
            },
            reference.Force,
            new IncreasingPriority(-1, () => { forcePool.reset(); }),
            forcePool
        );

        AnyRequestPool<float> magPool = new();
        _magnitude = new ManagedAnyRequestable<float>(
            () => { return rb.velocity.magnitude; }, 
            (float m) => { rb.velocity = rb.velocity.normalized * m; },
            reference.Magnitude,
            new IncreasingPriority(-1, () => { magPool.reset(); }),
            magPool
        );

        AnyRequestPool<Quaternion> rotPool = new();
        _rotation = new ManagedAnyRequestable<Quaternion>(
            () => { return rb.transform.rotation; }, 
            (Quaternion r) => { rb.transform.rotation = r; },
            reference.Rotation,
            new IncreasingPriority(-1, () => { rotPool.reset(); }),
            rotPool
        );

        AnyRequestPool<Vector3> posPool = new();
        _position = new ManagedAnyRequestable<Vector3>(
            () => { return rb.transform.position; },
            (Vector3 p) => { rb.transform.position = p; },
            reference.Position,
            new IncreasingPriority(-1, () => { posPool.reset(); }),
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

    public void setReference(ShipReference reference) {
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
            sender.onRequestsExecuted(senders[sender]);
        }

        senders.Clear();
    }

    private void onExecuted() {
        if (rb.velocity.magnitude > MaxSpeed.value)
            rb.velocity = rb.velocity.normalized * MaxSpeed.value;

        impendingVelocity = (Vector2)rb.velocity + forceAcceleration;
    }
}
