using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SHQZ
{
    public class Helper : MonoBehaviour
    {
        [Range(-1, 1)]
        public float vertical;
        [Range(-1, 1)]
        public float horizontal;
        Animator anim;
    
        public bool isPlayed;
        public bool twoHanded;
        public bool enableRootMotion;
        public bool useItem;
        public bool isInteracting;
        public bool lockOn;

        public string[] oh_attack;   
        public string[] th_attack;
    

        void Start()
        {
            anim = GetComponent<Animator>();
            anim.applyRootMotion = false;
        }

        void Update()
        {
            enableRootMotion = !anim.GetBool("CanMove");
            anim.applyRootMotion = enableRootMotion;

            isInteracting = anim.GetBool("IsInteracting");
            if (lockOn == false)
            {
                horizontal = 0;
                vertical = Mathf.Clamp01(vertical);
            }
            anim.SetBool("LockOn", lockOn);

            if (enableRootMotion)
                return;

            if (useItem)
            {
                //isPlayed = false;
                //twoHanded = false;
                anim.Play("UseItem");
                useItem = false;
            }
            if (isInteracting)
            {
                isPlayed = false;
                vertical = Mathf.Clamp(vertical, 0, 0.5f);
            }


            anim.SetBool("TwoHanded", twoHanded);
            if (isPlayed)
            {
                string targetAnim;
                if (twoHanded)
                {
                    int r = Random.Range(0, th_attack.Length);
                    targetAnim = th_attack[r];
                }
                else
                {
                    int r = Random.Range(0, oh_attack.Length-1);
                    targetAnim = oh_attack[r];
                    if (vertical > 0.5f)
                    {
                        targetAnim = "oh_attack_3";
                    }
                }
                vertical = 0;
                anim.CrossFade(targetAnim, 0.2f);
                //anim.SetBool("CanMove", false);
                //enableRootMotion = true;
                isPlayed = false;
            }
        
            anim.SetFloat("Vertical", vertical);
            anim.SetFloat("Horizontal", horizontal);
        }
    }
}
