using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace ChunkedTileMapExample
{
    public class LivenessManager
    {
        private Dictionary<int, int2> _livenesses;
        private Dictionary<int, int2> _pivots;
        private List<int2> _activeChunks;

        public LivenessManager()
        {
            _livenesses = new Dictionary<int, int2>();
            _activeChunks = new List<int2>();
            _pivots = new Dictionary<int, int2>();
        }

        public void AddLiveness(int entityId, int2 position)
        {
            _livenesses.Add(entityId, position);
            _pivots.Add(entityId, position);
        }

        public void UpdateLiveness(int entityId, int2 position)
        {
            _livenesses[entityId] = position;
        }

        public void RemoveLiveness(int entityId)
        {
            _livenesses.Remove(entityId);
            _pivots.Remove(entityId);
        }

        public int2[] GetLiveChunks()
        {
            return _activeChunks.ToArray();
        }

        public bool IsAlive(int2 position)
        {
            return _activeChunks.Contains(position);
        }

        public bool IsPivot(int2 position)
        {
            return _pivots.Values.Contains(position);
        }

        public void Update()
        {
            _activeChunks.Clear();
            foreach(var kvp in _livenesses)
            {
                var entityId = kvp.Key;
                var position = kvp.Value;
                var pivot = _pivots[entityId];

                var distance = position - pivot;
                if (Math.Abs(distance.x) > 1 || Math.Abs(distance.y) > 1)
                {
                    pivot = position;

                    foreach (var pkvp in _pivots)
                    {
                        var otherPivotEntityId = pkvp.Key;
                        var otherPivotPosition = pkvp.Value;

                        if (otherPivotEntityId == entityId)
                            continue;
                        if (otherPivotPosition.Equals(position))
                            continue;

                        var distanceToOtherPivot = otherPivotPosition - position;
                        if (Math.Abs(distanceToOtherPivot.x) > 1 || Math.Abs(distanceToOtherPivot.y) > 1)
                            continue;

                        pivot = otherPivotPosition;
                    }

                    _pivots[entityId] = pivot;
                }

                for(var x = 0; x < 5; x++)
                {
                    for(var y = 0; y < 5; y++)
                    {
                        var chunkPosition = new int2(
                            pivot.x - 2 + x,
                            pivot.y - 2 + y
                        );
                        if(!_activeChunks.Contains(chunkPosition))
                            _activeChunks.Add(chunkPosition);
                    }
                }
            }
        }
    }
}
