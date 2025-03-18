using UnityEngine;

public class ActionVisual : MonoBehaviour
{
    [SerializeField] Transform[] tiles;
    [SerializeField] Transform tileManager;
    [SerializeField] GameObject pointer;

    LineRenderer lineRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetMovement()
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 1;
        pointer.SetActive(true);
    }

    public void AddSteps(Vector3 movement)
    {
        Debug.Log("Adding steps");
        int lastIndex = lineRenderer.positionCount++;
        lineRenderer.SetPosition(lastIndex, movement);
    }

    public void ShowEffectedTiles(Action action)
    {
        tileManager.localPosition = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        for (int i = 0; i < tiles.Length; i++)
        {
            bool withinBounds = action.EffectTiles.Length > i;
            tiles[i].gameObject.SetActive(withinBounds);
            
            if (!withinBounds) continue;
            Vector2 position = action.EffectTiles[i];
            tiles[i].localPosition = new Vector3(position.x, 0, position.y);
        }
    }

    public void HideEffectedTiles()
    {
        foreach (Transform tile in tiles)
        {
            tile.gameObject.SetActive(false);
        }
    }

    public void HideVisuals()
    {
        HideEffectedTiles();
        lineRenderer.enabled = false;
        pointer.SetActive(false);
    }

    public void ShowVisuals(Action action)
    {
        ShowEffectedTiles(action);
        lineRenderer.enabled = true;
    }
}
