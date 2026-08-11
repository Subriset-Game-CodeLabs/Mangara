using System.Collections.Generic;
using UnityEngine;

namespace RandomEvent
{
    [RequireComponent(typeof(BoxCollider))]
    public class EventSpawnZone : MonoBehaviour
    {
        [SerializeField] private string _zoneID;
        [SerializeField] private RandomEventSO _assignedEvent;
        [SerializeField] private LayerMask _groundLayer;

        private BoxCollider _boxCollider;

        public string ZoneID => _zoneID;
        public RandomEventSO AssignedEvent => _assignedEvent;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
        }

        public List<Vector3> GetRandomSpawnPositions(int count)
        {
            List<Vector3> positions = new List<Vector3>();
            if (_boxCollider == null)
            {
                _boxCollider = GetComponent<BoxCollider>();
            }

            if (_boxCollider == null)
            {
                Debug.LogWarning($"[EventSpawnZone] Zone '{_zoneID}' lacks a BoxCollider component.");
                return positions;
            }

            Bounds bounds = _boxCollider.bounds;
            int maxAttempts = count * 10;
            int attempts = 0;

            while (positions.Count < count && attempts < maxAttempts)
            {
                attempts++;
                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomZ = Random.Range(bounds.min.z, bounds.max.z);

                Vector3 rayOrigin = new Vector3(randomX, bounds.max.y + 1f, randomZ);
                float rayDistance = (bounds.max.y - bounds.min.y) + 100f;

                if (_groundLayer.value != 0 && Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, _groundLayer))
                {
                    positions.Add(hit.point);
                }
                else if (_groundLayer.value == 0 && Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance))
                {
                    positions.Add(hit.point);
                }
            }

            return positions;
        }

        private void OnDrawGizmos()
        {
            DrawZoneGizmo(new Color(0.2f, 0.8f, 0.2f, 0.2f), Color.green);
        }

        private void OnDrawGizmosSelected()
        {
            DrawZoneGizmo(new Color(0.2f, 0.9f, 0.2f, 0.4f), Color.yellow);
        }

        private void DrawZoneGizmo(Color cubeColor, Color wireColor)
        {
            BoxCollider col = GetComponent<BoxCollider>();
            if (col == null) return;

            Gizmos.color = cubeColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(col.center, col.size);
        }
    }
}
