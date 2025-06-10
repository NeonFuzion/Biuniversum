using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BattleVisualManager : MonoBehaviour
{
    [SerializeField] Transform prefabBattleVisuals;
    [SerializeField] List<TextMeshProUGUI> buttonTextList;

    List<BattleVisuals> battleVisualsList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BattleManager.Instance.OnCharacterSelected += SetupCharacter;
        BattleManager.Instance.OnMovement += AddStep;
        BattleManager.Instance.OnSendEntities += SetCameraPosition;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetupBattleVisuals()
    {
        battleVisualsList = new();
        Transform[] entityTransforms = BattleManager.Instance.BattleDataList.Select(x => x.Transform).ToArray();
        for (int i = 0; i < BattleManager.Instance.BattleDataList.Count; i++)
        {
            GameObject battleVisualsGameObject = Instantiate(prefabBattleVisuals.gameObject, entityTransforms[i]);
            BattleVisuals battleVisuals = battleVisualsGameObject.GetComponent<BattleVisuals>();
            battleVisualsList.Add(battleVisuals);
            battleVisuals.InitializeCharacter(i);
            battleVisualsGameObject.SetActive(false);
        }
    }

    void SetupCharacter(object sender, BattleManager.CharacterIndexArgs args)
    {
        if (battleVisualsList == null) SetupBattleVisuals();
        for (int i = 0; i < battleVisualsList.Count; i++)
            battleVisualsList[i].gameObject.SetActive(args.Index == i);
        battleVisualsList[args.Index].SelectCharacter();

        ActionSO[] actionTextList = BattleManager.Instance.BattleDataList[args.Index].EntitySO.Actions;
        for (int i = 0; i < 2; i++)
            buttonTextList[i].SetText(actionTextList[i].ActionName);
    }

    void AddStep(object sender, BattleManager.MovementArgs args)
    {
        if (!args.CanAdd) return;
        battleVisualsList[args.Index].AddStep(args.Step);
    }

    public void SetCameraPosition(object sender, BattleManager.EntityArgs args)
    {
        if (BattleManager.Instance.ArenaSide != ArenaSide.South) return;
        Camera.main.transform.position = new(0, 3, 5);
        Camera.main.transform.eulerAngles = new(30, 180, 0);
    }

    public void ShowEffectedTiles(int index)
    {
        int entityIndex = BattleManager.Instance.EntityIndex;
        if (battleVisualsList == null) return;
        battleVisualsList[entityIndex].ShowEffectedTiles(index);
    }

    public void HideEffectedTiles(int index)
    {
        int entityIndex = BattleManager.Instance.EntityIndex;
        int existingActionIndex = BattleManager.Instance.TurnDataList[entityIndex].ActionIndex;

        if (index == existingActionIndex) return;
        if (battleVisualsList == null) return;
        BattleVisuals battleVisuals = battleVisualsList[entityIndex];
        if (existingActionIndex == -1) battleVisuals.HideEffectedTiles();
        else battleVisuals.ShowEffectedTiles(existingActionIndex);
    }

    public void ShowAll()
    {
        if (battleVisualsList == null) return;
        battleVisualsList.ForEach(x => x.SelectCharacter());
    }

    public void HideAll()
    {
        if (battleVisualsList == null) return;
        int index = BattleManager.Instance.EntityIndex;
        battleVisualsList[index].SelectCharacter();
    }
}
