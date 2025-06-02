using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Monster
{
    public class MonsterSpawner : MonoBehaviour
    {
        [Header("Spawning Configuration")]
        public GameObject monsterPrefab; // Assign in Inspector
        public float spawnInterval = 3f;
        public int maxMonsters = 5;

        [Header("Spawn Location")]
        public Transform[] spawnPoints; // Specific points for spawning
        public float spawnRadius = 5f;  // Radius for random spawning if spawnPoints is empty

        private List<GameObject> activeMonsters = new List<GameObject>();

        void Start()
        {
            if (monsterPrefab == null)
            {
                Debug.LogError("MonsterSpawner: Monster Prefab is not assigned! Spawning will not work.", this);
                return; // Stop if no prefab is assigned
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("MonsterSpawner: No specific spawn points assigned. Spawning randomly around spawner position within spawnRadius.", this);
            }

            StartCoroutine(SpawnMonsterRoutine());
        }

        IEnumerator SpawnMonsterRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);

                // Clean up destroyed monsters from the list
                activeMonsters.RemoveAll(monster => monster == null);

                if (activeMonsters.Count < maxMonsters)
                {
                    SpawnMonster();
                }
            }
        }

        void SpawnMonster()
        {
            if (monsterPrefab == null) return;

            Vector3 spawnPosition;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                // Use a specific spawn point
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (spawnPoint != null)
                {
                    spawnPosition = spawnPoint.position;
                }
                else
                {
                    Debug.LogWarning("MonsterSpawner: A spawn point in the array is null. Defaulting to spawner position.", this);
                    spawnPosition = GetRandomPositionAroundSpawner();
                }
            }
            else
            {
                // Use spawner's position + random offset within spawnRadius
                spawnPosition = GetRandomPositionAroundSpawner();
            }

            GameObject newMonster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
            activeMonsters.Add(newMonster);
            // Debug.Log($"Spawned monster {newMonster.name} at {spawnPosition}. Active monsters: {activeMonsters.Count}", this);

            // Optional: Link monster's death to spawner for proactive list removal
            // HealthSystem healthSystem = newMonster.GetComponent<HealthSystem>();
            // if (healthSystem != null)
            // {
            //     healthSystem.OnDeath += () => OnMonsterDied(newMonster);
            // }
        }

        Vector3 GetRandomPositionAroundSpawner()
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0; // Keep spawn on the same horizontal plane as the spawner
            return transform.position + randomOffset;
        }

        // Optional method for proactive removal if OnDeath is used
        // void OnMonsterDied(GameObject monsterInstance)
        // {
        //     if (monsterInstance != null && activeMonsters.Contains(monsterInstance))
        //     {
        //         activeMonsters.Remove(monsterInstance);
        //         // Debug.Log($"Monster {monsterInstance.name} died and was removed from spawner list. Active monsters: {activeMonsters.Count}", this);
        //     }
        // }

        void OnDrawGizmosSelected()
        {
            // Visualize spawn radius if no specific spawn points are set
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, spawnRadius);
            }

            // Visualize specific spawn points
            if (spawnPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (Transform point in spawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.5f); // Draw a small sphere at each spawn point
                    }
                }
            }
        }
    }
}
