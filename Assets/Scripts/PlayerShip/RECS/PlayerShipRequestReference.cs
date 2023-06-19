using UnityEngine;

public interface RigidbodyReference {
	public IPriorityReference Position { get; } 
	public IPriorityReference Rotation { get; }
	public IPriorityReference Velocity { get; }
	public IPriorityReference AngularVelocity { get; }
	public IPriorityReference Force { get; }
	public IPriorityReference Torque { get; }
	public IPriorityReference Magnitude { get; }
}

public interface PlayerShipReference : RigidbodyReference {
	public IPriorityReference LinearMax { get; }
	public IPriorityReference LinearAcceleration { get; }
	public IPriorityReference AngularMax { get; }
	public IPriorityReference AngularAcceleration { get; }
}

public class PlayerShipPriorityReference : PlayerShipReference {
	private IPriorityReference _position = new PriorityReferenceMap(
		new() {
			{ PriorityAlias.Rope, 0 },
		},
		new()
	);

	private IPriorityReference _rotation = new PriorityReferenceMap(
		new() {
			{ PriorityAlias.LookAtMouse, 0 },
		},
		new()
	);

	private IPriorityReference _velocity = new PriorityReferenceMap(
		new(),
		new()
	);

	private IPriorityReference _angularVelocity = new PriorityReferenceMap(
		new(),
		new()
	);

	private IPriorityReference _force = new PriorityReferenceMap(
		new() {
			{ PriorityAlias.Move, 0 }, 
			{ PriorityAlias.Boost, 1 },
			{ PriorityAlias.Brake, 2 }, 
			{ PriorityAlias.BoostReset, 3 },
			{ PriorityAlias.Rope, 4 }
		},
		new()
	);

	private IPriorityReference _torque = new PriorityReferenceMap(
		new() {
			{ PriorityAlias.LookAtMouse, 0 },
		},
		new()
	);

	private IPriorityReference _magnitude = new PriorityReferenceMap(
		new() {
			{ PriorityAlias.Brake, 0 },
			{ PriorityAlias.Boost, 1 },
			{ PriorityAlias.BoostReset, 2 },
		},
		new()
	);

	private IPriorityReference _linearMax = new PriorityReferenceMap(
		new() {
			{ PriorityAlias.Boost, 0 },
			{ PriorityAlias.Brake, 1 },
			{ PriorityAlias.BoostReset, 2 },
		},
		new()
	);

	private IPriorityReference _linearAcceleration = new PriorityReferenceMap(
		new() {
			{ PriorityAlias.Boost, 0 },
			{ PriorityAlias.Brake, 1 },
			{ PriorityAlias.BoostReset, 2 },
		},
		new()
	);

	private IPriorityReference _angularMax = new PriorityReferenceMap(
		new(),
		new()
	);

	private IPriorityReference _angularAcceleration = new PriorityReferenceMap(
		new(),
		new()
	);

	public IPriorityReference Position {
		get { return _position; }
	}

	public IPriorityReference Rotation {
		get { return _rotation; }
	}

	public IPriorityReference Velocity {
		get { return _velocity; }
	}

	public IPriorityReference AngularVelocity {
		get { return _angularVelocity; }
	}

	public IPriorityReference Force {
		get { return _force; }
	}

	public IPriorityReference Torque {
		get { return _torque; }
	}

	public IPriorityReference Magnitude {
		get { return _magnitude; }
	}

	public IPriorityReference LinearMax {
		get { return _linearMax; }
	}

	public IPriorityReference LinearAcceleration {
		get { return _linearAcceleration; }
	}

	public IPriorityReference AngularMax {
		get { return _angularMax; }
	}

	public IPriorityReference AngularAcceleration {
		get { return _angularAcceleration; }
	}
}
