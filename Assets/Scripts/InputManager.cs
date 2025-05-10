using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManager : NetworkBehaviour
{
    [SerializeField] List<EntityManager> characters;
    [SerializeField] UnityEvent onSelectMovement, onSelectAction, onEndSelection;

    ArenaSide side;
    EntityTurnData[] turnData;
    CycleStage cycleStage;
    List<Vector2Int> currentMovement;
    List<EntityBattleData> battleDataList;

    int actionIndex, entityIndex, maxSteps;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        turnData = new EntityTurnData[characters.Count];
        actionIndex = 0;
        entityIndex = 0;
        side = ArenaSide.South;
        for (int i = 0; i < characters.Count; i++)
        {
            Vector2Int position = new Vector2Int(i + (i > characters.Count / 2 ? 1 : 0), -3);
            EntityBattleData data = new EntityBattleData(characters[i], side, position);
            battleDataList.Add(data);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        side = (ArenaSide)(NetworkManager.Singleton.LocalClientId + 1);
    }

    void EndSelection()
    {
        Debug.Log("Ending selection");
        actionIndex = 0;
        entityIndex = 0;
        cycleStage = CycleStage.Cycling;

        onEndSelection?.Invoke();
        BattleData.Instance.SendTurnData(turnData.ToList());
    }

    void SetupMovement()
    {
        if (cycleStage == CycleStage.Cycling) return;
        Debug.Log("Setting up movement");
        cycleStage = CycleStage.SelectingMovement;
        currentMovement.Clear();
        maxSteps = battleDataList[entityIndex].EntityManager.Entity.MoveTiles;
        battleDataList[entityIndex].EntityManager.ActionVisual.SetMovement();
    }

    public void MovementInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (actionIndex != 0) return;
        if (currentMovement.Count >= maxSteps) return;
        if (cycleStage == CycleStage.Cycling) return;
        Debug.Log("Selecting path");
        Vector2 input = context.action.ReadValue<Vector2>();
        Vector2Int movement = new (Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y));

        if (Mathf.Abs(movement.x) == Mathf.Abs(movement.y)) return;
        if (currentMovement.Count > 0) movement += currentMovement[currentMovement.Count - 1];

        Vector2Int worldPosition = movement + battleDataList[entityIndex].Position;
        if (currentMovement.Count > 0) worldPosition += currentMovement[currentMovement.Count - 1];
        Debug.Log(worldPosition);
        if (!BattleData.Instance.IsPositionClamped(worldPosition)) return;

        if (currentMovement.Contains(worldPosition)) return;
        ActionVisual visual = battleDataList[entityIndex].EntityManager.ActionVisual;
        visual.AddSteps(EntityObject.TileToWorldPosition(movement));
        currentMovement.Add(movement);
    }

    public void RemoveStep(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (actionIndex != 0) return;
        if (cycleStage == CycleStage.Cycling) return;
        if (currentMovement.Count == 0) return;
        Debug.Log("Removing steps");
        currentMovement.RemoveAt(currentMovement.Count - 1);
        ActionVisual visual = battleDataList[entityIndex].EntityManager.ActionVisual;
        visual.RemoveSteps();
    }

    public void ActionInput(int actionChoice)
    {
        EntityTurnData currentTurnData = new EntityTurnData(actionChoice, currentMovement.ToArray());
        turnData[entityIndex] = currentTurnData;
        actionIndex = 0;
        entityIndex++;
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
}

public enum CycleStage { None, SelectingMovement, SelectingAction, Cycling }