using UnityEngine;

public class Rope : MonoBehaviour, RopeBehaviour {
	public int substeps = 1;//Number of simulation steps taken every fixed time step.
    [SerializeField] protected double _angleLimit = 360;//Max difference, in degrees, between the orientations, in relation to the origin, of two segments
	[SerializeField] protected double _linearDrag = 1;
	[SerializeField] protected double _angulerDrag = 1;
	public double stiffness = 1;//Complience of angle constraint
	public double stretchiness = 1;//Complience of distance constraint
	public double maxSpeed = 1;//Max speed of baseSegment, maxSpeed for each subsequent segment increases by a factor determined by maxSpeedScale
	public double maxSpeedScale = 1;//1 == linear, >1 == exponential, <1 == logarithmic
	public bool uniformSegments = false;//Set to true if all segments are the same length, the validity of this value is not checked

	public double angleLimitDegrees {
		get { return _angleLimit; }
		set { 
			_angleLimit = value; 
			_angleLimitRadians = value * Mathf.Deg2Rad;
		}
	}
	public double angleLimitRadians {
		get { return _angleLimitRadians; }
		set { 
			_angleLimitRadians = value; 
			_angleLimit = value * Mathf.Rad2Deg;
		}
	}
	public double linearDrag {
		get { return _linearDrag; }
		set { 
			_linearDrag = value; 
			_subLinearDrag = System.Math.Pow(value, substeps);		
		}
	}
	public double angulerDrag {
		get { return _angulerDrag; }
		set { 
			_angulerDrag = value; 
			_subAngulerDrag = System.Math.Pow(value, substeps);
		}
	}
	public double subLinearDrag { get { return _subLinearDrag; } }
	public double subAngulerDrag { get { return _subAngulerDrag; } }
	public double h { get { return _h; } }

	private double _angleLimitRadians;
	private double _subLinearDrag;
	private double _subAngulerDrag;
	private double _h;

	public Segment[] segments { get; protected set; }
	[ReadOnlyField] public int activeSegments = 0;
	[ReadOnlyField] public int baseSegment = -1;

	/*
	 * Builds a rope using the supplied array of segments
	 */
    public void buildRope(Segment [] segments) {
        this.segments = segments;
    }

	/*
	 * Builds a rope consisting of count copies of segment
	 */
	public void buildRope(Segment segment, int count) {
		this.segments = new Segment[count];
		for (int i = 0; i < count; i++) {
			segments[i] = new Segment(new Vector2d(segment.position.x, segment.position.y), 
									new Vector2d(segment.orientation.x, segment.orientation.y),
									segment.mass, segment.inertia, segment.length);
		}
		uniformSegments = true;
	}

	public void configure(double angleLimitDegrees, double linearDrag, double angulerDrag, double maxSpeed, double maxSpeedScale) {
		this.angleLimitDegrees = angleLimitDegrees;
		this.linearDrag = linearDrag;
		this.angulerDrag = angulerDrag;
		this.maxSpeed = maxSpeed;
		this.maxSpeedScale = maxSpeedScale;
	}

	void Start() {
		start();
	}

	public virtual void OnUpdate() {}
	public virtual void OnSubUpdate() {}
	public virtual void ApplyConstraints() {
		 for (int i = baseSegment; i >= 1; i--) {
            SegmentConstraint.distanceConstraint(segments[i], segments[i - 1]);
            SegmentConstraint.angleConstraint(segments[i], segments[i - 1], angleLimitRadians);
        }
	}
	public virtual void OnUpdateLate() {}

	void OnValidate() {
		substeps = substeps < 1 ? 1 : substeps;

		_angleLimitRadians = _angleLimit * Mathf.Deg2Rad;

		_subLinearDrag = System.Math.Pow(_linearDrag, substeps);
		_subAngulerDrag = System.Math.Pow(_angulerDrag, substeps);

		_h = Time.fixedDeltaTime / substeps;

		onValidate();
	}

	protected virtual void start() {}
	protected virtual void onValidate() {}

	private Vector2 giz1 = new Vector3(0, 0, 0);
	private Vector2 giz2 = new Vector3(0, 0, 0);

	private void OnDrawGizmos() {
		if (!Application.isPlaying) 
			return;

		for (int i = 0; i < segments.Length; i++) {
			Gizmos.color = i % 2 == 0 ? Color.green : Color.white;

			giz1.x = (float)segments[i].p1.x;
			giz1.y = (float)segments[i].p1.y;
			giz2.x = (float)segments[i].p2.x;
			giz2.y = (float)segments[i].p2.y;

			Gizmos.DrawLine(giz1, giz2);
		}
	}
}