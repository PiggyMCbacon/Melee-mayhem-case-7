using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float stoppingDistance = 2f; // when to stop and charge
    public float chargeTime = 1.5f; // how long enemy stands still to charge
    public int health = 1; // how many hits before ragdoll

    public float attackRange = 2f;
    public event Action onDie;

    Rigidbody rb;
    Transform player;
    bool isCharging = false;
    float chargeTimer = 0f;
    bool isLaunched = false;

    // For ragdoll: expect these child rigidbodies (optional).
    public Rigidbody[] ragdollBodies;
    public Collider[] ragdollColliders;
    public Animator animator; // optional

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // initially disable ragdoll bodies if any (they should be kinematic).
        SetRagdoll(false);
    }

    void Update()
    {
        if (isLaunched) return;

        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > stoppingDistance)
        {
            // move toward player
            if (!isCharging)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                Vector3 move = dir * moveSpeed * Time.deltaTime;
                rb.MovePosition(transform.position + move);
                transform.forward = Vector3.Lerp(transform.forward, dir, 0.2f);
            }
        }
        else
        {
            // in range => charge
            if (!isCharging)
            {
                isCharging = true;
                chargeTimer = 0f;
            }

            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeTime)
            {
                // perform attack (not implemented full weapon logic)
                // reset charge to try again after a delay
                chargeTimer = 0f;
                isCharging = false; // enemy will move a bit then charge again
            }
        }
    }

    public void TakeHit(Vector3 hitDirection, float force, bool instantRagdoll = false)
    {
        health--;
        if (health <= 0 || instantRagdoll)
        {
            Launch(hitDirection, force);
        }
    }

    public void Launch(Vector3 direction, float force)
    {
        if (isLaunched) return;
        isLaunched = true;

        // enable ragdoll
        SetRagdoll(true);

        // apply impulse to all ragdoll bodies if available
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
            // fallback: add force to main rigidbody
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        }

        // schedule destroy
        Destroy(gameObject, 6f);

        onDie?.Invoke();
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

        // main collider/rigidbody
        // keep main collider enabled to avoid sinking through ground (optional)
    }
}