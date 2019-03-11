using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarSim.Randomization;

public class Domain : MonoBehaviour
{
    public int seed;

    System.Random rnd = new System.Random();

    int NextSeed() {
        return (int) rnd.Next(1, 10000000);
    }
    // Start is called before the first frame update
    void Start()
    {
        RandomizeDomain();
    }

    public void RandomizeDomain() {
        int seed = NextSeed();
        IRandomizable[] components = GetComponentsInChildren<IRandomizable>();
        foreach (IRandomizable component in components) {
            Debug.Log("Moi");
            component.Randomize(seed);
            seed++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
