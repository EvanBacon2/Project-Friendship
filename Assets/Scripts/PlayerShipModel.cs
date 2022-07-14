using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerShipModel : MonoBehaviour {
    public Rigidbody selfRigidBody;
    public Transform selfTransform;

    public bool boosting;

    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;

    [SerializeField] private float boostCooldown;
    [SerializeField] private float boostAccelerationMod;
    [SerializeField] private int boostMaxSpeedMod;
    public int boostLevel;
    public int maxBoostLevel;

    void Start() {
        selfRigidBody = GetComponent<Rigidbody>();
        selfTransform = GetComponent<Transform>();

        acceleration = 40;
        maxSpeed = 25;

        boostCooldown = 3.0f;
        boostAccelerationMod = 2.0f;
        boostMaxSpeedMod = 25;
        boosting = false;
        boostLevel = 0;
        maxBoostLevel = 3;
    }

    void Update() {
        /*horizontalAcceleration = Input.GetAxisRaw("Horizontal");
        verticalAcceleration = Input.GetAxisRaw("Vertical");
        mousePos = Input.mousePosition;
        playerPos = Camera.main.WorldToScreenPoint(transform.position);
        brakeOn = Input.GetKey(KeyCode.LeftShift);
        if (Input.GetKeyDown(KeyCode.Space) && !boosting && isAccelerating())
            activateBoost = true;

        Debug.Log(selfRigidBody.velocity.magnitude);*/
    }

	private void FixedUpdate() {
        //rotateToMouse();
	}

	//State view methods
	/*public bool isAccelerating() {
        return horizontalInput != 0 || verticalInput != 0;
    }*/

	public float speedLimit() {
        return maxSpeed + boostMaxSpeedMod * boostLevel;
    }

    public float accelerationForce() {
        return acceleration * (boostLevel > 0 ? boostAccelerationMod : 1.0f);
    }

    public float brakeForce() {
        return acceleration * .8f;
    }

    //State change methods
	/*public void rotateToMouse() {
        Vector3 playerPos = distort(Camera.main.WorldToViewportPoint(transform.position));
        Vector3 mouseInput = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        float turnAngle = Mathf.Atan2(mouseInput.y - playerPos.y, mouseInput.x - playerPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(turnAngle - 90, Vector3.forward);
    }

    public Vector2 distort(Vector2 uv) {
        LensDistortion fisheye;
        GameObject.Find("Fisheye").GetComponent<Volume>().profile.TryGet(out fisheye);

        float amount = 1.6f * Mathf.Max(Mathf.Abs(fisheye.intensity.value * 100), 1f);
        float theta = Mathf.Deg2Rad * Mathf.Min(160f, amount);
        float sigma = 2f * Mathf.Tan(theta * 0.5f);

        Vector4 p0 = new Vector4(fisheye.center.value.x * 2f - 1f, fisheye.center.value.y * 2f - 1f,
                                 Mathf.Max(fisheye.xMultiplier.value, 1e-4f), Mathf.Max(fisheye.xMultiplier.value, 1e-4f));
        Vector4 p1 = new Vector4((fisheye.intensity.value >= 0f ? theta : 1f / theta), sigma, 
                                  1f / fisheye.scale.value, fisheye.intensity.value * 100f);
        
        Vector2 half = Vector2.one / 2f;
        Vector2 center = fisheye.center.value * 2f - Vector2.one;

        uv = uv - half * p1.z + half;
        Vector2 ruv = new Vector2(p0.z, p0.w) * (uv - half - center);
        float ru = ruv.magnitude;

        if (p1.w > 0.0f) {
            float wu = ru * p1.x;
            ru = Mathf.Tan(wu) * (1.0f / (ru * sigma));
            uv = uv - ruv * (ru - 1.0f);
        } else {
            ru = (1.0f / ru) * p1.x * Mathf.Atan(ru * sigma);
            uv = uv - ruv * (ru - 1.0f);
        }
        
        return uv;
    }*/

    /*public void accelerateShip() {
        Vector3 movement = new Vector3(horizontalInput, verticalInput,0).normalized;
        Vector3 newVelocity = selfRigidBody.velocity + movement * accelerationForce() * Time.fixedDeltaTime;
        if (newVelocity.magnitude > speedLimit())
            newVelocity = newVelocity.normalized * speedLimit();

        selfRigidBody.velocity = newVelocity;
    }*/

    /*public void checkSpeed() {
        if (selfRigidBody.velocity.magnitude > speedLimit())
            selfRigidBody.velocity = selfRigidBody.velocity.normalized * speedLimit();
    }*/

    public void boostShip() {
        if (boostLevel < maxBoostLevel) {
            startBoost();
            StartCoroutine(endBoost());
        }
    }
    
    public void startBoost() {   
        boosting = true;
        boostLevel += 1;

        //selfRigidBody.velocity = new Vector2(horizontalInput, verticalInput).normalized * selfRigidBody.velocity.magnitude;
    }

    public IEnumerator endBoost() {
        yield return new WaitForSeconds(boostCooldown);
        boosting = false;
    }

    /*public void shaveBoostSpeed() {
        boostLevel = 0;
    }

    public void slowShip() {
        if (selfRigidBody.velocity.magnitude < acceleration * .005f)
            selfRigidBody.velocity = Vector3.zero;
        else
            selfRigidBody.AddForce(selfRigidBody.velocity.normalized * -brakeForce());
    }*/
}
