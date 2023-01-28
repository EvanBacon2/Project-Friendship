using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LookAtMouseRequest : RequestSystem<ShipState> {
    private RECSRigidBody model;

    //public float maxAngleDiff = 20;
    private float rotationDrive = 14;

    public LookAtMouseRequest(RECSRigidBody model) {
        this.model = model;
    }

    public void OnStateReceived(object sender, ShipState state) {
        Vector2 mousePos = state.lookInput;
        Vector2 playerPos = Camera.main.ViewportToScreenPoint(distort(Camera.main.WorldToViewportPoint(state.position)));
        
        Quaternion rotationGoal = lookAtMouse(mousePos, playerPos);
        Quaternion rotationStep = Quaternion.Slerp(model.Rotation.value, rotationGoal, Time.fixedDeltaTime * rotationDrive);
        
        model.Rotation.set(RequestClass.LookAtMouse, rotationStep);
    }

    public void onRequestsExecuted(HashSet<Guid> executedRequests) {
    
    }

    private Quaternion lookAtMouse(Vector3 mouseInput, Vector3 playerPos) {
        float lookAngle = Mathf.Atan2(mouseInput.y - playerPos.y, mouseInput.x - playerPos.x) * Mathf.Rad2Deg - 90;
        return Quaternion.AngleAxis(lookAngle, Vector3.forward);
    }

    /*
     * Adjusts viewport coordinates of playerShip to account for LensDistortion
     */
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
        Vector2 center = new Vector2(p0.x, p0.y);

        uv = uv - half * p1.z + half;
        Vector2 ruv = new Vector2(p0.z, p0.w) * (uv - half - center);
        float ru = ruv.magnitude;

        if (p1.w > 0.0f) {
            float wu = ru * p1.x;
            ru = Mathf.Tan(wu) * (1.0f / (ru * sigma));
            uv = uv - ruv * (ru - 1.0f);
        }
        else {
            ru = (1.0f / ru) * p1.x * Mathf.Atan(ru * sigma);
            uv = uv - ruv * (ru - 1.0f);
        }

        return uv;
    }
}
