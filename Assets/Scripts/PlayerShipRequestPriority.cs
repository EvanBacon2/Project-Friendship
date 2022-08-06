using System.Collections;
using System.Collections.Generic;

public enum RequestType {
	NoRequest = -1,
	Move,
	Brake,
	Boost,
	LookAtMouse,
}

public static class PlayerShipRequestPriorities {
	private static int counter = 0;

	private static readonly Dictionary<RequestType, int> accelerationPriorities = new Dictionary<RequestType, int>()
	{
		{ RequestType.Boost, counter++ },
		{ RequestType.Brake, counter++ },
	};
	private static readonly Dictionary<RequestType, int> maxSpeedPriorities = new Dictionary<RequestType, int>()
	{
		{ RequestType.Boost, counter++ },
		{ RequestType.Brake, counter++ },
	};
	private static readonly Dictionary<RequestType, int> forcePriorities = new Dictionary<RequestType, int>()
	{
		{ RequestType.Move, counter++ }, //low priority
		{ RequestType.Boost, counter++ },
		{ RequestType.Brake, counter++ }, //high priority
	};
	private static readonly Dictionary<RequestType, int> magnitudePriorities = new Dictionary<RequestType, int>()
	{
		{ RequestType.Brake, counter++ },
		{ RequestType.Boost, counter++ },
	};
	private static readonly Dictionary<RequestType, int> rotationPriorities = new Dictionary<RequestType, int>()
	{
		{ RequestType.LookAtMouse, counter++ }
	};

	private static readonly Dictionary<string, Dictionary<RequestType, int>> propertyMap = new Dictionary<string, Dictionary<RequestType, int>>()
	{
		{ PlayerShipProperties.Acceleration, accelerationPriorities },
		{ PlayerShipProperties.MaxSpeed, maxSpeedPriorities },
		{ PlayerShipProperties.Force, forcePriorities },
		{ PlayerShipProperties.Magnitude, magnitudePriorities },
		{ PlayerShipProperties.Rotation, rotationPriorities }
	};

	public static int getPriority(string property, RequestType request) {
		return propertyMap[property].ContainsKey(request) ? propertyMap[property][request] : (int)RequestType.NoRequest;
	}
}
