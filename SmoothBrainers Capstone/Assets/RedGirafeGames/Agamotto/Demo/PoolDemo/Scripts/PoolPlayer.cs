using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace RedGirafeGames.Agamotto.Demo.PoolDemo.Scripts
{
    public class PoolPlayer : MonoBehaviour
    {
        public TimeStone stone;

        public Slider forceSlider;
        public Slider directionSlider;
        public Button shootButton;
        
        public Camera cam;
        public Transform camArm;
        public PoolBall targetPoolBall;

        public float camRotationSpeed = 50.0f;

        private LineRenderer impactLine;
        private Vector3 lastSimuledShoot;
            
        private void Start()
        {
            stone = FindObjectOfType<TimeStone>();
            impactLine = GetComponent<LineRenderer>();
            shootButton.onClick.AddListener(Shoot);
        }

        void Update()
        {
            float rotation = Input.GetAxis("Horizontal") * camRotationSpeed * Time.deltaTime;
            camArm.Rotate(0, rotation, 0);

            if (stone.Simulating)
            {
                var activeSimulation = false;
                // On async simulation we can stop dynamically the simulation when the simulation is static
                // We use TimeStoneUtils to search for simulated pool balls
                var balls = TimeStoneUtils.FindObjectsOfTypeInSimulationAgents<PoolBall>(stone);
                foreach (var ball in balls)
                {
                    var ballRb = ball.GetComponent<Rigidbody>();
                    if (ballRb.velocity.magnitude > 0.001f)
                    {
                        activeSimulation = true;
                        break;
                    }
                }

                if (!activeSimulation)
                {
                    Debug.Log("Simulation stopped because simulation is static");
                    stone.StopSimulation();
                }
            }

            if (targetPoolBall == null || targetPoolBall.GetComponent<Rigidbody>().velocity.magnitude > 0.001f)
            {
                shootButton.interactable = false;
                impactLine.positionCount = 0;
            }
            else
            {
                shootButton.interactable = true;
                impactLine.positionCount = 2;
                impactLine.SetPosition(0, targetPoolBall.transform.position);
                impactLine.SetPosition(1, (targetPoolBall.transform.position + GetShootDirection() / 5));
            }
            
            // Pool ball selection
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    Transform objectHit = hit.transform;
                    if (hit.transform.CompareTag("PoolBall"))
                    {
                        targetPoolBall = hit.transform.GetComponent<PoolBall>();
                        Debug.Log("Targeting " + targetPoolBall.name);
                    }
                }
            }
        }

        public void ConfigSliderChanged()
        {
            var shootVector = GetShootDirection();
            if (shootVector != lastSimuledShoot)
            {
                stone.StartSimulation();
                lastSimuledShoot = shootVector;
            }
        }
        
        public void Shoot()
        {
            Debug.Log("SHOOT");
            if (targetPoolBall == null) return;
            
            var shoot = GetShootDirection();
            targetPoolBall.Shoot(shoot);
            impactLine.positionCount = 0;
            
            stone.StopSimulation();
            stone.Clear();
        }
        
        public Vector3 GetShootDirection()
        {
            var shootVector = new Vector3(targetPoolBall.transform.position.x, 0, targetPoolBall.transform.position.z + 1);
            
            // Apply direction
            shootVector = Quaternion.Euler(0, directionSlider.value, 0) * shootVector;
            // Apply force
            shootVector *= forceSlider.value;
            
            return shootVector;
        }
    }
}