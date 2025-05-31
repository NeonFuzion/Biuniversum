using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class BattleVisuals : MonoBehaviour
{
    [SerializeField] Transform planningTransform, pointer, camera;
    [SerializeField] List<GameObject> actionButtons, effectTiles;

    LineRenderer lineRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        BattleManager.Instance.OnCharacterSelected += SetupCharacter;
        BattleManager.Instance.OnMovement += OnAddStep;
        BattleManager.Instance.OnSendEntities += SetCameraPosition;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowEffectedTiles(int index)
    {
        if (BattleManager.Instance.BattleDataList.Count == 0) return;
        BattleData data = BattleManager.Instance.BattleDataList[BattleManager.Instance.EntityIndex];
        Vector2Int[] effectedPositions = data.EntitySO.Actions[index].EffectedPositions;
        planningTransform.position = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        for (int i = 0; i < effectTiles.Count; i++)
        {
            bool isActive = i < effectedPositions.Length;
            effectTiles[i].SetActive(isActive);

            if (!isActive) continue;
            Vector2Int step = effectedPositions[i];
            effectTiles[i].transform.position = new Vector3(step.x, 1, step.y) - transform.position;
        }
    }

    public void HideEffectedTiles()
    {
        effectTiles.ForEach(x => x.SetActive(false));
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
        lineRenderer.SetPosition(index, lineRenderer.GetPosition(index - 1) + worldStep);
    }

    public void SetupCharacter(object sender, BattleManager.CharacterIndexArgs args)
    {
        int index = args.Index;
        Debug.Log(index + " | " + BattleManager.Instance.BattleDataList.Count);
        TurnData turnData = BattleManager.Instance.TurnDataList[index];
        List<BattleData> list = BattleManager.Instance.BattleDataList;
        Vector2Int position = list[index].Position;
        transform.position = new(position.x, 0, position.y);
        lineRenderer.positionCount = 1;
        lineRenderer.enabled = true;
        pointer.gameObject.SetActive(true);
        turnData.Movement.ForEach(x => AddStep(x));
        HideEffectedTiles();

        for (int i = 0; i < 2; i++)
        {
            actionButtons[i].GetComponentInChildren<TextMeshProUGUI>().SetText(list[index].EntitySO.Actions[i].ActionName);
        }
    }

    public void SetCameraPosition(object sender, BattleManager.EntityArgs args)
    {
        if (BattleManager.Instance.ArenaSide != ArenaSide.South) return;
        camera.position = new(0, 3, 5);
        camera.eulerAngles = new(30, 180, 0);
    }
}