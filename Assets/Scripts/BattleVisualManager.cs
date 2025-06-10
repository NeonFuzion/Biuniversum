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

        Transform[] entityTransforms = BattleManager.Instance.BattleDataList.Select(x => x.Transform).ToArray();
        for (int i = 0; i < BattleManager.Instance.BattleDataList.Count; i++)
        {
            GameObject battleVisualsGameObject = Instantiate(prefabBattleVisuals.gameObject, entityTransforms[i]);
            BattleVisuals battleVisuals = battleVisualsGameObject.GetComponent<BattleVisuals>();
            battleVisualsList.Add(battleVisuals);
            battleVisuals.InitializeCharacter(i);
            battleVisuals.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetupCharacter(object sender, BattleManager.CharacterIndexArgs args)
    {
        for (int i = 0; i < battleVisualsList.Count; i++)
            battleVisualsList[i].gameObject.SetActive(args.Index == i);
        battleVisualsList[args.Index].SelectCharacter();
    }

    public void AddStep(object sender, BattleManager.MovementArgs args)
    {
        battleVisualsList[args.Index].AddStep(args.Step);
    }

    public void SetCameraPosition(object sender, BattleManager.EntityArgs args)
    {
        if (BattleManager.Instance.ArenaSide != ArenaSide.South) return;
        Camera.main.transform.position = new(0, 3, 5);
        Camera.main.transform.eulerAngles = new(30, 180, 0);
    }
}
