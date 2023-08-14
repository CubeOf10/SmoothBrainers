using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedGirafeGames.Agamotto.Scripts.Runtime;
public class timestoneController : MonoBehaviour
{
    TimeStone timeStone;
    // Start is called before the first frame update
    void Start()
    {
        timeStone = gameObject.GetComponent<TimeStone>();    
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("d"))
        {
            timeStone.playbackSpeed*= -1;
        }
        if(Input.GetKeyDown("f"))
        {
            timeStone.recordingStep += 0.05f;
        }
        if(Input.GetKeyDown("g"))
        {
            timeStone.StartPlayback();
        }
    }

}
