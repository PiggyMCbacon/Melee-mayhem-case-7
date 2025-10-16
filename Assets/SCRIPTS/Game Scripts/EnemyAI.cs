using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float stoppingDistance = 1.8f;
    public float agentAcceleration = 20f;
    public float rotationSpeed = 10f;

    [Header("Combat")]
    public float chargeTime = 1.25f;
    public float attackRange = 2f;
    public int maxHealth = 2;
    public int damagePerAttack = 1;
    public float attackForce = 8f;
    public Transform clubTip;
    public float clubHitRange = 1.5f;

    [Header("Lunge")]
    public float lungeDistance = 1.2f;   // forward attack lunge
    public float lungeDuration = 0.2f;   // duration of the lunge

    [Header("Ragdoll")]
    public Rigidbody[] ragdollBodies;
    public Collider[] ragdollColliders;
    public Animator animator;

    [Header("Physics / Knockback")]
    public float knockbackDuration = 0.35f;
    public float physicsDrag = 4f;
    public float physicsAngularDrag = 2f;

    [Header("UI")]
    public GameObject enemyHpBarPrefab;
    public Vector3 hpBarOffset = Vector3.up * 2f;

    public event Action onDie;

    private int currentHealth;
    private Transform player;
    private Rigidbody rb;
    private NavMeshAgent agent;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private bool isLaunched = false;
    private bool isAttacking = false;
    private GameObject hpBarInstance;
    private EnemyHealthUI hpUi;

    [SerializeField] private Transform enemyClubField;
    private Transform enemyClub => enemyClubField;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        currentHealth = maxHealth;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.acceleration = agentAcceleration;
            agent.stoppingDistance = stoppingDistance;
            agent.updateRotation = false;
            agent.updateUpAxis = true;
        }

        SetRagdoll(false);
        SpawnHpBar(); // Spawn HP bar once at start
    }

    void Update()
    {
        if (isLaunched || isAttacking || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange)
            HandleAttackLogic();
        else
            MoveTowardsPlayer();

        // Always update HP bar
        if (hpUi != null)
        {
            hpUi.SetHealth((float)currentHealth / maxHealth);
            hpUi.SetPosition(transform.position + hpBarOffset);
        }
    }

    private void HandleAttackLogic()
    {
        if (!isCharging)
        {
            isCharging = true;
            chargeTimer = 0f;
            if (agent != null) agent.isStopped = true;
        }

        chargeTimer += Time.deltaTime;
        if (chargeTimer >= chargeTime)
        {
            StartCoroutine(PerformAttack());
            chargeTimer = 0f;
            isCharging = false;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.acceleration = agentAcceleration;
        agent.SetDestination(player.position);

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * rotationSpeed
            );
        }
    }

    IEnumerator PerformAttack()
    {
        if (isAttacking) yield break;
        isAttacking = true;

        if (agent != null) agent.isStopped = true;

        // Rotate towards player
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        // Optional: club windup
        if (enemyClub != null)
        {
            float t = 0f;
            Quaternion start = Quaternion.Euler(Vector3.zero);
            Quaternion end = Quaternion.Euler(-50, 0, 0);
            while (t < 1f)
            {
                t += Time.deltaTime * 3f;
                enemyClub.localRotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.12f);

        // Lunge forward
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + transform.forward * lungeDistance;
        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / lungeDuration);
            yield return null;
        }

        // Hit check
        Vector3 origin = clubTip ? clubTip.position : transform.position + transform.forward * 0.8f;
        Collider[] hits = Physics.OverlapSphere(origin, clubHitRange);
        foreach (var c in hits)
        {
            if (c.CompareTag("Player"))
            {
                var ph = c.GetComponentInParent<PlayerHealth>();
                if (ph != null)
                {
                    Vector3 forceDir = (c.transform.position - transform.position).normalized;
                    ph.TakeDamage(damagePerAttack, forceDir * attackForce);
                }
            }
        }

        // Swing back to idle
        if (enemyClub != null)
        {
            float t = 0f;
            Quaternion start = Quaternion.Euler(-50, 0, 0);
            Quaternion end = Quaternion.Euler(Vector3.zero);
            while (t < 1f)
            {
                t += Time.deltaTime * 8f;
                enemyClub.localRotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.2f);
        if (agent != null) agent.isStopped = false;
        isAttacking = false;
    }

    public void TakeHit(Vector3 hitDirection, float force, int damage = 1, bool instantRagdoll = false)
    {
        if (isLaunched) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update existing HP bar only
        if (hpUi != null)
            hpUi.SetHealth((float)currentHealth / maxHealth);

        if (currentHealth > 0 && !instantRagdoll)
            StartCoroutine(TemporaryKnockback(hitDirection, force));
        else
            Launch(hitDirection, force);
    }

    IEnumerator TemporaryKnockback(Vector3 dir, float force)
    {
        if (agent != null) agent.enabled = false;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearDamping = physicsDrag;
        rb.angularDamping = physicsAngularDrag;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(dir.normalized * force, ForceMode.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out var hit, 3f))
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (agent != null)
        {
            agent.enabled = true;
            if (NavMesh.SamplePosition(transform.position, out var navHit, 2f, NavMesh.AllAreas))
                agent.Warp(navHit.position);
        }
    }

    public void Launch(Vector3 direction, float force)
    {
        if (isLaunched) return;
        isLaunched = true;

        // Delete the club
        if (enemyClub != null)
            Destroy(enemyClub.gameObject);

        // Kill AI & animation
        if (agent != null) agent.enabled = false;
        if (animator != null) animator.enabled = false;

        // Enable physics
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        // Ensure collider is active
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // Apply force + random torque for tumbling
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        rb.AddTorque(new Vector3(
            UnityEngine.Random.Range(-1f,1f),
            UnityEngine.Random.Range(-1f,1f),
            UnityEngine.Random.Range(-1f,1f)) * force, ForceMode.Impulse);

        // Remove HP bar
        if (hpBarInstance != null) Destroy(hpBarInstance);
        onDie?.Invoke();

        // Destroy after a few seconds
        Destroy(gameObject, 6f);
    }

    private void SpawnHpBar()
    {
        if (enemyHpBarPrefab == null || hpBarInstance != null) return;

        hpBarInstance = Instantiate(enemyHpBarPrefab, transform.position + hpBarOffset, Quaternion.identity);
        hpUi = hpBarInstance.GetComponentInChildren<EnemyHealthUI>();

        if (hpUi != null)
        {
            hpUi.SetTarget(transform);
            hpUi.SetHealth(2f);
        }
    }

    private void SetRagdoll(bool on)
    {
        if (animator) animator.enabled = !on;

        if (ragdollBodies != null)
        {
            foreach (var r in ragdollBodies)
            {
                if (r == null) continue;
                r.isKinematic = !on;
                r.useGravity = on;
                r.linearDamping = on ? 2f : 0.05f;
                r.angularDamping = on ? 1f : 0.05f;
            }
        }

        if (ragdollColliders != null)
        {
            foreach (var c in ragdollColliders)
                if (c != null) c.enabled = on;
        }

        if (rb != null)
        {
            rb.isKinematic = !on;
            rb.useGravity = on;
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = !on;

            rb.constraints = on ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeRotation;
        }
    }
}