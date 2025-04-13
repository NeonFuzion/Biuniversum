using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    [SerializeField] GameObject actionMenu, endMovmentButton;
    [SerializeField] UnityEvent onContinue, onEndSelection;
    [SerializeField] List<EntityBattleData> battleData;
    
    EntityTurnData[] turnData;
    List<Vector2Int> currentMovement;

    int entityIndex, actionIndex, maxSteps;
    bool actionSelectable, cyclingTurn;

    void Awake()
    {
        turnData = new EntityTurnData[battleData.Count];
        currentMovement = new();
        BattleData.Initialize();

        foreach (EntityBattleData currentBattleData in battleData)
        {
            bool flip = currentBattleData.ArenaSide == ArenaSide.North;
            EntityManager manager = currentBattleData.EntityManager;
            manager.ModelManager.transform.eulerAngles = new Vector3(0, flip ? 180 : 0, 0);
            manager.EntityObject.OnFinishActionState.AddListener(IncrementCycle);
            manager.EntityObject.Initizalize(currentBattleData);
            manager.EntityObject.transform.position = new (currentBattleData.Position.x, manager.EntityObject.transform.position.y, currentBattleData.Position.y);
            manager.Initialize();
            BattleData.AddBattleData(currentBattleData);
        }

        BattleData.SortBySpeed();
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
            if (entityIndex >= BattleData.GetList.Count)
            {
                EndSelection();
                break;
            }
            else
            {
                EnemyAI script = BattleData.GetList[entityIndex].EntityManager.Entity.EnemyAI;
                if (script == null) break;
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
        actionSelectable = true;
        cyclingTurn = false;
        endMovmentButton.SetActive(true);
        currentMovement.Clear();
        maxSteps = BattleData.GetList[entityIndex].EntityManager.Entity.MoveTiles;
        BattleData.GetList[entityIndex].EntityManager.ActionVisual.SetMovement();
    }

    Vector2Int[] CheckPath(Vector2Int[] movement)
    {
        Debug.Log("Raw: " + string.Join(", ", movement));
        Vector2Int currentPosition = BattleData.GetList[entityIndex].Position;
        for (int i = 0; i < movement.Length; i++)
        {
            Vector2Int newPosition = currentPosition + movement[i];

            //Debug.Log(entityIndex + " | " + newPosition + " | " + string.Join(", ", BattleData.GetList.Select(data => data.Position)));
            if (BattleData.IsPositionEmpty(newPosition)) continue;
            Debug.Log("Unfiltered: " + string.Join(", ", movement));
            List<Vector2Int> transfer = movement.ToList().GetRange(0, i);
            Debug.Log("Filtered: " + string.Join(", ", transfer));
            return transfer.ToArray();
        }
        return movement;
    }

    public void MovementInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (actionIndex != 0) return;
        if (currentMovement.Count >= maxSteps) return;
        if (cyclingTurn) return;
        Debug.Log("Selecting path");
        Vector2 input = context.action.ReadValue<Vector2>();
        Vector2Int movement = new (Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y));

        if (Mathf.Abs(movement.x) == Mathf.Abs(movement.y)) return;
        if (currentMovement.Count > 0) movement += currentMovement[currentMovement.Count - 1];

        Vector2Int worldPosition = movement + BattleData.GetList[entityIndex].Position;
        if (currentMovement.Count > 0) worldPosition += currentMovement[currentMovement.Count - 1];
        Debug.Log(worldPosition);
        if (!BattleData.IsPositionClamped(worldPosition)) return;

        if (currentMovement.Contains(worldPosition)) return;
        ActionVisual visual = BattleData.GetList[entityIndex].EntityManager.ActionVisual;
        visual.AddSteps(EntityObject.TileToWorldPosition(movement));
        currentMovement.Add(movement);
    }

    public void RemoveStep(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (actionIndex != 0) return;
        if (cyclingTurn) return;
        if (currentMovement.Count == 0) return;
        Debug.Log("Removing steps");
        currentMovement.RemoveAt(currentMovement.Count - 1);
        ActionVisual visual = BattleData.GetList[entityIndex].EntityManager.ActionVisual;
        visual.RemoveSteps();
    }

    public void ActionInput(int actionChoice)
    {
        EntityTurnData currentTurnData = new EntityTurnData(actionChoice, currentMovement.ToArray());
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
        EntityBattleData currentBattleData = BattleData.GetList[entityIndex];

        if (!currentBattleData.EntityManager.gameObject.activeInHierarchy)
        {
            actionIndex = 1;
            IncrementCycle();
            return;
        }
        EntityTurnData currentTurnData = turnData[entityIndex];
        int actionChoice = currentTurnData.ActionChoice;
        Vector2Int[] movement = currentTurnData.Movement;
        if (actionChoice == -1)
        {
            Entity entity = currentBattleData.EntityManager.Entity;
            actionChoice = entity.EnemyAI.ChooseAction(currentBattleData);
            movement = entity.EnemyAI.ChooseMovement(currentBattleData);
        }
        //Debug.Log(currentBattleData.EntityManager.Entity.Name + " (" + actionIndex + ") " + ":" + movement.Length);

        switch ((ActionStage)actionIndex)
        {
            case ActionStage.Moving:
                Vector2Int[] checkedPath = CheckPath(movement.ToArray());
                
                if (checkedPath.Length == 0)
                {
                    IncrementCycle();
                    break;
                }
                Debug.Log("Premove" + entityIndex + ": " + string.Join(", ", BattleData.GetList.Select(data => data.Position)));
                BattleData.UpdatePosition(entityIndex, checkedPath);
                Debug.Log("Postmove" + entityIndex + ": " + string.Join(", ", BattleData.GetList.Select(data => data.Position)));

                currentBattleData.EntityManager.EntityObject.PerformMovement(checkedPath.Select(x => EntityObject.TileToWorldPosition(x)).ToArray());
                break;
            case ActionStage.Performing:
                Debug.Log(actionChoice + " | " + currentBattleData.EntityManager.Entity.Actions.Length);
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

        Debug.Log(entityIndex + " | " + BattleData.GetList.Count);
        if (entityIndex >= BattleData.GetList.Count)
        {
            BattleData.SortBySpeed();
            actionIndex = 0;
            entityIndex = 0;
            actionSelectable = true;
            cyclingTurn = false;
            
            turnData = new EntityTurnData[BattleData.GetList.Count];
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
        EntityObject entityObject = BattleData.GetList[entityIndex].EntityManager.EntityObject;
        EntityManager entityManager = BattleData.GetList[entityIndex].EntityManager;
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
        BattleData.GetList[entityIndex].EntityManager.ActionVisual.ShowEffectedTiles(action);
    }

    public void HideEffectedTiles()
    {
        BattleData.GetList[entityIndex].EntityManager.ActionVisual.HideEffectedTiles();
    }

    public void ShowVisuals(Action action)
    {
        BattleData.GetList[entityIndex].EntityManager.ActionVisual.ShowVisuals(action);
    }

    public void HideVisuals()
    {
        BattleData.GetList[entityIndex].EntityManager.ActionVisual.HideVisuals();
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

public enum ActionStage { Moving, Performing }
public enum ArenaSide { North, South }