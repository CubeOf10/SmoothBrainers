using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PlayPaths : ButtonEffect
{
    GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
    public override void ButtonPressed()
    {        
        foreach(GameObject unit in gameManager.GetComponent<GameManager>().units)
        {
            unit.GetComponent<Entity>().moveTargetIndex = 0;
            unit.transform.position = unit.GetComponent<DragMe>().getMarkerHolder().GetChild(0).transform.position;
            
            unit.GetComponent<Entity>().entityBehaviour = Entity.EntityBehaviours.Follow;   
        }
    }

    void UpdateList()
    {
    }
}
