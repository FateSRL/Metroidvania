using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    [SerializeField] protected float cooldown;
    protected float cooldownTimer;

    protected Player player;

    protected virtual void Start()
    {
        player = PlayerManager.instance.player;

    }

    protected virtual void Update()
    {
        cooldownTimer -= Time.deltaTime;
    }

    public virtual bool IsReady()
    {
        return cooldownTimer <= 0;
    }

    public float GetCooldownFillAmount()
    {
        if (cooldown <= 0)
            return 0;

        return Mathf.Clamp01(cooldownTimer / cooldown);
    }

    public virtual bool CanUseSkill()
    {
        if(IsReady()){
            UseSkill();
            StartCooldown();
            return true;
        } 
        Debug.Log("Skill is on cooldown");
        return false;
    }

    public void StartCooldown()
    {
        cooldownTimer = cooldown;
    }
    public virtual void UseSkill()
    {
        
    }
    
}
