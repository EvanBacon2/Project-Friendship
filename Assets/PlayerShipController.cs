using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipController : MonoBehaviour {
    /*private Rigidbody selfRigidBody;

    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;

    [SerializeField] private float boostCooldown;
    [SerializeField] private float boostAccelerationMod;
    [SerializeField] private int boostMaxSpeedMod;
    private bool boostOn;
    private int boostLevel;*/

    PlayerShipModel shipModel;

    void Start() {
        /*selfRigidBody = GetComponent<Rigidbody>();

        acceleration = 40;
        maxSpeed = 25;

        boostCooldown = 3.0f;
        boostAccelerationMod = 2.0f;
        boostMaxSpeedMod = 5;
        boostOn = false;
        boostLevel = 0;*/
        shipModel = GetComponent<PlayerShipModel>();
    }

    void Update() {
        
    }

	private void FixedUpdate() {
        /*if (Input.GetKey(KeyCode.LeftShift))
            slowShip();
        else {
            if (Input.GetKeyDown(KeyCode.Space) && !boostOn)
                boostShip();
            accelerateShip();
        }

        rotateToMouse();*/

        if (shipModel.brakeOn) {
            shipModel.slowShip();
        } else {
            if (shipModel.isAccelerating()) {
                if (shipModel.activateBoost)
                    shipModel.boostShip();
                shipModel.accelerateShip();
            }
            else {
                
            }
        }

        shipModel.rotateToMouse();
    }

	/*private void rotateToMouse() {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerPos = Camera.main.WorldToScreenPoint(transform.position);
        float turnAngle = Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(turnAngle - 90, Vector3.forward);
    }

    private void accelerateShip() {
        Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        selfRigidBody.AddForce(movement.normalized * acceleration * (boostLevel > 1 ? boostAccelerationMod : 1.0f));
        if (selfRigidBody.velocity.magnitude > maxSpeed + boostMaxSpeedMod * boostLevel)
            selfRigidBody.velocity = selfRigidBody.velocity.normalized * (maxSpeed + boostMaxSpeedMod * boostLevel);
    }

    private void boostShip() {
        startBoost();
        StartCoroutine(endBoost());
    }

    private void startBoost() {
        boostOn = true;
        boostLevel += 1;
    }

    private IEnumerator endBoost() {
        yield return new WaitForSeconds(boostCooldown);
        boostOn = false;
    }

    private void slowShip()
    {
        if (selfRigidBody.velocity.magnitude < acceleration * .005f)
            selfRigidBody.velocity = Vector3.zero;
        else
            selfRigidBody.AddForce(selfRigidBody.velocity.normalized * -acceleration * .8f);
    }*/
}
