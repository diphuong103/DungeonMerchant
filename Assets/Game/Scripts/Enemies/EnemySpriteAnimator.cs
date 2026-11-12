using UnityEngine;

namespace DungeonMerchant.Enemies
{
    public enum EnemyMotionState { Idle, Patrol, Chase, Attack, Hurt, Special, Dead }
    public enum EnemyAttackType { Melee, Projectile }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemySpriteAnimator : MonoBehaviour
    {
        [Header("Directional frames (side faces right; left is flipped)")]
        [SerializeField] private Sprite[] idleDown, idleUp, idleSide;
        [SerializeField] private Sprite[] walkDown, walkUp, walkSide;
        [SerializeField] private Sprite[] attackDown, attackUp, attackSide;
        [SerializeField] private Sprite[] hurtDown, hurtUp, hurtSide;
        [SerializeField] private Sprite[] specialDown, specialUp, specialSide;
        [SerializeField] private Sprite[] deathDown, deathUp, deathSide;
        [SerializeField] private float idleFps = 6f, walkFps = 10f, attackFps = 12f, hurtFps = 10f, specialFps = 10f, deathFps = 8f;
        private SpriteRenderer spriteRenderer;
        private EnemyMotionState state;
        private Vector2 facing = Vector2.down;
        private float elapsed;
        private bool loop;
        private Sprite[] frames;
        private int lastFrame = -1;
        public int FrameIndex { get; private set; }
        public int FrameCount => frames == null ? 0 : frames.Length;
        public float NormalizedTime => frames == null || frames.Length <= 1 ? 1f : FrameIndex / (float)(frames.Length - 1);

        private void Awake() { spriteRenderer = GetComponent<SpriteRenderer>(); }
        public void Play(EnemyMotionState next, Vector2 direction, bool shouldLoop)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (next == state && direction == facing && shouldLoop == loop) return;
            state = next;
            if (direction.sqrMagnitude > 0.001f) facing = direction;
            loop = shouldLoop;
            elapsed = 0f;
            FrameIndex = 0;
            lastFrame = -1;
            frames = SelectFrames(state, facing);
            spriteRenderer.flipX = Mathf.Abs(facing.x) > Mathf.Abs(facing.y) && facing.x < 0f;
            ApplyFrame();
        }
        public void SetFacing(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return;
            facing = direction;
            spriteRenderer.flipX = Mathf.Abs(facing.x) > Mathf.Abs(facing.y) && facing.x < 0f;
            frames = SelectFrames(state, facing);
            lastFrame = -1;
            ApplyFrame();
        }
        private void Update()
        {
            if (frames == null || frames.Length == 0) return;
            float fps = state == EnemyMotionState.Idle ? idleFps : state == EnemyMotionState.Patrol || state == EnemyMotionState.Chase ? walkFps : state == EnemyMotionState.Attack ? attackFps : state == EnemyMotionState.Hurt ? hurtFps : state == EnemyMotionState.Special ? specialFps : deathFps;
            elapsed += Time.deltaTime * fps;
            int next = Mathf.FloorToInt(elapsed);
            FrameIndex = loop ? next % frames.Length : Mathf.Min(next, frames.Length - 1);
            ApplyFrame();
        }
        private void ApplyFrame()
        {
            if (frames == null || frames.Length == 0 || lastFrame == FrameIndex) return;
            lastFrame = FrameIndex;
            if (frames[FrameIndex] != null) spriteRenderer.sprite = frames[FrameIndex];
        }
        private Sprite[] SelectFrames(EnemyMotionState s, Vector2 d)
        {
            bool horizontal = Mathf.Abs(d.x) > Mathf.Abs(d.y);
            if (horizontal) return s == EnemyMotionState.Idle ? idleSide : s == EnemyMotionState.Patrol || s == EnemyMotionState.Chase ? walkSide : s == EnemyMotionState.Attack ? attackSide : s == EnemyMotionState.Hurt ? hurtSide : s == EnemyMotionState.Special ? specialSide : deathSide;
            bool up = d.y > 0f;
            if (s == EnemyMotionState.Idle) return up ? idleUp : idleDown;
            if (s == EnemyMotionState.Patrol || s == EnemyMotionState.Chase) return up ? walkUp : walkDown;
            if (s == EnemyMotionState.Attack) return up ? attackUp : attackDown;
            if (s == EnemyMotionState.Hurt) return up ? hurtUp : hurtDown;
            if (s == EnemyMotionState.Special) return up ? specialUp : specialDown;
            return up ? deathUp : deathDown;
        }
    }
}
