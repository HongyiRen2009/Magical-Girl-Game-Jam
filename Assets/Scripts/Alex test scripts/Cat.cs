using UnityEngine;
using System;

[Serializable]
public class Cat : Animal
{
    public override void Speak()
    {
        Debug.Log("I am a cat that spoke");
    }
}
