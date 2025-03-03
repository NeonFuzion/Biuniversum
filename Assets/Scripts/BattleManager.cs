using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    [SerializeField] int northClamp, eastClamp, southClamp, westClamp;
    [SerializeField] GameObject actionMenu;
    [SerializeField] UnityEvent onContinue;
    [SerializeField] EntityObject[] objects;
    
    List<Entity> entities;
    List<EntityObject> entityObjects;
    EntityTurnData[] entityTurnData;
    Vector3[] currentMovement;

    int entityIndex, actionIndex, maxSteps, stepCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entities = new List<Entity>();
        entityTurnData = new EntityTurnData[objects.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].OnFinishActionState.AddListener(PreformCycle);
            entities.Add(objects[i].Entity);
            entityObjects.Add(objects[i]);
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
        if (entityIndex >= entities.Count) return;
        while (true)
        {
            Entity currentEntity = entities[entityIndex];

            if (!currentEntity.EnemyAI) break;
            entityIndex++;

            if (entityIndex < entities.Count) continue;
            EndSelection();
        }

    }

    void EndSelection()
    {
        actionIndex = 0;
        entityIndex = 0;
        
        for (int i = 0; i < entityTurnData.Length; i++)
        {
            if (entityTurnData[i] != null) continue;
            entityTurnData[i] = new EntityTurnData(-1, new Vector3[] {});
        }
        PreformCycle();
    }

    void SetupMovement()
    {
        stepCount = 0;
        maxSteps = entities[entityIndex].MoveTiles;
        currentMovement = new Vector3[maxSteps];
        entityObjects[entityIndex].SetMovement();
    }

    void SortEntitiesBySpeed()
    {
        for (int i = entities.Count; i > 0; i--)
        {
            float maxSpeed = int.MinValue;
            int index = 0;
            for (int j = 0; j < i; j++)
            {
                Entity entity = entities[j];
                if (entity.Speed < maxSpeed) continue;
                index = j;
                maxSpeed = entity.Speed;
            }

            List<EntityObject> sameSpeedEntities = new List<EntityObject>();
            for (int j = 0; j < i; j++)
            {
                EntityObject entityObject = entityObjects[j];
                if (entityObject.Entity.Speed != maxSpeed) continue;
                sameSpeedEntities.Add(entityObject);
            }

            

            Entity fastestEntity = entities[index];
            entities.RemoveAt(index);
            entities.Add(fastestEntity);

            EntityObject fastestObject = entityObjects[index];
            entityObjects.RemoveAt(index);
            entityObjects.Add(fastestObject);
        }
    }

    public void MovementInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (actionIndex != 0) return;
        Vector2 input = context.action.ReadValue<Vector2>();
        Vector3 movement = new Vector3(input.x, 0, input.y);
        EntityObject entityObject = entityObjects[entityIndex];
        movement += stepCount <= 0 ? Vector3.zero : currentMovement[stepCount - 1];

        Vector3 worldPosition = movement + entityObject.transform.position;
        Debug.Log(worldPosition);
        if (worldPosition.x > eastClamp) return;
        if (worldPosition.x < westClamp) return;
        if (worldPosition.z > northClamp) return;
        if (worldPosition.z < southClamp) return;

        entityObject.AddSteps(movement);
        currentMovement[stepCount] = movement;
        stepCount++;

        if (stepCount < maxSteps) return;
        actionIndex++;
    }

    public void ActionInput(int actionChoice)
    {
        EntityTurnData turnData = new EntityTurnData(actionChoice, currentMovement);
        entityTurnData[entityIndex] = turnData;
        actionIndex = 0;
        entityIndex++;
        SetNextControllableCharacter();

        if (entityIndex < entities.Count)
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
        if (entityIndex >= entities.Count)
        {
            SortEntitiesBySpeed();
            actionIndex = 0;
            entityIndex = 0;
            
            entityTurnData = new EntityTurnData[entities.Count];
            SetNextControllableCharacter();
            return;
        }

        EntityObject entityObject = entityObjects[entityIndex].GetComponent<EntityObject>();
        EntityTurnData turnData = entityTurnData[entityIndex];
        Vector3[] movement = turnData.Movement;
        int actionChoice = turnData.ActionChoice;
        //Debug.Log($"Turn {entityIndex}: {entityObject.Entity.Name}, {turnData.Movement.Length}");

        if (entityObject.Entity.EnemyAI)
        {
            Entity entity = entityObject.Entity;
            movement = entity.EnemyAI.ChooseMovement(entityObjects.ToArray(), entityObject);
            actionChoice = entity.EnemyAI.ChooseAction(entityObjects.ToArray(), entityObject);
        }

        switch ((ActionStage)actionIndex)
        {
            case ActionStage.Moving:
                entityObject.PerformMovement(movement, entityObjects.ToArray());
                actionIndex++;
                break;
            case ActionStage.Performing:
                entityIndex++;
                actionIndex = 0;
                entityObject.PerformAction(actionChoice, entityObjects.ToArray());
                break;
        }
    }

    public void DisplayActionMenu(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (actionIndex != 1) return;

        actionMenu.SetActive(true);
        Entity currentEntity = entities[entityIndex];
        for (int i = 0; i < actionMenu.transform.childCount; i++)
        {
            GameObject actionButton = actionMenu.transform.GetChild(i).gameObject;
            if (currentEntity.Actions.Length > i)
            {
                TextMeshProUGUI textMeshPro = actionButton.GetComponentInChildren<TextMeshProUGUI>();
                Action currentAction = currentEntity.Actions[i];
                textMeshPro.SetText(currentAction.ActionName);
                actionButton.SetActive(true);
            }
            else
            {
                actionButton.SetActive(false);
            }
        }
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


public enum ActionStage { Moving, Performing }