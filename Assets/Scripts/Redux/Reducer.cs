using System.Collections.Generic;

public class Reducer
{

    public delegate Dictionary<string, object> ReducerFunction(Dictionary<string, object> state, Action action);

    public ReducerFunction func { get; private set; }

    public Reducer(ReducerFunction _func) { func = _func; }

}