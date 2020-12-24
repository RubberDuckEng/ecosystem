using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prey : MonoBehaviour
{
    GameObject targetFood;

    public float maxSpeed = 2f;
    bool dead = false;

    float eatThreshold = 2f;

    void Start()
    {
        StartCoroutine(Plan());
    }

    public GameObject FindClosestFood()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Plant");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    IEnumerator Plan()
    {
        while (!dead)
        {
            targetFood = FindClosestFood();
            yield return new WaitForSeconds(1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targetFood)
        {
            float distanceToFood = (targetFood.transform.position - transform.position).magnitude;
            if (distanceToFood > eatThreshold)
            {
                transform.LookAt(targetFood.transform);
                transform.Translate(Vector3.forward * maxSpeed * Time.deltaTime);
            }
            else
            {
                targetFood.GetComponent<Plant>().Eat();
            }
        }
    }
}
