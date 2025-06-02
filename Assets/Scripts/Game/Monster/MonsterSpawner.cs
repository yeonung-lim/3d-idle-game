using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Monster
{
    public class MonsterSpawner : MonoBehaviour
    {
        [Header("스폰 설정")]
        public GameObject monsterPrefab;
        public float spawnInterval = 3f;
        public int maxMonsters = 5;

        [Header("스폰 위치")]
        public Transform[] spawnPoints; // 몬스터 스폰 지점
        public float spawnRadius = 5f;  // 스폰 지점이 없을 경우 사용할 랜덤 스폰 반경

        private List<GameObject> activeMonsters = new List<GameObject>();

        void Start()
        {
            if (monsterPrefab == null)
            {
                Debug.LogError("MonsterSpawner: 몬스터 프리팹이 할당되지 않았습니다! 스폰이 작동하지 않습니다.", this);
                return; // 프리팹이 없으면 실행 중지
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("MonsterSpawner: 지정된 스폰 지점이 없습니다. 스포너 위치를 중심으로 반경 내에서 랜덤하게 스폰됩니다.", this);
            }

            StartCoroutine(SpawnMonsterRoutine());
        }

        IEnumerator SpawnMonsterRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);

                // 파괴된 몬스터를 리스트에서 제거
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
                // 지정된 스폰 지점 사용
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (spawnPoint != null)
                {
                    spawnPosition = spawnPoint.position;
                }
                else
                {
                    Debug.LogWarning("MonsterSpawner: 스폰 지점 배열에 null 값이 있습니다. 스포너 위치를 기본값으로 사용합니다.", this);
                    spawnPosition = GetRandomPositionAroundSpawner();
                }
            }
            else
            {
                // 스포너 위치 + 스폰 반경 내 랜덤 위치 사용
                spawnPosition = GetRandomPositionAroundSpawner();
            }

            GameObject newMonster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
            activeMonsters.Add(newMonster);
            // Debug.Log($"몬스터 {newMonster.name}가 {spawnPosition}에 스폰되었습니다. 현재 활성 몬스터: {activeMonsters.Count}", this);

            // 선택사항: 몬스터의 사망을 스포너와 연결하여 선제적으로 리스트에서 제거
            // HealthSystem healthSystem = newMonster.GetComponent<HealthSystem>();
            // if (healthSystem != null)
            // {
            //     healthSystem.OnDeath += () => OnMonsterDied(newMonster);
            // }
        }

        Vector3 GetRandomPositionAroundSpawner()
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0; // 스포너와 같은 수평면에 스폰
            return transform.position + randomOffset;
        }

        // OnDeath 이벤트 사용 시 선제적 제거를 위한 선택적 메서드
        // void OnMonsterDied(GameObject monsterInstance)
        // {
        //     if (monsterInstance != null && activeMonsters.Contains(monsterInstance))
        //     {
        //         activeMonsters.Remove(monsterInstance);
        //         // Debug.Log($"몬스터 {monsterInstance.name}가 사망하여 스포너 리스트에서 제거되었습니다. 현재 활성 몬스터: {activeMonsters.Count}", this);
        //     }
        // }

        void OnDrawGizmosSelected()
        {
            // 지정된 스폰 지점이 없을 경우 스폰 반경 시각화
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, spawnRadius);
            }

            // 지정된 스폰 지점 시각화
            if (spawnPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (Transform point in spawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.5f); // 각 스폰 지점에 작은 구체 표시
                    }
                }
            }
        }
    }
}
