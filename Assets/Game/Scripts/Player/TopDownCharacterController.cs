using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonMerchant.Combat;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Cainos.PixelArtTopDown_Basic
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public sealed class TopDownCharacterController : MonoBehaviour
    {
        [SerializeField] private float speed = 3f;
        [SerializeField] private Transform weaponSprite;
        [SerializeField] private Sprite[] attackSprites = new Sprite[32];
        [SerializeField, Min(1f)] private float attackFramesPerSecond = 14f;
        [SerializeField, Min(0)] private int attackDamage = 20;
        [SerializeField, Range(0, 7)] private int attackHitFrame = 4;
        [SerializeField, Min(0f)] private float attackHitDistance = 0.48f;
        [SerializeField, Min(0.05f)] private float attackHitRadius = 0.38f;
        [SerializeField, Min(0f)] private float attackKnockback = 1.5f;

        private static readonly int WalkDown = Animator.StringToHash("WalkDown");
        private static readonly int WalkUp = Animator.StringToHash("WalkUp");
        private static readonly int WalkLeft = Animator.StringToHash("WalkLeft");
        private static readonly int WalkRight = Animator.StringToHash("WalkRight");

        private Rigidbody2D body;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Sprite idleSprite;
        private Vector2 movement;
        private Vector2 lastFacing = Vector2.down;
        private bool isAttacking;
        private bool warnedAboutAttackSprites;

        public Vector2 Facing => lastFacing;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            idleSprite = spriteRenderer != null ? spriteRenderer.sprite : null;

            body.gravityScale = 0f;
            body.freezeRotation = true;
            animator.applyRootMotion = false;
            animator.Play(WalkDown, 0, 0f);
            animator.speed = 0f;

            if (weaponSprite == null)
                weaponSprite = transform.Find("WeaponPivot_I111/WeaponSprite_I111");

            UpdateWeaponFacing(lastFacing);
        }

        private void Update()
        {
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            movement = Vector2.ClampMagnitude(movement, 1f);

            if (!isAttacking && movement.sqrMagnitude > 0f)
            {
                Vector2 facing = Mathf.Abs(movement.x) > Mathf.Abs(movement.y)
                    ? new Vector2(Mathf.Sign(movement.x), 0f)
                    : new Vector2(0f, Mathf.Sign(movement.y));

                if (facing != lastFacing)
                {
                    lastFacing = facing;
                    animator.Play(GetFacingState(facing), 0, 0f);
                    UpdateWeaponFacing(facing);
                }

                animator.speed = 1f;
            }
            else if (!isAttacking)
            {
                // The supplied art contains walk cycles but no idle clips. Pause on the
                // current frame so the player keeps facing the last direction.
                animator.speed = 0f;
            }

            if (!isAttacking && LeftMousePressedThisFrame())
                BeginAttack();
        }

        private static bool LeftMousePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
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

        private void UpdateWeaponFacing(Vector2 facing)
        {
            if (weaponSprite == null) return;

            float angle = facing.y > 0f ? 90f
                : facing.y < 0f ? -90f
                : facing.x < 0f ? 180f
                : 0f;

            weaponSprite.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void BeginAttack()
        {
            if (attackSprites == null || attackSprites.Length < 32 || spriteRenderer == null)
            {
                if (!warnedAboutAttackSprites)
                {
                    Debug.LogWarning("Player attack needs 32 directional sprites assigned to Attack Sprites.", this);
                    warnedAboutAttackSprites = true;
                }
                return;
            }

            isAttacking = true;
            StartCoroutine(PlayAttack());
        }

        private IEnumerator PlayAttack()
        {
            Vector2 attackFacing = lastFacing;
            int frameOffset = GetAttackDirectionIndex(attackFacing) * 8;
            float frameDuration = 1f / attackFramesPerSecond;

            animator.enabled = false;
            if (weaponSprite != null)
                weaponSprite.gameObject.SetActive(false);

            for (int frame = 0; frame < 8; frame++)
            {
                SetAttackFrame(attackSprites[frameOffset + frame]);
                if (frame == attackHitFrame)
                    ApplyAttackDamage(attackFacing);

                yield return new WaitForSeconds(frameDuration);
            }

            if (weaponSprite != null)
                weaponSprite.gameObject.SetActive(true);

            spriteRenderer.sprite = idleSprite;
            animator.enabled = true;
            animator.Play(GetFacingState(lastFacing), 0, 0f);
            animator.speed = 1f;
            isAttacking = false;
        }

        private void SetAttackFrame(Sprite attackSprite)
        {
            if (attackSprite == null || spriteRenderer == null)
                return;

            spriteRenderer.sprite = attackSprite;
        }

        private void ApplyAttackDamage(Vector2 facing)
        {
            Vector2 hitCenter = (Vector2)transform.position + facing * attackHitDistance;
            Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, attackHitRadius);
            HashSet<int> damagedTargets = new HashSet<int>();

            foreach (Collider2D hit in hits)
            {
                if (hit == null || hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;

                MonoBehaviour[] behaviours = hit.GetComponentsInParent<MonoBehaviour>();
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (!(behaviour is IDamageable damageable))
                        continue;

                    if (damagedTargets.Add(behaviour.GetInstanceID()))
                        damageable.TakeDamage(attackDamage, facing, attackKnockback);
                    break;
                }
            }
        }

        private static int GetAttackDirectionIndex(Vector2 facing)
        {
            if (facing.y < 0f) return 0; // down
            if (facing.x < 0f) return 1; // left
            if (facing.x > 0f) return 2; // right
            return 3; // up
        }
    }
}
