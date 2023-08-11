using System;
using System.Collections.Generic;
using RedGirafeGames.Agamotto.Demo.Shared.Scripts;
using RedGirafeGames.Agamotto.Scripts.Runtime;
using RedGirafeGames.Agamotto.Scripts.Runtime.Agents;
using UnityEngine;
using UnityEngine.UI;

namespace RedGirafeGames.Agamotto.Demo.SorcererDemo.Scripts
{
    public class Sorcerer : MonoBehaviour
    {
        public TimeStone stone;

        [Space] [Header("Telekinesis spell")] public float throwPower = 65.0f;
        public float grabDistance = 3.0f;
        public float grabMaxDistance = 5;
        private Projectile _grabbedProjectile;
        private bool _isGrabbingProjectile;

        [Space] [Header("Stop Time spell")] public bool simulateFuture = true;
        public bool simulateAsync = true;
        private bool _timeStopped = false;

        [Space] [Header("Move Time spell")] public float moveTimeSpeed = 1.0f;
        
        private FpsController _fpsController;
        private Text _timeStoppedText;


        void Start()
        {
            stone = FindObjectOfType<TimeStone>();
            _fpsController = GetComponent<FpsController>();
            _timeStoppedText = GameObject.Find("TimeState").GetComponent<Text>();
        }

        void Update()
        {
            // Grab
            if (Input.GetButtonDown("Fire1"))
            {
                if (!_timeStopped)
                {
                    TryGrabProjectile();
                }
            }
            // Keep Grabbing
            else if (Input.GetButton("Fire1"))
            {
                if (_isGrabbingProjectile && !_timeStopped)
                {
                    UpdateGrabbedProjectile();
                }
            }
            // Stop Grabbing and throw
            else if (Input.GetButtonUp("Fire1"))
            {
                if (_isGrabbingProjectile && !_timeStopped)
                {
                    ThrowProjectile(_grabbedProjectile);
                }
            }

            _timeStoppedText.enabled = _timeStopped;
        }

        private void LateUpdate()
        {
            // In late update to let the the stone record this frame
            if (Input.GetButtonDown("Fire2"))
            {
                if (!_isGrabbingProjectile)
                {
                    if (_timeStopped)
                    {
                        ReleaseTime();
                    }
                    else
                    {
                        StopTime();
                    }
                }
            }
            else
            {
                if (_timeStopped && !stone.Simulating)
                {
                    var timeLineAxis = Input.GetKey(KeyCode.Y) ? 1 : Input.GetKey(KeyCode.T) ? -1 : 0;
                    MoveTime(timeLineAxis);
                }
            }
        }

        private void MoveTime(float speed)
        {
            if (speed == 0)
            {
                stone.StopPlayback();
            }
            else
            {
                stone.playbackSpeed = speed * moveTimeSpeed;
                stone.StartPlayback();
            }
        }

        /// <summary>
        /// Stop time in the scene.
        /// We don't use Time.timeScale = 0 because it has a lot of side effects, so we stop time "manually" by :
        /// - stopping physics
        /// - pausing particle effects
        /// </summary>
        private void StopTime()
        {
            Debug.Log("[Sorcerer] Stopped time.");
            Physics.autoSimulation = false;
            _timeStopped = true;

            stone.StopRecording();

            if (simulateFuture)
            {
                stone.StartSimulation(simulateAsync);
            }
        }

        private void ReleaseTime()
        {
            Debug.Log("[Sorcerer] Released time.");
            if (stone.Simulating)
            {
                stone.StopSimulation();
            }

            Physics.autoSimulation = true;
            _timeStopped = false;

            stone.StopPlayback();
            stone.StartRecording();
        }

        private void TryGrabProjectile()
        {
            if (Physics.Raycast(_fpsController.GetLookTransform().position,
                _fpsController.GetLookTransform().forward,
                out var hit, grabMaxDistance))
            {
                if (hit.transform.CompareTag("Projectile"))
                {
                    _grabbedProjectile = hit.transform.GetComponent<Projectile>();
                    _isGrabbingProjectile = true;
                }
            }
        }

        private void ThrowProjectile(Projectile projectile)
        {
            var velocity = _fpsController.GetLookTransform().forward * throwPower;
            projectile.Throw(velocity);
            _isGrabbingProjectile = false;
        }

        private void UpdateGrabbedProjectile()
        {
            _grabbedProjectile.transform.position = GetGrabbedProjectilePosition();
            _grabbedProjectile.transform.rotation = GetGrabbedProjectileRotation();
        }

        private Vector3 GetGrabbedProjectilePosition()
        {
            return new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z) +
                   _fpsController.GetLookTransform().forward * grabDistance;
        }

        private Quaternion GetGrabbedProjectileRotation()
        {
            return _fpsController.GetLookTransform().rotation;
        }
    }
}