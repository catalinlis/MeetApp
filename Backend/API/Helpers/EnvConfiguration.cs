using System;
using System.Collections;

public class EnvironmentVariables{

    private readonly Dictionary<string, string> _variables;

    public EnvironmentVariables(){
        _variables = new Dictionary<string, string>();

        foreach(DictionaryEntry env in Environment.GetEnvironmentVariables()){
            _variables[env.Key.ToString()] = env.Value?.ToString();
        }
    }

    public string Get(string key){
        if( _variables.TryGetValue(key, out var value))
            return value;

        return null;
    }

    public string this[string key] => Get(key);

}