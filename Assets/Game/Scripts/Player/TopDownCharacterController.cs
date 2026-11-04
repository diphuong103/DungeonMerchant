using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public sealed class TopDownCharacterController : MonoBehaviour
    {
        [SerializeField] private float speed = 3f;

        private static readonly int WalkDown = Animator.StringToHash("WalkDown");
        private static readonly int WalkUp = Animator.StringToHash("WalkUp");
        private static readonly int WalkLeft = Animator.StringToHash("WalkLeft");
        private static readonly int WalkRight = Animator.StringToHash("WalkRight");

        private Rigidbody2D body;
        private Animator animator;
        private Vector2 movement;
        private Vector2 lastFacing = Vector2.down;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            body.gravityScale = 0f;
            body.freezeRotation = true;
            animator.applyRootMotion = false;
            animator.Play(WalkDown, 0, 0f);
            animator.speed = 0f;
        }

        private void Update()
        {
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            movement = Vector2.ClampMagnitude(movement, 1f);

            if (movement.sqrMagnitude > 0f)
            {
                Vector2 facing = Mathf.Abs(movement.x) > Mathf.Abs(movement.y)
                    ? new Vector2(Mathf.Sign(movement.x), 0f)
                    : new Vector2(0f, Mathf.Sign(movement.y));

                if (facing != lastFacing)
                {
                    lastFacing = facing;
                    animator.Play(GetFacingState(facing), 0, 0f);
                }

                animator.speed = 1f;
            }
            else
            {
                // The supplied art contains walk cycles but no idle clips. Pause on the
                // current frame so the player keeps facing the last direction.
                animator.speed = 0f;
            }
        }

        private void FixedUpdate()
        {
            body.linearVelocity = movement * speed;
        }

        private static int GetFacingState(Vector2 facing)
        {
            if (facing.x < 0f) return WalkLeft;
            if (facing.x > 0f) return WalkRight;
            return facing.y > 0f ? WalkUp : WalkDown;
        }
    }
}
