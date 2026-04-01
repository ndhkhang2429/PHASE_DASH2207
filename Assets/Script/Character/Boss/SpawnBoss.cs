using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnBoss : EnemyBase
{
    [Header("Patrol Settings (Di chuyển)")]
    public Transform pointA;
    public Transform pointB;

    // Đã thay đổi: Dùng tọa độ tuyệt đối để chốt giới hạn
    private float leftLimit;
    private float rightLimit;
    private int moveDirection = 1;

    public SpawnBossRoomController roomManager;

    [Header("Skill Settings")]
    public float attackInterval = 4f;
    public float spawnInterval = 10f;
    private float lastAttackTime = 0f;
    private float lastSpawnTime = 0f;

    [Header("Radial Attack")]
    public GameObject projectilePrefab;
    public Transform attackPoint;
    public float projectileSpeed = 8f;
    public int numberOfProjectiles = 12;

    [Header("Spawns")]
    public GameObject[] flyingEnemyPrefabs;
    public List<Transform> flyingSpawnPoints; // Transforms từ Inspector
    public GameObject[] groundEnemyPrefabs;
    public List<Transform> groundSpawnPoints; // Transforms từ Inspector

    // THÊM MỚI: Danh sách chứa tọa độ tuyệt đối để điểm Spawn không bị chạy theo Boss
    private List<Vector3> fixedFlyingSpawnPos = new List<Vector3>();
    private List<Vector3> fixedGroundSpawnPos = new List<Vector3>();

    // Trạng thái kiểm soát
    // SỬA LẠI: Ban đầu isActive nên để = false, chờ Player đi vào Trigger mới = true
    private bool isActive = false;
    private bool isBusy = false;

    protected override void Awake()
    {
        base.Awake();

        // 1. CHỐT tọa độ giới hạn tuần tra
        if (pointA != null && pointB != null)
        {
            leftLimit = pointA.position.x;
            rightLimit = pointB.position.x;
        }

        // 2. CHỐT tọa độ tuyệt đối của các điểm Spawn
        foreach (Transform t in flyingSpawnPoints)
        {
            if (t != null) fixedFlyingSpawnPos.Add(t.position);
        }

        foreach (Transform t in groundSpawnPoints)
        {
            if (t != null) fixedGroundSpawnPos.Add(t.position);
        }
    }

    protected override void LogicUpdate()
    {
        if (!isActive || isBusy) return;

        Patrol();

        // Khóa thời gian ngay lập tức để tránh gọi đúp Coroutine
        if (Time.time - lastAttackTime >= attackInterval)
        {
            lastAttackTime = Time.time;
            StartCoroutine(AttackRoutine());
        }
        else if (Time.time - lastSpawnTime >= spawnInterval)
        {
            lastSpawnTime = Time.time;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void ActivateBoss()
    {
        isActive = true;
        lastAttackTime = Time.time;
        lastSpawnTime = Time.time;
        Debug.Log("Boss Activated!");

        if (BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.ShowHealthBar();
        }
    }

    void Patrol()
    {
        animator.SetBool("walk", true);

        // Di chuyển bằng velocity
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        // Kiểm tra và quay đầu dựa trên tọa độ tuyệt đối
        if (moveDirection == 1 && transform.position.x >= rightLimit)
        {
            SetDirection(-1);
        }
        else if (moveDirection == -1 && transform.position.x <= leftLimit)
        {
            SetDirection(1);
        }
    }

    private void SetDirection(int direction)
    {
        moveDirection = direction;
        if ((moveDirection == 1 && !isFacingRight) ||
            (moveDirection == -1 && isFacingRight))
        {
            Flip();
        }
    }

    IEnumerator AttackRoutine()
    {
        isBusy = true;

        // SỬA LẠI: Phải ép vận tốc về 0 để Boss dừng hẳn lại khi dùng chiêu
        rb.velocity = Vector2.zero;

        animator.SetBool("walk", false);
        animator.SetTrigger("Fire");

        yield return new WaitForSeconds(0.5f);

        PerformRadialAttack();

        yield return new WaitForSeconds(0.5f);

        isBusy = false;
    }

    IEnumerator SpawnRoutine()
    {
        isBusy = true;

        // SỬA LẠI: Phải ép vận tốc về 0 để Boss dừng hẳn lại khi dùng chiêu
        rb.velocity = Vector2.zero;

        animator.SetBool("walk", false);
        animator.SetTrigger("Spawn");

        yield return new WaitForSeconds(0.6f);

        SpawnEnemies();

        yield return new WaitForSeconds(0.4f);

        isBusy = false;
    }

    void PerformRadialAttack()
    {
        if (projectilePrefab != null && attackPoint != null)
        {
            float angleStep = 360f / numberOfProjectiles;
            float baseAngle = 0f;

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float dirX = attackPoint.position.x + Mathf.Sin((baseAngle * Mathf.Deg2Rad));
                float dirY = attackPoint.position.y + Mathf.Cos((baseAngle * Mathf.Deg2Rad));
                Vector3 direction = (new Vector3(dirX, dirY, 0) - attackPoint.position).normalized;

                GameObject proj = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
                Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
                if (rbProj != null) rbProj.velocity = direction * projectileSpeed;

                baseAngle += angleStep;
            }
        }
    }

    void SpawnEnemies()
    {
        // Truyền danh sách tọa độ (Vector3) thay vì Transform
        if (Random.value > 0.7f) SpawnFromList(flyingEnemyPrefabs, fixedFlyingSpawnPos);
        else SpawnFromList(groundEnemyPrefabs, fixedGroundSpawnPos);
    }

    // Nhận List<Vector3> thay vì List<Transform>
    void SpawnFromList(GameObject[] prefabs, List<Vector3> points)
    {
        if (prefabs.Length > 0 && points.Count > 0)
        {
            GameObject randomPrefab = prefabs[Random.Range(0, prefabs.Length)];
            Vector3 randomPoint = points[Random.Range(0, points.Count)];

            GameObject spawnedEnemy = Instantiate(randomPrefab, randomPoint, Quaternion.identity);

            EnemyBase enemyScript = spawnedEnemy.GetComponent<EnemyBase>();
            // Khởi tạo tại tọa độ tuyệt đối
            if (enemyScript != null)
            {
                // 3. Ép nó dùng chung giới hạn leftLimit và rightLimit của con Boss (chính là kích thước phòng)
                enemyScript.SetRoomPatrolLimits(this.leftLimit, this.rightLimit);
            }
        }
    }

    public override void OnDeath()
    {
        StopAllCoroutines();
        isBusy = true;

        if (roomManager != null)
        {
            roomManager.UnlockRoom();
        }
        base.OnDeath();
    }
}