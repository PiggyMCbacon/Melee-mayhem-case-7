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
    public Transform club; // your cylinder object
    public Vector3 idleRotation = new Vector3(0, 0, 0);
    public Vector3 chargedRotation = new Vector3(-60, 0, 0);
    public Vector3 swingRotation = new Vector3(90, 0, 0);
    public float swingSpeed = 6f;

    private Rigidbody rb;
    private bool isCharging = false;
    private bool chargedReady = false;
    private float chargeTimer = 0f;
    private Quaternion targetRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (club != null)
            targetRot = Quaternion.Euler(idleRotation);
    }

    void Update()
    {
        HandleAttack();
    }

    void HandleAttack()
    {
        // Start charging
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeTimer = 0f;
        }

        if (isCharging)
        {
            chargeTimer += Time.deltaTime;

            // rotate club backward smoothly while charging
            if (club != null)
            {
                targetRot = Quaternion.Euler(chargedRotation);
                club.localRotation = Quaternion.Lerp(club.localRotation, targetRot, Time.deltaTime * 4f);
            }

            // fully charged
            if (chargeTimer >= chargeDuration)
                chargedReady = true;

            // release the swing
            if (Input.GetMouseButtonUp(0))
            {
                isCharging = false;
                float power = chargedReady ? 1f : 0.5f;
                StartCoroutine(SwingClub(power));
                chargedReady = false;
            }
        }
        else if (club != null && !isCharging)
        {
            // return club to idle
            targetRot = Quaternion.Euler(idleRotation);
            club.localRotation = Quaternion.Lerp(club.localRotation, targetRot, Time.deltaTime * 4f);
        }
    }

    IEnumerator SwingClub(float multiplier)
    {
        if (club == null) yield break;

        // forward swing
        Quaternion startRot = Quaternion.Euler(chargedRotation);
        Quaternion endRot = Quaternion.Euler(swingRotation);
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * swingSpeed;
            club.localRotation = Quaternion.Slerp(startRot, endRot, t);
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
            club.localRotation = Quaternion.Slerp(endRot, Quaternion.Euler(idleRotation), t);
            yield return null;
        }
    }
}