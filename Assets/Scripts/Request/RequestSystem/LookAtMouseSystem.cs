using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LookAtMouseSystem : RequestSystem<ShipState> {
    private RECSShipbody rb;
    private float responsiveness = 15;

    public override void OnStateReceived(object sender, ShipState state) {
        rb = state.rigidbody;

        Vector2 mousePos = state.lookDirection;
        Vector2 playerPos = Camera.main.ViewportToScreenPoint(distort(Camera.main.WorldToViewportPoint(rb.Position.pendingValue())));
        
        float pendingRotation = rb.Rotation.pendingValue().eulerAngles.z + 
                                rb.AngularVelocity.pendingValue().z * Time.fixedDeltaTime; 
        float goalRotation = Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x) * Mathf.Rad2Deg - 90;
        
        if (pendingRotation < 0) 
            pendingRotation += 360;
        if (goalRotation < 0) 
            goalRotation += 360;

        float angleGap = goalRotation - pendingRotation;
        if (Mathf.Abs(angleGap) > 180)
            angleGap = -(Mathf.Sign(angleGap) * 360 - angleGap);
        
        float targetVelocity = Mathf.Clamp(angleGap * responsiveness, 
                                -rb.AngularMax.pendingValue(), rb.AngularMax.pendingValue());
        float currentVelocity = rb.AngularVelocity.pendingValue().z * Mathf.Rad2Deg;
        float velocityChange = targetVelocity - currentVelocity;
        
        velocityChange = Mathf.Clamp(velocityChange, 
                                    -rb.AngularAcceleration.pendingValue(), 
                                    rb.AngularAcceleration.pendingValue()) * Mathf.Deg2Rad;

        rb.Torque.mutate(RequestClass.LookAtMouse, (List<(Vector3, ForceMode)> torques) => {
            torques.Add((new Vector3(0, 0, velocityChange), ForceMode.VelocityChange));
            return torques;
        });
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
