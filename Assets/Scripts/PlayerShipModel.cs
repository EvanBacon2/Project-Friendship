using UnityEngine;

public class PlayerShipModel : MonoBehaviour {
    private Rigidbody rigidBody;

    public const float baseAcceleration = 40;
    public const float baseMaxSpeed = 25;

    public float acceleration;
    public float maxSpeed;
    public Quaternion rotation {
        get { return transform.rotation; }
        set { transform.rotation = value; }
    }

    public Vector3 position {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public Vector3 velocity {
        get { return rigidBody.velocity; }
        set { rigidBody.velocity = value; }
    }

    void Start() {
        rigidBody = GetComponent<Rigidbody>();

        acceleration = baseAcceleration;
        maxSpeed = baseMaxSpeed;
    }

    public void addForce(Vector3 force, ForceMode mode) {
        rigidBody.AddForce(force, mode);
        if (rigidBody.velocity.magnitude > maxSpeed)
            rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
    }
}
