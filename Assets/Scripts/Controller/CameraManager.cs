using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SHQZ
{
    public class CameraManager : MonoBehaviour
    {
        public bool lockOn;
        public float mouseSpeed = 2;
        public float controllerSpeed = 7;
        public float followSpeed = 9;

        float turnSmoothing = .1f;
        float smoothX;
        float smoothY;
        float smoothXVelocity;
        float smoothYVelocity;

        public float lookAngle;
        public float tilAngle;
        public float minAngle = -35;
        public float maxAngle = 35;

        public static CameraManager singleton;
        public Transform target;
        public Transform lockOnTarget;

        [HideInInspector]
        public Transform pivot;
        [HideInInspector]
        public Transform cameraTrans;
        
        private void Awake()
        {
            singleton = this;
        }
        public void Init(Transform t)
        {
            target = t;
            cameraTrans = Camera.main.transform;
            pivot = cameraTrans.parent;
        }
        public void Tick(float d)
        {
            float h = Input.GetAxis("Mouse X");
            float v = Input.GetAxis("Mouse Y");
            float c_h = Input.GetAxis("RightAxis X");
            float c_v = Input.GetAxis("RightAxis Y");
            float targetSpeed = mouseSpeed;

            if(c_h != 0 || c_v != 0)
            {
                h = c_h;
                v = c_v;
                targetSpeed = controllerSpeed;
            }
            FollowTarget(d);
            HandleRotation(d, h, v, targetSpeed);
        }
        void FollowTarget(float d)
        {
            float speed = d * followSpeed;
            Vector3 targetPos = Vector3.Lerp(transform.position, target.position, speed);
            transform.position = targetPos;
        }
        void HandleRotation(float d, float h, float v, float targetSpeed)
        {
            if(turnSmoothing > 0)
            {
                smoothX = Mathf.SmoothDamp(smoothX, h, ref smoothXVelocity, turnSmoothing);
                smoothY = Mathf.SmoothDamp(smoothY, v, ref smoothYVelocity, turnSmoothing);
            }
            else
            {
                smoothX = h;
                smoothY = v;
            }

            tilAngle -= smoothY * targetSpeed;
            tilAngle = Mathf.Clamp(tilAngle, minAngle, maxAngle);
            pivot.localRotation = Quaternion.Euler(tilAngle, 0f, 0f);
            
            if (lockOn && (lockOnTarget != null))
            {
                Vector3 targetDir = lockOnTarget.position - transform.position;
                targetDir.Normalize();
                //targetDir.y = 0;
                if (targetDir == Vector3.zero)
                    targetDir = transform.forward;

                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, d * 9);
                lookAngle = transform.eulerAngles.y;
                return;
            }

            lookAngle += smoothX * targetSpeed;
            //Debug.Log(smoothX);
            transform.rotation = Quaternion.Euler(0f, lookAngle, 0f);
             
        }
    }
}