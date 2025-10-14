using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float chargeDuration = 1.5f;
    public float hitForce = 15f;
    public float hitRange = 2f;
    public LayerMask enemyLayer;

    [Header("Club Settings")]
    public Transform club; // your cylinder object
    public Vector3 idleRotation = new Vector3(0, 0, 0);
    public Vector3 chargedRotation = new Vector3(-60, 0, 0);
    public Vector3 swingRotation = new Vector3(90, 0, 0);
    public float swingSpeed = 6f;

    [Header("Movement")]
    public float moveSpeed = 4f;

    Rigidbody rb;
    bool isCharging = false;
    bool chargedReady = false;
    float chargeTimer = 0f;
    Quaternion targetRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (club != null)
            targetRot = Quaternion.Euler(idleRotation);
    }

    void Update()
    {
        // Movement (simple)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v);
        if (move.magnitude > 0.1f)
        {
            transform.forward = Vector3.Lerp(transform.forward, move.normalized, 0.2f);
            rb.MovePosition(transform.position + move * moveSpeed * Time.deltaTime);
        }

        // Charge
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeTimer = 0f;
        }

        if (isCharging)
        {
            chargeTimer += Time.deltaTime;

            // rotate club backward smoothly
            if (club != null)
            {
                targetRot = Quaternion.Euler(chargedRotation);
                club.localRotation = Quaternion.Lerp(club.localRotation, targetRot, Time.deltaTime * 4f);
            }

            if (chargeTimer >= chargeDuration)
            {
                chargedReady = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isCharging = false;
                if (chargedReady)
                    StartCoroutine(SwingClub());
                else
                    StartCoroutine(SwingClub(0.5f)); // short swing if not fully charged

                chargedReady = false;
            }
        }
        else if (club != null && !isCharging)
        {
            // return to idle
            targetRot = Quaternion.Euler(idleRotation);
            club.localRotation = Quaternion.Lerp(club.localRotation, targetRot, Time.deltaTime * 4f);
        }
    }

    System.Collections.IEnumerator SwingClub(float multiplier = 1f)
    {
        // quick forward swing
        if (club != null)
        {
            Quaternion swingRot = Quaternion.Euler(swingRotation);
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * swingSpeed;
                club.localRotation = Quaternion.Slerp(Quaternion.Euler(chargedRotation), swingRot, t);
                yield return null;
            }

            // detect hit
            Collider[] hits = Physics.OverlapSphere(club.position, hitRange, enemyLayer);
            foreach (var c in hits)
            {
                var enemy = c.GetComponentInParent<EnemyAI>();
                if (enemy != null)
                {
                    Vector3 dir = (enemy.transform.position - transform.position).normalized;
                    enemy.TakeHit(dir, hitForce * multiplier);
                }
            }

            // return to idle after swing
            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * swingSpeed;
                club.localRotation = Quaternion.Slerp(swingRot, Quaternion.Euler(idleRotation), t);
                yield return null;
            }
        }
    }
}