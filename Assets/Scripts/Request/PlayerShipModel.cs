using UnityEngine;

public class PlayerShipModel {
    private Rigidbody rigidbody;
    public ShipModel model;

    public const float BASE_ACCELERATION = 40;
    public const float BASE_MAXSPEED = 25;

    public float acceleration { 
        get { return model.Acceleration.value; } 
    }
    public float maxSpeed { 
        get { return model.MaxSpeed.value; } 
    }
    public float magnitude {
        get { return rigidbody.velocity.magnitude; }
    }
    public Quaternion rotation {
        get { return rigidbody.transform.rotation; }
    }

    public Vector3 position {
        get { return rigidbody.transform.position; }
    }
    public Vector3 velocity {
        get { return rigidbody.velocity; }
    }

    public PlayerShipModel(Rigidbody rigidbody) {
        this.rigidbody = rigidbody;
        this.model = new ShipModel(rigidbody, new PlayerShipPriorityReference(), PlayerShipModel.BASE_ACCELERATION, PlayerShipModel.BASE_MAXSPEED);
    }

	/*public void FixedUpdate() {
        transform.rotation = Quaternion.Slerp(transform.rotation, model.Rotation.value, Time.fixedDeltaTime * 14);

        Vector2 acceleration = Vector2.zero;
        
        rigidBody.AddForce(model.Force.value.Item1, model.Force.value.Item2);

        Vector2 newForceVector = model.Force.value.Item1;
        ForceMode newForceMode = model.Force.value.Item2;
        if (newForceMode == ForceMode.Force || newForceMode == ForceMode.Acceleration)
            newForceVector *= Time.fixedDeltaTime;
        acceleration = newForceVector;

        if (model.Magnitude.value >= 0) {//////////////
            rigidBody.velocity = rigidBody.velocity.normalized * model.Magnitude.value;
        }
        
        if (magnitude > maxSpeed)
            rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;

        impendingVelocity = (Vector2)rigidBody.velocity + acceleration;
    }*/
}
