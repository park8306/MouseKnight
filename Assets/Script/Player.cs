using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    public static Player instance; // 싱글턴을 위한 변수
    private void Awake()
    {
        instance = this;
        m_state = StateType.NotInit; // ???
    }
    public float speed = 20;    // 플레이어의 속도에 곱해줄 수
    public float walkDistance = 12; // 플레이어가 걷을 수 있는 플레이어와 마우스의 거리
    public float stopDistance = 7;  // 플레이어가 멈출 수 있는 플레이어와 마우스의 거리
    float normalSpeed;  // 일반적은 플레이어의 속도
    //public Transform mousePointer;  // 현재 마우스의 위치를 저장할 변수
    public Transform spriteTr;  // 
    SpriteTrailRenderer.SpriteTrailRenderer spriteTrailRenderer;
    Plane plane = new Plane(new Vector3(0, 1, 0), 0);   // y쪽 방향으로 향하는 평면 생성 0은 원점으로부터의 거리
    NavMeshAgent agent;
    private void Start()
    {
        animator = GetComponentInChildren<Animator>();  // 스프라이트의 animator를 할당
        spriteTr = GetComponentInChildren<SpriteRenderer>().transform;  // ??
        normalSpeed = speed;
        spriteTrailRenderer = GetComponentInChildren<SpriteTrailRenderer.SpriteTrailRenderer>();
        spriteTrailRenderer.enabled = false;
        agent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        //RaycastHit hit;
        if(StageManager.instance.gameState != GameStateType.Playing)
        {
            return;
        }
        if (CanMoveState()) // 상태가 어택, TakeHit, Death면 실행시키지 못하고
        {   // 아니면 실행 시킬 수 있다.
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
            item.GetComponent<Monster>().TakeHit(power);
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
        State = StateType.Dash;
        speed = normalSpeed * dashSpeedMultiplySpeed;

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
            // 점프상태이면 함수를 나감
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
        NotInit,
        Idle,
        Walk,
        JumpUp,
        JumpDown,
        Attack,
        TakeHit,
        Death,
        Dash,
    }
    [SerializeField] StateType m_state = StateType.Idle;
    StateType State
    {
        get { return m_state; }
        set
        {
            if (m_state == value)
                return;
            m_state = value;

            animator.Play(m_state.ToString());
        }
    }
    Animator animator;
    JumpStateType jumpState;
    [BoxGroup("점프")] public float jumpYMultiply = 1;
    [BoxGroup("점프")] public float jumpTimeMultiply = 1; // 점프 가능한 시간
    private IEnumerator JumpCo()
    {
        jumpState = JumpStateType.Jump; // 점프 상태를 점프로 바꿈
        State = StateType.JumpUp; // 상태를 점프로 바꿈
        agent.enabled = false;  // agent가 바닥으로 잡고 있으니 잠시 해제를 해주자
        float jumpStartTime = Time.time;    // 점프가 시작한 현재 시간
        float jumpDuration = jumpYac[jumpYac.length - 1].time; // 0에서부터 인덱스간 애니메이션커브의 총 시간, length는 점의 갯수
        jumpDuration *= jumpTimeMultiply;
        float jumpEndTime = jumpStartTime + jumpDuration;
        float sumEvaluateTime = 0;
        float previousY = float.MinValue;
        while (Time.time < jumpEndTime)
        {
            float y = jumpYac.Evaluate(sumEvaluateTime / jumpTimeMultiply);
            Debug.Log(Time.deltaTime);
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
        if (plane.Raycast(ray, out float enter))    // 쏘는 빔이 평면과 평행을 이루면(접촉이 없다면) false , enter는 아마도 평면에서의 좌표값?
        {
            Vector3 hitPoint = ray.GetPoint(enter); // GetPoint로 enter값을 벡터값(마우스 포인터의 position)으로 변경
            //mousePointer.position = hitPoint;
            float distance = Vector3.Distance(hitPoint, transform.position);    // 마우스 포인터와 캐릭터의 거리

            float moveableDistance = stopDistance;  // 플레이어를 멈추게 하는 거리
            //State가 Walk일땐 7(stopDIstance)사용.
            // Idle에서 Walk로 갈땐 12(WalkDistance)사용.

            Vector3 dir = hitPoint - transform.position;    // 평소의 움직이는 방향
            if (State == StateType.Idle)
            {
                moveableDistance = walkDistance; // 플레이어를 움직일 수 있게 하는 거리
            }
            if (m_state == StateType.Dash)
            {
                dir = dashDirection;    // 대쉬 상태면 움직이는 방향을  dashDirection으로 바꿈
            }
            dir.Normalize();
            // 만약 마우스 포인터와 캐릭터의 거리가 움직일 수 있는 거리보다 크거나 상태가 대쉬 상태면 실행
            if (distance > moveableDistance || State == StateType.Dash)
            {
                transform.Translate(dir * speed * Time.deltaTime, Space.World);
                if (ChangeableState())  // 점프상태이거나 대쉬 상태가 아니면
                    State = StateType.Walk; // 워크 상태로 변환
            }
            else // 마우스 포인터와 캐릭터의 거리가 움직일 수 있는 거리보다 작고 상태가 대쉬가 아니면
            {
                if (ChangeableState())// Idle상태로 변할 수 있는 지 확인하고
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
                if (m_state == StateType.Dash)
                {
                    return false;
                }
                return true;
            }
        }
    }


}
