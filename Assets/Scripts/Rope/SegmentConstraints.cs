using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A collection of commenly used segment constraints
 */
public class SegmentConstraint {

    private static Vector2d pointDiff = Vector2d.zero;
	private static Vector2d point_r = Vector2d.zero;

    /*
	 * A constraint which constrains a segment to a point with infinite mass/inertia.
	 */
	public static void pointConstraint(Vector2d point, Segment segment, bool point1) {
		if (point1) {
			pointDiff.x = point.x - segment.p1.x;
			pointDiff.y = point.y - segment.p1.y;
			point_r.x = segment.p1.x - segment.position.x;
			point_r.y = segment.p1.y - segment.position.y;
		} else {
			pointDiff.x = point.x - segment.p2.x;
			pointDiff.y = point.y - segment.p2.y;
			point_r.x = segment.p2.x - segment.position.x;
			point_r.y = segment.p2.y - segment.position.y;
		}
		
		double torque = Vector2d.cross(point_r, pointDiff);

		if (point1) 
			segment.setP1(segment.p1.x + pointDiff.x, segment.p1.y + pointDiff.y);
		else 
			segment.setP2(segment.p2.x + pointDiff.x, segment.p2.y + pointDiff.y);

		Segment.rotate(segment, torque);
	}

    /*
	 * Constrains the mutual orientation between two segemnts
	 */
	public static void angleConstraint(Segment s1, Segment s2, double angleLimitR) {
		double angle = Vector2d.SignedAngle(s1.orientation, s2.orientation);
		double ratio = s1.inverseInertia / (s1.inverseInertia + s2.inverseInertia);

		if (System.Math.Abs(angle) > angleLimitR) {		
			double difference = angle - (angle > 0 ? angleLimitR : -angleLimitR);
			Segment.rotate(s1, difference * ratio);
			Segment.rotate(s2, -difference * (1 - ratio));
		} 
	}

    private static Vector2d s1p2 = Vector2d.zero;
	private static Vector2d s2p1 = Vector2d.zero;

	private static Vector2d direction = Vector2d.zero;

	private static Vector2d r1 = Vector2d.zero;
	private static Vector2d r2 = Vector2d.zero;

	private static Vector2d correction1 = Vector2d.zero;
	private static Vector2d correction2 = Vector2d.zero;

	/*
	 * Constrains the distance between two segments
	 */
	public static void distanceConstraint(Segment s1, Segment s2) {
		s1p2.x = s1.p2.x;
		s1p2.y = s1.p2.y;

		s2p1.x = s2.p1.x;
		s2p1.y = s2.p1.y;

		//direction of gap between the ends of each segment
		direction.x = s2p1.x - s1p2.x;
		direction.y = s2p1.y - s1p2.y;

		//calculate radius
		r1.x = s1p2.x - s1.position.x;
		r1.y = s1p2.y - s1.position.y;

		r2.x = s2p1.x - s2.position.x;
		r2.y = s2p1.y - s2.position.y;

		double inverseMass1 = s1.inverseMass + System.Math.Pow(Vector2d.cross(r1, direction), 2) * s1.inverseInertia;
		double inverseMass2 = s2.inverseMass + System.Math.Pow(Vector2d.cross(r2, direction), 2) * s2.inverseInertia;
		double ratio = inverseMass1 / (inverseMass1 + inverseMass2);
		
		correction1.x = direction.x * ratio;
		correction1.y = direction.y * ratio;

		correction2.x = direction.x * (1 - ratio);
		correction2.y = direction.y * (1 - ratio);

		double torque1 = Vector2d.cross(r1, correction1);
		double torque2 = Vector2d.cross(r2, correction2);

		s1.setP2(s1.p2.x + correction1.x, s1.p2.y + correction1.y);
		s2.setP1(s2.p1.x - correction2.x, s2.p1.y - correction2.y);

		Segment.rotate(s1, torque1 * ratio);
		Segment.rotate(s2, -torque2 * (1 - ratio));
	}

	public static void dostanceConstraint(Segment s1, Segment s2) {
		s1p2.x = s1.p2.x;
		s1p2.y = s1.p2.y;

		s2p1.x = s2.p1.x;
		s2p1.y = s2.p1.y;

		//direction of gap between the ends of each segment
		direction.x = s2p1.x - s1p2.x;
		direction.y = s2p1.y - s1p2.y;

		//calculate radius
		r1.x = s1p2.x - s1.position.x;
		r1.y = s1p2.y - s1.position.y;

		r2.x = s2p1.x - s2.position.x;
		r2.y = s2p1.y - s2.position.y;

		double inverseMass1 = s1.inverseMass + System.Math.Pow(Vector2d.cross(r1, direction), 2) * s1.inverseInertia;
		double inverseMass2 = s2.inverseMass + System.Math.Pow(Vector2d.cross(r2, direction), 2) * s2.inverseInertia;
		double ratio = inverseMass1 / (inverseMass1 + inverseMass2);
		
		correction1.x = direction.x * ratio;
		correction1.y = direction.y * ratio;

		correction2.x = direction.x * (1 - ratio);
		correction2.y = direction.y * (1 - ratio);

		double torque1 = Vector2d.cross(r1, correction1);
		double torque2 = Vector2d.cross(r2, correction2);

		s1.setP2(s1.p2.x + correction1.x, s1.p2.y + correction1.y);
		s2.setP1(s2.p1.x - correction2.x, s2.p1.y - correction2.y);

		//Segment.rotate(s1, torque1 * ratio);
		//Segment.rotate(s2, -torque2 * (1 - ratio));
	}
}
