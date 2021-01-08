using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum Objective
{
    Wander,
    Gather,
    Reproduce,
    Flee,
}

// FIXME: This should eventually be a ScriptableObject.
public class AnimalConfig
{
    public string foodTag;
    public string eatSoundName;
    public string fleeFromTag;
}

[System.Serializable]
public class Evolvable
{
    public float gatherSpeed;
}

[RequireComponent(typeof(NavMeshAgent))]
public class Animal : MonoBehaviour
{
    public event System.Action<GameObject, GameObject> OnReproduce;

    GameObject m_targetObject;
    Vector3 m_targetLocation;
    Objective m_plan = Objective.Wander;
    float m_fullness;
    Renderer m_renderer;

    int planUpdatesPerSecond = 2;

    public Evolvable traits;
    public float wanderSpeed;
    public float reproduceSpeed;
    public float fleeSpeed;

    public float reachDistance;
    [Range(0, 0.1f)]
    public float metabolism;
    [Range(0, 1)]
    public float reproduceCost;

    AnimalConfig m_config;

    public Renderer planningIndicator;
    // https://coolors.co/f2ff49-ff4242-fb62f6-645dd7-b3fffc
    static Color wanderColor = new Color32(100, 93, 215, 255);
    static Color reproduceColor = new Color32(251, 98, 246, 255);
    static Color gatherColor = new Color32(179, 255, 252, 255);
    static Color fleeColor = new Color32(242, 255, 73, 255);

    [Range(0, 1)]
    public float gatherThreshold;
    [Range(0, 1)]
    public float reproduceThreshold;

    [Min(1)]
    public float wanderingness = 3.0f;

    public float gatherSightRadius;
    public float reproductionSightRadius;
    public float fleeSightRadius;

    float timeCanReproduceAfter;
    public float reproduceCooldown;

    Color m_fullColor;
    Color m_deadColor = Color.black;

    public Material visionRadiusMaterial;

    GameObject m_sightRing;

    void AddSightRadius()
    {
        Debug.Assert(m_sightRing == null);
        m_sightRing = new GameObject { name = "Planning Radius" };
        m_sightRing.transform.parent = transform;
        m_sightRing.transform.localPosition = Vector3.zero;
        m_sightRing.DrawCircle(gatherSightRadius, .1f);
        m_sightRing.GetComponent<LineRenderer>().material = visionRadiusMaterial;
    }

    protected void OnStart(AnimalConfig config)
    {
        m_config = config;
        m_targetLocation = transform.position;
        m_fullness = 0.5f + Random.value * 0.5f;
        m_renderer = GetComponent<Renderer>();
        m_fullColor = m_renderer.material.color;
        timeCanReproduceAfter = Time.time + reproduceCooldown;
        StartCoroutine(PlanLoop());
    }

    public void ShowOrHideSightRadius()
    {
        if (User.showSightRings && m_sightRing == null)
        {
            AddSightRadius();
        }

        if (!User.showSightRings)
        {
            Destroy(m_sightRing);
            m_sightRing = null;
        }
    }

    public GameObject FindNearestVisibleObjectWithTag(string tag, float radius)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        float squaredSearchRadius = radius * radius;
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

        GameObject nearbyThreat = null;
        if (m_config.fleeFromTag != null)
        {
            nearbyThreat = FindNearestVisibleObjectWithTag(m_config.fleeFromTag, fleeSightRadius);
        }

        if (nearbyThreat != null)
        {
            m_targetObject = nearbyThreat;
            m_plan = Objective.Flee;
        }
        else if (m_fullness < gatherThreshold)
        {
            m_plan = Objective.Gather;
            m_targetObject = FindNearestVisibleObjectWithTag(m_config.foodTag, gatherSightRadius);
        }
        else if (m_fullness >= reproduceThreshold && Time.time > timeCanReproduceAfter)
        {
            m_plan = Objective.Reproduce;
            m_targetObject = FindNearestVisibleObjectWithTag(tag, reproductionSightRadius);
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
        transform.Translate(Vector3.forward * traits.gatherSpeed * Time.deltaTime);
        return false;
    }


    void DoGatherUpdate()
    {
        planningIndicator.material.color = gatherColor;
        if (ChaseTargetObject())
        {
            Edible food = m_targetObject.GetComponent<Edible>();
            AdjustFullness(food.Eat());
            if (m_config.eatSoundName != null)
                SoundManager.PlaySound(m_config.eatSoundName);
        }
    }

    void DoWanderUpdate()
    {
        Debug.DrawRay(transform.position, m_targetLocation - transform.position);
        planningIndicator.material.color = wanderColor;
        transform.LookAt(m_targetLocation);
        transform.Translate(Vector3.forward * wanderSpeed * Time.deltaTime);
    }

    void DoReproduceUpdate()
    {
        planningIndicator.material.color = reproduceColor;
        if (ChaseTargetObject())
        {
            AdjustFullness(-reproduceCost);
            timeCanReproduceAfter = Time.time + reproduceCooldown;
            if (OnReproduce != null)
            {
                OnReproduce(gameObject, m_targetObject);
            }
            // Plan after reproducing so we don't change m_targetObject.
            PlanOnce();
        }
    }

    void DoFleeUpdate()
    {
        planningIndicator.material.color = fleeColor;

        // Flee the target object.
        if (m_targetObject == null) return;

        // Simply run in the opposite direction of the closest flee thing.
        transform.LookAt(NormalizeToCurrentHeight(m_targetObject.transform.position));
        transform.Rotate(Vector3.up * 180);
        transform.Translate(Vector3.forward * fleeSpeed * Time.deltaTime);
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

    protected void OnUpdate()
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
            case Objective.Flee:
                DoFleeUpdate();
                break;
        }

    }
}
