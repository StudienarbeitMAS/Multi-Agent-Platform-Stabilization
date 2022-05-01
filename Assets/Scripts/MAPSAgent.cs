using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MAPSAgent : Agent
{

    private Rigidbody m_Body;

    private MAPSSettings m_MAPSSettings;
    private MAPSAreaController m_Controller;

    private Vector3 normalizeVecXYWithBounds(Vector3 v)
    {
        var extents = m_Controller.PlatformBounds.extents;
        return new Vector3(v.x / extents.x, 0, v.z / extents.z);
    }

    private Vector3 normalizeVecXYWithSize(Vector3 v)
    {
        var size = m_Controller.PlatformBounds.size;
        return new Vector3(v.x / size.x, 0, v.z / size.z);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Angle (2 floats)
        Quaternion rot = m_Controller.transform.rotation;
        Vector3 normalizedRot = rot.eulerAngles / 180.0f - Vector3.one;
        sensor.AddObservation(normalizedRot.x);
        sensor.AddObservation(normalizedRot.z);
        //Debug.Log(rot.x + " " + rot.y + " " + rot.z + " " + rot.w);
        //sensor.AddObservation(m_Controller.transform.rotation.z);
        //sensor.AddObservation(m_Controller.transform.rotation.x);

        // Velocity (3 floats)
        sensor.AddObservation(m_Controller.BodyPlatform.velocity);

        // Agent Pos (2 floats)
        var agentPos = normalizeVecXYWithBounds(m_Body.transform.localPosition);
        sensor.AddObservation(agentPos.x);
        sensor.AddObservation(agentPos.z);

        foreach (MAPSAreaController.PlayerInfo player in m_Controller.PlayerInfos)
        {
            if (player.Body == m_Body) { continue; }

            // Position of Player relative to center (2 floats) 
            //var playerPos = normalizeVecXYWithSize(player.Body.transform.localPosition);
            var playerPos = normalizeVecXYWithBounds(player.Body.transform.localPosition);
            sensor.AddObservation(playerPos.x);
            sensor.AddObservation(playerPos.z);
        }

        foreach (MAPSAreaController.WeightInfo weightInfo in m_Controller.WeightInfos)
        {
            // Position of Weight relative to (2 floats) 
            // var weightPos = normalizeVecXYWithSize(weightInfo.Body.transform.localPosition - m_Body.transform.localPosition);
            var weightPos = normalizeVecXYWithBounds(weightInfo.Body.transform.localPosition);
            sensor.AddObservation(weightPos.x);
            sensor.AddObservation(weightPos.z);

            // Weight Mass (1 float)
            sensor.AddObservation(weightInfo.Body.mass / 10.0f);
        }
    }

    public override void Initialize()
    {
        m_MAPSSettings = FindObjectOfType<MAPSSettings>();
        m_Body = GetComponent<Rigidbody>();
        m_Controller = GetComponentInParent<MAPSAreaController>();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];

        ExecuteAction(action);  
    }

    private void ExecuteAction(int action) { 
    
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        switch (action)
        {
            case 1:
                dirToGo = m_Body.transform.forward * 1f;
                break;
            case 2:
                dirToGo = m_Body.transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_Body.AddForce(dirToGo * m_MAPSSettings.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }
}
