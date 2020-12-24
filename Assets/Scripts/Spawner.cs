using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Min(1)]
    public int mapSize;
    public Transform ground;
    public GameObject foodPrefab;
    public GameObject preyPrefab;

    [Min(0)]
    int initalPrey = 10;

    [Range(0, 1)]
    public float initalFoodDensity = 0.05f;
    [Range(0, 1)]
    public float maxFoodDensity = 0.1f;

    public float foodPerSecond = 1; // per second;

    int foodCount;
    int foodLimit;
    float nextFoodSpawn;

    void SpawnPreyAtRandomLocation()
    {
        Vector3 location = new Vector3(Random.value * mapSize - .5f * mapSize, 0, Random.value * mapSize - .5f * mapSize);
        SpawnPrey(location);
    }

    void SpawnPrey(Vector3 location)
    {
        var prey = Instantiate<GameObject>(preyPrefab, location, Quaternion.identity);
        prey.transform.parent = transform;
        prey.GetComponent<Prey>().OnReproduce += PreyReproduced;
    }

    void SpawnFoodAtRandomLocation()
    {
        Vector3 location = new Vector3(Random.value * mapSize - .5f * mapSize, 0, Random.value * mapSize - .5f * mapSize);
        var food = Instantiate<GameObject>(foodPrefab, location, Quaternion.identity);
        food.transform.parent = transform;
        food.GetComponent<Plant>().OnDeath += PlantDied;
        foodCount++;
    }

    void PreyReproduced(GameObject parent)
    {
        SpawnPrey(parent.transform.position);
    }

    void PlantDied()
    {
        foodCount--;
    }

    int CountFromDensity(float density)
    {
        return Mathf.RoundToInt(density * mapSize * mapSize);
    }

    // Start is called before the first frame update
    void Start()
    {
        ground.localScale = new Vector3(mapSize, 1, mapSize);
        int initalFood = CountFromDensity(initalFoodDensity);
        for (int i = 0; i < initalFood; i++)
        {
            SpawnFoodAtRandomLocation();
        }
        foodLimit = CountFromDensity(maxFoodDensity);

        for (int i = 0; i < initalPrey; i++)
        {
            SpawnPreyAtRandomLocation();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (nextFoodSpawn < Time.time && foodCount < foodLimit)
        {
            SpawnFoodAtRandomLocation();
            float timeBetweenSpawns = 1f / foodPerSecond;
            nextFoodSpawn = Time.time + 0.5f * timeBetweenSpawns + Random.value * timeBetweenSpawns;
        }
    }
}
