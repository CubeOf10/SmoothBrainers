using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace RedGirafeGames.Agamotto.Demo.SimplePhysicsDemo.Scripts
{
    public class GameLogic : MonoBehaviour
    {
        public TimeUtils.RigidbodyFreezeType freezeType = TimeUtils.RigidbodyFreezeType.PhysicsAutoSimulation;

        public TimeStone stone;

        public bool frozen;

        public bool doRecording = false;

        public Button freezeButton;
        public Button unfreezeButton;
        public Button simulateButton;

        // Start is called before the first frame update
        void Awake()
        {
            stone.onInitTimeAgentsList.AddListener(Freeze);
            freezeButton.onClick.AddListener(Freeze);
            unfreezeButton.onClick.AddListener(Unfreeze);
            simulateButton.onClick.AddListener(Simulate);
        }

        void Update()
        {
            freezeButton.enabled = !frozen;
            unfreezeButton.enabled = frozen;
        }
    
        private void Freeze()
        {
            Debug.Log("[GameLogic] Freeze");
            stone.FreezeTimeAgents(true, freezeType);
            frozen = true;
            if (doRecording)
                stone.StopRecording();
        }

        private void Unfreeze()
        {
            Debug.Log("[GameLogic] Unfreeze");
            stone.FreezeTimeAgents(false, freezeType);
            frozen = false;
            if (doRecording)
                stone.StartRecording();
        }

        public void Simulate()
        {
            stone.StartSimulation(true);
        }

    }
}