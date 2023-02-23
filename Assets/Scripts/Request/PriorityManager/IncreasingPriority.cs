using System;

/*
 * Defines a priority that can only increase.
 */
public class IncreasingPriority : IPriorityManager {
    private int _priority;
    private readonly int _basePriority;
    private Action onPriorityChange;

    public int priority {
        get { return _priority; }
    }

    public IncreasingPriority(int basePriority, Action onPriorityChange) {
        this._priority = basePriority;
        this._basePriority = basePriority;
        this.onPriorityChange = onPriorityChange;
    }

    public IncreasingPriority(Action onPriorityChange) : this(-1, onPriorityChange) {}

    public IncreasingPriority(int basePriority) : this(basePriority, () => {}) {}

    /*
     * Attempts to set the priority. 
     * 
     * Returns true if newPriority is greater than or equal to priority
     * If priority is successfully set then onSuccess callback is invoked.
     */
    public bool setPriority(int newPriority, Action onSuccess) {
        if (newPriority > _priority)
            onPriorityChange();

        if (newPriority >= _priority) {
            _priority = newPriority;
            onSuccess();

            return true;
        }
            
        return false;
    }

    /*
     * Attempts to set the priority. 
     * 
     * Returns true if newPriority is greater than or equal to priority
     */
    public bool setPriority(int newPriority) {
        return setPriority(newPriority, () => {});
    }

    public void reset() {
        this._priority = this._basePriority;
        onPriorityChange();
    }
}


