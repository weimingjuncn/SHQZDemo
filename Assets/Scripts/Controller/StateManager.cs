﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHQZ
{
    public class StateManager : MonoBehaviour
    {
        [Header("Init")]
        public GameObject activeModel;

        [Header("Stats")]
        public Attributes attributes;
        public CharacterStats characterStats;

        [Header("Inputs")]
        public float vertical;
        public float horizontal;
        public float moveAmount;
        public Vector3 moveDir;
        public bool rt, rb, lt, lb;
        public bool rollInput;
        public bool itemInput;

        [Header("Stats")]
        public float moveSpeed = 2;
        public float runSpeed = 3.5f;
        public float rotateSpeed = 5;
        public float toGround = 0.5f;
        public float rollSpeed = 1;
        public float parryOffset = 1.4f;
        public float backStabOffset = 1.4f;

        [Header("States")]
        public bool onGround;
        public bool run;
        public bool lockOn;
        public bool inAction;
        public bool canMove;
        public bool isSpellcasting;
        public bool enableIK;
        public bool isTwoHanded;
        public bool usingItem;
        public bool canBeParried;
        public bool parryIsOn;
        public bool isBlocking;
        public bool isLeftHand;


        [Header("Other")]
        public EnemyTarget lockOnTarget;
        public Transform lockOnTransform;
        public AnimationCurve roll_curve;
        //public EnemyStates parryTarget;

        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public Rigidbody rigid;
        [HideInInspector]
        public AnimatorHook a_hook;
        [HideInInspector]
        public ActionManager actionManager;
        [HideInInspector]
        public InventoryManager inventoryManager;
        //[HideInInspector]
        //public BoneHelper boneHelper;

        [HideInInspector]
        public float delta;
        [HideInInspector]
        public LayerMask ignoreLayers;

        [HideInInspector]
        public Action currentAction;

        float _actionDelay;

        public void Init()
        {
            SetupAnimator();
            rigid = GetComponent<Rigidbody>();
            rigid.angularDrag = 999;
            rigid.drag = 4;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            //boneHelper = gameObject.AddComponent<BoneHelper>();

            inventoryManager = GetComponent<InventoryManager>();
            inventoryManager.Init(this);

            actionManager = GetComponent<ActionManager>();
            actionManager.Init(this);

            a_hook = activeModel.GetComponent<AnimatorHook>();
            if (a_hook == null)
                a_hook = activeModel.AddComponent<AnimatorHook>();
            a_hook.Init(this, null);

            gameObject.layer = 8;
            ignoreLayers = ~(1 << 9);

            anim.SetBool(StaticStrings.onGround, true);
        }

        void SetupAnimator()
        {
            if (activeModel == null)
            {
                anim = GetComponentInChildren<Animator>();
                if (anim == null)
                {
                    Debug.Log("No model found");
                }
                else
                {
                    activeModel = anim.gameObject;
                }
            }

            if (anim == null)
                anim = activeModel.GetComponent<Animator>();

            anim.applyRootMotion = false;
        }

        public void FixedTick(float d)
        {
            delta = d;

            isBlocking = false;
            usingItem = anim.GetBool(StaticStrings.interacting);
            anim.SetBool(StaticStrings.spellcasting, isSpellcasting);
            DetectAction();
            DetectItemAction();
            inventoryManager.rightHandWeapon.weaponModel.SetActive(!usingItem);

            anim.SetBool(StaticStrings.blocking, isBlocking);
            anim.SetBool(StaticStrings.isLeft, isLeftHand);

            if (isBlocking == false && isSpellcasting == false)
            {
                enableIK = false;
            }

            a_hook.useIk = enableIK;

            if (inAction)
            {
                anim.applyRootMotion = true;

                _actionDelay += delta;
                if (_actionDelay > 0.3f)
                {
                    inAction = false;
                    _actionDelay = 0;
                }
                else
                {
                    return;
                }
            }

            canMove = anim.GetBool(StaticStrings.canMove);

            if (!canMove)
                return;


            anim.applyRootMotion = false;
            rigid.drag = (moveAmount > 0 || onGround == false) ? 0 : 4;

            float targetSpeed = moveSpeed;
            if (usingItem || isSpellcasting)
            {
                run = false;
                moveAmount = Mathf.Clamp(moveAmount, 0, 0.45f);
            }

            if (run)
                targetSpeed = runSpeed;

            if (onGround)
                rigid.velocity = moveDir * (targetSpeed * moveAmount);

            if (run)
                lockOn = false;

            HandleRotation();

            anim.SetBool(StaticStrings.lockon, lockOn);

            if (lockOn == false)
                HandleMovementAnimations();
            else
                HandleLockOnAnimations(moveDir);

            if (isSpellcasting)
            {
                HandleSpellcasting();
                return;
            }

            a_hook.CloseRoll();
            HandleRolls();
        }

        void HandleRotation()
        {
            Vector3 targetDir = (lockOn == false) ?
                moveDir
                :
                (lockOnTransform != null) ?
                    lockOnTransform.transform.position - transform.position
                    :
                    moveDir;

            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * rotateSpeed);
            transform.rotation = targetRotation;
        }

        public void DetectItemAction()
        {
            if (canMove == false || usingItem || isBlocking)
                return;

            if (itemInput == false)
                return;

            ItemAction slot = actionManager.consumableItem;
            string targetAnim = slot.targetAnim;
            if (string.IsNullOrEmpty(targetAnim))
                return;

            // inventoryManager.curWeapon.weaponModel.SetActive(false);
            usingItem = true;
            anim.Play(targetAnim);
        }

        public void DetectAction()
        {
            if (canMove == false || usingItem || isSpellcasting)
                return;

            if (rb == false && rt == false && lt == false && lb == false)
                return;

            Action slot = actionManager.GetActionSlot(this);
            if (slot == null)
                return;

            switch (slot.type)
            {
                case ActionType.attack:
                    AttackAction(slot);
                    break;
                case ActionType.block:
                    BlockAction(slot);
                    break;
                case ActionType.spells:
                    SpellAction(slot);
                    break;
                case ActionType.parry:
                    ParryAction(slot);
                    break;
                default:
                    break;
            }

        }

        void AttackAction(Action slot)
        {
            if (CheckForParry(slot))
                return;

            if (CheckForBackstab(slot))
                return;

            string targetAnim = null;
            targetAnim = slot.targetAnim;

            if (string.IsNullOrEmpty(targetAnim))
                return;

            currentAction = slot;

            canMove = false;
            inAction = true;

            float targetSpeed = 1;
            if (slot.changeSpeed)
            {
                targetSpeed = slot.animSpeed;
                if (targetSpeed == 0)
                    targetSpeed = 1;
            }

            canBeParried = slot.canBeParried;
            anim.SetFloat(StaticStrings.animSpeed, targetSpeed);
            anim.SetBool(StaticStrings.mirror, slot.mirror);
            anim.CrossFade(targetAnim, 0.2f);

        }

        void SpellAction(Action slot)
        {
            if (slot.spellClass != inventoryManager.currentSpell.instance.spellClass)
            {
                //targetAnim = cant cast spell
                Debug.Log("spell class doesn't match");
                //anim.CrossFade(targetAnim, 0.2f);
                return;
            }

            ActionInput inp = actionManager.GetActionInput(this);
            if (inp == ActionInput.lb)
                inp = ActionInput.rb;
            if (inp == ActionInput.lt)
                inp = ActionInput.rt;

            Spell s_inst = inventoryManager.currentSpell.instance;
            SpellAction s_slot = s_inst.GetAction(s_inst.actions, inp);
            if (s_slot == null)
            {
                Debug.Log("Cant find spell slot");
                return;
            }

            SpellEffectsManager.singleton.UseSpellEffect(s_inst.spell_effect, this);

            isSpellcasting = true;
            spellcastTime = 0;
            max_spellCastTime = s_slot.castTime;
            spellTargetAnim = s_slot.throwAnim;
            spellIsMirrored = slot.mirror;
            curSpellType = s_inst.spellType;

            string targetAnim = s_slot.targetAnim;
            if (spellIsMirrored)
                targetAnim += StaticStrings._l;
            else
                targetAnim += StaticStrings._r;

            projectileCanidate = inventoryManager.currentSpell.instance.projectile;
            inventoryManager.CreateSpellParticle(inventoryManager.currentSpell, spellIsMirrored,
                (s_inst.spellType == SpellType.looping));
            anim.SetBool(StaticStrings.spellcasting, true);
            anim.SetBool(StaticStrings.mirror, slot.mirror);
            anim.CrossFade(targetAnim, 0.2f);

            a_hook.InitIKForBreathSpell(spellIsMirrored);

            if (spellCast_start != null)
                spellCast_start();
        }

        float spellcastTime;
        float max_spellCastTime;
        string spellTargetAnim;
        bool spellIsMirrored;
        SpellType curSpellType;
        GameObject projectileCanidate;

        public delegate void SpellCast_Start();
        public delegate void SpellCast_Loop();
        public delegate void SpellCast_Stop();
        public SpellCast_Start spellCast_start;
        public SpellCast_Loop spellCast_loop;
        public SpellCast_Stop spellCast_stop;

        void HandleSpellcasting()
        {
            if (curSpellType == SpellType.looping)
            {
                enableIK = true;
                a_hook.currentHand = (spellIsMirrored) ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;

                if (spellCast_loop != null)
                    spellCast_loop();

                if (rb == false && lb == false)
                {
                    isSpellcasting = false;

                    enableIK = false;

                    if (spellCast_stop != null)
                        spellCast_stop();
                }

                return;
            }

            spellcastTime += delta;

            if (inventoryManager.currentSpell.currentParticle != null)
                inventoryManager.currentSpell.currentParticle.SetActive(true);

            if (spellcastTime > max_spellCastTime)
            {
                canMove = false;
                inAction = true;
                isSpellcasting = false;

                string targetAnim = spellTargetAnim;
                anim.SetBool(StaticStrings.mirror, spellIsMirrored);
                anim.CrossFade(targetAnim, 0.2f);
            }
        }

        public void ThrowProjectile()
        {
            if (projectileCanidate == null)
                return;

            GameObject go = Instantiate(projectileCanidate) as GameObject;
            Transform p = anim.GetBoneTransform((spellIsMirrored) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            go.transform.position = p.position;

            if (lockOnTransform && lockOn)
                go.transform.LookAt(lockOnTransform.position);
            else
                go.transform.rotation = transform.rotation;

            Projectile proj = go.GetComponent<Projectile>();
            proj.Init();

        }

        bool CheckForParry(Action slot)
        {
            if (slot.canParry == false)
                return false;

            EnemyStates parryTarget = null;
            Vector3 origin = transform.position;
            origin.y += 1;
            Vector3 rayDir = transform.forward;
            RaycastHit hit;
            if (Physics.Raycast(origin, rayDir, out hit, 3, ignoreLayers))
            {
                parryTarget = hit.transform.GetComponentInParent<EnemyStates>();
            }

            if (parryTarget == null)
                return false;

            if (parryTarget.parriedBy == null)
                return false;

            /*      float dis = Vector3.Distance(parryTarget.transform.position, transform.position);

                  if (dis > 3)
                      return false;*/

            Vector3 dir = parryTarget.transform.position - transform.position;
            dir.Normalize();
            dir.y = 0;
            float angle = Vector3.Angle(transform.forward, dir);

            if (angle < 60)
            {
                Vector3 targetPosition = -dir * parryOffset;
                targetPosition += parryTarget.transform.position;
                transform.position = targetPosition;

                if (dir == Vector3.zero)
                    dir = -parryTarget.transform.forward;

                Quaternion eRotation = Quaternion.LookRotation(-dir);
                Quaternion ourRot = Quaternion.LookRotation(dir);

                parryTarget.transform.rotation = eRotation;
                transform.rotation = ourRot;

                parryTarget.IsGettingParried(slot);

                canMove = false;
                inAction = true;
                anim.SetBool(StaticStrings.mirror, slot.mirror);
                anim.CrossFade(StaticStrings.parry_attack, 0.2f);
                lockOnTarget = null;
                return true;
            }

            return false;
        }

        bool CheckForBackstab(Action slot)
        {
            if (slot.canBackStab == false)
                return false;

            EnemyStates backstab = null;
            Vector3 origin = transform.position;
            origin.y += 1;
            Vector3 rayDir = transform.forward;
            RaycastHit hit;
            if (Physics.Raycast(origin, rayDir, out hit, 1, ignoreLayers))
            {
                backstab = hit.transform.GetComponentInParent<EnemyStates>();
            }

            if (backstab == null)
                return false;

            Vector3 dir = transform.position - backstab.transform.position;
            dir.Normalize();
            dir.y = 0;
            float angle = Vector3.Angle(backstab.transform.forward, dir);

            if (angle > 150)
            {

                Vector3 targetPosition = dir * backStabOffset;
                targetPosition += backstab.transform.position;
                transform.position = targetPosition;

                backstab.transform.rotation = transform.rotation;
                backstab.IsGettingBackstabbed(slot);
                canMove = false;
                inAction = true;
                anim.SetBool(StaticStrings.mirror, slot.mirror);
                anim.CrossFade(StaticStrings.parry_attack, 0.2f);
                lockOnTarget = null;
                return true;
            }

            return false;
        }

        void BlockAction(Action slot)
        {
            isBlocking = true;
            enableIK = true;
            isLeftHand = slot.mirror;
            a_hook.currentHand = (slot.mirror) ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;
            a_hook.InitIKForShield((slot.mirror));
        }

        void ParryAction(Action slot)
        {
            string targetAnim = null;
            targetAnim = slot.targetAnim;

            if (string.IsNullOrEmpty(targetAnim))
                return;

            float targetSpeed = 1;
            if (slot.changeSpeed)
            {
                targetSpeed = slot.animSpeed;
                if (targetSpeed == 0)
                    targetSpeed = 1;
            }

            anim.SetFloat(StaticStrings.animSpeed, targetSpeed);
            canBeParried = slot.canBeParried;
            canMove = false;
            inAction = true;
            anim.SetBool(StaticStrings.mirror, slot.mirror);
            anim.CrossFade(targetAnim, 0.2f);
        }

        public void Tick(float d)
        {
            delta = d;
            onGround = OnGround();
            anim.SetBool(StaticStrings.onGround, onGround);
        }

        void HandleRolls()
        {
            if (!rollInput || usingItem)
                return;

            float v = vertical;
            float h = horizontal;
            v = (moveAmount > 0.3f) ? 1 : 0;
            h = 0;

            /*     if(lockOn == false)
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
                a_hook.InitForRoll();
                a_hook.rm_multi = rollSpeed;
            }
            else
            {
                a_hook.rm_multi = 1.3f;
            }


            anim.SetFloat(StaticStrings.vertical, v);
            anim.SetFloat(StaticStrings.horizontal, h);

            canMove = false;
            inAction = true;
            anim.CrossFade(StaticStrings.Rolls, 0.2f);
            isBlocking = false;
        }

        void HandleMovementAnimations()
        {
            anim.SetBool(StaticStrings.run, run);
            anim.SetFloat(StaticStrings.vertical, moveAmount, 0.4f, delta);
        }

        void HandleLockOnAnimations(Vector3 moveDir)
        {
            Vector3 relativeDir = transform.InverseTransformDirection(moveDir);
            float h = relativeDir.x;
            float v = relativeDir.z;

            anim.SetFloat(StaticStrings.vertical, v, 0.2f, delta);
            anim.SetFloat(StaticStrings.horizontal, h, 0.2f, delta);
        }

        public bool OnGround()
        {
            bool r = false;

            Vector3 origin = transform.position + (Vector3.up * toGround);
            Vector3 dir = -Vector3.up;
            float dis = toGround + 0.3f;
            RaycastHit hit;
            Debug.DrawRay(origin, dir * dis);
            if (Physics.Raycast(origin, dir, out hit, dis, ignoreLayers))
            {
                r = true;
                Vector3 targetPosition = hit.point;
                transform.position = targetPosition;
            }

            return r;
        }

        public void HandleTwoHanded()
        {
            bool isRight = true;
            Weapon w = inventoryManager.rightHandWeapon.instance;
            if (w == null)
            {
                w = inventoryManager.leftHandWeapon.instance;
                isRight = false;
            }

            if (w == null)
            {
                return;
            }

            if (isTwoHanded)
            {
                anim.CrossFade(w.th_idle, 0.2f);
                actionManager.UpdateActionsTwoHanded();

                if (isRight)
                {
                    if (inventoryManager.leftHandWeapon != null)
                        inventoryManager.leftHandWeapon.weaponModel.SetActive(false);
                }
                else
                {
                    if (inventoryManager.rightHandWeapon != null)
                        inventoryManager.rightHandWeapon.weaponModel.SetActive(false);
                }
            }
            else
            {
                string targetAnim = w.oh_idle;
                targetAnim += (isRight) ? StaticStrings._r : StaticStrings._l;
                //    anim.CrossFade(targetAnim,0.2f);
                anim.Play(StaticStrings.equipWeapon_oh);
                actionManager.UpdateActionsOneHanded();

                if (isRight)
                {
                    if (inventoryManager.leftHandWeapon != null)
                        inventoryManager.leftHandWeapon.weaponModel.SetActive(true);
                }
                else
                {
                    if (inventoryManager.rightHandWeapon != null)
                        inventoryManager.rightHandWeapon.weaponModel.SetActive(true);
                }
            }
        }

        public void IsGettingParried()
        {

        }

        public void AddHealth()
        {
            characterStats.fp++;
        }
    }
}
