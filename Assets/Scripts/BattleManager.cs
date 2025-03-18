using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [SerializeField] int northClamp, eastClamp, southClamp, westClamp;
    [SerializeField] GameObject actionMenu, endMovmentButton;
    [SerializeField] UnityEvent onContinue, onEndSelection;
    [SerializeField] List<EntityBattleData> battleData;

    public static BattleDataManager BattleDataManager;
    
    EntityTurnData[] turnData;
    Vector2Int[] currentMovement;

    int entityIndex, actionIndex, maxSteps, stepCount;
    bool actionSelectable, cyclingTurn;

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
        cyclingTurn = false;
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
        Debug.Log("Ending selection");
        actionIndex = 0;
        entityIndex = 0;
        endMovmentButton.SetActive(false);
        
        for (int i = 0; i < turnData.Length; i++)
        {
            if (turnData[i] != null) continue;
            turnData[i] = new EntityTurnData(-1, new Vector2Int[] {});
        }
        cyclingTurn = true;
        PreformCycle();
    }

    void SetupMovement()
    {
        if (cyclingTurn) return;
        Debug.Log("Setting up movement");
        stepCount = 0;
        actionSelectable = true;
        cyclingTurn = false;
        endMovmentButton.SetActive(true);
        maxSteps = battleData[entityIndex].EntityManager.Entity.MoveTiles;
        currentMovement = new Vector2Int[maxSteps];
        battleData[entityIndex].EntityManager.ActionVisual.SetMovement();
    }

    void CheckPath()
    {
        //Vector2Int currentPosition = 
        foreach (Vector2Int position in currentMovement)
        {

        }
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
        if (cyclingTurn) return;
        Debug.Log("Selecting path");
        Vector2 input = context.action.ReadValue<Vector2>();
        Vector2Int movement = new (Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y));

        if (Mathf.Abs(movement.x) == Mathf.Abs(movement.y)) return;
        movement += stepCount <= 0 ? new () : currentMovement[stepCount - 1];

        Vector2Int worldPosition = movement + BattleDataManager.GetPositionByIndex(entityIndex);
        if (worldPosition.x > eastClamp) return;
        if (worldPosition.x < westClamp) return;
        if (worldPosition.y > northClamp) return;
        if (worldPosition.y < southClamp) return;

        battleData[entityIndex].EntityManager.ActionVisual.AddSteps(EntityObject.TileToWorldPosition(movement));
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
        Debug.Log(entityIndex);

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
        Vector2Int[] movement = currentTurnData.Movement;
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
                currentBattleData.EntityManager.EntityObject.PerformMovement(movement.Select(x => EntityObject.TileToWorldPosition(x)).ToArray());
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

        Debug.Log(entityIndex + " | " + battleData.Count);
        if (entityIndex >= battleData.Count)
        {
            SortEntitiesBySpeed();
            actionIndex = 0;
            entityIndex = 0;
            actionSelectable = true;
            cyclingTurn = false;
            
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

    Vector2Int[] movement;
    
    public int ActionChoice { get => actionChoice; set => actionChoice = value; }
    
    public Vector2Int[] Movement { get => movement; set => movement = value; }

    public EntityTurnData(int actionChoice, Vector2Int[] movement)
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
    [SerializeField] Vector2Int position;

    public EntityManager EntityManager { get => entityManager; }
    public ArenaSide ArenaSide { get => arenaSide; }
    public Vector2Int Position { get => position; set => position = value; }

    public EntityBattleData(EntityManager entityManager, ArenaSide arenaSide, Vector2Int position)
    {
        this.entityManager = entityManager;
        this.arenaSide = arenaSide;
        this.position = position;
    }
}

public class BattleDataManager
{
    List<EntityBattleData> battleData;

    public BattleDataManager(List<EntityBattleData> battleData)
    {
        this.battleData = new ();
        foreach (EntityBattleData data in battleData)
        {
            Vector3 entityPosition = data.EntityManager.transform.position;
            data.Position = EntityObject.WorldToTilePosition(entityPosition);
            this.battleData.Add(data);
        }
    }

    public Vector2Int GetPositionByIndex(int index) => battleData[index].Position;

    public void AddEntityPosition(EntityBattleData data) => battleData.Add(data);

    public void UpdatePosition(EntityManager entityManager, Vector2Int position)
    {
        foreach (EntityBattleData data in battleData)
        {
            if (data.EntityManager != entityManager) continue;
            data.Position = position;
            return;
        }
    }

    public void SortBySpeed()
    {
        List<EntityBattleData> dataCopy = battleData.Select(data => data).ToList();

        battleData.Clear();
        System.Random random = new ();
        while (dataCopy.Count > 0)
        {
            int maxSpeed = (int)dataCopy.Max(data => data.EntityManager.Entity.Speed);
            var fastest = dataCopy.Where(data => (int)data.EntityManager.Entity.Speed == maxSpeed);
            battleData.AddRange(fastest);
        }
        /*
        List<EntityBattleData> fastest = new ();
        battleData.Clear();
        while (dataCopy.Count > 0)
        {
            int maxSpeed = dataCopy.Max(x => (int)x.EntityManager.Entity.Speed);
            for (int i = 0; i < dataCopy.Count; i++)
            {
                EntityBattleData data = dataCopy[i];
                if (data.EntityManager.Entity.Speed != maxSpeed) continue;
                fastest.Add(data);
                dataCopy.RemoveAt(i--);
            }
            while (fastest.Count > 0)
            {
                int rand = UnityEngine.Random.Range(0, fastest.Count);
                battleData.Add(fastest[rand]);
                fastest.RemoveAt(rand);
            }
        }*/
    }
}

public enum ActionStage { Moving, Performing }
public enum ArenaSide { North, South }