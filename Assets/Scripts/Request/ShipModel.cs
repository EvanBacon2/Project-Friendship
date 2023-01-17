using UnityEngine;

public class ShipModel {
    private Rigidbody rb;

    private RequestableValue<float> _acceleration;
    private RequestableValue<float> _maxSpeed;
    private RequestableValue<(Vector3, ForceMode)> _force;
    private RequestableValue<float> _magnitude;
    private RequestableValue<Quaternion> _rotation;

    public static Vector2 impendingVelocity = Vector2.zero;//what the velocity of the ship will be after FixedUpdate has run
    private Vector2 forceAcceleration = Vector2.zero;
 
    public Requestable<float> Acceleration { 
        get { return _acceleration; }
    }
    public Requestable<float> MaxSpeed { 
        get { return _maxSpeed; }
    }
    public Requestable<(Vector3, ForceMode)> Force {
        get { return _force; }
    }
    public Requestable<float> Magnitude {
        get { return _magnitude; }
    }
    public Requestable<Quaternion> Rotation {
        get { return _rotation; }
    }

    public ShipModel(Rigidbody rb, ShipReference reference, float acceleration = 0, float maxSpeed = 0) {
        this.rb = rb;

        _acceleration = new RequestableValue<float>(acceleration, reference.Acceleration);
        _maxSpeed = new RequestableValue<float>(maxSpeed, reference.MaxSpeed);

        _force = new RequestableValue<(Vector3, ForceMode)>((Vector3.zero, ForceMode.Force), reference.Force, 
                ((Vector3, ForceMode) v) => { 
                    rb.AddForce(v.Item1, v.Item2);

                    if (v.Item2 == ForceMode.Force || v.Item2 == ForceMode.Acceleration)
                         v.Item1 *= Time.fixedDeltaTime;
                    forceAcceleration = v.Item1;
                });

        _magnitude = new RequestableValue<float>(rb.velocity.magnitude, reference.Magnitude, 
                (float m) => { 
                    rb.velocity = rb.velocity.normalized * m; 
                });

        _rotation = new RequestableValue<Quaternion>(rb.transform.rotation, reference.Rotation,
                (Quaternion r) => { 
                    rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, r, Time.fixedDeltaTime * 14); 
                });
    }

    public void executeRequests() {
        _acceleration.executeRequests();
        _maxSpeed.executeRequests();
        _force.executeRequests();
        _magnitude.executeRequests();
        _rotation.executeRequests();

        onExecuted();
    }

    public void setReference(ShipReference reference) {
        _acceleration.setReference(reference.Acceleration);
        _maxSpeed.setReference(reference.MaxSpeed);
        _force.setReference(reference.Force);
        _magnitude.setReference(reference.Magnitude);
        _rotation.setReference(reference.Rotation);
    }

    private void onExecuted() {
        if (rb.velocity.magnitude > MaxSpeed.value)
            rb.velocity = rb.velocity.normalized * MaxSpeed.value;

        impendingVelocity = (Vector2)rb.velocity + forceAcceleration;
    }
}
