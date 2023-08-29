using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class DragMe : MonoBehaviour
{
    public bool BeingDragged = false;
    public List<Vector3> pathPoints;
    public float distanceToPlane = 0.4f;
    public float timeBetweenPoints = 1.0f;
    public float distanceBetweenPoints = 0.2f;
    public GameObject marker;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    IEnumerator PlacePoint()
    {
        yield return new WaitForSeconds(timeBetweenPoints);
        if(BeingDragged && Vector2.Distance(new Vector3(transform.position.x, transform.position.y, transform.position.z), 
                                            new Vector3(pathPoints[pathPoints.Count].x,
                                                pathPoints[pathPoints.Count].y, pathPoints[pathPoints.Count].z)) 
                                            > distanceBetweenPoints)
        {
            pathPoints.Add(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            GameObject newMarker = Instantiate(marker); 

            newMarker.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            Debug.Log("Point Added at " + pathPoints[pathPoints.Count-1].ToString());
        }
        if(!BeingDragged)
        {
            yield break;
        }
        
        StartCoroutine(PlacePoint());
    }

    public void PickedUp()
    {
        BeingDragged = true;
        pathPoints.Add(new Vector3(transform.position.x, transform.position.y, transform.position.z));
        StartCoroutine(PlacePoint());
    }
    public void PutDown()
    {
        BeingDragged = false;
    }
}
