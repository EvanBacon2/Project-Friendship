using System;

/*
 * Manages a priority; controlling when and how it is set.
 */
public interface IPriorityManager {
    public int priority { get; }
    public PriorityAlias priorityClass { get; }

    /*
     * Attempts to set the priority to the given value.  
     * 
     * Returns true if successful, false otherwise.
     * If priority is successfully set then onSuccess callback is invoked.
     */
    public bool setPriority(PriorityAlias rClass, int priority, Action onSuccess);

    /*
     * Attempts to set the priority to the given value.  
     *
     * Returns true if successful, false otherwise.
     */
    public bool setPriority(PriorityAlias rClass, int priority);

    /*
     * Resets the priority to a default starting value;
     */
    public void reset();
}