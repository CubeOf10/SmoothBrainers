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
                    Debug.Log("pushed");

        foreach(GameObject unit in gameManager.GetComponent<GameManager>().units)
        {
            //Sleep
            unit.GetComponent<Entity>().entityBehaviour = Entity.EntityBehaviours.Idle;
            
        }
    }

    void UpdateList()
    {
    }
}
