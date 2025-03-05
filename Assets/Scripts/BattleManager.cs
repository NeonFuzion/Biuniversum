using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Unity.Netcode;
using NUnit.Framework;
using UnityEngine.UI;

public class BattleManager : NetworkBehaviour
{
    [SerializeField] int northClamp, eastClamp, southClamp, westClamp;
    [SerializeField] GameObject actionMenu;
    [SerializeField] UnityEvent onContinue;
    [SerializeField] List<EntityBattleData> battleData;
    
    EntityTurnData[] turnData;

    Vector3[] currentMovement;

    int entityIndex, actionIndex, maxSteps, stepCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        turnData = new EntityTurnData[battleData.Count];

        foreach (EntityBattleData currentEntityBattleData in battleData)
        {
            EntityObject entityObject = currentEntityBattleData.EntityObject;
            entityObject.OnFinishActionState.AddListener(PreformCycle);
            entityObject.GetComponent<Health>().Initialize((int)entityObject.Entity.Health);

            if (!currentEntityBattleData.ActionVisual) continue;
            currentEntityBattleData.ActionVisual.Initialize(entityObject.transform);
        }

        SortEntitiesBySpeed();
        actionIndex = 0;
        entityIndex = 0;
        SetNextControllableCharacter();
        SetupMovement();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetNextControllableCharacter()
    {
        if (entityIndex >= battleData.Count) return;
        while (true)
        {
            Entity currentEntity = battleData[entityIndex].EntityObject.Entity;

            if (!currentEntity.EnemyAI) break;
            entityIndex++;

            if (entityIndex < battleData.Count) continue;
            EndSelection();
        }

    }

    void EndSelection()
    {
        actionIndex = 0;
        entityIndex = 0;
        
        for (int i = 0; i < turnData.Length; i++)
        {
            if (turnData[i] != null) continue;
            turnData[i] = new EntityTurnData(-1, new Vector3[] {});
        }
        PreformCycle();
    }

    void SetupMovement()
    {
        stepCount = 0;
        maxSteps = battleData[entityIndex].EntityObject.Entity.MoveTiles;
        currentMovement = new Vector3[maxSteps];
        battleData[entityIndex].ActionVisual.SetMovement();
    }

    void SortEntitiesBySpeed()
    {
        for (int i = battleData.Count; i > 0; i--)
        {
            float maxSpeed = int.MinValue;
            int index = 0;
            for (int j = 0; j < i; j++)
            {
                Entity entity = battleData[j].EntityObject.Entity;
                if (entity.Speed < maxSpeed) continue;
                index = j;
                maxSpeed = entity.Speed;
            }

            List<EntityBattleData> sameSpeedEntities = new List<EntityBattleData>();
            for (int j = 0; j < i; j++)
            {
                EntityBattleData currentBattleData = battleData[j];
                if (currentBattleData.EntityObject.Entity.Speed != maxSpeed) continue;
                sameSpeedEntities.Add(currentBattleData);
            }

            EntityBattleData shiftData = sameSpeedEntities[index];
            sameSpeedEntities.RemoveAt(index);
            sameSpeedEntities.Add(shiftData);
        }
    }

    public void MovementInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (actionIndex != 0) return;
        Vector2 input = context.action.ReadValue<Vector2>();
        Vector3 movement = new Vector3(input.x, 0, input.y);
        EntityObject entityObject = battleData[entityIndex].EntityObject;
        movement += stepCount <= 0 ? Vector3.zero : currentMovement[stepCount - 1];

        Vector3 worldPosition = movement + entityObject.transform.position;
        if (worldPosition.x > eastClamp) return;
        if (worldPosition.x < westClamp) return;
        if (worldPosition.z > northClamp) return;
        if (worldPosition.z < southClamp) return;

        battleData[entityIndex].ActionVisual.AddSteps(movement);
        currentMovement[stepCount] = movement;
        stepCount++;

