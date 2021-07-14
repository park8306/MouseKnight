using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Monster
{
    // 방패 막기 추가.
    // 공격 하는 타이밍에 공격 대신 막기 랜덤하게 진행.
    // 막고 있는 동안에는 데미지 없음(대신 막았다는 이펙트 생성)
    override protected void SelectAttackType()
    {
        if(Random.Range(0,1f) > 0.5f)
            CurrentFsm = AttackFSM;
        else
            CurrentFsm = ShieldFSM;
    }
    bool isOnShield = false;
    public float activeShieldTime = 2;
    protected IEnumerator ShieldFSM()
    {
        PlayAnimation("Shield");
        isOnShield = true;
        yield return new WaitForSeconds(activeShieldTime);
        isOnShield = false;
        CurrentFsm = ChaseFSM;
    }

    public GameObject succeedBlockEffect;
    public Transform successBlockEffectPosition;

    enum Direction
    {
        Right,
        Left
    }
    override public void TakeHit(float damage)
    {
        bool succeedBlock = SucceedBlock();
        if (succeedBlock)
        {
            Instantiate(succeedBlockEffect, successBlockEffectPosition.position, Quaternion.identity);
        }
        else
        {
            base.TakeHit(damage);
        }
    }

    private bool SucceedBlock()
    {
        if (isOnShield == false)
        {
            return false;
        }
        //bool isInFront = false;
        Direction myDirection = transform.rotation.eulerAngles.y == 180 ? Direction.Left : Direction.Right;
        if (myDirection == Direction.Right)
        {
            if (player.transform.position.x - transform.position.x < 0)
            {
                // 스켈레톤이 오른쪽 보고 있을 때 플레이어가 왼쪽에서 공격 했다면 막기 실패.
                return false;
            }
        }
        else
        {
            if (transform.position.x - player.transform.position.x < 0)
            {
                // 스켈레톤이 오른쪽 보고 있을 때 플레이어가 왼쪽에서 공격 했다면 막기 실패.
                return false;
            }
        }
        return true;
    }
}
