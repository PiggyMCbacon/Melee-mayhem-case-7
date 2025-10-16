using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float chargeDuration = 1.2f;
    public float hitForce = 15f;
    public float hitRange = 2f;
    public LayerMask enemyLayer; // optional: used to filter overlap

    [Header("Club Settings")]
    public Transform club; // cylinder
    public Vector3 idleRotation = Vector3.zero;
    public Vector3 chargedRotation = new Vector3(-60, 0, 0);
    public Vector3 swingRotation = new Vector3(90, 0, 0);
    public float swingSpeed = 6f;

    // runtime
    private bool isCharging = false;
    private bool chargedReady = false;
    private float chargeTimer = 0f;

    // This set tracks which enemies were already hit during the current swing
    private HashSet<GameObject> hitThisSwing = new HashSet<GameObject>();

    void Update()
    {
        HandleAttack();
    }

    void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeTimer = 0f;
            chargedReady = false;
        }

        if (isCharging)
        {
            chargeTimer += Time.deltaTime;

            if (club != null)
                club.localRotation = Quaternion.Lerp(club.localRotation, Quaternion.Euler(chargedRotation), Time.deltaTime * 4f);

            if (chargeTimer >= chargeDuration)
                chargedReady = true;

            if (Input.GetMouseButtonUp(0))
            {
                isCharging = false;
                float powerMultiplier = chargedReady ? 1f : 0.5f;
                StartCoroutine(SwingClub(powerMultiplier));
                chargedReady = false;
            }
        }
        else if (club != null)
        {
            club.localRotation = Quaternion.Lerp(club.localRotation, Quaternion.Euler(idleRotation), Time.deltaTime * 4f);
        }
    }

    IEnumerator SwingClub(float multiplier)
    {
        if (club == null) yield break;

        // clear the per-swing hit list so new swing can hit enemies again
        hitThisSwing.Clear();

        Quaternion startRot = Quaternion.Euler(chargedRotation);
        Quaternion endRot = Quaternion.Euler(swingRotation);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * swingSpeed;
            club.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        // hit detection
        // Use layer mask if provided; otherwise fallback to all layers.
        Collider[] hits;
        if (enemyLayer.value != 0)
            hits = Physics.OverlapSphere(club.position, hitRange, enemyLayer);
        else
            hits = Physics.OverlapSphere(club.position, hitRange);

        foreach (var c in hits)
        {
            // find the enemy root (in case hit child colliders)
            var enemy = c.GetComponentInParent<EnemyAI>();
            if (enemy == null) continue;

            GameObject enemyRoot = enemy.gameObject;
            if (hitThisSwing.Contains(enemyRoot)) continue; // already hit this swing

            hitThisSwing.Add(enemyRoot);

            // compute direction and apply damage + knockback force
            Vector3 dir = (enemy.transform.position - transform.position).normalized;
            int damage = 1; // club does 1 damage per hit (adjust if you have charged attack)
            float force = hitForce * multiplier;

            enemy.TakeHit(dir, force, damage, false);
        }

        // return to idle
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * swingSpeed;
            club.localRotation = Quaternion.Slerp(endRot, Quaternion.Euler(idleRotation), t);
            yield return null;
        }
    }
}