using System.Collections;
using System.Collections.Generic;

public enum RequestClass {
	NoRequest = -1,
	Move,
	Brake,
	Boost,
	BoostReset,
	LookAtMouse,
}

public static class PlayerShipRequestPriority {
	private static readonly List<RequestClass> noOrder = new List<RequestClass>();

	private static readonly Dictionary<RequestClass, int> accelerationPriorities = new Dictionary<RequestClass, int>()
	{
		{ RequestClass.Boost, 0 },
		{ RequestClass.Brake, 1 },
		{ RequestClass.Move, 2 },
		{ RequestClass.BoostReset, 3 },
	};
	private static readonly Dictionary<int, List<RequestClass>> accelerationOrder = new Dictionary<int, List<RequestClass>>()
	{
	};

	private static readonly Dictionary<RequestClass, int> maxSpeedPriorities = new Dictionary<RequestClass, int>()
	{
		{ RequestClass.Boost, 0 },
		{ RequestClass.Brake, 1 },
		{ RequestClass.BoostReset, 2 },
	};
	private static readonly Dictionary<int, List<RequestClass>> maxSpeedOrder = new Dictionary<int, List<RequestClass>>() 
	{
	};

	private static readonly Dictionary<RequestClass, int> forcePriorities = new Dictionary<RequestClass, int>()
	{
		{ RequestClass.Move, 0 }, //low priority
		{ RequestClass.Boost, 1 },
		{ RequestClass.Brake, 2 }, //high priority
		{ RequestClass.BoostReset, 3 },
	};
	private static readonly Dictionary<int, List<RequestClass>> forceOrder = new Dictionary<int, List<RequestClass>>() 
	{
	};

	private static readonly Dictionary<RequestClass, int> magnitudePriorities = new Dictionary<RequestClass, int>()
	{
		{ RequestClass.Brake, 0 },
		{ RequestClass.Boost, 1 },
		{ RequestClass.BoostReset, 2 },
	};
	private static readonly Dictionary<int, List<RequestClass>> magnitudeOrder = new Dictionary<int, List<RequestClass>>() 
	{
	};

	private static readonly Dictionary<RequestClass, int> rotationPriorities = new Dictionary<RequestClass, int>()
	{
		{ RequestClass.LookAtMouse, 0 }
	};
	private static readonly Dictionary<int, List<RequestClass>> rotationOrder = new Dictionary<int, List<RequestClass>>() 
	{
	};

	private static readonly Dictionary<string, Dictionary<RequestClass, int>> propertyMap = new Dictionary<string, Dictionary<RequestClass, int>>()
	{
		{ PlayerShipProperties.Acceleration, accelerationPriorities },
		{ PlayerShipProperties.MaxSpeed, maxSpeedPriorities },
		{ PlayerShipProperties.Force, forcePriorities },
		{ PlayerShipProperties.Magnitude, magnitudePriorities },
		{ PlayerShipProperties.Rotation, rotationPriorities }
	};

	private static readonly Dictionary<string, Dictionary<int, List<RequestClass>>> orderMap = new Dictionary<string, Dictionary<int, List<RequestClass>>>()
	{
		{PlayerShipProperties.Acceleration, accelerationOrder},
		{PlayerShipProperties.MaxSpeed, maxSpeedOrder},
		{PlayerShipProperties.Force, forceOrder},
		{PlayerShipProperties.Magnitude, magnitudeOrder},
		{PlayerShipProperties.Rotation, rotationOrder},
	};

	public static int getPriority(string property, RequestClass request) {
		return propertyMap[property].ContainsKey(request) ? propertyMap[property][request] : (int)RequestClass.NoRequest;
	}

	public static List<RequestClass> getOrder(string property, int priority) {
		return orderMap[property].ContainsKey(priority) ? orderMap[property][priority] : noOrder;
	}

	static PlayerShipRequestPriority() {
		foreach (KeyValuePair<string, Dictionary<RequestClass, int>> entry in propertyMap) {
			foreach (int priority in entry.Value.Values) {
				if (!orderMap[entry.Key].ContainsKey(priority)) {
					orderMap[entry.Key][priority] = new List<RequestClass>();
				}
			}
		}
	}
}


