using System;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(AudioSource))]
public class VRSimpleGun : MonoBehaviour
{
    [Header("Aim")]
    public Transform aimPoint;
    public Transform muzzlePoint;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioClip fireSound;

    [Header("Shooting Session")]
    public int maxAmmo = 10;

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI liveScoreText;
    public GameObject resultPanel;
    public TextMeshProUGUI finalScoreText;

    private int currentShotCount;
    private int accumulatedShootingScore;
    private bool isSessionEnded;
    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource;
    private readonly float fireRange = 100f;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        UpdateShootingUI();

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.activated.AddListener(OnTriggerPulled);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.activated.RemoveListener(OnTriggerPulled);
    }

    private void Update()
    {
        if (aimPoint != null)
            Debug.DrawRay(aimPoint.position, aimPoint.forward * fireRange, Color.green);
    }

    private void OnTriggerPulled(ActivateEventArgs args)
    {
        if (isSessionEnded)
            return;

        FireWeapon();
    }

    private void FireWeapon()
    {
        if (currentShotCount >= maxAmmo)
            return;

        currentShotCount++;

        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);

        if (muzzleFlash != null && muzzlePoint != null)
        {
            muzzleFlash.transform.SetPositionAndRotation(muzzlePoint.position, muzzlePoint.rotation);
            muzzleFlash.Play();
        }

        if (TryGetHitTargetScore(out int earnedScore))
            accumulatedShootingScore += Mathf.Max(1, earnedScore);

        UpdateShootingUI();

        if (currentShotCount >= maxAmmo)
            EndShootingSession();
    }

    private bool TryGetHitTargetScore(out int score)
    {
        score = 0;

        if (aimPoint == null)
            return false;

        Ray aimRay = new Ray(aimPoint.position, aimPoint.forward);
        RaycastHit[] hits = Physics.RaycastAll(aimRay, fireRange);
        if (hits == null || hits.Length == 0)
            return false;

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider.transform.IsChildOf(transform))
                continue;

            ShootingTarget target = FindTargetFromHit(hit.collider);
            if (target == null)
                continue;

            if (target.TryCalculateScoreFromRay(aimRay, fireRange, out score, out _))
                return true;

            score = target.CalculateEllipseScore(hit.point);
            return true;
        }

        return false;
    }

    private ShootingTarget FindTargetFromHit(Collider hitCollider)
    {
        ShootingTarget target = hitCollider.GetComponent<ShootingTarget>();
        if (target != null)
            return target;

        target = hitCollider.GetComponentInParent<ShootingTarget>();
        if (target != null)
            return target;

        target = hitCollider.GetComponentInChildren<ShootingTarget>();
        if (target != null)
            return target;

        Transform root = hitCollider.transform.root;
        return root != null ? root.GetComponentInChildren<ShootingTarget>() : null;
    }

    private void UpdateShootingUI()
    {
        if (ammoText != null)
            ammoText.text = $"Ammo: {maxAmmo - currentShotCount} / {maxAmmo}";

        if (liveScoreText != null)
            liveScoreText.text = $"Score: {accumulatedShootingScore}";
    }

    private void EndShootingSession()
    {
        isSessionEnded = true;

        TrainingScoreManager.GetOrCreate().SetShootingScore(accumulatedShootingScore);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {accumulatedShootingScore} / 100";
    }
}
