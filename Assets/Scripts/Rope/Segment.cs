using UnityEngine;

/*
* A line segment representing a single link of rope.
*/
public class Segment {
    private Vector3 spritePos = new Vector3(0, 0, 0);

    public readonly Vector2d p1;
    public readonly Vector2d p2;

    public readonly Vector2d position;
    public Vector2d previousPosition;
    public Vector2d velocity;

    private double _mass;
    private double _inverseMass;
    public double mass { 
        get { return _mass; }
        set { 
            _mass = value;
            _inverseMass = value == 0 ? double.PositiveInfinity : 1 / value;
        }
    }
    public double inverseMass {
        get { return _inverseMass; }
        set {
            _mass = value == 0 ? double.PositiveInfinity : 1 / value;
            _inverseMass = value;
        }
    }

    public readonly Vector2d orientation;//should be a unit vector
    public Vector2d previousOrientation;
    public double angulerVelocity;//radians
    private double _inertia;
    private double _inverseInertia;
    public double inertia {
        get { return _inertia; }
        set { 
            _inertia = value;
            _inverseInertia = value == 0 ? double.PositiveInfinity : 1 / value;
        }
    }
    public double inverseInertia {
        get { return _inverseInertia; }
        set {
            _inertia = value == 0 ? double.PositiveInfinity : 1 / value;
            _inverseInertia = value;
        }
    }

    public double length;
    private double halfLength;

    public Segment(Vector2d position, Vector2d orientation, double mass, double inertia, double length) {
        this.p1 = new Vector2d(position.x - orientation.x * halfLength, position.y - orientation.y * halfLength);
        this.p2 = new Vector2d(position.x + orientation.x * halfLength, position.y + orientation.y * halfLength);

        this.position = new Vector2d(position.x, position.y);
        this.previousPosition = new Vector2d(position.x, position.y);
        this.velocity = new Vector2d(0, 0);

        this.orientation = new Vector2d(orientation.x, orientation.y);
        this.previousOrientation = new Vector2d(orientation.x, orientation.y);
        this.angulerVelocity = 0;

        this.mass = mass;
        this.inverseMass = 1 / mass;
        this.inertia = inertia;
        this.inverseInertia = 1 / inertia;

        this.length = length;
        this.halfLength = length / 2;
    }

    public Segment(Vector2d position, Vector2d orientation, double length) : this(position, orientation, 1, 1, length) {}

    /*public void updateSprite() {
        spritePos.x = (float)position.x;
        spritePos.y = (float)position.y;
        sprite.transform.position = spritePos;

        float angle = Mathf.Atan2((float)orientation.y, (float)orientation.x) * Mathf.Rad2Deg - 90;
        sprite.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }*/

    public void setPosition(double x, double y) {
        position.x = x;
        position.y = y;
        p1.x = x - orientation.x * length;
        p1.y = y - orientation.y * length;
        p2.x = x + orientation.x * length;
        p2.y = y + orientation.y * length;
    }

    public void setP1(double x, double y) {
        position.x = x + orientation.x * halfLength;
        position.y = y + orientation.y * halfLength;
        p1.x = x;
        p1.y = y;
        p2.x = x + orientation.x * length;
        p2.y = y + orientation.y * length;
    }

    public void setP2(double x, double y) {
        position.x = x - orientation.x * halfLength;
        position.y = y - orientation.y * halfLength;
        p1.x = x - orientation.x * length;
        p1.y = y - orientation.y * length;
        p2.x = x;
        p2.y = y;
    }

    public void setOrientation(double x, double y) {
        orientation.x = x;
        orientation.y = y;
        orientation.normalize();
        p1.x = position.x - orientation.x * halfLength;
        p1.y = position.y - orientation.y * halfLength;
        p2.x = position.x + orientation.x * halfLength;
        p2.y = position.y + orientation.y * halfLength;
    }

    /*
     * Set's p2 position to the given coordinates without changing p1's position
     */
    public void stretchP2(double x, double y) {
        p2.x = x;
        p2.y = y;
        orientation.x = p2.x - p1.x;
        orientation.y = p2.y - p1.y;
        length = orientation.magnitude;
        halfLength = length / 2;
        orientation.normalize();
        position.x = p1.x + orientation.x * halfLength;
        position.y = p1.y + orientation.y * halfLength;
    }

    private static Vector2d real = Vector2d.zero;
	private static Vector2d complex = Vector2d.zero;

    /*
     * Rotates segment s by the rotation r given in radians
	 */
    public static void rotate(Segment s, double r) {
		double cosR = System.Math.Cos(r);
		double sinR = System.Math.Sin(r);

		real.x = cosR * s.orientation.x;
		real.y = cosR * s.orientation.y;
		complex.x = sinR * s.orientation.x;
		complex.y = sinR * s.orientation.y;
		s.setOrientation(real.x - complex.y, real.y + complex.x);
	}

    private static Vector2d oldP1 = Vector2d.zero;

    public static void rotateAroundP1(Segment s, double r) {
        oldP1.x = s.p1.x;
        oldP1.y = s.p1.y;
        rotate(s, r);
        s.setP1(oldP1.x, oldP1.y);
    }
}