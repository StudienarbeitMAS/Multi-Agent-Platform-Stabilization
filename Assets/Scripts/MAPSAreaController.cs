using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class MAPSAreaController : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody BodyPlatform;

    [System.Serializable]
    public class PlayerInfo
    {
        public MAPSAgent Agent;

        [HideInInspector]
        public Vector3 StartingPos;

        [HideInInspector]
        public Quaternion StartingRotation;

        [HideInInspector]
        public Rigidbody Body;
    }

    [System.Serializable]
    public class WeightInfo
    {
        public GameObject Obj;

        [HideInInspector]
        public Vector3 StartingPos;

        [HideInInspector]
        public Quaternion StartingRotation;

        [HideInInspector]
        public Rigidbody Body;
    }

    [Header("Max Environment Steps")]
    public int MaxEnvironmentSteps;

    [Header("Agents")]
    public List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();

    [Header("Weights")]
    public List<WeightInfo> WeightInfos = new List<WeightInfo>();

    [Header("Detection Plane")]
    public Transform DetectionPlane;

    [Header("Weight Settings")]
    public Mesh WeightMesh;
    public Material WeightMaterial;

    private SimpleMultiAgentGroup m_AgentGroup;
    private int m_ResetTimer = 0;

    public Bounds PlatformBounds;
    public Transform GroundPlane;
    private MAPSSettings m_Settings;

    private Vector3[] m_PlatformPoints;
    private Vector3[] m_PlanePoints;
    private float m_InitialPlanePlatformDistance;

    private GameObject m_WeightObj;

    // Use this for initialization
    void Start()
    {
        m_Settings = FindObjectOfType<MAPSSettings>();
        m_AgentGroup = new SimpleMultiAgentGroup();
        BodyPlatform = GetComponent<Rigidbody>();

        foreach (PlayerInfo agent in PlayerInfos)
        {
            agent.StartingPos = agent.Agent.transform.position;
            agent.StartingRotation = agent.Agent.transform.rotation;
            agent.Body = agent.Agent.GetComponent<Rigidbody>();

            m_AgentGroup.RegisterAgent(agent.Agent);
        }

        if (!m_Settings.useRandomizedWeightPositions)
        {
            foreach (WeightInfo weightInfo in WeightInfos)
            {
                weightInfo.Body = weightInfo.Obj.GetComponent<Rigidbody>();
                weightInfo.StartingPos = weightInfo.Body.transform.position;
                weightInfo.StartingRotation = weightInfo.Body.transform.rotation;
            }
        }

        foreach (Transform t in transform)
        {
            if (t.name == "Ground")
            {
                GroundPlane = t;

                PlatformBounds = t.GetComponent<Collider>().bounds;
                m_InitialPlanePlatformDistance = Mathf.Abs(t.transform.position.y - DetectionPlane.transform.position.y);

                Vector3 pos1 = new Vector3(t.transform.position.x + PlatformBounds.extents.x, DetectionPlane.transform.position.y, t.transform.position.z + PlatformBounds.extents.z);
                Vector3 pos2 = new Vector3(t.transform.position.x + PlatformBounds.extents.x, DetectionPlane.transform.position.y, t.transform.position.z - PlatformBounds.extents.z);
                Vector3 pos3 = new Vector3(t.transform.position.x - PlatformBounds.extents.x, DetectionPlane.transform.position.y, t.transform.position.z + PlatformBounds.extents.z);
                Vector3 pos4 = new Vector3(t.transform.position.x - PlatformBounds.extents.x, DetectionPlane.transform.position.y, t.transform.position.z - PlatformBounds.extents.z);
                m_PlanePoints = new Vector3[]{pos1, pos2, pos3, pos4};
                break;
            }
        }

        m_PlatformPoints = new Vector3[] {
            new Vector3(PlatformBounds.extents.x, 0, PlatformBounds.extents.z),
            new Vector3(PlatformBounds.extents.x, 0, -PlatformBounds.extents.z),
            new Vector3(-PlatformBounds.extents.x, 0, PlatformBounds.extents.z),
            new Vector3(-PlatformBounds.extents.x, 0, -PlatformBounds.extents.z)};

        ResetScene();
    }

    public void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_AgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        m_AgentGroup.AddGroupReward(
            (0.5f / MaxEnvironmentSteps) * 
            (m_Settings.usePlatformDistanceDiscount ? GetHeightPctToDetectionPlane() : 1));
    }

    public void onAngleTrigger()
    {
        ResetScene();

        m_AgentGroup.AddGroupReward(-1);
        m_AgentGroup.GroupEpisodeInterrupted();
    }

    private void ResetScene()
    {
        ResetPositions();

        m_ResetTimer = 0;
    }

    private void ResetPositions()
    {
        BodyPlatform.angularVelocity = Vector3.zero;
        BodyPlatform.velocity = Vector3.zero;
        BodyPlatform.transform.rotation = Quaternion.identity;

        foreach (PlayerInfo agent in PlayerInfos)
        {
            agent.Agent.transform.position = agent.StartingPos;
            agent.Agent.transform.rotation = agent.StartingRotation;
            agent.Body.angularVelocity = Vector3.zero;
            agent.Body.velocity = Vector3.zero;
        }

        if (!m_Settings.useRandomizedWeightPositions)
        {
            foreach (WeightInfo weight in WeightInfos)
            {
                weight.Body.velocity = Vector3.zero;
                weight.Body.angularVelocity = Vector3.zero;
                weight.Body.transform.rotation = weight.StartingRotation;
                weight.Body.transform.position = weight.StartingPos;
            }
        } else
        {
            foreach (WeightInfo weight in WeightInfos)
            {
                Destroy(weight.Obj);
            }

            WeightInfos.Clear();
            spawnWeight();
        }
    }

    private WeightInfo spawnWeight()
    {
        GameObject weight = new GameObject();

        MeshFilter filter = weight.AddComponent<MeshFilter>();
        filter.mesh = WeightMesh;

        MeshRenderer renderer = weight.AddComponent<MeshRenderer>();
        renderer.material = WeightMaterial;

        weight.AddComponent<BoxCollider>();

        Rigidbody rb = weight.AddComponent<Rigidbody>();
        rb.mass = 10;
        rb.drag = 0;
        rb.angularDrag = 0;
        weight.transform.parent = transform;
        weight.transform.position = GetRandomSpawnPos();

        FixedJoint joint = weight.AddComponent<FixedJoint>();
        joint.connectedBody = BodyPlatform;

        WeightInfo info = new WeightInfo();
        info.Obj = weight;
        info.Body = rb;
        WeightInfos.Add(info);

        return info;
    }

    private float GetHeightPctToDetectionPlane()
    {
        float shortestDistance = float.MaxValue;

        for (int i = 0; i < 4; i++)
        {
            Vector3 planePoint = m_PlanePoints[i];
            Vector3 platformPoint = transform.TransformPoint(m_PlatformPoints[i]);

            float distance = Vector3.Distance(planePoint, platformPoint);

            if (distance < shortestDistance) { shortestDistance = distance;}
        }

        return shortestDistance / m_InitialPlanePlatformDistance;
    }

    public Vector3 GetRandomSpawnPos()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;

        while (!foundNewSpawnLocation)
        {
            var randomPosX = Random.Range(-PlatformBounds.extents.x * m_Settings.spawnAreaMarginMultiplier,
                PlatformBounds.extents.x * m_Settings.spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-PlatformBounds.extents.z * m_Settings.spawnAreaMarginMultiplier,
                PlatformBounds.extents.z * m_Settings.spawnAreaMarginMultiplier);
            
            randomSpawnPos = GroundPlane.transform.position + new Vector3(randomPosX, 0.5f, randomPosZ);

            if (!Physics.CheckBox(randomSpawnPos, new Vector3(1.5f, 0.01f, 1.5f)))
            {
                foundNewSpawnLocation = true;
            }
        }

        return randomSpawnPos;
    }
}