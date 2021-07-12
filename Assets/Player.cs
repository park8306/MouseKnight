using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] StateType state = StateType.Idle;
    public float speed = 20;
    public float walkDistance = 12;
    public float stopDistance = 7;
    float normalSpeed;
    public Transform mousePointer;
    public Transform spriteTr;
    SpriteTrailRenderer.SpriteTrailRenderer spriteTrailRenderer;
    Plane plane = new Plane(new Vector3(0, 1, 0), 0);

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        spriteTr = GetComponentInChildren<SpriteRenderer>().transform;
        normalSpeed = speed;
        spriteTrailRenderer = GetComponentInChildren<SpriteTrailRenderer.SpriteTrailRenderer>();
        spriteTrailRenderer.enabled = false;
    }
    void Update()
    {
        //RaycastHit hit;
        Move();
        Jump();

        Dash();
    }
    #region

    public float dashCoolTime = 2f;
    float nextDashableTime;
    [Foldout("대시")] public float dashableDistance = 10;
    [Foldout("대시")] public float dashableTime = 0.4f;
    float mouseDownTime;
    Vector3 mouseDownPosition;
    private void Dash()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            mouseDownTime = Time.time;
            mouseDownPosition = Input.mousePosition;
        }
        if (nextDashableTime < Time.time)
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                bool isDashDrag = IsSuccessDashDrag();
                if (isDashDrag)
                {
                    nextDashableTime = Time.time + dashCoolTime;
                    StartCoroutine(DashCo());
                }
            }
        }
    }
    [Foldout("대시")] public float dashTime = 0.3f;
    [Foldout("대시")] public float dashSpeedMultiplySpeed = 4f;
    Vector3 dashDirection;
    private IEnumerator DashCo()
    {
        // dashDirection x방향만 사용.
        spriteTrailRenderer.enabled = true;
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
        float dragTime = Time.time - mouseDownTime;
        if (dragTime > dashableTime)
        {
            return false;
        }

        float dragDistance = Vector3.Distance(mouseDownPosition, Input.mousePosition);
        if (dragDistance < dashableDistance)
        {
            return false;
        }

        return true;
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
    public float jumpYMultiply = 1;
    public float jumpTimeMultiply = 1;
    private IEnumerator JumpCo()
    {
        jumpState = JumpStateType.Jump;
        State = StateType.JumpUp;
        float jumpStartTime = Time.time;
        float jumpDuration = jumpYac[jumpYac.length - 1].time;
        jumpDuration *= jumpTimeMultiply;
        float jumpEndTime = jumpStartTime + jumpDuration;
        float sumEvaluateTime = 0;
        float previousY = 0;
        while (Time.time < jumpEndTime)
        {
            float y = jumpYac.Evaluate(sumEvaluateTime / jumpTimeMultiply);
            y *= jumpYMultiply;
            transform.Translate(0, y, 0);
            yield return null;

            if (previousY > y)
            {
                State = StateType.JumpDown;
            }
            previousY = y;
            sumEvaluateTime += Time.deltaTime;
        }
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

            if (distance > moveableDistance)
            {
                var dir = hitPoint - transform.position;
                dir.Normalize();
                if (state == StateType.Dash)
                {
                    dir = dashDirection;
                }
                transform.Translate(dir * speed * Time.deltaTime, Space.World);

                // 방향(dir)에 따라서
                // 오른쪽이라면 Y : 0, spriteX : 45
                // 왼쪽이라면 Y : 180, spriteX : -45
                bool isRightSide = dir.x > 0;
                if (isRightSide)
                {
                    transform.rotation = Quaternion.Euler(Vector3.zero);
                    spriteTr.rotation = Quaternion.Euler(45, 0, 0);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    spriteTr.rotation = Quaternion.Euler(-45, -180, 0);
                }
                if (ChangeableState())
                    State = StateType.Walk;
            }
            else
            {
                if (ChangeableState())
                    State = StateType.Idle;
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
