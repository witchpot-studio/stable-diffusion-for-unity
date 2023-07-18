using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMove : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent m_agent;

    [SerializeField]
    private Camera m_Camera;

    [SerializeField]
    private Animator m_animator;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100))
            {
                m_agent.destination = hit.point;
            }

        }


        if (m_agent.remainingDistance < 1f)
        {
            m_animator.SetFloat("Speed", 0f);
        }
        else
        {
            m_animator.SetFloat("Speed", m_agent.desiredVelocity.magnitude);
        }
    }
}
