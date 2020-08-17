using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public FishBoids fishPrefab;
    public GannetBoids gannetPrefab;

    public Transform fishSpawn;
    public Transform gannetSpawn;
  
    public int numberOfFish = 350;
    public int numberOfGannets = 50;
    

    void Awake () {

        if (fishPrefab)
        {
            for (int i = 0; i < numberOfFish; i++) 
            {
                FishBoids fish = Instantiate (fishPrefab);
                Vector3 pos = fishSpawn.position + Random.insideUnitSphere ; 
                fish.transform.position = pos;
                fish.transform.forward = Random.insideUnitSphere; 
            }
        }

        if (gannetPrefab)
        for (int i = 0; i < numberOfGannets; i++) 
            {
                GannetBoids gannet = Instantiate (gannetPrefab);
                Vector3 pos = gannetSpawn.position + Random.insideUnitSphere ; 
                gannet.transform.position = pos;
                gannet.transform.forward = Random.insideUnitSphere; 
            }
    
    }
    
    void Update()
    {
        FishBoids[] boids = FindObjectsOfType<FishBoids>();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(boids.Length);
        }
    }


}


