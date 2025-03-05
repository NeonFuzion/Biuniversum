using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EntityObject : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    [SerializeField] Entity entity;
    [SerializeField] UnityEvent onFinishActionState;

    Rigidbody rigidbody;
    EntityState state;
    Health healthScript;
    List<Vector3> movePositions;

    public Entity Entity { get => entity; }
    public UnityEvent OnFinishActionState { get => onFinishActionState; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        healthScript = GetComponent<Health>();

        healthScript.Initialize((int)entity.Health);
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

                rigidbody.linearVelocity = (movePositions[0] - transform.position).normalized * moveSpeed * Time.deltaTime;

                if (Vector3.Distance(transform.position, movePositions[0]) > 0.1f) break;
                transform.position = movePositions[0];
                movePositions.RemoveAt(0);

                if (movePositions.Count > 0) break;
                rigidbody.linearVelocity = Vector3.zero;
                state = EntityState.Attacking;
                onFinishActionState?.Invoke();
                break;
        }
    }

    public void PerformAction(int actionChoice, EntityBattleData[] entityBattleData, EntityBattleData currentEntityBattleData)
    {
        Debug.Log("Preforming action");
        Action action = entity.Actions[actionChoice];
        if (action as DamagingAction)
        {
            DamagingAction damagingAction = action as DamagingAction;
            foreach (EntityBattleData battleData in entityBattleData)
            {
                if (battleData == currentEntityBattleData) continue;
                GameObject currentEntity = battleData.EntityObject.gameObject;
                
                if (battleData.ArenaSide == currentEntityBattleData.ArenaSide) continue;
                if (!damagingAction.EffectTiles.Contains((Vector2)currentEntity.transform.position)) continue;
                Health healthScript = currentEntity.GetComponent<Health>();
                healthScript.TakeDamage(damagingAction.Damage + (int)entity.Attack);
            }
        }
        onFinishActionState?.Invoke();
    }

    public void PerformMovement(Vector3[] movement, EntityBattleData[] entityBattleData, EntityBattleData currentEntityBattleData)
    {
        Debug.Log("Moving");
        state = EntityState.Moving;
        movePositions = new List<Vector3>();
        for (int i = 0; i < movement.Length; i++)
        {
            Vector3 lastPosition = i == 0 ? Vector3.zero : movement[i - 1];
            Vector3 newPosition = transform.position + movement[i];

            foreach (EntityBattleData battleData in entityBattleData)
            {
                EntityObject entityObject = battleData.EntityObject;
                Vector3 distanceVector = entityObject.transform.position - newPosition;
                distanceVector = new Vector3(distanceVector.x, 0, distanceVector.z);
                if (distanceVector.magnitude == 0) return;
            }

            movePositions.Add(newPosition);
        }
    }
}

public enum EntityState { Idle, Moving, Attacking }