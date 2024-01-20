using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIEnemy : MonoBehaviour
{
    public GameObject Target; 
    private NavMeshAgent _agent;
    public Vector3 Velocity; 


    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Velocity += (Target.transform.position - transform.position).normalized * Time.deltaTime * 10;
        Velocity = Vector3.Lerp(Velocity, Vector3.zero, Time.deltaTime * 5); 

        _agent.velocity = Velocity; 
    }
}
