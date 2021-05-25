using System.Collections;
using UnityEngine;

namespace Gameplay.Animation
{
    public class PlayerAnimationController : MonoBehaviour
    {
        public enum TurnType { Slerp, SmoothDamp }

        [SerializeField] private Transform playerModel;
        [SerializeField] private Animator animator;
        [SerializeField] private TileMover tileMover;
        [SerializeField] private float turnTime = 0.33f;
        [SerializeField] private TurnType turnType;

        private const string AnimParamWalkSpeedKey = "walk_speed";
        private int AnimParamWalkSpeedHash;
        private Quaternion targetRotation;
        private Vector3 rotationVelocity;
        private float turnSlerpRate;
        private Coroutine turnToTargetRotationCoroutine;


        private void Awake()
        {
            AnimParamWalkSpeedHash = Animator.StringToHash(AnimParamWalkSpeedKey);
        }

        private void OnEnable()
        {
            tileMover.OnMoved += HandleOnMoved;
            tileMover.OnStoppedOnTile += HandleOnStoppedOnTile;
        }

        private void OnDisable()
        {
            tileMover.OnMoved -= HandleOnMoved;
            tileMover.OnStoppedOnTile -= HandleOnStoppedOnTile;
        }

        private void OnValidate()
        {
            turnSlerpRate = 1 / turnTime;
        }

        private void HandleOnMoved(TileMover tileMover)
        {
            RotateToTileMoverVelocity();

            UpdateAnimatorSettings();
        }

        private void HandleOnStoppedOnTile(TileMover tileMover)
        {
            animator.SetFloat(AnimParamWalkSpeedHash, 0);
        }

        /// <summary>
        /// Updates animator settings.
        /// </summary>
        private void UpdateAnimatorSettings()
        {
            //Clamp movementSpeed so value is appropriate for animation speed scaling
            float movementSpeed = tileMover.CurrentSpeed;
            movementSpeed = Mathf.Clamp(movementSpeed, 0.05f, 1);

            animator.SetFloat(AnimParamWalkSpeedHash, movementSpeed);
        }

        /// <summary>
        /// Updates targetRotation to tileMover.velocity and starts TurnToRotationCoroutine if not running.
        /// </summary>
        private void RotateToTileMoverVelocity()
        {
            targetRotation = Quaternion.LookRotation(tileMover.Velocity);

            if (turnToTargetRotationCoroutine == null)
                turnToTargetRotationCoroutine = StartCoroutine(TurnToRotationCoroutine());
        }

        /// <summary>
        /// Turns model toward targetRotation over time.
        /// </summary>
        IEnumerator TurnToRotationCoroutine()
        {
            while (playerModel.rotation != targetRotation)
            {
                switch (turnType)
                {
                    case TurnType.SmoothDamp:
                        playerModel.rotation = SmoothDampQuaternion(playerModel.rotation, targetRotation, ref rotationVelocity, turnTime);
                        break;
                    case TurnType.Slerp:
                        playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, turnSlerpRate * Time.deltaTime);
                        break;
                }

                yield return null;
            }

            turnToTargetRotationCoroutine = null;
        }
       
        //https://forum.unity.com/threads/quaternion-smoothdamp.793533/
        static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
        {
            Vector3 c = current.eulerAngles;
            Vector3 t = target.eulerAngles;
            return Quaternion.Euler(
              Mathf.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime),
              Mathf.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime),
              Mathf.SmoothDampAngle(c.z, t.z, ref currentVelocity.z, smoothTime)
            );
        }
    }
}
