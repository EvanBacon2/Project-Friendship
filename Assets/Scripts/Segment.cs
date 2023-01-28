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
    public double inverseMass;

    public readonly Vector2d orientation;//should be a unit vector
    public Vector2d previousOrientation;
    public double angulerVelocity;//radians
    public double inverseInertia;

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

        this.inverseMass = mass;
        this.inverseInertia = inertia;

        this.length = length;
        this.halfLength = length / 2;
    }

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

    private static Vector2d real = Vector2d.zero;
	private static Vector2d complex = Vector2d.zero;

    /*
     * Rotates segment s by the rotation r given in radians
	 */
    public static void rotateOrientation(Segment s, double r) {
		double cosR = System.Math.Cos(r);
		double sinR = System.Math.Sin(r);

		real.x = cosR * s.orientation.x;
		real.y = cosR * s.orientation.y;
		complex.x = sinR * s.orientation.x;
		complex.y = sinR * s.orientation.y;

		s.setOrientation(real.x - complex.y, real.y + complex.x);
	}
}