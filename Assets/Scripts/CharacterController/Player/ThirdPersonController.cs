using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour
{
    Transform playerTransform;
    Transform cameraTransform;
    Animator animator;
    Collider[] colliders;

    CharacterController characterController;

    private CharacterStats characterStats;
    private ThirdPersonController playerController;

    public enum PlayerPosture
    {
        Crouch,
        Stand,
        Falling,
        Jumping,
        Landing,
        Transition,
        Dead
    };
    [HideInInspector]
    public PlayerPosture playerPosture = PlayerPosture.Stand;

    float crouchThreshold = -1f;
    float standThreshold = 0f;
    float midairThreshold = 1.2f;
    float landingThreshold = 1f;

    public enum LocomotionState
    {
        Idle,
        Walk,
        Run
    };
    [HideInInspector]
    public LocomotionState locomotionState = LocomotionState.Idle;

    //�����ж���ʽ�ٶ�
    float crouchSpeed = 1.5f;
    float walkSpeed = 2.5f;
    float runSpeed = 5.5f;

    public enum ArmState
    {
        Normal,
        Aim         //�ֶ�״̬
    }
    [HideInInspector]
    public ArmState armState = ArmState.Normal;

    //����״̬
    public enum FightState 
    {
        Normal,
        Attack1,
        Attack2
    }
    [HideInInspector]
    public FightState fightState = FightState.Normal;

    

    public float gravity = -9.8f;
    public float maxHeight = 1.5f;

    //��ֱ�����ٶ�
    float VerticalVelocity;

    //����ʱ���ٶȵı���
    float fallMultiplier = 1.5f;

    float feetTween;

    //Vector3 lastVelOnGround;
    Vector3 averageVel = Vector3.zero;

    static readonly int CACHE_SIZE = 3;
    Vector3[] velCache = new Vector3[CACHE_SIZE];
    int currentCacheIndex = 0;

    Vector2 moveInput;
    Vector3 playerMovement = Vector3.zero;

    #region �ŵؼ��
    //��ɫ�Ƿ��ŵ�
    bool isGrounded;

    //�ر������ߵ�ƫ����
    float groundCheckOffset = 0.5f;
    #endregion

    #region ������
    //��ɫ�Ƿ���Ե���
    bool couldFall;

    //��С����߶ȣ�С�ڴ˸߶�ʱ�����л�������״̬
    float fallHeight = 0.5f;
    #endregion

    #region ��ԾCD���
    //��ɫ�Ƿ�����ԾCD״̬
    bool isLanding;

    //��ԾCD
    float jumpCD = 0.15f;
    #endregion

    #region ���벼��ֵ
    bool isRunning;
    bool isCrouch;
    bool isAiming;
    bool isJumping;
    //��������ֵ
    bool isAttacking;
    bool isDead;
    [HideInInspector]
    public bool isTransition;
    public bool isSave;
    public bool isLoad;
    public bool isEscape;
    #endregion

    #region ״̬����ϣֵ
    int postureHash;
    int moveSpeedHash;
    int turnSpeedHash;
    int verticalVelocityHash;
    int feetTweenHash;
    //����״̬��ϣֵ
    int attack1Hash;
    int attack2Hash;
    int deadHash;
    #endregion

    

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = transform;
        cameraTransform = Camera.main.transform;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        characterStats = GetComponent<CharacterStats>();
        playerController = GetComponent<ThirdPersonController>();
        colliders = GetComponentsInChildren<Collider>();

        postureHash = Animator.StringToHash("Posture");
        moveSpeedHash = Animator.StringToHash("Move Speed");
        turnSpeedHash = Animator.StringToHash("Turn Speed");
        verticalVelocityHash = Animator.StringToHash("Vertical Velocity");
        feetTweenHash = Animator.StringToHash("FeetTween");
        //��ȡ����״̬
        attack1Hash = Animator.StringToHash("Attack1");
        attack2Hash = Animator.StringToHash("Attack2");
        deadHash = Animator.StringToHash("Dead");

        GameManager.Instance.RigisterPlayer(characterStats);
        GameManager.Instance.RigisterPlayerController(playerController);
        SaveManager.Instance.LoadPlayerData();

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerDead();
        DeadNotify();
        CheckGround();
        SwitchPlayerStates();
        CaculateGravity();
        Jump();
        CaculateInputDirection();
        SetupAnimator();
        //Debug.Log(moveInput+","+playerMovement);
    }

    #region �������
    public void GetMoveInput(InputAction.CallbackContext ctx) 
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    public void GetRunInput(InputAction.CallbackContext ctx) 
    {
        isRunning = ctx.ReadValueAsButton();
    }
    public void GetCrouchInput(InputAction.CallbackContext ctx) 
    {
        isCrouch = ctx.ReadValueAsButton();
    }
    public void GetAimInput(InputAction.CallbackContext ctx) 
    {
        isAiming = ctx.ReadValueAsButton();
    }
    public void GetJumpInput(InputAction.CallbackContext ctx) 
    {
        isJumping = ctx.ReadValueAsButton();
    }
    //����������ȡ
    public void GetAttackInput(InputAction.CallbackContext ctx) 
    {
        isAttacking = ctx.ReadValueAsButton();
    }
    //���Ͳ�������
    public void GetTransitionInput(InputAction.CallbackContext ctx)
    {
        isTransition = ctx.ReadValueAsButton();
    }
    public void GetSaveInput(InputAction.CallbackContext ctx)
    {
        isSave = ctx.ReadValueAsButton();
    }
    public void GetLoadInput(InputAction.CallbackContext ctx)
    {
        isLoad = ctx.ReadValueAsButton();
    }
    public void GetEscapeInput(InputAction.CallbackContext ctx)
    {
        isEscape = ctx.ReadValueAsButton();
    }
    #endregion

    void SwitchPlayerStates() 
    {
        if (isDead)
        {
            playerPosture = PlayerPosture.Dead;
            playerMovement = Vector3.zero;
            foreach(Collider col in colliders)
            {
                col.enabled = false;
            }
            return;
        }
        else
        {
            if (!isGrounded)
            {
                if (VerticalVelocity > 0)
                {
                    playerPosture = PlayerPosture.Jumping;

                    //isJumping = !isJumping;     ��bug����ʱ���ֻ��һ�Σ���ʱ������ܶ��
                }
                else if (playerPosture != PlayerPosture.Jumping)
                {
                    if (couldFall)
                    {
                        playerPosture = PlayerPosture.Falling;
                    }
                }
            }
            else if (playerPosture == PlayerPosture.Jumping)
            {
                StartCoroutine(CoolDownJump());
            }
            else if (isLanding)
            {
                playerPosture = PlayerPosture.Landing;
            }
            else if (isCrouch)
            {
                playerPosture = PlayerPosture.Crouch;
            }
            else
            {
                playerPosture = PlayerPosture.Stand;
            }

            if (moveInput.magnitude == 0)
            {
                locomotionState = LocomotionState.Idle;
            }
            else if (!isRunning)
            {
                locomotionState = LocomotionState.Walk;
            }
            else
            {
                locomotionState = LocomotionState.Run;
            }

            if (isAiming)
            {
                armState = ArmState.Aim;
            }
            else
            {
                armState = ArmState.Normal;
            }

            //���������ж�
            if (isAttacking)
            {
                fightState = FightState.Attack1;
                isAttacking = !isAttacking;
                //����������Ա�֤����һ�ΰ�������ֻ����һ�Σ���ͬʱ״̬��Ҳ��ص�Ĭ��״̬���޷�������һ����������
            }
            //if (fightState == FightState.Attack1)
            //{
                //if (isAttacking)
                //{
                    //fightState = FightState.Attack2;
                //}
            //}
            else
            {
                fightState = FightState.Normal;
            }
        }
    }

    //������
    void CheckGround() 
    {
        if (Physics.SphereCast(playerTransform.position + Vector3.up * groundCheckOffset, characterController.radius, Vector3.down, out RaycastHit hit, groundCheckOffset - characterController.radius + 2 * characterController.skinWidth))
        {
            isGrounded = true;
        }
        else 
        {
            isGrounded = false;
            couldFall = !Physics.Raycast(playerTransform.position, Vector3.down, fallHeight);
        }
    }

    //��Ծ��ȴ
    IEnumerator CoolDownJump() 
    {
        landingThreshold = Mathf.Clamp(VerticalVelocity, -10, 0);
        landingThreshold /= 20f;
        landingThreshold += 1f;
        isLanding = true;
        playerPosture = PlayerPosture.Landing;
        yield return new WaitForSeconds(jumpCD);
        isLanding = false;
    }
    
    //ʩ������
    void CaculateGravity() 
    {
        if (playerPosture!=PlayerPosture.Jumping && playerPosture!=PlayerPosture.Falling)
        {
            if (!isGrounded)
            {
                VerticalVelocity += gravity * fallMultiplier * Time.deltaTime;
            }
            else 
            {
                VerticalVelocity = gravity * Time.deltaTime;
            }
        }
        else 
        {
            if (VerticalVelocity <= 0 || !isJumping)
            {
                VerticalVelocity += gravity * fallMultiplier * Time.deltaTime;
            }
            else 
            {
                VerticalVelocity += gravity * Time.deltaTime;
            }
        }
    }

    //��Ծ
    void Jump() 
    {
        if (playerPosture==PlayerPosture.Stand && isJumping) 
        {
            VerticalVelocity = Mathf.Sqrt(-2 * gravity * maxHeight);
            feetTween = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1);
            feetTween = feetTween < 0.5f ? 1 : -1;
            if (locomotionState == LocomotionState.Run)
            {
                feetTween *= 3;
            }
            else if (locomotionState == LocomotionState.Walk) 
            {
                feetTween *= 2;
            }
            else 
            {
                feetTween = Random.Range(0.5f, 1f) * feetTween;
            }
        }
    }
    
    void CaculateInputDirection() 
    {
        Vector3 camForwardProjection = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        playerMovement = camForwardProjection * moveInput.y + cameraTransform.right * moveInput.x;
        playerMovement = playerTransform.InverseTransformVector(playerMovement);
    }

    void SetupAnimator() 
    {
        if (playerPosture == PlayerPosture.Stand)
        {
            animator.SetFloat(postureHash, standThreshold, 0.1f, Time.deltaTime);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    animator.SetFloat(moveSpeedHash, 0, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Walk:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * walkSpeed, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Run:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * runSpeed, 0.1f, Time.deltaTime);
                    break;
            }
        }
        else if (playerPosture == PlayerPosture.Crouch)
        {
            animator.SetFloat(postureHash, crouchThreshold, 0.1f, Time.deltaTime);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    animator.SetFloat(moveSpeedHash, 0, 0.1f, Time.deltaTime);
                    break;
                default:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * crouchSpeed, 0.1f, Time.deltaTime);
                    break;
            }
        }
        else if (playerPosture == PlayerPosture.Jumping)
        {
            animator.SetFloat(postureHash, midairThreshold);
            animator.SetFloat(verticalVelocityHash, VerticalVelocity);
            animator.SetFloat(feetTweenHash, feetTween);
        }
        else if (playerPosture == PlayerPosture.Landing)
        {
            animator.SetFloat(postureHash, landingThreshold, 0.03f, Time.deltaTime);
            switch (locomotionState)
            {
                case LocomotionState.Idle:
                    animator.SetFloat(moveSpeedHash, 0, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Walk:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * walkSpeed, 0.1f, Time.deltaTime);
                    break;
                case LocomotionState.Run:
                    animator.SetFloat(moveSpeedHash, playerMovement.magnitude * runSpeed, 0.1f, Time.deltaTime);
                    break;
            }
        }
        else if (playerPosture == PlayerPosture.Falling) 
        {
            animator.SetFloat(postureHash, midairThreshold);
            animator.SetFloat(verticalVelocityHash, VerticalVelocity);
        }
        else if (playerPosture == PlayerPosture.Dead)
        {
            animator.SetBool(deadHash, isDead);
        }

        if (armState == ArmState.Normal && playerPosture != PlayerPosture.Dead)
        {
            float rad = Mathf.Atan2(playerMovement.x, playerMovement.z);
            animator.SetFloat(turnSpeedHash, rad, 0.1f, Time.deltaTime);
            playerTransform.Rotate(0, rad * 200 * Time.deltaTime, 0);      //���ڿ���ת��
        }

        //���빥��״̬
        if (playerPosture == PlayerPosture.Stand && fightState == FightState.Attack1) 
        {
            animator.SetTrigger(attack1Hash);
            Debug.Log("attack");

            //animator.SetTrigger(attackHash);

            //attackCD -= 10f * Time.deltaTime;
            //attackCD = Mathf.Clamp(attackCD, 0, 1f);
            //if (attackCD == 0) 
            //{
                //animator.SetBool(attackHash, false);
            //}
        }
        //if (playerPosture == PlayerPosture.Stand && fightState == FightState.Attack2)
        //{
            //animator.SetTrigger(attack2Hash);
        //}
        else 
        {
            //animator.SetBool(attackHash, false);
            //animator.SetTrigger("Normal");
        }
    }

    void PlayerDead()
    {
        if (characterStats.CurrentHealth == 0)
        {
            isDead = true;
        }
    }

    void DeadNotify()
    {
        if (isDead)
        {
            GameManager.Instance.NotifyObservers();
        }
    }
    Vector3 AverageVel(Vector3 newVel) 
    {
        velCache[currentCacheIndex] = newVel;
        currentCacheIndex++;
        currentCacheIndex %= CACHE_SIZE;
        Vector3 average = Vector3.zero;
        foreach (Vector3 vel in velCache) 
        {
            average += vel;
        }
        return average / CACHE_SIZE;
    }

    private void OnAnimatorMove()
    {
        if (playerPosture != PlayerPosture.Jumping && playerPosture != PlayerPosture.Falling && playerPosture != PlayerPosture.Dead)
        {
            Vector3 playerDeltaMovement = animator.deltaPosition;
            playerDeltaMovement.y = VerticalVelocity * Time.deltaTime;
            characterController.Move(playerDeltaMovement);
            averageVel = AverageVel(animator.velocity);
        }
        if (playerPosture == PlayerPosture.Dead)
        {
            characterController.enabled = false;
        }
        else
        {
            averageVel.y = VerticalVelocity;
            Vector3 playerDeltaMovement = averageVel * Time.deltaTime;
            playerDeltaMovement.y = VerticalVelocity * Time.deltaTime;
            characterController.Move(playerDeltaMovement);
        }
    }
}
