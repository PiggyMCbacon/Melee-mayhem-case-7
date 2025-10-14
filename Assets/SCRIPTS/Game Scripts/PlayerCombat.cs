using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float chargeDuration = 1.5f;
    public float hitForce = 15f;
    public float hitRange = 2f;
    public LayerMask enemyLayer;

    [Header("Club Settings")]
    public Transform club; // cylinder
    public Vector3 idleRotation = Vector3.zero;
    public Vector3 chargedRotation = new Vector3(-60, 0, 0);
    public Vector3 swingRotation = new Vector3(90, 0, 0);
    public float swingSpeed = 6f;

    private bool isCharging = false;
    private bool chargedReady = false;
    private float chargeTimer = 0f;

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
        }

        if (isCharging)
        {
            chargeTimer += Time.deltaTime;

            if (club != null)
            {
                club.localRotation = Quaternion.Lerp(club.localRotation, Quaternion.Euler(chargedRotation), Time.deltaTime * 4f);
            }

            if (chargeTimer >= chargeDuration)
                chargedReady = true;

            if (Input.GetMouseButtonUp(0))
            {
                isCharging = false;
                float power = chargedReady ? 1f : 0.5f;
                StartCoroutine(SwingClub(power));
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