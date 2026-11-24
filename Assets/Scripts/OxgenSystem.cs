using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OxygenSystem : MonoBehaviour
{
    [Header("Oksigen")]
    public float maxOxygen = 30f;
    public float refillSpeed = 10f;

    [Header("Air")]
    public float waterSurfaceY = 73f;
    public float headOffset = 1.5f;

    [Header("Respawn")]
    public Transform respawnPoint;   // kosongkan untuk pakai posisi awal Player

    [Header("UI Bar")]
    public Slider oxygenSlider;
    public Image fillImage;
    public Color normalColor = new Color(0f, 0.8f, 1f);
    public Color lowColor = Color.red;
    [Range(0f, 1f)] public float lowThreshold = 0.3f;

    [Header("Peringatan")]
    public TMP_Text warningText;     // drag WarningText
    public Image redFlash;           // drag RedFlash
    public float maxFlashAlpha = 0.35f;
    public AudioSource warningSound; // opsional
    public float blinkSpeed = 6f;

    float currentOxygen;
    CharacterController cc;
    Vector3 startPosition;
    Quaternion startRotation;

    void Start()
    {
        currentOxygen = maxOxygen;
        cc = GetComponent<CharacterController>();
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (oxygenSlider != null) oxygenSlider.maxValue = maxOxygen;
        if (warningText != null) warningText.gameObject.SetActive(false);
        if (redFlash != null) redFlash.color = new Color(1f, 0f, 0f, 0f);
    }

    void Update()
    {
        bool headUnderwater = (transform.position.y + headOffset) < waterSurfaceY;

        if (headUnderwater) currentOxygen -= Time.deltaTime;
        else currentOxygen += refillSpeed * Time.deltaTime;

        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);

        if (oxygenSlider != null) oxygenSlider.value = currentOxygen;

        bool isLow = headUnderwater && (currentOxygen / maxOxygen) <= lowThreshold;
        float blink = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f; // 0 sampai 1

        // Bar berkedip merah
        if (fillImage != null)
        {
            if (isLow) fillImage.color = Color.Lerp(lowColor, Color.white, blink * 0.5f);
            else fillImage.color = normalColor;
        }

        // Teks peringatan
        if (warningText != null)
        {
            warningText.gameObject.SetActive(isLow);
            if (isLow)
            {
                Color c = warningText.color;
                c.a = Mathf.Lerp(0.3f, 1f, blink);
                warningText.color = c;
            }
        }

        // Layar merah berkedip
        if (redFlash != null)
        {
            float targetAlpha = isLow ? Mathf.Lerp(0.05f, maxFlashAlpha, blink) : 0f;
            Color c = redFlash.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 10f);
            redFlash.color = c;
        }

        // Suara
        if (warningSound != null)
        {
            if (isLow && !warningSound.isPlaying) warningSound.Play();
            else if (!isLow && warningSound.isPlaying) warningSound.Stop();
        }

        if (currentOxygen <= 0f) Respawn();
    }

    void Respawn()
    {
        Vector3 pos = respawnPoint != null ? respawnPoint.position : startPosition;
        Quaternion rot = respawnPoint != null ? respawnPoint.rotation : startRotation;

        if (cc != null) cc.enabled = false;
        transform.SetPositionAndRotation(pos, rot);
        if (cc != null) cc.enabled = true;

        currentOxygen = maxOxygen;

        if (redFlash != null) redFlash.color = new Color(1f, 0f, 0f, 0f);
        if (warningText != null) warningText.gameObject.SetActive(false);
        if (warningSound != null) warningSound.Stop();
    }
}