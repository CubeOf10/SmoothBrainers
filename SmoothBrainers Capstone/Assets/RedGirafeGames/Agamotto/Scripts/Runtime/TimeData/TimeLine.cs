using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedGirafeGames.Agamotto.Scripts.Runtime.TimeData
{
    /// <summary>
    /// The TimeLine holding the TimeStone's data for its TimeAgents
    /// </summary>
    public class TimeLine
    {
        /// <summary>
        /// The id to persist the tick data in the timeline. The first data in the timeline is always the tick delta times.
        /// </summary>
        public static Guid tickGuid = Guid.NewGuid();

        /// <summary>
        /// The DeltaTime id to persist the tick in the timeline
        /// </summary>
        public const string TickDeltaTimeDataId = "DeltaTime";

        /// <summary>
        /// The data, ordered in a Dictionary by the hash (TimeAgent.guid + dataId) to optimize the performances.
        /// Then each entry contains the ordered list of data for each tick.
        /// <example> Data will be stored this way (pseudo-code) :
        /// <code>
        /// Dictionary
        /// [hashForTickDeltaTimes] = [ {Value=0, DeltaTime=0, origin=Record,  Type=float}, {Value=0.005, DeltaTime=0.005, origin=Record,  Type=float}, {Value=0.005, DeltaTime=0.005, origin=Record,  Type=float} ] 
        /// [hashForComponent1Position] = [ {Value={1,0,1}, DeltaTime=0, origin=Record,  Type=Vector3}, {Value=1,1,1, DeltaTime=0.005, origin=Record,  Type=Vector3}, {Value=1,2,1, DeltaTime=0.005, origin=Record,  Type=Vector3} ]
        /// [hashForComponent2Position] = [...] 
        /// </code>
        /// </example>
        /// </summary>
        public readonly Dictionary<int, List<TimeData>> TimeLineData =
            new Dictionary<int, List<TimeData>>();

        /// <summary>
        /// The cached ticks count
        /// </summary>
        private int _ticksCount = 0;

        /// <summary>
        /// Persist new TimeData in the TimeLine.
        /// </summary>
        /// <param name="index">The index where to persist</param>
        /// <param name="timeAgentGuid">The Guid of the timeAgent persisted</param>
        /// <param name="dataId">The dataId of the persisted data</param>
        /// <param name="deltaTime">The deltaTime</param>
        /// <param name="value">The value of the persisted data</param>
        /// <param name="origin">The origin of the data persistence, Record or Simulation</param>
        /// <param name="tickBehaviour">The way the data will be added</param>
        /// <typeparam name="T">The generic type of the data persisted</typeparam>
        /// <exception cref="IndexOutOfRangeException">You can't add data out of the timeLine capacity</exception>
        /// <exception cref="ArgumentOutOfRangeException">You've done really strange things to get that, just say it</exception>
        public void PersistTimeDataAt<T>(int index, Guid timeAgentGuid, string dataId, float deltaTime, T value,
            TimeStone.TimeTickOrigin origin,
            TimeStone.TimeLineAddTickBehaviour tickBehaviour)
        {
            if (index > _ticksCount)
            {
                throw new IndexOutOfRangeException(
                    "[TimeStonePersistence] Warning : you tried to add a new tick at unexisting index <" +
                    index + ">, max index is <" + _ticksCount + ">");
            }

            var hash = GetHashFromGuidAndDataId(timeAgentGuid, dataId);
            if (!TimeLineData.ContainsKey(hash))
            {
                TimeLineData[hash] = new List<TimeData>();
            }

            var dataList = TimeLineData[hash];
            // All data list need to be the same size, so if we try to modify a data list that has not the good size
            // just filling it with null values
            while (dataList.Count < index)
            {
                dataList.Add(null);
            }
            var newTimeData = new TimeData<T> {Value = value, DeltaTime = deltaTime, Origin = origin};
            switch (tickBehaviour)
            {
                case TimeStone.TimeLineAddTickBehaviour.ClearAfter:
                    if (dataList.Count - index > 0)
                        dataList.RemoveRange(index, dataList.Count - index);
                    dataList.Add(newTimeData);
                    break;
                case TimeStone.TimeLineAddTickBehaviour.Insert:
                    dataList.Insert(index, newTimeData);
                    break;
                case TimeStone.TimeLineAddTickBehaviour.Replace:
                    if (index == dataList.Count)
                        dataList.Add(newTimeData);
                    else
                        dataList[index] = newTimeData;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tickBehaviour), tickBehaviour,
                        "You do some dirty things if you display this message my friend!");
            }

            _ticksCount = TimeLineData.Values.First().Count;
        }

        /// <summary>
        /// Returns the data persisted at index for the timeAgent
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="timeAgentGuid">The searched timeAgent guid</param>
        /// <param name="dataId">The id of the searched data</param>
        /// <param name="dataExists">Out param returning if the data exist</param>
        /// <typeparam name="T">The generic type of the searched data</typeparam>
        /// <returns>The searched data or the type default if data does not exist</returns>
        public TimeData<T> GetDataAt<T>(int index, Guid timeAgentGuid, string dataId, out bool dataExists)
        {
            var hash = GetHashFromGuidAndDataId(timeAgentGuid, dataId);

            if (index < 0 || !TimeLineData.ContainsKey(hash) || TimeLineData[hash].Count <= index)
            {
                dataExists = false;
                return null;
            }

            var timeData = TimeLineData[hash][index];
            if (timeData != null)
            {
                dataExists = true;
                return (TimeData<T>) timeData;
            }

            dataExists = false;
            return null;
        }

        /// <summary>
        /// Get the closest tick for a given time.
        /// </summary>
        /// <param name="time">The searched time</param>
        /// <returns>The closest tick</returns>
        public int GetClosestTickIndexAtTime(float time)
        {
            var timeAtIndex = 0.0f;
            for (var i = 0; i < _ticksCount; i++)
            {
                // First timeData contains the tick
                var timeTick = TimeLineData.Values.First()[i];
                timeAtIndex += timeTick.DeltaTime;
                if (Mathf.Abs(timeAtIndex - time) < 0.000001f)
                {
                    return i;
                }

                if (timeAtIndex > time && i > 0)
                {
                    return i - 1;
                }
            }

            return _ticksCount - 1;
        }

        /// <summary>
        /// Get the closest Data at given time
        /// </summary>
        /// <param name="time">The searched time</param>
        /// <param name="timeAgentGuid">The search TimeAgent's guid </param>
        /// <param name="dataId">The searched dataId</param>
        /// <typeparam name="T">The generic type of the searched data</typeparam>
        /// <returns>The closest data for the time or generic type default if data don't exist</returns>
        public TimeData<T> GetClosestDataAtTime<T>(float time, Guid timeAgentGuid, string dataId)
        {
            var tickIndex = GetClosestTickIndexAtTime(time);
            return GetDataAt<T>(tickIndex, timeAgentGuid, dataId, out var dataExists);
        }

        /// <summary>
        /// Get the time corresponding to a tick
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float GetTimeAtTickIndex(int index)
        {
            if (_ticksCount == 0)
            {
                return 0;
            }

            if (index > _ticksCount)
            {
                Debug.LogWarning(
                    "[TimeStonePersistence] WARNING : Trying to get time at out of range index <" +
                    index + ">, max index is <" + _ticksCount + ">. Returning maximum value.");
                return 0;
            }

            var timeAtIndex = 0.0f;
            for (var i = 0; i <= index; i++)
            {
                // First data is the tick value
                var timeTick = TimeLineData.Values.First()[i];
                timeAtIndex += timeTick.DeltaTime;
            }

            return timeAtIndex;
        }

        /// <summary>
        /// The TimeLine's tick count
        /// </summary>
        /// <returns></returns>
        public int GetTickCount()
        {
            return _ticksCount;
        }

        /// <summary>
        /// Remove ticks from tickFrom to tickTo
        /// </summary>
        /// <param name="tickFrom">The start index</param>
        /// <param name="tickTo">The end index</param>
        public void Remove(int tickFrom, int tickTo)
        {
            foreach (var dataList in TimeLineData.Values)
            {
                dataList.RemoveRange(tickFrom, tickTo - tickFrom);
            }

            if (TimeLineData.Count > 0)
            {
                _ticksCount = TimeLineData.First().Value.Count;
            }
        }

        /// <summary>
        /// Clear the data
        /// </summary>
        public void Clear()
        {
            TimeLineData.Clear();
            _ticksCount = 0;
        }

        /// <summary>
        /// Returns a unique hash using TimeAgent's guid and dataId
        /// </summary>
        /// <param name="timeAgentGuid">The TimeAgent's guid</param>
        /// <param name="dataId">The dataId</param>
        /// <returns></returns>
        private static int GetHashFromGuidAndDataId(Guid timeAgentGuid, string dataId)
        {
            return timeAgentGuid.GetHashCode() + dataId.GetHashCode();
        }

        /// <summary>
        /// Class storing a tick's data
        /// </summary>
        public class TimeData
        {
            public float DeltaTime { get; set; }
            public TimeStone.TimeTickOrigin Origin { get; set; }
        }

        /// <summary>
        /// Generic version of TimeData to store a generic value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Serializable]
        public class TimeData<T> : TimeData
        {
            private T _value;

            public T Value
            {
                get => _value;
                set => _value = value;
            }

            public Type Type => typeof(T);
        }
    }
}