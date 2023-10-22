using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//Will place markers to denote the path of manually controlled units. 
// Only runs while the unit is picked up, as determined by the BeingDragged Boolean
public class DragMe : MonoBehaviour
{
    bool CoroutineRunning;
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

    //Used in other scripts to iterate over the units markers,  which are stored as 
    // transforms within a hierarchy 
    public Transform getMarkerHolder()
    {
        return markerHolder.transform;
    }

    //Places points every [timeBetweenPoints] seconds. 
    // Needs the unit to be above the table, and picked up by the user
    // Should place markers a set distance apart, though this doesn't seem to work. 
    //          (not a major issue, gives refined control of units if dragging slowly)
    IEnumerator PlacePoint()
    {
        yield return new WaitForSeconds(timeBetweenPoints);
        CoroutineRunning = true;
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
                    if(gameObject.GetComponent<FollowPath>() != null)
                        newMarker.transform.position = findTable(transform.position); 
                    else
                    {
                        newMarker.transform.position = transform.position;

                        if(newMarker.transform.position.y <= desk.transform.position.y) //Set to desk height if below
                            newMarker.transform.position = 
                            new Vector3(newMarker.transform.position.x, desk.transform.position.y, newMarker.transform.position.z);
                    }
                    pathPoints.Add(findTable(transform.position));
                }
            }
        }
        if(!BeingDragged)
        {
            CoroutineRunning = false;
            yield break;
        }
        
        StartCoroutine(PlacePoint());
    }

    //Find a spot on the table directly below the game object
    Vector3 findTable(Vector3 originalPos)
    {
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

    //Checks if a position is above the table
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

        if(!CoroutineRunning) //Prevent multiple markers being spawned if rapidly grabbed
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
