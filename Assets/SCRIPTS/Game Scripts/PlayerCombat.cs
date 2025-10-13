using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCombat : MonoBehaviour
{
    public float chargeDuration = 1.5f;
    public Transform clubTip; // where to raycast / detect hits from
    public float hitRange = 2f;
    public float hitForce = 15f;
    public LayerMask enemyLayer;
    public float moveSpeed = 4f;

    Rigidbody rb;
    float chargeTimer = 0f;
    bool isCharging = false;
    bool chargedReady = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Basic movement (placeholder) — replace with your movement system
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v);
        if (move.magnitude > 0.1f)
        {
            Vector3 target = transform.position + move.normalized;
            transform.forward = Vector3.Lerp(transform.forward, move.normalized, 0.2f);
            rb.MovePosition(transform.position + move * moveSpeed * Time.deltaTime);
        }

        // Charge attack input (left click)
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeTimer = 0f;
            chargedReady = false;
        }

        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeDuration)
            {
                chargedReady = true;
                // feedback: you could play a sound / VFX here
            }

            if (Input.GetMouseButtonUp(0))
            {
                // release
                isCharging = false;
                if (chargedReady)
                {
                    PerformChargedHit();
                }
                else
                {
                    // light swing (optional)
                    PerformLightHit();
                }
                chargedReady = false;
            }
        }
    }

    void PerformChargedHit()
    {
        // detect enemies in front using SphereCast or OverlapSphere
        Vector3 origin = clubTip != null ? clubTip.position : transform.position + transform.forward * 1f;
        Collider[] hits = Physics.OverlapSphere(origin, hitRange, enemyLayer);
        foreach (var c in hits)
        {
            var enemy = c.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                Vector3 dir = (c.transform.position - transform.position).normalized;
                enemy.TakeHit(dir, hitForce, false);
            }
        }
    }

    void PerformLightHit()
    {
        // small push / damage; optional
        Vector3 origin = clubTip != null ? clubTip.position : transform.position + transform.forward * 1f;
        Collider[] hits = Physics.OverlapSphere(origin, hitRange * 0.7f, enemyLayer);
        foreach (var c in hits)
        {
            var enemy = c.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                Vector3 dir = (c.transform.position - transform.position).normalized;
                enemy.TakeHit(dir, hitForce * 0.6f, false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (clubTip != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(clubTip.position, hitRange);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + transform.forward * 1f, hitRange);
        }
    }
}