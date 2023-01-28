/*
 * An implmentation of a Vector2 that uses doubles instead of floats
 */
public class Vector2d {
    public double x;
    public double y;
    public double magnitude { get { return System.Math.Sqrt(x * x + y * y); } }
    public double sqrmagnitude { get { return x * x + y * y; } }

    public Vector2d(double x, double y) {
        this.x = x;
        this.y = y;
	}

    public static Vector2d zero { get { return new Vector2d(0, 0); } }
    public static Vector2d up { get { return new Vector2d(0, 1); } }

    public void normalize() {
        if (magnitude > 0) {
            x = x / magnitude;
            y = y / magnitude;
        }
        else {
            x = 0;
            y = 0;
        }
    }

    public static double dot(Vector2d lhs, Vector2d rhs) {
        return lhs.x * rhs.x + lhs.y * rhs.y;
    }

    public static double cross(Vector2d lhs, Vector2d rhs) {
        return lhs.x * rhs.y - lhs.y * rhs.x;
    }

    public static double SignedAngle(Vector2d from, Vector2d to) {
        double dot = Vector2d.dot(from, to);
        double cross = Vector2d.cross(from, to);
        double denominator = System.Math.Sqrt(from.sqrmagnitude * to.sqrmagnitude);
        if (denominator < 1e-15) return 0;
        double cos = System.Math.Clamp(dot / denominator, -1, 1);
        if (cross == 0) 
            return System.Math.Acos(cos);
        else
            return System.Math.Acos(cos) * System.Math.Sign(cross);
    }

    public override string ToString() {
        return "(" + x + ", " + y + ")";
    }
}
