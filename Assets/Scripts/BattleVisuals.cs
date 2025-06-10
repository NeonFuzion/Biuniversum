using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class BattleVisuals : MonoBehaviour
{
    [SerializeField] Transform planningTransform, pointer;
    [SerializeField] TextMeshProUGUI nameText, healthText;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] List<Transform> effectTiles;

    int entityIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowEffectedTiles(int index)
    {
        if (BattleManager.Instance.BattleDataList.Count == 0) return;
        BattleData data = BattleManager.Instance.BattleDataList[entityIndex];
        Vector2Int[] effectedPositions = data.EntitySO.Actions[index].EffectedPositions;
        for (int i = 0; i < effectTiles.Count; i++)
        {
            bool isActive = i < effectedPositions.Length;
            effectTiles[i].gameObject.SetActive(isActive);

            if (!isActive) continue;
            Vector2Int step = effectedPositions[i] * BattleManager.Instance.ArenaSideFlip();
            effectTiles[i].localPosition = new Vector3(step.x, 0, step.y);// - transform.position;
        }
    }

    public void HideEffectedTiles()
    {
        effectTiles.ForEach(x => x.gameObject.SetActive(false));
    }

    public void OnAddStep(object sender, BattleManager.MovementArgs args)
    {
        if (!args.CanAdd) return;
        AddStep(args.Step);
    }

    public void AddStep(Vector2 step)
    {
        int index = lineRenderer.positionCount++;
        Vector3 worldStep = new(step.x, 0, step.y);
        Vector3 newPosition = lineRenderer.GetPosition(index - 1) + worldStep;
        lineRenderer.SetPosition(index, newPosition);
        planningTransform.localPosition = newPosition;
    }

    public void InitializeCharacter(int entityIndex)
    {
        this.entityIndex = entityIndex;
        BattleData battleData = BattleManager.Instance.BattleDataList[entityIndex];
        Vector2Int position = battleData.Position;
        transform.position = new(position.x, 0, position.y);
        //nameText.SetText(battleData.EntitySO.EntityName);
    }

    public void SelectCharacter()
    {
        BattleData battleData = BattleManager.Instance.BattleDataList[entityIndex];
        TurnData turnData = BattleManager.Instance.TurnDataList[entityIndex];
        Vector2Int position = battleData.Position;
        transform.position = new(position.x, 0, position.y);
        lineRenderer.positionCount = 1;
        turnData.Movement.ForEach(x => AddStep(x));
        if (turnData.ActionIndex != -1)
            ShowEffectedTiles(turnData.ActionIndex);
        else
            HideEffectedTiles();
        //healthText.SetText(battleData.Health.ToString());
    }
}