using System.Collections;
using System.Collections.Generic;

public static class Request {
	public const int NoRequest = -1;
	public const int Move = 0;
	public const int Brake = 1;
	public const int Boost = 2;
	public const int LookAtMouse = 3;
}

public static class PlayerShipRequestPriorities {
	private static int counter = 0;

	private static readonly Dictionary<int, int> accelerationPriorities = new Dictionary<int, int>()
	{
		{ Request.Boost, counter++ },
		{ Request.Brake, counter++ },
	};
	private static readonly Dictionary<int, int> maxSpeedPriorities = new Dictionary<int, int>()
	{
		{ Request.Boost, counter++ },
		{ Request.Brake, counter++ },
	};
	private static readonly Dictionary<int, int> forcePriorities = new Dictionary<int, int>()
	{
		{ Request.Move, counter++ }, //low priority
		{ Request.Boost, counter++ },
		{ Request.Brake, counter++ }, //high priority
	};
	private static readonly Dictionary<int, int> magnitudePriorities = new Dictionary<int, int>()
	{
		{ Request.Boost, counter++ },
		{ Request.Brake, counter++ },
	};
	private static readonly Dictionary<int, int> rotationPriorities = new Dictionary<int, int>()
	{
		{ Request.LookAtMouse, counter++ }
	};

	private static readonly Dictionary<string, Dictionary<int, int>> propertyMap = new Dictionary<string, Dictionary<int, int>>()
	{
		{ PlayerShipProperties.Acceleration, accelerationPriorities },
		{ PlayerShipProperties.MaxSpeed, maxSpeedPriorities },
		{ PlayerShipProperties.Force, forcePriorities },
		{ PlayerShipProperties.Magnitude, magnitudePriorities },
		{ PlayerShipProperties.Rotation, rotationPriorities }
	};

	public static int getPriority(string property, int request) {
		return propertyMap[property].ContainsKey(request) ? propertyMap[property][request] : Request.NoRequest;
	}
}
