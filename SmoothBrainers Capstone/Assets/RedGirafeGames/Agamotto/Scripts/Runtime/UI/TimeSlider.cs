using System;
using UnityEngine;
using UnityEngine.UI;

namespace RedGirafeGames.Agamotto.Scripts.Runtime.UI
{
    /// <summary>
    /// A slider manipulating a TimeStone data to navigate in time
    /// </summary>
    [AddComponentMenu("Agamotto/UI/Time Slider")]
    [RequireComponent(typeof(Slider))]
    public class TimeSlider : MonoBehaviour
    {
        public enum SliderUnit
        {
            Time,
            Tick
        }

        public SliderUnit unit = SliderUnit.Time;

        public TimeStone stone;
        private Slider _slider;

        private bool _internalChange;

        public void Start()
        {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
            _slider.enabled = false;

            if (unit == SliderUnit.Tick)
                _slider.wholeNumbers = true;

            if(stone == null)
                throw new ArgumentException("[Slider] Stone must be defined");
            
            stone.onSimulationComplete.AddListener(StoneSimulationComplete);
            stone.onTimeLineChange.AddListener(StoneTimeLineChange);
        }

        private void StoneSimulationComplete()
        {
            switch (unit)
            {
                case SliderUnit.Time:
                    _slider.maxValue = stone.GetTimeLineDuration();
                    break;
                case SliderUnit.Tick:
                    _slider.maxValue = stone.GetTickCount() - 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StoneTimeLineChange(TimeStone.TimeTickOrigin origin)
        {
            switch (unit)
            {
                case SliderUnit.Time:
                    _slider.maxValue = stone.GetTimeLineDuration();
                    break;
                case SliderUnit.Tick:
                    _slider.maxValue = stone.GetTickCount() - 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            if (stone == null) return;
            
            if (stone.Simulating || stone.GetTickCount() == 0)
            {
                _slider.enabled = false;
                return;
            }

            _slider.enabled = !stone.Recording;


            _internalChange = true;
            /*
             * Two way binding, update slider if simulation time is modified from somewhere else
             */
            switch (unit)
            {
                case SliderUnit.Time:
                    _slider.value = stone.GetTimeAtTickIndex(stone.playbackTickCursor);
                    break;
                case SliderUnit.Tick:
                    _slider.value = stone.playbackTickCursor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _internalChange = false;
        }

        private void ValueChangeCheck()
        {
            if (stone == null) return;
            
            if (!_slider.enabled || _internalChange) return;

            switch (unit)
            {
                case SliderUnit.Time:
                    stone.SetPlaybackTime(_slider.value);
                    break;
                case SliderUnit.Tick:
                    stone.SetPlaybackTick(Mathf.RoundToInt(_slider.value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}