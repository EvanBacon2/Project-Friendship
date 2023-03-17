using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LookAtMouseSystem : RequestSystem<ShipState> {
    private RECSShipbody rb;
    private float responsiveness = 15;

    private ScaledArc shipMouseDiff = new ScaledArc(3 * 360);
    private Vector3 startPoint = new Vector3();
    private Vector3 endPoint = new Vector3();

    private Color[] lineColors =  {Color.white, Color.cyan, Color.green};

    public override void OnStateReceived(object sender, ShipState state) {
        rb = state.rigidbody;

        Vector3 rbPos = rb.Position.pendingValue();
        float rbAngV = rb.AngularVelocity.pendingValue().z;
        float rbAngA = rb.AngularAcceleration.pendingValue();
        float rbAngM = rb.AngularMax.pendingValue();

        Vector2 mousePos = state.lookDirection;
        Vector2 playerPos = Camera.main.ViewportToScreenPoint(distort(Camera.main.WorldToViewportPoint(rbPos)));
        Vector3 cameraPos = Camera.main.transform.position;
        
        float shipAngle = rb.Rotation.pendingValue().eulerAngles.z + rbAngV * Time.fixedDeltaTime;
        float mouseAngle = Mathf.Atan2(mousePos.y - playerPos.y, mousePos.x - playerPos.x) * Mathf.Rad2Deg - 90;

        if (mouseAngle < 0) 
            mouseAngle += 360;

        shipMouseDiff.setStart(shipAngle);
        shipMouseDiff.setEnd(mouseAngle);

        float angSign = Mathf.Sign(shipMouseDiff.arc);
        
        float targetVelocity = Mathf.Clamp(shipMouseDiff.arc * responsiveness, -rbAngM, rbAngM);
        float currentVelocity = rbAngV * Mathf.Rad2Deg;
        float velocityChange = targetVelocity - currentVelocity;
        
        velocityChange = Mathf.Clamp(velocityChange, -rbAngA, rbAngA) * Mathf.Deg2Rad;

        rb.Torque.mutate(RequestClass.LookAtMouse, (List<(Vector3, ForceMode)> torques) => {
            torques.Add((new Vector3(0, 0, velocityChange), ForceMode.VelocityChange));
            return torques;
        });

        for (int i = 0; i < Mathf.RoundToInt(Mathf.Abs(shipMouseDiff.arc)); i++) {
            int ring = Mathf.Min(2, (int)Mathf.Floor(i / 360.0f));
            float radius = (5 + (ring * 1));
            Color lineColor = angSign < 0 ? Color.red : Color.cyan;

            float ang = shipMouseDiff.start + (i * angSign) + 90;

            startPoint.x = rb.Position.value.x + Mathf.Cos(ang * Mathf.Deg2Rad) * radius;
            startPoint.y = rb.Position.value.y + Mathf.Sin(ang * Mathf.Deg2Rad) * radius;

            endPoint.x = rb.Position.value.x + Mathf.Cos((ang + angSign) * Mathf.Deg2Rad) * radius;
            endPoint.y = rb.Position.value.y + Mathf.Sin((ang + angSign) * Mathf.Deg2Rad) * radius;

            Debug.DrawLine(startPoint, endPoint, lineColor, Time.fixedDeltaTime);
        }
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

public class ScaledArc {
    private float _start;
    private float _end;
    private float _arc;

    private float max;

    public float start {
        get { return _start; }
    }

    public float end {
        get { return _end; }
    }

    public float arc { 
        get { 
            return Mathf.Clamp(_arc, -max, max);
        }
    }

    public ScaledArc(float max) {
        this._start = 0;
        this._end = 0;
        this._arc = 0;

        this.max = max;
    }

    public void setStart(float newStart) {
        setArc(_arc - calculateRange(start, newStart));
        _start = newStart;
    }

    public void setEnd(float newEnd) {
        setArc(_arc + calculateRange(end, newEnd));
        _end = newEnd;
    }

    private void setArc(float newArc) {
        _arc = newArc;

        if (_arc > max + 360)
            _arc -= 360;
        
        if (_arc < -max - 360)
            _arc += 360;
    }

    private float calculateRange(float start, float end) {
        float diff = end - start;
        float invDiff;

        if (diff <= 0)
            invDiff = 360 + diff;
        else 
            invDiff = diff - 360;

        if (Mathf.Abs(diff) < Mathf.Abs(invDiff)) 
            return diff;
        else
            return invDiff;
    }
}
