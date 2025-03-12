using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [SerializeField] int northClamp, eastClamp, southClamp, westClamp;
    [SerializeField] GameObject actionMenu, endMovmentButton;
    [SerializeField] UnityEvent onContinue;
    [SerializeField] List<EntityBattleData> battleData;
    
    EntityTurnData[] turnData;
    Vector3[] currentMovement;

    int entityIndex, actionIndex, maxSteps, stepCount;
    bool actionSelectable;

    void Awake()
    {
        turnData = new EntityTurnData[battleData.Count];

        foreach (EntityBattleData currentBattleData in battleData)
        {
            bool flip = currentBattleData.ArenaSide == ArenaSide.North;
            EntityManager manager = currentBattleData.EntityManager;
            manager.ModelManager.transform.eulerAngles = new Vector3(0, flip ? 180 : 0, 0);
            manager.EntityObject.OnFinishActionState.AddListener(IncrementCycle);
            manager.EntityObject.Initizalize(battleData, currentBattleData);
            manager.Initialize();
        }

        SortEntitiesBySpeed();
        actionIndex = 0;
        entityIndex = 0;
        actionSelectable = false;
        SetNextControllableCharacter();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            Entity currentEntity = battleData[entityIndex].EntityManager.Entity;

            if (!currentEntity.EnemyAI) break;
            entityIndex++;

            if (entityIndex < battleData.Count) continue;
            EndSelection();
            break;
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
        actionSelectable = true;
        endMovmentButton.SetActive(true);
        maxSteps = battleData[entityIndex].EntityManager.Entity.MoveTiles;
        currentMovement = new Vector3[maxSteps];
        battleData[entityIndex].EntityManager.ActionVisual.SetMovement();
    }

    void SortEntitiesBySpeed()
    {
        for (int i = battleData.Count; i > 0; i--)
        {
            float maxSpeed = int.MinValue;
            int index = 0;
            for (int j = 0; j < i; j++)
            {
                Entity entity = battleData[j].EntityManager.Entity;
                if (entity.Speed < maxSpeed) continue;
                index = j;
                maxSpeed = entity.Speed;
            }

            List<EntityBattleData> sameSpeedEntities = new List<EntityBattleData>();
            for (int j = 0; j < i; j++)
            {
                EntityBattleData currentBattleData = battleData[j];
                if (currentBattleData.EntityManager.Entity.Speed != maxSpeed) continue;
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
        if (stepCount >= maxSteps) return;
        Vector2 input = context.action.ReadValue<Vector2>();
        Vector3 movement = new Vector3(input.x, 0, input.y);
        EntityObject entityObject = battleData[entityIndex].EntityManager.EntityObject;
        movement += stepCount <= 0 ? Vector3.zero : currentMovement[stepCount - 1];

        Vector3 worldPosition = movement + entityObject.transform.position;
        if (worldPosition.x > eastClamp) return;
        if (worldPosition.x < westClamp) return;
        if (worldPosition.z > northClamp) return;
        if (worldPosition.z < southClamp) return;

        battleData[entityIndex].EntityManager.ActionVisual.AddSteps(movement);
        currentMovement[stepCount] = movement;
        stepCount++;
    }

    public void ActionInput(int actionChoice)
    {
        EntityTurnData currentTurnData = new EntityTurnData(actionChoice, currentMovement);
        turnData[entityIndex] = currentTurnData;
        HideVisuals();
        actionIndex = 0;
        actionSelectable = false;
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
        EntityBattleData currentBattleData = battleData[entityIndex];
        EntityTurnData currentTurnData = turnData[entityIndex];
        Vector3[] movement = currentTurnData.Movement;
        int actionChoice = currentTurnData.ActionChoice;

        if (currentBattleData.EntityManager.Entity.EnemyAI)
        {
            Entity entity = currentBattleData.EntityManager.Entity;
            movement = entity.EnemyAI.ChooseMovement(battleData.ToArray(), currentBattleData);
            actionChoice = entity.EnemyAI.ChooseAction(battleData.ToArray(), currentBattleData);
        }

        switch ((ActionStage)actionIndex)
        {
            case ActionStage.Moving:
                currentBattleData.EntityManager.EntityObject.PerformMovement(movement);
                break;
            case ActionStage.Performing:
                Action action = currentBattleData.EntityManager.Entity.Actions[actionChoice];
                currentBattleData.EntityManager.EntityObject.PerformAction(action);
                currentBattleData.EntityManager.EntityAnimationManager.Animate(action);
                break;
        }
    }

    public void IncrementCycle()
    {
        switch ((ActionStage)actionIndex)
        {
            case ActionStage.Moving:
                actionIndex++;
                break;
            case ActionStage.Performing:
                entityIndex++;
                actionIndex = 0;
                break;
        }

        if (entityIndex >= battleData.Count)
        {
            SortEntitiesBySpeed();
            actionIndex = 0;
            entityIndex = 0;
            actionSelectable = true;
            
            turnData = new EntityTurnData[battleData.Count];
            SetNextControllableCharacter();
            SetupMovement();
        }
        else
        {
            PreformCycle();
        }
    }

    public void DisplayActionMenuInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        DisplayActionMenu();
    }

    public void DisplayActionMenu()
    {
        //if (actionIndex == 0) return;
        if (!actionSelectable) return;
        actionMenu.SetActive(true);
        endMovmentButton.SetActive(false);
        EntityObject entityObject = battleData[entityIndex].EntityManager.EntityObject;
        EntityManager entityManager = battleData[entityIndex].EntityManager;
        for (int i = 0; i < actionMenu.transform.childCount; i++)
        {
            GameObject actionButton = actionMenu.transform.GetChild(i).gameObject;
            if (entityManager.Entity.Actions.Length > i)
            {
                ActionHover script = actionButton.GetComponent<ActionHover>();
                Action currentAction = entityManager.Entity.Actions[i];
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
        battleData[entityIndex].EntityManager.ActionVisual.ShowEffectedTiles(action);
    }

    public void HideEffectedTiles()
    {
        battleData[entityIndex].EntityManager.ActionVisual.HideEffectedTiles();
    }

    public void ShowVisuals(Action action)
    {
        battleData[entityIndex].EntityManager.ActionVisual.ShowVisuals(action);
    }

    public void HideVisuals()
    {
        battleData[entityIndex].EntityManager.ActionVisual.HideVisuals();
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
    [SerializeField] EntityManager entityManager;
    [SerializeField] ArenaSide arenaSide;

    public EntityManager EntityManager { get => entityManager; }
    public ArenaSide ArenaSide { get => arenaSide; }

    public EntityBattleData(EntityManager entityManager, ArenaSide arenaSide)
    {
        this.entityManager = entityManager;
        this.arenaSide = arenaSide;
    }
}


public enum ActionStage { Moving, Performing }
public enum ArenaSide { North, South }