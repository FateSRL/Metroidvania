using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{ 
    [SerializeField] protected LayerMask whatIsPlayer;

    [Header("Detection info")]
    [SerializeField] protected float playerDetectionDistance = 8;
    [SerializeField] protected float playerDetectionEpsilon = .35f;

    [Header("Stunned info")]
    public float stunDuration;
    public Vector2 stunDirection;
    protected bool canBeStunned;
    [SerializeField] protected GameObject counterImage;

    [Header("Move info")]
    public float moveSpeed;
    public float idleTime;
    public float battleTime;
    public float targetOverlapEpsilon = .25f;

    [Header("Attack info")]
    public float attackDistance;
    public float attackCooldown;
    [HideInInspector]public float lastTimeAttacked;
    private float defaultMoveSpeed;
    private float defaultAnimSpeed = 1;
    private float defaultGravityScale;
    private RigidbodyType2D defaultBodyType;
    private RigidbodyConstraints2D defaultConstraints;
    private bool isFrozen;
    

    public EnemyStateMachine stateMachine { get; private set; } 
    private bool isDead;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();
    }

    protected override void Start()
    {
        base.Start();

        defaultMoveSpeed = moveSpeed;

        if (anim != null)
            defaultAnimSpeed = anim.speed;

        if (rb != null)
        {
            defaultGravityScale = rb.gravityScale;
            defaultBodyType = rb.bodyType;
            defaultConstraints = rb.constraints;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isDead)
            return;

        if (isFrozen)
        {
            if (rb != null)
                rb.velocity = Vector2.zero;

            return;
        }

        stateMachine.currentState.Update();
    }

    public virtual void OpenCounterAttackWindow()
    {
        canBeStunned = true;
        counterImage.SetActive(true);
    }
    public virtual void CloseCounterAttackWindow()
    {
        canBeStunned = false;
        counterImage.SetActive(false);
    }

    public virtual bool CanBeStunned(){
        if(canBeStunned)
        {
            CloseCounterAttackWindow();
            return true;
        }
        return false;
    }


    public virtual void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    public virtual void TakeBlackholeDamage()
    {
        int blackholeDamage = GetPlayerAttackDamage();

        if (stats != null && !stats.TakeDamage(blackholeDamage))
            return;

        if (fx != null)
            fx.StartCoroutine("FlashFX");

        AudioManager.PlaySFX(SfxType.EnemyHit, .45f);

        Debug.Log(transform.name + " took blackhole damage");
    }

    private int GetPlayerAttackDamage()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null || PlayerManager.instance.player.stats == null)
            return 1;

        return Mathf.Max(1, PlayerManager.instance.player.stats.GetDamage());
    }

    public virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        AudioManager.PlaySFX(SfxType.EnemyDeath, .7f);

        if (anim != null)
        {
            anim.speed = defaultAnimSpeed;
            anim.SetBool("Die", true);
        }

        if (counterImage != null)
            counterImage.SetActive(false);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }

    public virtual void FreezeTime(bool _timeFrozen)
    {
        if (isDead)
            return;

        isFrozen = _timeFrozen;

        if (_timeFrozen)
        {
            moveSpeed = 0;

            if (anim != null)
                anim.speed = 0;

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0;
                rb.gravityScale = 0;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        else
        {
            moveSpeed = defaultMoveSpeed;

            if (anim != null)
                anim.speed = defaultAnimSpeed;

            if (rb != null)
            {
                rb.bodyType = defaultBodyType;
                rb.constraints = defaultConstraints;
                rb.gravityScale = defaultGravityScale;
                rb.velocity = Vector2.zero;
            }
        }
    }
    

    public virtual RaycastHit2D IsPlayerDetected()
    {
        if (wallCheck == null)
            return default;

        int detectionMask = whatIsPlayer | whatIsGround;
        float detectionDistance = playerDetectionDistance + playerDetectionEpsilon;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, detectionDistance, detectionMask);

        if (hit.collider == null)
            return default;

        if (((1 << hit.collider.gameObject.layer) & whatIsPlayer.value) == 0)
            return default;

        return hit;
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color=Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance*facingDir, transform.position.y));

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + playerDetectionDistance * facingDir, wallCheck.position.y));

            if (playerDetectionEpsilon > 0)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(
                    new Vector3(wallCheck.position.x + playerDetectionDistance * facingDir, wallCheck.position.y),
                    new Vector3(wallCheck.position.x + (playerDetectionDistance + playerDetectionEpsilon) * facingDir, wallCheck.position.y));
            }
        }
    }
}
