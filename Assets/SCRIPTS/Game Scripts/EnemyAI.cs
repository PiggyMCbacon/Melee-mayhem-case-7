using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float stoppingDistance = 2f;

    [Header("Combat")]
    public float chargeTime = 1.5f;
    public float attackRange = 2f;
    public int maxHealth = 2;
    public int damagePerAttack = 2;
    public float attackForce = 8f;
    public Transform clubTip;
    public float clubHitRange = 1.5f;

    [Header("Club Swing")]
    public Transform enemyClub;
    public Vector3 idleRotation = Vector3.zero;
    public Vector3 chargedRotation = new Vector3(-50, 0, 0);
    public Vector3 swingRotation = new Vector3(80, 0, 0);
    public float swingSpeed = 8f;

    [Header("Ragdoll")]
    public Rigidbody[] ragdollBodies;
    public Collider[] ragdollColliders;
    public Animator animator;

    [Header("UI")]
    public GameObject enemyHpBarPrefab;
    public Vector3 hpBarOffset = Vector3.up * 2f;

    public event Action onDie;

    // Runtime
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.updateRotation = false;
            agent.updateUpAxis = true;
        }

        SetRagdoll(false);
    }

    void Update()
    {
        if (isLaunched || isAttacking) return;
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // --- Attack Logic ---
        if (dist <= attackRange)
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
        else
        {
            // --- Chase Player ---
            if (!isCharging && agent != null)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);

                Vector3 lookDir = (agent.steeringTarget - transform.position);
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(lookDir),
                        Time.deltaTime * 8f
                    );
                }
            }
        }

        // --- Update HP Bar ---
        if (hpUi != null && hpBarInstance != null)
        {
            hpUi.SetPosition(transform.position + hpBarOffset);
            hpUi.SetHealth((float)currentHealth / maxHealth);
        }
    }

    IEnumerator PerformAttack()
    {
        if (isAttacking) yield break;
        isAttacking = true;

        if (agent != null) agent.isStopped = true;

        // Rotate towards player
        Vector3 lookDir = (player.position - transform.position);
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        // Charge windup
        if (enemyClub != null)
        {
            float t = 0f;
            Quaternion start = Quaternion.Euler(idleRotation);
            Quaternion end = Quaternion.Euler(chargedRotation);
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                enemyClub.localRotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.15f);

        // Hit detection
        Vector3 origin = (clubTip != null)
            ? clubTip.position
            : transform.position + transform.forward * 0.8f;

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

        // Swing forward
        if (enemyClub != null)
        {
            float t = 0f;
            Quaternion start = Quaternion.Euler(chargedRotation);
            Quaternion end = Quaternion.Euler(swingRotation);
            while (t < 1f)
            {
                t += Time.deltaTime * swingSpeed;
                enemyClub.localRotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * (swingSpeed * 0.8f);
                enemyClub.localRotation = Quaternion.Slerp(Quaternion.Euler(swingRotation), Quaternion.Euler(idleRotation), t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.25f);

        if (agent != null)
        {
            agent.isStopped = false;
            agent.updatePosition = true;
        }

        isAttacking = false;
    }

    public void TakeHit(Vector3 hitDirection, float force, bool instantRagdoll = false)
    {
        if (isLaunched) return;

        currentHealth--;

        if (currentHealth < maxHealth && hpBarInstance == null && enemyHpBarPrefab != null)
            SpawnHpBar();

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
        rb.constraints = RigidbodyConstraints.None;

        rb.AddForce(dir.normalized * force, ForceMode.Impulse);

        yield return new WaitForSeconds(0.3f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (!isLaunched)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            if (agent != null) agent.enabled = true;
        }
    }

    public void Launch(Vector3 direction, float force)
    {
        if (isLaunched) return;
        isLaunched = true;

        SetRagdoll(true);

        // Unlock rotation & enable gravity for rolling ragdoll
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }

        if (ragdollBodies != null && ragdollBodies.Length > 0)
        {
            foreach (var r in ragdollBodies)
            {
                if (r == null) continue;
                r.AddForce(direction.normalized * force, ForceMode.Impulse);
            }
        }
        else
        {
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        }

        if (hpBarInstance != null) Destroy(hpBarInstance);

        onDie?.Invoke();
        Destroy(gameObject, 6f);
    }

    void SpawnHpBar()
    {
        if (enemyHpBarPrefab == null) return;
        hpBarInstance = Instantiate(enemyHpBarPrefab, transform.position + hpBarOffset, Quaternion.identity);
        hpUi = hpBarInstance.GetComponent<EnemyHealthUI>();
        if (hpUi != null)
        {
            hpUi.SetMaxHealth(maxHealth);
            hpUi.SetHealth((float)currentHealth / maxHealth);
            hpUi.SetTarget(transform);
        }
    }

    void SetRagdoll(bool on)
    {
        if (animator) animator.enabled = !on;

        if (ragdollBodies != null)
        {
            foreach (var r in ragdollBodies)
            {
                if (r == null) continue;
                r.isKinematic = !on;
            }
        }

        if (ragdollColliders != null)
        {
            foreach (var c in ragdollColliders)
            {
                if (c == null) continue;
                c.enabled = on;
            }
        }

        if (rb != null)
        {
            rb.isKinematic = on;
            var collider = GetComponent<Collider>();
            if (collider != null) collider.enabled = !on;
        }
    }
}