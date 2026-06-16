using System.Collections.Generic;
using UnityEngine;

public interface IInputAxes
{
    float GetAxis(string axisName);
    void AddAxis(string axisName, float value);
    void RemoveAxis(string axisName);
    bool IsAxisExisted(string axisName);
    void ClearAxes();
};

public sealed class DefaultInputAxesStorage : IInputAxes
{
    private readonly Dictionary<string, float> __axeses = new Dictionary<string, float>();

    public float GetAxis(string axisName)
    {
        return __axeses.TryGetValue(axisName, out float value) ? value : 0f;
    }

    public void AddAxis(string axisName, float value)
    {
        value = Mathf.Clamp(value, -1.0f, 1.0f);
        __axeses.Add(axisName, value);
    }

    public void RemoveAxis(string axisName)
    {
        __axeses.Remove(axisName);
    }

    public bool IsAxisExisted(string axisName)
    {
        return __axeses.ContainsKey(axisName);
    }

    public void ClearAxes()
    {
        __axeses.Clear();
    }
}

public class HumanoidInputController : MonoBehaviour
{

    private IInputAxes _axisStorage;
    public void SetAxisStorage(IInputAxes __axesStorage)
    {
        _axisStorage = __axesStorage ?? new DefaultInputAxesStorage();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
