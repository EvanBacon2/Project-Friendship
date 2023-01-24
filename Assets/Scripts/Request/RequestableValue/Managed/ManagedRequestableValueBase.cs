public abstract class ManagedRequestableValueBase<T> : RequestableValueBase<T> {
    public ManagedRequestableValueBase(T value) : base(value) {}

    public abstract void executeRequests();
}
