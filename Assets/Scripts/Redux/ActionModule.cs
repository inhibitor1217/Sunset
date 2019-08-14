using System.Collections.Generic;

public abstract class ActionModule
{
    public abstract Dictionary<string, object> GetInitialState();
    public abstract Dictionary<string, Reducer> GetReducers();

}