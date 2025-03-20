using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        BattleDataManager = new ();

        foreach (EntityBattleData currentBattleData in battleData)
        {
            bool flip = currentBattleData.ArenaSide == ArenaSide.North;
            EntityManager manager = currentBattleData.EntityManager;
            manager.ModelManager.transform.eulerAngles = new Vector3(0, flip ? 180 : 0, 0);
            manager.EntityObject.OnFinishActionState.AddListener(IncrementCycle);
            manager.EntityObject.Initizalize(currentBattleData);
            manager.Initialize();
        }
        BattleDataManager = new (battleData);

        BattleDataManager.SortBySpeed();
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
        while (true)
        {
            if (entityIndex >= BattleDataManager.Count)
            {
                EndSelection();
                break;
            }
            else
            {
                EnemyAI script = BattleDataManager.GetData(entityIndex).EntityManager.Entity.EnemyAI;
                if (script != null) break;
                entityIndex++;
            }
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
        maxSteps = BattleDataManager.GetData(entityIndex).EntityManager.Entity.MoveTiles;
        currentMovement = new Vector2Int[maxSteps];
        BattleDataManager.GetData(entityIndex).EntityManager.ActionVisual.SetMovement();
    }

    void CheckPath()
    {
        //Vector2Int currentPosition = 
        foreach (Vector2Int position in currentMovement)
        {

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

        Vector2Int worldPosition = movement + BattleDataManager.GetPosition(entityIndex);
        if (worldPosition.x > eastClamp) return;
        if (worldPosition.x < westClamp) return;
        if (worldPosition.y > northClamp) return;
        if (worldPosition.y < southClamp) return;

        BattleDataManager.GetData(entityIndex).EntityManager.ActionVisual.AddSteps(EntityObject.TileToWorldPosition(movement));
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
        EntityBattleData currentBattleData = BattleDataManager.GetData(entityIndex);
        EntityTurnData currentTurnData = turnData[entityIndex];
        Vector2Int[] movement = currentTurnData.Movement;
        int actionChoice = currentTurnData.ActionChoice;

        if (currentBattleData.EntityManager.Entity.EnemyAI)
        {
            Entity entity = currentBattleData.EntityManager.Entity;
            movement = entity.EnemyAI.ChooseMovement(currentBattleData);
            actionChoice = entity.EnemyAI.ChooseAction(currentBattleData);
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

        Debug.Log(entityIndex + " | " + BattleDataManager.Count);
        if (entityIndex >= BattleDataManager.Count)
        {
            BattleDataManager.SortBySpeed();
            actionIndex = 0;
            entityIndex = 0;
            actionSelectable = true;
            cyclingTurn = false;
            
            turnData = new EntityTurnData[BattleDataManager.Count];
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
        EntityObject entityObject = BattleDataManager.GetData(entityIndex).EntityManager.EntityObject;
        EntityManager entityManager = BattleDataManager.GetData(entityIndex).EntityManager;
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
        BattleDataManager.GetData(entityIndex).EntityManager.ActionVisual.ShowEffectedTiles(action);
    }

    public void HideEffectedTiles()
    {
        BattleDataManager.GetData(entityIndex).EntityManager.ActionVisual.HideEffectedTiles();
    }

    public void ShowVisuals(Action action)
    {
        BattleDataManager.GetData(entityIndex).EntityManager.ActionVisual.ShowVisuals(action);
    }

    public void HideVisuals()
    {
        BattleDataManager.GetData(entityIndex).EntityManager.ActionVisual.HideVisuals();
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
    List<EntityBattleData> battleDataManager;

    public BattleDataManager(List<EntityBattleData> battleDataManager = null)
    {
        this.battleDataManager = new ();

        if (battleDataManager == null) return;
        foreach (EntityBattleData data in battleDataManager)
        {
            Vector3 entityPosition = data.EntityManager.transform.position;
            data.Position = EntityObject.WorldToTilePosition(entityPosition);
            this.battleDataManager.Add(data);
        }
    }

    public ReadOnlyCollection<EntityBattleData> GetList => battleDataManager.AsReadOnly();

    public int Count => battleDataManager.Count;

    public Vector2Int GetPosition(int index) => battleDataManager[index].Position;

    public EntityBattleData GetData(int index) => battleDataManager[index];

    public void AddEntityPosition(EntityBattleData data) => battleDataManager.Add(data);

    public void UpdatePosition(EntityManager entityManager, Vector2Int position)
    {
        foreach (EntityBattleData data in battleDataManager)
        {
            if (data.EntityManager != entityManager) continue;
            data.Position = position;
            return;
        }
    }

    public void SortBySpeed()
    {
        List<EntityBattleData> output = new ();

        System.Random random = new ();
        int lastMaxSpeed = int.MaxValue;
        while (output.Count < battleDataManager.Count)
        {
            int maxSpeed = (int)battleDataManager.Max(data => data.EntityManager.Entity.Speed * (data.EntityManager.Entity.Speed >= lastMaxSpeed ? 0 : 1));
            var fastest = battleDataManager.Where(data => (int)data.EntityManager.Entity.Speed == maxSpeed);
            lastMaxSpeed = maxSpeed;
            output.AddRange(fastest);
        }
        battleDataManager = output;
        Debug.Log(string.Join(", ", battleDataManager.Select(x => x.EntityManager.gameObject.name)));
    }
}

public enum ActionStage { Moving, Performing }
public enum ArenaSide { North, South }