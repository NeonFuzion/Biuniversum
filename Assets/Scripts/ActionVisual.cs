using UnityEngine;

public class ActionVisual : MonoBehaviour
{
    [SerializeField] Transform[] tiles;
    [SerializeField] Transform tileManager;

    Transform target;
    LineRenderer lineRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(Transform target)
    {
        this.target = target;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, new Vector3(transform.position.x, 0, transform.position.z));
    }

    public void SetMovement()
    {
        Debug.Log(target.position);
        transform.position = new Vector3(target.position.x, 0, target.position.z);

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 1;
    }

    public void AddSteps(Vector3 movement)
    {
        int lastIndex = lineRenderer.positionCount++;
        lineRenderer.SetPosition(lastIndex, movement);
    }

    public void ShowEffectedTiles(Action action)
    {
        tileManager.position = lineRenderer.GetPosition(lineRenderer.positionCount - 1) + transform.position;
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
    }

    public void ShowVisuals(Action action)
    {
        ShowEffectedTiles(action);
        lineRenderer.enabled = true;
    }
}
