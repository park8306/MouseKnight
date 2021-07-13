using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    public static Player instance;
    private void Awake()
    {
        instance = this;
    }
    [SerializeField] StateType state = StateType.Idle;
    public float speed = 20;
    public float walkDistance = 12;
    public float stopDistance = 7;
    float normalSpeed;
    public Transform mousePointer;
    public Transform spriteTr;
    SpriteTrailRenderer.SpriteTrailRenderer spriteTrailRenderer;
    Plane plane = new Plane(new Vector3(0, 1, 0), 0);
    NavMeshAgent agent;
    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        spriteTr = GetComponentInChildren<SpriteRenderer>().transform;
        normalSpeed = speed;
        spriteTrailRenderer = GetComponentInChildren<SpriteTrailRenderer.SpriteTrailRenderer>();
        spriteTrailRenderer.enabled = false;
        agent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        //RaycastHit hit;
        if (CanMoveState())
        {
            Move();
            Jump();
        }

        bool isSuccessDash = Dash();
        Attack(isSuccessDash);
    }

    private bool CanMoveState()
    {
        if (State == StateType.Attack)
            return false;
        if (State == StateType.TakeHit)
            return false;
        if (State == StateType.Death)
            return false;
        return true;
    }

    private void Attack(bool isSuccessDash)
    {
        if (isSuccessDash)
        {
            return;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            StartCoroutine(AttackCo());
        }
    }

    public float attackTime = 1;
    public float attackApplyTime = 0.2f;
    public LayerMask enemyLayer;
    public SphereCollider attackCollider;
    public float power = 10;
    private IEnumerator AttackCo()
    {
        State = StateType.Attack;
        yield return new WaitForSeconds(attackApplyTime);
        // 실제 어택하는 부분.
        var enemyColliders = Physics.OverlapSphere(attackCollider.transform.position,
            attackCollider.radius, enemyLayer);
        foreach (var item in enemyColliders)
        {

            // 왜 고블린 hp가 안달까...
            item.GetComponent<Goblin>().TakeHit(power);
        }

        yield return new WaitForSeconds(attackTime);
        State = StateType.Idle;
    }
    #region

    public float dashCoolTime = 2f;
    float nextDashableTime;
    [Foldout("대시")] public float dashableDistance = 10;
    [Foldout("대시")] public float dashableTime = 0.4f;
    float mouseDownTime;
    Vector3 mouseDownPosition;
    private bool Dash()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            mouseDownTime = Time.time;
            mouseDownPosition = Input.mousePosition;
        }
        if (nextDashableTime < Time.time) // 대쉬 가능한 시간이 현재 시간보다 작다면 실행 ( 대쉬 쿨타임 적용을 위함 )
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                bool isDashDrag = IsSuccessDashDrag();
                if (isDashDrag)
                {
                    nextDashableTime = Time.time + dashCoolTime; // 대쉬가 실행된 현재 시간에 쿨타임 시간을 더해주기
                    StartCoroutine(DashCo());
                    return true;
                }
            }
        }
        return false;
    }
    public float hp = 100;

    internal void TakeHit(int damage)
    {
        if (State == StateType.Death)
        {
            return;
        }
        hp -= damage;
        // 피격 모션하자.

        StartCoroutine(TakeHitCo());

        // hp < 0 으면 죽자.
    }

    public float takeHitTime = 0.3f;
    private IEnumerator TakeHitCo()
    {
        State = StateType.TakeHit;
        yield return new WaitForSeconds(takeHitTime);
        if (hp > 0)
            State = StateType.Idle;
        else
            StartCoroutine(DeathCo());
    }
    public float deathTime = 0.5f;
    private IEnumerator DeathCo()
    {
        State = StateType.Death;
        yield return new WaitForSeconds(deathTime);
        Debug.Log("게임 종료");
    }

    [Foldout("대시")] public float dashTime = 0.3f;
    [Foldout("대시")] public float dashSpeedMultiplySpeed = 4f;
    Vector3 dashDirection;
    private IEnumerator DashCo()
    {
        // dashDirection x방향만 사용.
        spriteTrailRenderer.enabled = true; // 대쉬하는 동안 트레일을 활성
        dashDirection = Input.mousePosition - mouseDownPosition;
        dashDirection.y = 0;
        dashDirection.z = 0;
        dashDirection.Normalize();
        speed = normalSpeed * dashSpeedMultiplySpeed;
        State = StateType.Dash;

        yield return new WaitForSeconds(dashTime);
        speed = normalSpeed;
        State = StateType.Idle;
        spriteTrailRenderer.enabled = false;

    }

    private bool IsSuccessDashDrag()
    {
        // 시간 체크
        float dragTime = Time.time - mouseDownTime; // 마우스를 클릭을 땐 현재 시간과 처음 마우스를 눌렀던 시간의 차이 (총 drag 시간)
        if (dragTime > dashableTime) // 총 drag시간이 설정한 dashableTime(0.4f)보다 작다면
        {
            return false; // 대쉬를 실행 안함
        }

        float dragDistance = Vector3.Distance(mouseDownPosition, Input.mousePosition); // 총 drag한 길이
        if (dragDistance < dashableDistance) // 총 drag한 길이가 대쉬가 가능한 거리 보다 작으면 
        {
            return false;   // 대쉬를 실행안함
        }

        return true;    // 그 이외의 상황들은 대쉬를 실행
    }
    #endregion
    [BoxGroup("점프")]public AnimationCurve jumpYac;
    private void Jump()
    {
        if (jumpState == JumpStateType.Jump)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartCoroutine(JumpCo());
        }
    }
    public enum JumpStateType
    {
        Ground,
        Jump,
    }
    public enum StateType
    {
        Idle,
        Walk,
        JumpUp,
        JumpDown,
        Attack,
        TakeHit,
        Death,
        Dash,
    }
    StateType State
    {
        get { return state; }
        set
        {
            if (state == value)
                return;
            state = value;

            animator.Play(state.ToString());
        }
    }
    Animator animator;
    JumpStateType jumpState;
    [BoxGroup("점프")] public float jumpYMultiply = 1;
    [BoxGroup("점프")] public float jumpTimeMultiply = 1;
    private IEnumerator JumpCo()
    {
        jumpState = JumpStateType.Jump;
        State = StateType.JumpUp;
        agent.enabled = false;
        float jumpStartTime = Time.time;
        float jumpDuration = jumpYac[jumpYac.length - 1].time; // 점프커브의 총 시간, length는 점의 갯수?
        jumpDuration *= jumpTimeMultiply;
        float jumpEndTime = jumpStartTime + jumpDuration;
        float sumEvaluateTime = 0;
        float previousY = float.MinValue;
        while (Time.time < jumpEndTime)
        {
            float y = jumpYac.Evaluate(sumEvaluateTime / jumpTimeMultiply);
            y *= jumpYMultiply * Time.deltaTime;
            transform.Translate(0, y, 0);
            yield return null;

            if (previousY > transform.position.y)
            {
                State = StateType.JumpDown;
            }

            if (transform.position.y < 0)
            {
                break;
            }
            previousY = transform.position.y;
            sumEvaluateTime += Time.deltaTime;
        }
        agent.enabled = true;
        jumpState = JumpStateType.Ground;
        State = StateType.Idle;
    }

    private void Move()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            mousePointer.position = hitPoint;
            float distance = Vector3.Distance(hitPoint, transform.position);

            float moveableDistance = stopDistance;
            //State가 Walk일땐 7(stopDIstance)사용.
            // Idle에서 Walk로 갈땐 12(WalkDistance)사용.
            if (State == StateType.Idle)
            {
                moveableDistance = walkDistance;
            }
            Vector3 dir = hitPoint - transform.position;
            if (state == StateType.Dash)
            {
                dir = dashDirection;
            }
            dir.Normalize();
            if (distance > moveableDistance || State == StateType.Dash)
            {
                
                transform.Translate(dir * speed * Time.deltaTime, Space.World);

                // 방향(dir)에 따라서
                // 오른쪽이라면 Y : 0, spriteX : 45
                // 왼쪽이라면 Y : 180, spriteX : -45
                
                if (ChangeableState())
                    State = StateType.Walk;
            }
            else
            {
                if (ChangeableState())
                    State = StateType.Idle;
            }

            bool isRightSide = dir.x > 0;
            if (isRightSide)
            {
                transform.rotation = Quaternion.Euler(Vector3.zero);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            bool ChangeableState()
            {
                if (jumpState == JumpStateType.Jump)
                {
                    return false;
                }
                if (state == StateType.Dash)
                {
                    return false;
                }
                return true;
            }
        }
    }


}
