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

    private float newMagValue = -1;
    private (Vector3, ForceMode)? newForce = null;
    private Quaternion newRotation;

    public float acceleration { get; set; }
    [SerializeField] public float maxSpeed { get; set; }
    public float magnitude {
        get { return rigidBody.velocity.magnitude; }
        set { newMagValue = value; }
    }
    public Quaternion rotation {
        get { return transform.rotation; }
        set { newRotation = value; }
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

        newRotation = transform.rotation;
    }

	public void Update() {
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 8);
	}

	public void FixedUpdate() {
        if (newForce.HasValue) {
            rigidBody.AddForce(newForce.Value.Item1, newForce.Value.Item2);
            newForce = null;
        }

        if (newMagValue >= 0) {
            rigidBody.velocity = rigidBody.velocity.normalized * newMagValue;
            newMagValue = -1;
        }

        if (magnitude > maxSpeed)
            rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
    }

	public void addForce((Vector3, ForceMode) force) {
        newForce = force;
    }
}
