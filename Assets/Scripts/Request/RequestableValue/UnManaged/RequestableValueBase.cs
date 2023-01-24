using System;

public abstract class RequestableValueBase<T> {
    protected T _value;

    protected Func<T> getVal;
    protected Action<T> setVal;

    public T value {
        get { return _value; }
    }

    public RequestableValueBase(T value) {
        _value = value;
        getVal = () => { return _value; };
        setVal = (T val) => { _value = val; };
    }

    public abstract void setReference(IRequestReference reference);
}
