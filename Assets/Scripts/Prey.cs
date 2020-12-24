using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Objective
{
    Wander,
    Gather,
    Reproduce,
}

public class Prey : MonoBehaviour
{
    public event System.Action<GameObject> OnReproduce;

    GameObject m_targetObject;
    Vector3 m_targetLocation;
    Objective m_plan = Objective.Wander;
    float m_fullness = 0.5f;
    Renderer m_renderer;

    public float gatherSpeed = 4f;
    public float wanderSpeed = 2f;
    public float reproduceSpeed = 3f;

    public float reachDistance = 2f;
    public float metabolism = 0.1f;
    public float reproduceCost = 0.5f;

    Renderer m_planRenderer;
    static Color wanderColor = Color.blue;
    static Color reproduceColor = Color.red;
    static Color gatherColor = Color.green;

    [Range(0, 1)]
    public float gatherThreshold = 0.5f;

    [Range(0, 1)]
    public float reproduceThreshold = 0.7f;

    Color fullColor;
    Color deadColor = Color.red;

    void Start()
    {
        m_renderer = GetComponent<Renderer>();
        fullColor = m_renderer.material.color;
        StartCoroutine(PlanLoop());
        m_planRenderer = transform.Find("Plan Indicator").GetComponent<Renderer>();
    }

    public GameObject FindClosestObjectWithTag(string tag)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            // Don't match ourselves.
            if (go == gameObject) continue;

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

    Vector3 NormalizeToCurrentHeight(Vector3 target)
    {
        target.y = transform.position.y;
        return target;
    }

    void PlanOnce()
    {
        if (m_fullness < gatherThreshold)
        {
            m_plan = Objective.Gather;
            m_targetObject = FindClosestObjectWithTag("Plant");
        }
        else if (m_fullness >= reproduceThreshold)
        {
            m_plan = Objective.Reproduce;
            m_targetObject = FindClosestObjectWithTag("Prey");
        }
        else
        {
            m_targetObject = null;
        }

        if (m_targetObject == null)
        {
            m_plan = Objective.Wander;
            m_targetLocation = NormalizeToCurrentHeight(transform.position + Random.onUnitSphere * gatherSpeed);
        }
    }

    IEnumerator PlanLoop()
    {
        while (gameObject != null)
        {
            PlanOnce();
            yield return new WaitForSeconds(1.0f);
        }
    }

    bool ChaseTargetObject()
    {
        if (m_targetObject == null)
        {
            return false;
        }
        float distanceToTarget = (m_targetObject.transform.position - transform.position).magnitude;
        if (distanceToTarget <= reachDistance)
        {
            return true;
        }
        transform.LookAt(NormalizeToCurrentHeight(m_targetObject.transform.position));
        transform.Translate(Vector3.forward * gatherSpeed * Time.deltaTime);
        return false;
    }

    void DoGatherUpdate()
    {
        m_planRenderer.material.color = gatherColor;
        if (ChaseTargetObject())
        {
            Plant plant = m_targetObject.GetComponent<Plant>();
            AdjustFullness(plant.Eat());
        }
    }

    void DoWanderUpdate()
    {
        m_planRenderer.material.color = wanderColor;
        transform.LookAt(m_targetLocation);
        transform.Translate(Vector3.forward * wanderSpeed * Time.deltaTime);
    }

    void DoReproduceUpdate()
    {
        m_planRenderer.material.color = reproduceColor;
        if (ChaseTargetObject())
        {
            AdjustFullness(-reproduceCost);
            PlanOnce();
            if (OnReproduce != null)
            {
                OnReproduce(gameObject);
            }
        }
    }

    void AdjustFullness(float delta)
    {
        m_fullness += delta;
        m_fullness = Mathf.Clamp(m_fullness, 0.0f, 1.0f);
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        AdjustFullness(-metabolism * Time.deltaTime);
        m_renderer.material.color = Color.Lerp(deadColor, fullColor, m_fullness);

        if (m_fullness <= 0.0f)
        {
            Die();
            return;
        }

        switch (m_plan)
        {
            case Objective.Wander:
                DoWanderUpdate();
                break;
            case Objective.Gather:
                DoGatherUpdate();
                break;
            case Objective.Reproduce:
                DoReproduceUpdate();
                break;
        }

    }
}
