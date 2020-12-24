using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Objective
{
    Wander,
    Gather,
    Reproduce,
}

public class Animal : MonoBehaviour
{
    public event System.Action<GameObject> OnReproduce;

    GameObject m_targetObject;
    Vector3 m_targetLocation;
    Objective m_plan = Objective.Wander;
    float m_fullness;
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

    [Min(1)]
    public float wanderingness = 3.0f;

    public float sightRadius;

    float timeCanReproduceAfter;
    public float reproduceCooldown;

    Color m_fullColor;
    Color m_deadColor = Color.black;

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
        m_targetLocation = transform.position;
        m_fullness = 0.5f + Random.value * 0.5f;
        m_renderer = GetComponent<Renderer>();
        m_fullColor = m_renderer.material.color;
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
        // Save this before we possibly try to reproduce/gather and fail.
        bool planWasWander = m_plan == Objective.Wander;

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
        else if (m_fullness >= reproduceThreshold && Time.time > timeCanReproduceAfter)
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
            float distance = (m_targetLocation - transform.position).magnitude;

            // If we're starting a new wander, always pick a new location to wander to.
            // If we're less than 1s away from our destination, we can change our minds.
            if (distance < reachDistance || !planWasWander)
            {
                m_targetLocation = NormalizeToCurrentHeight(transform.position + Random.onUnitSphere * wanderSpeed * wanderingness);
            }
            m_plan = Objective.Wander;
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
        Debug.DrawRay(transform.position, m_targetLocation - transform.position);
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
            timeCanReproduceAfter = Time.time + reproduceCooldown;
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
        // Only start lerping once below gather threshold.
        m_renderer.material.color = Color.Lerp(m_deadColor, m_fullColor, m_fullness / gatherThreshold);

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
