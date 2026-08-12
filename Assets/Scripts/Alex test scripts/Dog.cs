using UnityEngine;
using System;

[Serializable]
public class Dog : Animal
{
    public override void Speak()
    {
        Debug.Log("Woof");
    }
}
