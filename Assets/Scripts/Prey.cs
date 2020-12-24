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

    int planUpdatesPerSecond = 2;

    public float gatherSpeed;
    public float wanderSpeed;
    public float reproduceSpeed;

    public float reachDistance;
    [Range(0, 0.1f)]
    public float metabolism;
    [Range(0, 1)]
    public float reproduceCost;

    Renderer m_planRenderer;
    static Color wanderColor = Color.blue;
    static Color reproduceColor = Color.red;
    static Color gatherColor = Color.green;

    [Range(0, 1)]
    public float gatherThreshold;
    [Range(0, 1)]
    public float reproduceThreshold;

    public float sightRadius = 10;

    Color fullColor;
    Color deadColor = Color.red;

    public Material visionRadiusMaterial;

    void AddSightRadius()
    {
        var circle = new GameObject { name = "Planning Radius" };
        circle.transform.parent = transform;
        circle.transform.localPosition = Vector3.zero;
        circle.DrawCircle(sightRadius, .1f);
        circle.GetComponent<LineRenderer>().material = visionRadiusMaterial;
    }

    void Start()
    {
        m_renderer = GetComponent<Renderer>();
        fullColor = m_renderer.material.color;
        m_planRenderer = transform.Find("Plan Indicator").GetComponent<Renderer>();
        AddSightRadius();
        StartCoroutine(PlanLoop());
    }

    public GameObject FindNearestVisibleObjectWithTag(string tag)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        float squaredSearchRadius = sightRadius * sightRadius;
        foreach (GameObject go in gos)
        {
            // Don't match ourselves.
            if (go == gameObject) continue;

            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;

            if (curDistance > squaredSearchRadius) continue;

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
        // I've seen Prey wander when hungry?  Unclear why.
        // if (m_fullness < 0.4 && m_plan == Objective.Wander)
        // {
        //     Debug.Break();
        // }
        if (m_fullness < gatherThreshold)
        {
            m_plan = Objective.Gather;
            m_targetObject = FindNearestVisibleObjectWithTag("Plant");
        }
        else if (m_fullness >= reproduceThreshold)
        {
            m_plan = Objective.Reproduce;
            m_targetObject = FindNearestVisibleObjectWithTag("Prey");
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
            yield return new WaitForSeconds(1.0f / planUpdatesPerSecond);
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
