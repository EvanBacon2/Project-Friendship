using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerShipModel : MonoBehaviour {
    public Rigidbody selfRigidBody;
    public Transform selfTransform;

    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;

    [SerializeField] private float boostCooldown;
    [SerializeField] private float boostAccelerationMod;
    [SerializeField] private int boostMaxSpeedMod;

    public int boostLevel;
    public int maxBoostLevel;
    public bool boosting;

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

	public float speedLimit() {
        return maxSpeed + boostMaxSpeedMod * boostLevel;
    }

    public float accelerationForce() {
        return acceleration * (boostLevel > 0 ? boostAccelerationMod : 1.0f);
    }

    public float brakeForce() {
        return acceleration * .8f;
    }

    public void boostShip() {
        if (boostLevel < maxBoostLevel) {
            startBoost();
            StartCoroutine(endBoost());
        }
    }
    
    public void startBoost() {   
        boosting = true;
        boostLevel += 1;
    }

    public IEnumerator endBoost() {
        yield return new WaitForSeconds(boostCooldown);
        boosting = false;
    }
}
