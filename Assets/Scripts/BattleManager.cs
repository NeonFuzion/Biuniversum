using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class BattleManager : NetworkBehaviour
{
    [SerializeField] GameObject prefabEntity, actionMenu, endMovmentButton, camera;
    [SerializeField] UnityEvent onContinue, onEndSelection;
    [SerializeField] List<Entity> entities;

    EntityTurnData[] turnData;
    List<Vector2Int> currentMovement;
    SelectionPhase selectionPhase;
    ArenaSide arenaSide;

    int[] turnOrder;
    int entityIndex, actionIndex, maxSteps, speedIndex;

    void Awake()
    {

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

    public override void OnNetworkSpawn()
    {
        currentMovement = new();
        arenaSide = (ArenaSide)(NetworkManager.Singleton.LocalClientId + 1);

        actionIndex = 0;
        entityIndex = 0;
        selectionPhase = SelectionPhase.None;
        SetNextControllableCharacter();
    }

    void SetNextControllableCharacter()
    {
        while (true)
        {
            if (entityIndex >= BattleData.Instance.GetList.Count)
            {
                EndSelection();
                break;
            }
            else
            {
                EntityBattleData currentBattleData = BattleData.Instance.GetList[entityIndex];
                EnemyAI script = currentBattleData.EntityManager.Entity.EnemyAI;
                if (script == null && currentBattleData.ArenaSide == arenaSide) break;
                entityIndex++;
            }
        }
    }

    void EndSelection()
    {
        Debug.Log("Ending selection");
        actionIndex = 0;
        entityIndex = 0;
        selectionPhase = SelectionPhase.Cycling;
        endMovmentButton.SetActive(false);
        
        for (int i = 0; i < turnData.Length; i++)
        {
            if (turnData[i] != null) continue;
            turnData[i] = new EntityTurnData(-1, new Vector2Int[] {});
        }
        turnOrder = BattleData.Instance.GetSpeedBracket();
        PreformCycle();
    }

    void SetupMovement()
    {
        if (selectionPhase == SelectionPhase.Cycling) return;
        Debug.Log("Setting up movement");
        selectionPhase = SelectionPhase.Movement;
        endMovmentButton.SetActive(true);
        currentMovement.Clear();
        maxSteps = BattleData.Instance.GetList[entityIndex].EntityManager.Entity.MoveTiles;
        BattleData.Instance.GetList[entityIndex].EntityManager.ActionVisual.SetMovement();
    }

    Vector2Int[] CheckPath(Vector2Int[] movement)
    {
        Debug.Log("Raw: " + string.Join(", ", movement));
        Vector2Int currentPosition = BattleData.Instance.GetList[speedIndex].Position;
        for (int i = 0; i < movement.Length; i++)
        {
            Vector2Int newPosition = currentPosition + movement[i];

            //Debug.Log(entityIndex + " | " + newPosition + " | " + string.Join(", ", BattleData.Instance.GetList.Select(data => data.Position)));
            if (BattleData.Instance.IsPositionEmpty(newPosition)) continue;
            Debug.Log("Unfiltered: " + string.Join(", ", movement));
            List<Vector2Int> transfer = movement.ToList().GetRange(0, i);
            Debug.Log("Filtered: " + string.Join(", ", transfer));
            return transfer.ToArray();
        }
        return movement;
    }

    public void SpawnEntities(List<Entity> allEntities, List<ArenaSide> arenaSides)
    {
        for (int i = 0; i < BattleData.Instance.GetList.Count; i++)
        {
            bool flip = arenaSides[i] == ArenaSide.North;
            Vector2Int position = new (flip ? 3 : -3, i - 2);
            Vector3 worldPosition = EntityObject.TileToWorldPosition(position) + Vector3.up * 2;
            GameObject newEntity = Instantiate(prefabEntity, worldPosition, Quaternion.identity);
            newEntity.gameObject.name = allEntities[i].Name;
            EntityManager entityManager = newEntity.GetComponent<EntityManager>();
            entityManager.Entity = allEntities[i];

            EntityBattleData currentBattleData = new (entityManager, arenaSide, position);
            entityManager.ModelManager.transform.eulerAngles = new Vector3(0, flip ? 180 : 0, 0);
            entityManager.EntityObject.OnFinishActionState.AddListener(IncrementCycle);
            entityManager.EntityObject.Initizalize(currentBattleData);
            entityManager.Initialize();
            BattleData.Instance.AddBattleData(currentBattleData);

            camera.transform.position = new (0, 3, flip ? 5 : -5);
            camera.transform.eulerAngles = new (30, flip ? 180 : 0, 0);
        }
    }

    public void MovementInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (actionIndex != 0) return;
        if (currentMovement.Count >= maxSteps) return;
        if (selectionPhase == SelectionPhase.Cycling) return;
        Debug.Log("Selecting path");
        Vector2 input = context.action.ReadValue<Vector2>();
        Vector2Int movement = new (Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y));

        if (Mathf.Abs(movement.x) == Mathf.Abs(movement.y)) return;
        if (currentMovement.Count > 0) movement += currentMovement[currentMovement.Count - 1];

        Vector2Int worldPosition = movement + BattleData.Instance.GetList[entityIndex].Position;
        if (currentMovement.Count > 0) worldPosition += currentMovement[currentMovement.Count - 1];
        Debug.Log(worldPosition);
        if (!BattleData.Instance.IsPositionClamped(worldPosition)) return;

        if (currentMovement.Contains(worldPosition)) return;
        ActionVisual visual = BattleData.Instance.GetList[entityIndex].EntityManager.ActionVisual;
        visual.AddSteps(EntityObject.TileToWorldPosition(movement));
        currentMovement.Add(movement);
    }

    public void RemoveStep(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (actionIndex != 0) return;
        if (selectionPhase == SelectionPhase.Cycling) return;
        if (currentMovement.Count == 0) return;
        Debug.Log("Removing steps");
        currentMovement.RemoveAt(currentMovement.Count - 1);
        ActionVisual visual = BattleData.Instance.GetList[entityIndex].EntityManager.ActionVisual;
        visual.RemoveSteps();
    }

    public void ActionInput(int actionChoice)
    {
        EntityTurnData currentTurnData = new EntityTurnData(actionChoice, currentMovement.ToArray());
        turnData[entityIndex] = currentTurnData;
        HideVisuals();
        actionIndex = 0;
        selectionPhase = SelectionPhase.Action;
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
        speedIndex = turnOrder[entityIndex];
        EntityBattleData currentBattleData = BattleData.Instance.GetList[speedIndex];

        if (!currentBattleData.EntityManager.gameObject.activeInHierarchy)
        {
            actionIndex = 1;
            IncrementCycle();
            return;
        }
        EntityTurnData currentTurnData = turnData[speedIndex];
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
                Debug.Log("Premove" + entityIndex + ": " + string.Join(", ", BattleData.Instance.GetList.Select(data => data.Position)));
                BattleData.Instance.UpdatePosition(speedIndex, checkedPath);
                Debug.Log("Postmove" + entityIndex + ": " + string.Join(", ", BattleData.Instance.GetList.Select(data => data.Position)));

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

        Debug.Log(entityIndex + " | " + BattleData.Instance.GetList.Count);
        if (entityIndex >= BattleData.Instance.GetList.Count)
        {
            actionIndex = 0;
            entityIndex = 0;
            selectionPhase = SelectionPhase.Movement;
            
            turnData = new EntityTurnData[BattleData.Instance.GetList.Count];
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
        if (selectionPhase != SelectionPhase.Action) return;
        actionMenu.SetActive(true);
        endMovmentButton.SetActive(false);
        EntityManager entityManager = BattleData.Instance.GetList[entityIndex].EntityManager;
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
        BattleData.Instance.GetList[entityIndex].EntityManager.ActionVisual.ShowEffectedTiles(action);
    }

    public void HideEffectedTiles()
    {
        BattleData.Instance.GetList[entityIndex].EntityManager.ActionVisual.HideEffectedTiles();
    }

    public void ShowVisuals(Action action)
    {
        BattleData.Instance.GetList[entityIndex].EntityManager.ActionVisual.ShowVisuals(action);
    }

    public void HideVisuals()
    {
        BattleData.Instance.GetList[entityIndex].EntityManager.ActionVisual.HideVisuals();
    }
}

public class EntityTurnData
{
    public int ActionChoice { get; private set; }
    
    public Vector2Int[] Movement { get; private set; }

    public EntityTurnData(int actionChoice, Vector2Int[] movement)
    {
        ActionChoice = actionChoice;

        Movement = movement;
    }
}

public class EntityBattleData
{
    EntityManager entityManager;
    ArenaSide arenaSide;
    Vector2Int position;

    public EntityManager EntityManager { get => entityManager; set => entityManager = value; }
    public ArenaSide ArenaSide { get => arenaSide; set => arenaSide = value; }
    public Vector2Int Position { get => position; set => position = value; }

    public EntityBattleData(EntityManager entityManager, ArenaSide arenaSide, Vector2Int position)
    {
        this.entityManager = entityManager;
        this.arenaSide = arenaSide;
        this.position = position;
    }
}

public enum ActionStage { Moving, Performing }
public enum ArenaSide { None, North, South }
public enum SelectionPhase { None, Movement, Action, Cycling }