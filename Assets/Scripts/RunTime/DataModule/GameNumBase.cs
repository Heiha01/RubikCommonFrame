using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNumBase<T>
{
    private Action<T> _onDataChanged;

    private T _value;

    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            _onDataChanged?.Invoke(_value);
        }
    }

    public GameNumBase(T value)
    {
        _value = value;
    }

    public void StartListening(Action<T> onDataChanged, bool returnImmediately = true)
    {
        _onDataChanged += onDataChanged;

        if (returnImmediately)
            onDataChanged?.Invoke(_value);
    }

    public void StopListening(Action<T> onDataChanged)
    {
        _onDataChanged -= onDataChanged;
    }
}
