using System;

/*
 * Manages a priority
 */
public interface IPriorityManager {
    public int priority { get; }
    public RequestClass priorityClass { get; }

    /*
     * Attempts to set the priority.  
     * 
     * Returns true if successful, false otherwise.
     * If priority is successfully set then onSuccess callback is invoked.
     */
    public bool setPriority(RequestClass rClass, int priority, Action onSuccess);

    /*
     * Attempts to set the priority.  
     *
     * Returns true if successful, false otherwise.
     */
    public bool setPriority(RequestClass rClass, int priority);

    /*
     * Resets the priority to a default starting value;
     */
    public void reset();
}