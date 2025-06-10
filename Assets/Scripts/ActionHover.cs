using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ActionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] int index;
    [SerializeField] UnityEvent<int> onPointerEnter, onPointerExit;

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
        onPointerEnter?.Invoke(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExit?.Invoke(index);
    }
}
