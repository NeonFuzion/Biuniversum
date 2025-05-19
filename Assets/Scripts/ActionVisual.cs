using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionVisual : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] List<GameObject> actionAreaTiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddStep(Vector2Int step)
    {
        int lastIndex = lineRenderer.positionCount++ - 1;
        Vector3 lastPosition = lineRenderer.GetPosition(lastIndex);
        Vector3 worldStep = new (step.x, 0, step.y);
        lineRenderer.SetPosition(lastIndex, lastPosition + worldStep);
    }

    public void ShowEffectArea(List<Vector2Int> area)
    {
        for (int i = 0; i < actionAreaTiles.Count; i++)
        {
            GameObject tile = actionAreaTiles[i];
            bool isActive = area.Count < i;
            tile.SetActive(isActive);

            if (!isActive) continue;
            tile.transform.position = new (area[i].x, tile.transform.position.y, area[i].y);
        }
    }

    public void ShowSteps(List<Vector2Int> steps)
    {

    }

    public void HideVisuals()
    {
        lineRenderer.enabled = false;
        actionAreaTiles.ForEach(tile => tile.SetActive(false));
    }

    public void ShowVisuals(int entityIndex, TurnData turnData)
    {
        lineRenderer.enabled = true;
        
    }

    public void SetTarget(int index)
    {
        
    }
}
