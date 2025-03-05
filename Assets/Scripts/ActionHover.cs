using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] BattleManager battleManager;

    TextMeshProUGUI textMeshPro;
    Action action;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        battleManager.ShowEffectedTiles(action);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        battleManager.HideEffectedTiles();
    }

    public void Initialize(Action action)
    {
        this.action = action;

        if (!textMeshPro) textMeshPro = GetComponentInChildren<TextMeshProUGUI>();

        textMeshPro.SetText(action.ActionName);
    }
}
