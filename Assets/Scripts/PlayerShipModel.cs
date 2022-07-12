using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipModel : MonoBehaviour {
    public Rigidbody selfRigidBody;

    public float horizontalInput;
    public float verticalInput;
    private Vector3 mousePos;
    private Vector3 playerPos;
    public bool brakeInput;
    public bool boostInput;
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

        acceleration = 40;
        maxSpeed = 25;

        boostCooldown = 3.0f;
        boostAccelerationMod = 2.0f;
        boostMaxSpeedMod = 25;
        boostInput = false;
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

    //State view methods
    public bool isAccelerating() {
        return horizontalInput != 0 || verticalInput != 0;
    }

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
	public void rotateToMouse() {
        float turnAngle = Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(turnAngle - 90, Vector3.forward);
    }

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
        boostInput = false;
        boosting = true;
        boostLevel += 1;

        selfRigidBody.velocity = new Vector2(horizontalInput, verticalInput).normalized * selfRigidBody.velocity.magnitude;
    }

    public IEnumerator endBoost() {
        yield return new WaitForSeconds(boostCooldown);
        boosting = false;
    }

    public void shaveBoostSpeed() {
        boostLevel = 0;
    }

    public void slowShip() {
        if (selfRigidBody.velocity.magnitude < acceleration * .005f)
            selfRigidBody.velocity = Vector3.zero;
        else
            selfRigidBody.AddForce(selfRigidBody.velocity.normalized * -brakeForce());
    }
}
