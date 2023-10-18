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
    public float timeBetweenPoints = 0.5f;
    public float distanceBetweenPoints = 0.2f;
    public GameObject marker;
    public GameObject desk;
    public GameObject groundPos;
    GameObject markerHolder;
    void Start()
    {
        desk = GameObject.Find("Terrain");
        markerHolder = new GameObject();
        markerHolder.name = "Marker Holder";
    }
    public Transform getMarkerHolder()
    {
        return markerHolder.transform;
    }
    IEnumerator PlacePoint()
    {
        yield return new WaitForSeconds(timeBetweenPoints);
        if(AboveTable(transform.position))
        {
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
                

                if(hit.transform.gameObject == desk)
                {
                    return new Vector3(originalPos.x, hit.transform.position.y, originalPos.z);
                }
            }
        }
        return new Vector3(originalPos.x, originalPos.y, originalPos.z);
    }
    bool AboveTable(Vector3 entityPos)
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(entityPos, -Vector3.up);
        {   
            for(int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if(hit.transform.gameObject == desk)
                    return true;
            }
        }
        return false;
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
