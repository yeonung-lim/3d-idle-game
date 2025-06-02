using UnityEngine;
using Game.Core;
using Game.Combat;
using Game.Monster;

namespace Game.Character
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StatsController))]
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerController : MonoBehaviour
    {
        public float rotateSpeed = 200f; // 기본 회전 속도
        public float attackRange = 1.5f;
        public KeyCode attackKey = KeyCode.Space;

        // 프라이빗 참조
        private CharacterController characterController;
        private StatsController statsController;
        private HealthSystem healthSystem;

        // 전투 매개변수
        private float attackDamage;

        // 스탯에 의해 구동되는 이동 매개변수
        private float currentMoveSpeed;

        void Awake()
        {
            // 필요한 컴포넌트에 대한 참조 가져오기
            characterController = GetComponent<CharacterController>();
            statsController = GetComponent<StatsController>();
            healthSystem = GetComponent<HealthSystem>();

            if (characterController == null)
            {
                Debug.LogError("PlayerController: 이 GameObject에서 CharacterController 컴포넌트를 찾을 수 없습니다.");
            }
            if (statsController == null)
            {
                Debug.LogError("PlayerController: 이 GameObject에서 StatsController 컴포넌트를 찾을 수 없습니다.");
            }
            if (healthSystem == null)
            {
                Debug.LogError("PlayerController: 이 GameObject에서 HealthSystem 컴포넌트를 찾을 수 없습니다.");
            }
            else
            {
                healthSystem.OnDeath += HandlePlayerDeath;
            }
        }

        void Start()
        {
            InitializePlayerStats();
            // 초기화 후 StatsController에서 moveSpeed와 attackDamage 가져오기
            currentMoveSpeed = statsController.GetStatValue(StatType.MoveSpeed);
            if (currentMoveSpeed <= 0) {
                Debug.LogWarning($"PlayerController: 스탯에서 MoveSpeed가 {currentMoveSpeed}입니다. 플레이어가 움직이지 않을 수 있습니다. Stat 초기화 또는 기본 MoveSpeed 스탯을 확인하세요.");
                // 설정되지 않았거나 0인 경우 기본값 제공
                currentMoveSpeed = 5f;
            }
            attackDamage = statsController.GetStatValue(StatType.AttackPower);
             if (attackDamage <= 0) {
                Debug.LogWarning($"PlayerController: 스탯에서 AttackPower가 {attackDamage}입니다. 플레이어가 데미지를 주지 못할 수 있습니다. Stat 초기화 또는 기본 AttackPower 스탯을 확인하세요.");
            }
        }

        void InitializePlayerStats()
        {
            if (statsController != null)
            {
                // 스탯이 이미 존재하는 경우 업데이트하는 AddStat 사용
                statsController.AddStat(StatType.MaxHealth, 100f);
                statsController.AddStat(StatType.Health, 100f); // 현재 체력은 최대 체력으로 시작
                statsController.AddStat(StatType.AttackPower, 10f);
                statsController.AddStat(StatType.MoveSpeed, 5f); // 기본 이동 속도
                statsController.AddStat(StatType.Defense, 5f);
            }
            else
            {
                Debug.LogError("PlayerController: StatsController를 찾을 수 없어 플레이어 스탯을 초기화할 수 없습니다.");
            }
        }

        void Update()
        {
            if (characterController == null || statsController == null)
            {
                // 컴포넌트를 사용하기 전에 존재하는지 확인
                return;
            }

            // 동적으로 변경될 수 있는 경우 매 프레임마다 스탯에서 currentMoveSpeed 업데이트
            // Start 후 moveSpeed가 변경되지 않을 것으로 예상되는 경우 이 줄을 Update에서 제거할 수 있습니다.
            // currentMoveSpeed = statsController.GetStatValue(StatType.MoveSpeed); // 집약적일 수 있음; 속도가 자주 변경되는 경우에만 업데이트
                                                                                // 지금은 Start()에서 설정

            HandleMovement();
            HandleAttack();
        }

        void HandleMovement()
        {
            // 입력 읽기
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // 이동 방향 계산
            Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput);

            // 크기가 1보다 크면 정규화 (대각선 이동이 더 빠른 것을 방지)
            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            // 이동 적용
            if (characterController.enabled) // CharacterController가 활성화되었는지 확인
            {
                 // Rigidbody를 사용하지 않는 경우 중력을 수동으로 적용
                if (!characterController.isGrounded)
                {
                    characterController.Move(Physics.gravity * Time.deltaTime);
                }
                characterController.Move(moveDirection * currentMoveSpeed * Time.deltaTime);
            }


            // 플레이어를 이동 방향을 향하도록 회전
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            }
        }

        void HandleAttack()
        {
            if (Input.GetKeyDown(attackKey))
            {
                PerformMeleeAttack();
            }
        }

        void PerformMeleeAttack()
        {
            if (statsController == null) return;

            attackDamage = statsController.GetStatValue(StatType.AttackPower); // 현재 공격력 가져오기
            Debug.Log($"{gameObject.name}이 {attackDamage} 파워로 공격합니다!");

            // 공격 범위 감지를 위한 간단한 구 중첩
            // 공격 지점은 플레이어보다 약간 앞에 있을 수 있습니다
            Vector3 attackPoint = transform.position + transform.forward * (attackRange / 2f); // 구를 약간 앞쪽으로 중심화
            Collider[] hitColliders = Physics.OverlapSphere(attackPoint, attackRange);

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject == gameObject) continue; // 자신을 공격하지 않음

                HealthSystem targetHealthSystem = hitCollider.GetComponent<HealthSystem>();
                if (targetHealthSystem != null)
                {
                    // 선택사항: 몬스터나 다른 공격 가능한 엔티티인지 확인
                    if (hitCollider.GetComponent<MonsterController>() != null) // 예시 확인
                    {
                        Debug.Log($"{gameObject.name}이 {hitCollider.name}를 공격했습니다!");
                        targetHealthSystem.TakeDamage(attackDamage);
                    }
                }
            }
        }

        void HandlePlayerDeath()
        {
            Debug.Log("플레이어가 죽었습니다! 게임 오버 (플레이스홀더).");
            // Time.timeScale = 0; // 게임 일시정지
            // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            enabled = false; // 플레이어 컨트롤러 비활성화
        }

        void OnDestroy()
        {
            // 메모리 누수를 방지하기 위해 이벤트 구독 해제
            if (healthSystem != null)
            {
                healthSystem.OnDeath -= HandlePlayerDeath;
            }
        }

        // 에디터에서 공격 범위 기즈모 시각화용
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 attackPoint = transform.position + transform.forward * (attackRange / 2f);
            Gizmos.DrawWireSphere(attackPoint, attackRange);
        }
    }
}
