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
    public GameObject predatorPrefab;

    [Min(0)]
    int initialPrey = 10;
    int initialPredators = 4;

    [Range(0, 1)]
    public float initialFoodDensity = 0.02f;
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

    void SpawnPredatorAtRandomLocation()
    {
        Vector3 location = new Vector3(Random.value * mapSize - .5f * mapSize, 0, Random.value * mapSize - .5f * mapSize);
        SpawnPredator(location);
    }

    void SpawnPredator(Vector3 location)
    {
        var predator = Instantiate<GameObject>(predatorPrefab, location, Quaternion.identity);
        predator.transform.parent = transform;
        predator.GetComponent<Predator>().OnReproduce += PredatorReproduced;
    }

    void SpawnFoodAtRandomLocation()
    {
        Vector3 location = new Vector3(Random.value * mapSize - .5f * mapSize, 0, Random.value * mapSize - .5f * mapSize);
        var food = Instantiate<GameObject>(foodPrefab, location, Quaternion.identity);
        food.transform.parent = transform;
        food.GetComponent<Edible>().OnDeath += PlantDied;
        foodCount++;
    }

    void PredatorReproduced(GameObject parent)
    {
        SpawnPredator(parent.transform.position);
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

    void Start()
    {
        ground.localScale = new Vector3(mapSize, 1, mapSize);
        int initialFood = CountFromDensity(initialFoodDensity);
        for (int i = 0; i < initialFood; i++)
        {
            SpawnFoodAtRandomLocation();
        }
        foodLimit = CountFromDensity(maxFoodDensity);

        for (int i = 0; i < initialPrey; i++)
        {
            SpawnPreyAtRandomLocation();
        }

        for (int i = 0; i < initialPredators; i++)
        {
            SpawnPredatorAtRandomLocation();
        }
    }

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
