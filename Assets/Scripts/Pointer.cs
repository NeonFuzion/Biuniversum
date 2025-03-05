using UnityEngine;

public class Pointer : MonoBehaviour
{
    [SerializeField] GameObject pointer, tileParent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectEntity(GameObject target)
    {
        LineRenderer lineRenderer = target.GetComponent<LineRenderer>();
        tileParent.transform.position = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
        pointer.SetActive(true);
    }

    public void ShowEffectedTiles(Action action)
    {
        for (int i = 0; i < action.EffectTiles.Length; i++)
        {
            for (int j = 0; j < tileParent.transform.childCount; j++)
            {
                Transform tile = tileParent.transform.GetChild(j);
                Vector3 difference = new Vector2(tile.localPosition.x, tile.localPosition.z) - action.EffectTiles[i];

                if (difference.magnitude > 0) continue;
                tile.gameObject.SetActive(true);
                break;
            }
        }
    }

    public void ClearTiles()
    {
        for (int i = 0; i < tileParent.transform.childCount; i++)
        {
            GameObject child = tileParent.transform.GetChild(i).gameObject;
            
            if (!child.activeInHierarchy) continue;
            child.SetActive(false);
        }
    }
}
