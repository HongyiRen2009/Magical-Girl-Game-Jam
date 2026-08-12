using System;
using UnityEngine;

[Serializable]
public abstract class Animal
{
    [SerializeField] string name;
    public abstract void Speak();
}
