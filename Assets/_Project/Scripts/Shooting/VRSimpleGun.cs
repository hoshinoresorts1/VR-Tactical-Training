using UnityEngine;
using UnityEngine.UI;
using TMPro;         
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(AudioSource))]
public class VRSimpleGun : MonoBehaviour
{
    [Header("사격 및 조준 기준점")]
    public Transform aimPoint; 
    public Transform muzzlePoint; 

    [Header("시각 및 청각 효과")]
    public ParticleSystem muzzleFlash; 
    public AudioClip fireSound;

    [Header("사격 세션 설정 (10발 제한)")]
    public int maxAmmo = 10;
    private int currentShotCount = 0;
    private int accumulatedShootingScore = 0;
    private bool isSessionEnded = false;

    [Header("UI 연동 (사격장 전용)")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI liveScoreText;
    public GameObject resultPanel;
    public TextMeshProUGUI finalScoreText;

    private XRGrabInteractable grabInteractable;
    private AudioSource audioSource; 
    private float fireRange = 100f;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; 

        UpdateShootingUI();
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (grabInteractable != null) grabInteractable.activated.AddListener(OnTriggerPulled);
    }

    private void OnDisable()
    {
        if (grabInteractable != null) grabInteractable.activated.RemoveListener(OnTriggerPulled);
    }

    private void Update()
    {
        if (aimPoint != null)
        {
            Debug.DrawRay(aimPoint.position, aimPoint.forward * fireRange, Color.green);
        }
    }

    private void OnTriggerPulled(ActivateEventArgs args)
    {
        if (isSessionEnded) return;
        FireWeapon();
    }

    private void FireWeapon()
    {
        currentShotCount++;

        if (audioSource != null && fireSound != null) audioSource.PlayOneShot(fireSound);
        if (muzzleFlash != null && muzzlePoint != null)
        {
            muzzleFlash.transform.position = muzzlePoint.position;
            muzzleFlash.transform.rotation = muzzlePoint.rotation;
            muzzleFlash.Play();
        }

        if (aimPoint != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(aimPoint.position, aimPoint.forward, out hit, fireRange))
            {
                ShootingTarget target = hit.collider.GetComponent<ShootingTarget>();
                
                if (target != null)
                {
                    // 새로 개편한 타원형 점수 계산 공식을 호출하여 정밀 판정!
                    int earnedScore = target.CalculateEllipseScore(hit.point);
                    accumulatedShootingScore += earnedScore;
                }
            }
        }

        UpdateShootingUI();

        if (currentShotCount >= maxAmmo)
        {
            EndShootingSession();
        }
    }

    private void UpdateShootingUI()
    {
        // 한글을 지우고 영어로 바꾸면 기본 폰트에서 절대 깨지지 않습니다!
        if (ammoText != null) ammoText.text = $"AMMO: {maxAmmo - currentShotCount} / {maxAmmo}";
        if (liveScoreText != null) liveScoreText.text = $"SCORE: {accumulatedShootingScore}";
    }

    private void EndShootingSession()
    {
        isSessionEnded = true;

        if (TrainingScoreManager.Instance != null)
        {
            TrainingScoreManager.Instance.AddShootingScore(accumulatedShootingScore);
        }

        if (resultPanel != null) resultPanel.SetActive(true);
        if (finalScoreText != null)
        {
            // 결과창 문구도 영어로 깔끔하게 치환
            finalScoreText.text = $"FINISH\n\nTOTAL: {accumulatedShootingScore} / 100";
        }
    }
}