using System.Collections.Generic;
using UnityEngine;

namespace Part4_UnseenPattern
{
    /// <summary>
    /// Spatial grid for fast proximity queries.
    /// Pattern: Spatial Partition (https://www.unitydesignpatterns.com/patterns/spatialpartition)
    /// </summary>
    public class SpatialGrid
    {
        private readonly float _cellSize;
        private readonly Dictionary<Vector2Int, List<Transform>> _cells = new();

        public SpatialGrid(float cellSize)
        {
            _cellSize = Mathf.Max(0.1f, cellSize);
        }

        public void Clear()
        {
            _cells.Clear();
        }

        public void Insert(Transform entity)
        {
            var key = GetCellKey(entity.position);
            if (!_cells.TryGetValue(key, out var bucket))
            {
                bucket = new List<Transform>();
                _cells[key] = bucket;
            }

            bucket.Add(entity);
        }

        public List<Transform> QueryNearby(Vector3 position, float radius)
        {
            var results = new List<Transform>();
            var min = GetCellKey(position - new Vector3(radius, 0f, radius));
            var max = GetCellKey(position + new Vector3(radius, 0f, radius));

            for (var x = min.x; x <= max.x; x++)
            {
                for (var y = min.y; y <= max.y; y++)
                {
                    var key = new Vector2Int(x, y);
                    if (!_cells.TryGetValue(key, out var bucket))
                    {
                        continue;
                    }

                    foreach (var entity in bucket)
                    {
                        if (entity == null)
                        {
                            continue;
                        }

                        var flatDistance = Vector3.Distance(
                            new Vector3(position.x, 0f, position.z),
                            new Vector3(entity.position.x, 0f, entity.position.z));

                        if (flatDistance <= radius)
                        {
                            results.Add(entity);
                        }
                    }
                }
            }

            return results;
        }

        private Vector2Int GetCellKey(Vector3 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / _cellSize),
                Mathf.FloorToInt(position.z / _cellSize));
        }
    }
}
