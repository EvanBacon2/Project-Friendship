using UnityEngine;

public class PlayerShipProperties {
	public const string Acceleration = "acceleration";
	public const string MaxSpeed = "maxSpeed";
    public const string Force = "force";
    public const string Magnitude = "magnitude";
    public const string Rotation = "rotation";
}

public class PlayerShipModel : MonoBehaviour {
    private Rigidbody rigidBody;

    public const float baseAcceleration = 40;
    public const float baseMaxSpeed = 25;

    public float acceleration { get; set; }
    [SerializeField] public float maxSpeed { get; set; }
    public float magnitude {
        get { return rigidBody.velocity.magnitude; }
        set { rigidBody.velocity = rigidBody.velocity.normalized * value; }
    }
    public Quaternion rotation {
        get { return transform.rotation; }
        set { transform.rotation = value; }
    }

    public Vector3 position {
        get { return transform.position; }
    }
    public Vector3 velocity {
        get { return rigidBody.velocity; }
    }

    void Start() {
        rigidBody = GetComponent<Rigidbody>();

        acceleration = baseAcceleration;
        maxSpeed = baseMaxSpeed;
    }

	public void FixedUpdate() {
        if (magnitude > maxSpeed)
            magnitude = maxSpeed;
    }

	public void addForce((Vector3, ForceMode) force) {
        rigidBody.AddForce(force.Item1, force.Item2);
        if (magnitude > maxSpeed)
            magnitude = maxSpeed;
    }
}
