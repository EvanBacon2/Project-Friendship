using System;

/*
 * Defines a priority that can only increase.
 */
public class IncreasingPriority : IPriorityManager {
    private int _priority;
    private readonly int _basePriority;
    private Action onPriorityChange;

    private PriorityAlias _priorityClass;

    public int priority {
        get { return _priority; }
    }

    public PriorityAlias priorityClass {
        get { return _priorityClass; }
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
    public bool setPriority(PriorityAlias newPriorityClass, int newPriority, Action onSuccess) {
        if (newPriority > _priority)
            onPriorityChange();

        if (newPriority >= _priority) {
            _priority = newPriority;
            _priorityClass = newPriorityClass;
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
    public bool setPriority(PriorityAlias rClass, int newPriority) {
        return setPriority(rClass, newPriority, () => {});
    }

    public void reset() {
        this._priority = this._basePriority;
        this._priorityClass = PriorityAlias.NoRequest;
        onPriorityChange();
    }
}


