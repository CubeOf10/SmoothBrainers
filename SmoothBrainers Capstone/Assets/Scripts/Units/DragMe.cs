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
    public float timeBetweenPoints = 0.5f;
    public float distanceBetweenPoints = 0.2f;
    public GameObject marker;
    public GameObject groundPos;
    GameObject markerHolder;
    void Start()
    {
        markerHolder = new GameObject();
        markerHolder.name = "Marker Holder";
    }
    IEnumerator PlacePoint()
    {
        yield return new WaitForSeconds(timeBetweenPoints);
        if(BeingDragged)
        {
            if(Vector2.Distance(new Vector3(transform.position.x, transform.position.y, transform.position.z), new Vector3(
                                    pathPoints[pathPoints.Count-1].x, 
                                    pathPoints[pathPoints.Count-1].y, 
                                    pathPoints[pathPoints.Count-1].z)) >
                                    distanceBetweenPoints){

                GameObject newMarker = Instantiate(marker, markerHolder.transform);
                newMarker.transform.position = findTable(transform.position); 
                pathPoints.Add(findTable(transform.position));
            }
        }
        if(!BeingDragged)
        {
            yield break;
        }
        
        StartCoroutine(PlacePoint());
    }

    Vector3 findTable(Vector3 originalPos)
    {
        GameObject groundMarker = Instantiate(groundPos);
        groundMarker.transform.position = transform.position;

        RaycastHit[] hits;
        hits = Physics.RaycastAll(originalPos, -Vector3.up);
        {   
            for(int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                //print("hit " + hit.transform.name + hit.distance);

                if(hit.transform.name == "Terrain")
                {
                //Debug.Log("Hit Terrain");
                    return new Vector3(originalPos.x, hit.transform.position.y, originalPos.z);
                }
            }
        }
        return new Vector3(originalPos.x, originalPos.y, originalPos.z);
    }
    public void PickedUp()
    {
        BeingDragged = true;
        pathPoints.Add(findTable(transform.position));
        StartCoroutine(PlacePoint());
    }
    public void PutDown()
    {
        BeingDragged = false;
    }
    void OnDestroy()
    {
        Destroy(markerHolder);
    }
}
