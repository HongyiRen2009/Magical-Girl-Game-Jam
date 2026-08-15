using Unity.VisualScripting;
using UnityEngine;

public class Shelter : MonoBehaviour
{
    [SubclassSelector] [SerializeReference] Animal[] animals;

    [SerializeField] Dog[] dogs;

    [SerializeField] int[] ints;



    void Start()
    {
        foreach (Animal animal in animals)
        {
            animal.Speak();
        }
    }
}
