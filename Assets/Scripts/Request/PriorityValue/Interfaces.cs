/*
 * Base interface for a priority value
 *
 * A priority value is simply a variable with an accomponing priority that can be used for determining
 * how/when the value can be set.
 *
 * The logic/methods for setting a priority value should be defined in implementing classes.
 */
public interface IPriorityValue<T> {
    public T value { get; }
    public int priority { get; }
    public PriorityAlias priorityClass { get; }
}

/*
 * Base interface for a managed priority value
 */
public interface IManagedPriorityValue<T> : IPriorityValue<T> {
    // Returns what the value will be if the current priority request is executed
    public T pendingValue();
}