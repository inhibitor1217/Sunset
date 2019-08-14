using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Store))]
public class StoreEditor : Editor
{

    public static StoreEditor instance { get; private set; }

    void Awake()
    {
        instance = this;
    }

    public override void OnInspectorGUI()
    {
        Store store = (Store)target;

        var state = store.GetState();

        if (state != null)
        {
            var keys = new List<string>(state.Keys);
            foreach (string key in keys)
            {
                if (state[key].GetType() == typeof(int))
                {
                    int value = EditorGUILayout.IntField(key, (int)state[key]);
                    store.SetValue(key, value);
                }
                else if (state[key].GetType() == typeof(float))
                {
                    float value = EditorGUILayout.FloatField(key, (float)state[key]);
                    store.SetValue(key, value);
                }
                else if (state[key].GetType() == typeof(Vector2))
                {
                    Vector2 value = EditorGUILayout.Vector2Field(key, (Vector2)state[key]);
                    store.SetValue(key, value);
                }
                else if (state[key].GetType() == typeof(Vector3))
                {
                    Vector3 value = EditorGUILayout.Vector3Field(key, (Vector3)state[key]);
                    store.SetValue(key, value);
                }
                else if (state[key].GetType() == typeof(Vector4))
                {
                    Vector4 value = EditorGUILayout.Vector4Field(key, (Vector4)state[key]);
                    store.SetValue(key, value);
                }
            }
        }
    }

}