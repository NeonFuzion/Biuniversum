using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Pointer pointer;

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
        pointer.ShowEffectedTiles(action);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointer.ClearTiles();
    }

    public void Initialize(Action action)
    {
        this.action = action;

        if (!textMeshPro) textMeshPro = GetComponentInChildren<TextMeshProUGUI>();

        textMeshPro.SetText(action.ActionName);
    }
}
