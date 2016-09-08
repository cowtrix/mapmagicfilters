using System;
using System.Linq;
using UnityEngine;

public static class GameObjectExtensions
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T ret = gameObject.GetComponent<T>();
        if (ret == null)
        {
            ret = gameObject.AddComponent<T>();
        }
        return ret;
    }

    public static Component GetOrAddComponent(this GameObject gameObject, Type t)
    {
        Component ret = gameObject.GetComponent(t);
        if (ret == null)
        {
            ret = gameObject.AddComponent(t);
        }
        return ret;
    }


    public static void CopyComponents(this GameObject target, GameObject template, params Type[] excludedTypes)
    {
#if UNITY_EDITOR
        //target.ClearComponents();
        var components = template.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            var component = components[i];

            if (excludedTypes.Contains(component.GetType()))
            {
                continue;
            }

            var json = UnityEditor.EditorJsonUtility.ToJson(component);
            UnityEditor.EditorJsonUtility.FromJsonOverwrite(json, target.GetOrAddComponent(component.GetType()));
        }
#else
        Debug.LogError("Copying components is not supported with this method at runtime!");
#endif
    }

    public static void ClearComponents(this GameObject target)
    {
        var components = target.GetComponents<Component>();
        foreach (var component in components)
        {
            if (!(component is Transform))
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(component);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(component);
                }
            }
        }
    }
}