using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EntityObject : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] AnimationCurve speedCurve;
    [SerializeField] UnityEvent onFinishActionState;

    int actionChoice, actionStage;
    float speedCurveTime;

    EntityState state;
    Health healthScript;
    List<Vector3> movePositions;
    Action action;
    Vector3 targetVector, startPostion;
    List<EntityBattleData> entityBattleData;
    EntityBattleData currentEntityBattleData;

    public UnityEvent OnFinishActionState { get => onFinishActionState; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthScript = GetComponent<Health>();

        speedCurveTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case EntityState.Moving:
                if (movePositions.Count == 0)
                {
                    state = EntityState.Attacking;
                    onFinishActionState?.Invoke();
                    break;
                }

                speedCurveTime += Time.deltaTime;
                transform.position = startPostion + speedCurve.Evaluate(speedCurveTime) * targetVector;

                if (speedCurveTime < 1) break;
                movePositions.RemoveAt(0);
                speedCurveTime = 0;

                if (movePositions.Count == 0) break;
                startPostion = transform.position;
                targetVector = movePositions[0] - transform.position;

                bool pathBlocked = false;
                foreach (EntityBattleData battleData in entityBattleData)
                {
                    if (battleData == currentEntityBattleData) continue;
                    if (Vector3.Distance(battleData.EntityManager.transform.position, movePositions[0]) > 0.1f) continue;
                    pathBlocked = true;
                    break;
                }
                if (pathBlocked)
                {
                    state = EntityState.Attacking;
                    onFinishActionState?.Invoke();
                    break;
                }
                break;
        }
    }

    public void Initizalize(List<EntityBattleData> entityBattleData, EntityBattleData currentEntityBattleData)
    {
        this.entityBattleData = entityBattleData;
        this.currentEntityBattleData = currentEntityBattleData;
    }

    public void PerformAction(Action action)
    {
        Debug.Log("Preforming action");
        this.action = action;
    }

    public void DealDamage()
    {
        Debug.Log("Dealing damage");
        action.PreformAction(entityBattleData, currentEntityBattleData, actionStage++);
    }

    public void FinishActionAnimation()
    {
        Debug.Log("Finishing action animation");
        //transform.position = new (Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z));
        onFinishActionState?.Invoke();
    }

    public void PerformMovement(Vector3[] movement)
    {
        Debug.Log("Moving");
        state = EntityState.Moving;
        movePositions = new List<Vector3>();
        speedCurveTime = 0;
        startPostion = transform.position;
        for (int i = 0; i < movement.Length; i++)
        {
            Vector3 newPosition = transform.position + movement[i];

            foreach (EntityBattleData battleData in entityBattleData)
            {
                Vector3 distanceVector = battleData.EntityManager.transform.position - newPosition;
                distanceVector = new Vector3(distanceVector.x, 0, distanceVector.z);
                if (distanceVector.magnitude == 0) return;
            }

            movePositions.Add(newPosition);
        }
        targetVector = movePositions[0] - transform.position;
        Debug.Log(string.Join(", ", movement));
    }
}

public enum EntityState { Idle, Moving, Attacking }