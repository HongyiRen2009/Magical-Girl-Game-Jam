using UnityEngine;
using System;
using Unity.VisualScripting;
using NaughtyAttributes;

[Serializable]
public class Dog : Animal
{
    [SerializeField] bool showData;
    [ShowIf("showData")] [SerializeField] DogData data;

    public override void Speak()
    {
        Debug.Log("Woof");
    }
}

[Serializable]
public class DogData
{
    [SerializeField] float age;
    [SerializeField] Color color;
}
