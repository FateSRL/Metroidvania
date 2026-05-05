using System.Collections.Generic;
using UnityEngine;

public enum SfxType
{
    PlayerAttack,
    PlayerHit,
    PlayerDeath,
    SwordThrow,
    SwordCatch,
    EnemyHit,
    EnemyDeath,
    SkeletonAttack,
    BossAttack,
    BossSpell,
    BossLaser,
    BossDeath,
    MenuClick
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private const string SfxRoot = "Audio/SFX/";

    [SerializeField] private float sfxVolume = .75f;
    [SerializeField] private float pitchJitter = .08f;
    [SerializeField] private float duplicateSoundCooldown = .04f;

    private AudioSource sfxSource;
    private readonly Dictionary<SfxType, AudioClip> clips = new Dictionary<SfxType, AudioClip>();
    private readonly Dictionary<SfxType, float> lastPlayTimes = new Dictionary<SfxType, float>();

    private static readonly Dictionary<SfxType, string> clipNames = new Dictionary<SfxType, string>
    {
        { SfxType.PlayerAttack, "player_attack" },
        { SfxType.PlayerHit, "player_hit" },
        { SfxType.PlayerDeath, "player_death" },
        { SfxType.SwordThrow, "sword_throw" },
        { SfxType.SwordCatch, "sword_catch" },
        { SfxType.EnemyHit, "enemy_hit" },
        { SfxType.EnemyDeath, "enemy_death" },
        { SfxType.SkeletonAttack, "skeleton_attack" },
        { SfxType.BossAttack, "boss_attack" },
        { SfxType.BossSpell, "boss_spell" },
        { SfxType.BossLaser, "boss_laser" },
        { SfxType.BossDeath, "boss_death" },
        { SfxType.MenuClick, "menu_click" }
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureSource();
    }

    public static void PlaySFX(SfxType type, float volumeScale = 1f)
    {
        if (!Application.isPlaying)
            return;

        EnsureInstance().Play(type, volumeScale);
    }

    private static AudioManager EnsureInstance()
    {
        if (instance != null)
            return instance;

        AudioManager existingManager = FindObjectOfType<AudioManager>();
        if (existingManager != null)
            return existingManager;

        GameObject managerObject = new GameObject("AudioManager");
        return managerObject.AddComponent<AudioManager>();
    }

    private void EnsureSource()
    {
        if (sfxSource != null)
            return;

        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
    }

    private void Play(SfxType type, float volumeScale)
    {
        if (lastPlayTimes.TryGetValue(type, out float lastPlayTime) && Time.unscaledTime < lastPlayTime + duplicateSoundCooldown)
            return;

        AudioClip clip = GetClip(type);
        if (clip == null)
            return;

        EnsureSource();
        lastPlayTimes[type] = Time.unscaledTime;
        sfxSource.pitch = Random.Range(1f - pitchJitter, 1f + pitchJitter);
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(sfxVolume * volumeScale));
    }

    private AudioClip GetClip(SfxType type)
    {
        if (clips.TryGetValue(type, out AudioClip clip))
            return clip;

        if (!clipNames.TryGetValue(type, out string clipName))
            return null;

        clip = Resources.Load<AudioClip>(SfxRoot + clipName);

        if (clip == null)
            Debug.LogWarning("AudioManager: Missing SFX clip " + clipName);

        clips[type] = clip;
        return clip;
    }
}
