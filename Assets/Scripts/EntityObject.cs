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
    List<Vector3> movePositions;
    Action action;
    Vector3 targetVector, startPostion;
    EntityBattleData currentEntityBattleData;

    public UnityEvent OnFinishActionState { get => onFinishActionState; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speedCurveTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case EntityState.Moving:
                if (movePositions.Count == 0) break;
                speedCurveTime += Time.deltaTime * moveSpeed;
                Debug.Log(speedCurveTime);
                transform.position = startPostion + speedCurve.Evaluate(speedCurveTime) * targetVector;

                if (speedCurveTime < 1) break;
                movePositions.RemoveAt(0);
                speedCurveTime = 0;

                if (movePositions.Count == 0)
                {
                    FinishMovement();
                }
                else
                {
                    startPostion = transform.position;
                    targetVector = movePositions[0] - transform.position;
                }
                break;
        }
    }

    void FinishMovement()
    {
        state = EntityState.Attacking;
        onFinishActionState?.Invoke();
    }

    public void Initizalize(EntityBattleData currentEntityBattleData)
    {
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
        action.PreformAction(currentEntityBattleData, actionStage++);
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
            newPosition = new Vector3(newPosition.x, 0, newPosition.z);

            foreach (EntityBattleData battleData in BattleManager.BattleDataManager.GetList)
            {
                Vector3 currentPosition = battleData.EntityManager.transform.position;
                currentPosition = new (currentPosition.x, 0, currentPosition.z);
                //Debug.Log(newPosition + " | " + currentPosition);
                
                if (Vector3.Distance(newPosition, currentPosition) < 0.1f) return;
            }

            movePositions.Add(newPosition);
        }

        if (movePositions.Count > 0)
        {
            targetVector = movePositions[0] - transform.position;
            //Debug.Log(string.Join(", ", movement));
        }
        else
        {
            FinishMovement();
        }
    }

    public static Vector3 TileToWorldPosition(Vector2Int vector) => new Vector3(vector.x, 0, vector.y);
    public static Vector2Int WorldToTilePosition(Vector3 vector) => new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
}

public enum EntityState { Idle, Moving, Attacking }