
public class Action
{
    public string type { get; private set; }
    public Action(string _type) { type = _type; }
}

public class Action<T> : Action
{
    public T payload { get; private set; }
    public Action(string _type, T _payload) : base(_type) { payload = _payload; }
}