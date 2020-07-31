using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SHQZ
{
    public class StateManager : MonoBehaviour
    {
        [Header("Init")]
        public GameObject activeModel;

        [Header("Inputs")]
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public Vector3 moveDir;
        public bool rt, rb, lt, lb;
        public bool rollInput;

        [Header("Stats")]
        public float moveSpeed = 4.5f;
        public float runSpeed = 10.5f;
        public float rotateSpeed = 20;
        public float toGround = 0.5f;
        public float rollSpeed = 15;

        [Header("States")]
        public bool run;
        public bool onGround;
        public bool lockOn;
        public bool inAction;
        public bool canMove;
        public bool isTwoHanded;

        [Header("Other")]
        public EnemyTarget lockOnTarget;
        public AnimationCurve roll_curve;

        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        new public Rigidbody rigidbody;
        [HideInInspector]
        public AnimatorHook a_hook;

        [HideInInspector]
        public float delta;
        [HideInInspector]
        public LayerMask ignoreLayers;

        float _actionDelay;

        public void Init()
        {
            SetUpAnimator();
            anim.SetBool("OnGround", true);
            anim.SetBool("CanMove", true);
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.angularDrag = 999;
            rigidbody.drag = 4;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            a_hook = activeModel.AddComponent<AnimatorHook>();
            a_hook.Init(this);
            gameObject.layer = 8;
            ignoreLayers = ~(1 << 9);       
        }

        void SetUpAnimator()
        {

            if (activeModel == null)
            {
                anim = GetComponentInChildren<Animator>();
                if (anim == null)
                {
                    Debug.Log("No Active Model Found !");
                }
                else
                {
                    activeModel = anim.gameObject;
                }
            }
            if (anim == null)
                anim = activeModel.GetComponent<Animator>();
        }

        public void FixedTick(float d)
        {
            delta = d;

            DetectAction();

            if (inAction)
            {
                anim.applyRootMotion = true;
                _actionDelay += delta;
                if(_actionDelay > 0.3f)
                {
                    inAction = false;
                    _actionDelay = 0;
                }
                else
                {
                    return;
                }
            }
                
            canMove = anim.GetBool("CanMove");

            if (!canMove)
                return;

            a_hook.CloseRoll();
            HandleRolling();

            anim.applyRootMotion = false;
            rigidbody.drag = (moveAmount > 0||onGround == false) ? 0 : 4;

            float targetSpeed = moveSpeed;
  
            if (run)
            {
                targetSpeed = runSpeed;
                lockOn = false;
            }
            
            if(onGround)
                rigidbody.velocity = moveDir * (targetSpeed * moveAmount);

            Vector3 targetDir = (lockOn == false)? moveDir
                :(lockOnTarget.transform.position - transform.position);

            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            Quaternion tr = Quaternion.Slerp(transform.rotation, targetRotation, delta * moveAmount * rotateSpeed);
            transform.rotation = tr;
            Debug.DrawRay(transform.position, moveDir * 5, Color.yellow);
            anim.SetBool("LockOn", lockOn);

            if (lockOn == false)
                HandleMovementAnimations();
            else
                HandleLockOnAnimations(moveDir);
        }
        public void DetectAction()
        {
            if (canMove == false)
                return;
            if (rb == false && rt == false && lt == false && lb == false)
                return;
            string targetAnim = null;
            if (rb && isTwoHanded == false)
                targetAnim = "oh_attack_1";
            if (rt && isTwoHanded == false)
                targetAnim = "oh_attack_2";
            if (lt && isTwoHanded == false)
                targetAnim = "oh_attack_3";
            if (rb && isTwoHanded)
                targetAnim = "th_attack_1";

            if (string.IsNullOrEmpty(targetAnim))
                return;
            canMove = false;
            inAction = true;
            anim.CrossFade(targetAnim, 0.2f);
            
        }
        public void Tick(float d)
        {
            delta = d;
            onGround = OnGround();
            anim.SetBool("OnGround", onGround);
        }
        void HandleRolling()
        {
            if (!rollInput)
                return;

            float v = vertical;
            float h = horizontal;

            v = (moveAmount > 0.3f) ? 1 : 0;
            h = 0;

            /*if (lockOn == false)
            {
                v = (moveAmount > 0.3f)? 1 : 0;
                h = 0;
            }
            else
            {
                if (Mathf.Abs(v) < 0.3f)
                    v = 0;
                if (Mathf.Abs(h) < 0.3f)
                    h = 0;
            }*/
            if (v != 0)
            {
                if (moveDir == Vector3.zero)
                    moveDir = transform.forward;

                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = targetRot;
                Debug.Log(transform.rotation.eulerAngles);
                a_hook.InitForRoll();
                a_hook.rootMotionMultiplier = rollSpeed;
            }
            else
            {
                a_hook.rootMotionMultiplier = 1.3f;
            }
            

            anim.SetFloat("Vertical", v);
            anim.SetFloat("Horizontal", h);

            canMove = false;
            inAction = true;
            anim.CrossFade("Rolling", 0.2f);
        }
        void HandleMovementAnimations()
        {
            anim.SetBool("Run", run);
            anim.SetFloat("Vertical", moveAmount, 0.4f, delta);
        }
        void HandleLockOnAnimations(Vector3 moveDir)
        {
            Vector3 relativeDir = transform.InverseTransformDirection(moveDir);
            float h = relativeDir.x;
            float v = relativeDir.z;

            anim.SetFloat("Vertical", v, 0.2f, delta);
            anim.SetFloat("Horizontal", h, 0.2f, delta);
        }
        public bool OnGround()
        {
            bool r = false;
            Vector3 origin = transform.position + (Vector3.up * toGround);
            Vector3 dir = -Vector3.up;
            float dis = toGround + 0.3f;
            RaycastHit hit;
            Debug.DrawRay(origin, dir * dis, Color.red);
            if (Physics.Raycast(origin, dir, out hit, dis))
            {
                r = true;
                Vector3 targetPos = hit.point;
                transform.position = targetPos;
            }
            return r;
        }
        public void HandleTwoHanded()
        {
            anim.SetBool("TwoHanded", isTwoHanded);
        }
    }
}