        if (stepCount < maxSteps) return;
        actionIndex++;
    }

    public void ActionInput(int actionChoice)
    {
        EntityTurnData currentTurnData = new EntityTurnData(actionChoice, currentMovement);
        turnData[entityIndex] = currentTurnData;
        HideVisuals();
        actionIndex = 0;
        entityIndex++;
        SetNextControllableCharacter();

        if (entityIndex < turnData.Length)
        {
            SetupMovement();
        }
        else
        {
            EndSelection();
        }
    }

    public void PreformCycle()
    {
        if (entityIndex >= battleData.Count)
        {
            SortEntitiesBySpeed();
            actionIndex = 0;
            entityIndex = 0;
            
            turnData = new EntityTurnData[battleData.Count];
            SetNextControllableCharacter();
            return;
        }

        EntityBattleData currentBattleData = battleData[entityIndex];
        EntityTurnData currentTurnData = turnData[entityIndex];
        Vector3[] movement = currentTurnData.Movement;
        int actionChoice = currentTurnData.ActionChoice;
        //Debug.Log($"Turn {entityIndex}: {entityObject.Entity.Name}, {turnData.Movement.Length}");

        if (currentBattleData.EntityObject.Entity.EnemyAI)
        {
            Entity entity = currentBattleData.EntityObject.Entity;
            movement = entity.EnemyAI.ChooseMovement(battleData.ToArray(), currentBattleData);
            actionChoice = entity.EnemyAI.ChooseAction(battleData.ToArray(), currentBattleData);
        }

        switch ((ActionStage)actionIndex)
        {
            case ActionStage.Moving:
                currentBattleData.EntityObject.PerformMovement(movement, battleData.ToArray(), currentBattleData);
                actionIndex++;
                break;
            case ActionStage.Performing:
                entityIndex++;
                actionIndex = 0;
                currentBattleData.EntityObject.PerformAction(actionChoice, battleData.ToArray(), currentBattleData);
                break;
        }
    }

    public void DisplayActionMenu(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (actionIndex != 1) return;

        actionMenu.SetActive(true);
        EntityObject entityObject = battleData[entityIndex].EntityObject;
        for (int i = 0; i < actionMenu.transform.childCount; i++)
        {
            GameObject actionButton = actionMenu.transform.GetChild(i).gameObject;
            if (entityObject.Entity.Actions.Length > i)
            {
                ActionHover script = actionButton.GetComponent<ActionHover>();
                Action currentAction = entityObject.Entity.Actions[i];
                script.Initialize(currentAction);
                actionButton.SetActive(true);
            }
            else
            {
                actionButton.SetActive(false);
            }
        }
    }

    public void ShowEffectedTiles(Action action)
    {
        battleData[entityIndex].ActionVisual.ShowEffectedTiles(action);
    }

    public void HideEffectedTiles()
    {
        battleData[entityIndex].ActionVisual.HideEffectedTiles();
    }

    public void ShowVisuals(Action action)
    {
        battleData[entityIndex].ActionVisual.ShowVisuals(action);
    }

    public void HideVisuals()
    {
        battleData[entityIndex].ActionVisual.HideVisuals();
    }
}

public class EntityTurnData
{
    int actionChoice;

    Vector3[] movement;
    
    public int ActionChoice { get => actionChoice; set => actionChoice = value; }
    
    public Vector3[] Movement { get => movement; set => movement = value; }

    public EntityTurnData(int actionChoice, Vector3[] movement)
    {
        this.actionChoice = actionChoice;

        this.movement = movement;
    }
}

[Serializable]
public class EntityBattleData
{
    [SerializeField] EntityObject entityObject;
    [SerializeField] ArenaSide arenaSide;
    [SerializeField] ActionVisual actionVisual;

    public EntityObject EntityObject { get => entityObject; }
    public ArenaSide ArenaSide { get => arenaSide; }
    public ActionVisual ActionVisual { get => actionVisual; }

    public EntityBattleData(EntityObject entityObject, ArenaSide arenaSide, ActionVisual actionVisual)
    {
        this.entityObject = entityObject;
        this.arenaSide = arenaSide;
        this.actionVisual = actionVisual;
    }
}


public enum ActionStage { Moving, Performing }
public enum ArenaSide { North, South }