using UnityEngine;

public interface RigidbodyReference {
	public IRequestReference Position { get; } 
	public IRequestReference Rotation { get; }
	public IRequestReference Velocity { get; }
	public IRequestReference AngularVelocity { get; }
	public IRequestReference Force { get; }
	public IRequestReference Torque { get; }
	public IRequestReference Magnitude { get; }
}

public interface PlayerShipReference : RigidbodyReference {
	public IRequestReference LinearMax { get; }
	public IRequestReference LinearAcceleration { get; }
	public IRequestReference AngularMax { get; }
	public IRequestReference AngularAcceleration { get; }
}

public class PlayerShipRequestReference : PlayerShipReference {
	private IRequestReference _position = new RequestReferenceMap(
		new() {
			{ PriorityAlias.Rope, 0 },
		},
		new()
	);

	private IRequestReference _rotation = new RequestReferenceMap(
		new() {
			{ PriorityAlias.LookAtMouse, 0 },
		},
		new()
	);

	private IRequestReference _velocity = new RequestReferenceMap(
		new(),
		new()
	);

	private IRequestReference _angularVelocity = new RequestReferenceMap(
		new(),
		new()
	);

	private IRequestReference _force = new RequestReferenceMap(
		new() {
			{ PriorityAlias.Move, 0 }, 
			{ PriorityAlias.Boost, 1 },
			{ PriorityAlias.Brake, 2 }, 
			{ PriorityAlias.BoostReset, 3 },
			{ PriorityAlias.Rope, 4 }
		},
		new()
	);

	private IRequestReference _torque = new RequestReferenceMap(
		new() {
			{ PriorityAlias.LookAtMouse, 0 },
		},
		new()
	);

	private IRequestReference _magnitude = new RequestReferenceMap(
		new() {
			{ PriorityAlias.Brake, 0 },
			{ PriorityAlias.Boost, 1 },
			{ PriorityAlias.BoostReset, 2 },
		},
		new()
	);

	private IRequestReference _linearMax = new RequestReferenceMap(
		new() {
			{ PriorityAlias.Boost, 0 },
			{ PriorityAlias.Brake, 1 },
			{ PriorityAlias.BoostReset, 2 },
		},
		new()
	);

	private IRequestReference _linearAcceleration = new RequestReferenceMap(
		new() {
			{ PriorityAlias.Boost, 0 },
			{ PriorityAlias.Brake, 1 },
			{ PriorityAlias.BoostReset, 3 },
		},
		new()
	);

	private IRequestReference _angularMax = new RequestReferenceMap(
		new(),
		new()
	);

	private IRequestReference _angularAcceleration = new RequestReferenceMap(
		new(),
		new()
	);

	public IRequestReference Position {
		get { return _position; }
	}

	public IRequestReference Rotation {
		get { return _rotation; }
	}

	public IRequestReference Velocity {
		get { return _velocity; }
	}

	public IRequestReference AngularVelocity {
		get { return _angularVelocity; }
	}

	public IRequestReference Force {
		get { return _force; }
	}

	public IRequestReference Torque {
		get { return _torque; }
	}

	public IRequestReference Magnitude {
		get { return _magnitude; }
	}

	public IRequestReference LinearMax {
		get { return _linearMax; }
	}

	public IRequestReference LinearAcceleration {
		get { return _linearAcceleration; }
	}

	public IRequestReference AngularMax {
		get { return _angularMax; }
	}

	public IRequestReference AngularAcceleration {
		get { return _angularAcceleration; }
	}
}
