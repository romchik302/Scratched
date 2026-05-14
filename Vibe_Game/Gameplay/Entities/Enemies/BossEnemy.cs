using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Gameplay.Entities.Enemies;

public sealed class BossEnemy : Enemy
{
    private enum BossAttackType
    {
        SpikeBurst,
        SpinningSpikes,
        BurrowStrike,
        SummonMinions
    }

    private enum BurrowPhase
    {
        None,
        Windup,
        MovingTrail
    }

    private readonly IWallCollisionChecker _collision;
    private readonly Random _rng = new();
    private Texture2D _pixel;
    private BossAttackType? _lastAttack;
    private float _attackTimer;
    private float _cooldownTimer;
    private bool _isInAttack;
    private BurrowPhase _burrowPhase;
    private Vector2 _burrowTrailPosition;
    private float _burrowWindupRemaining;

    private bool _burstSpawned;
    private float _burstSpawnDelay;

    private bool _summonSpawned;
    private float _summonIntroRemaining;

    private BossAttackType _currentAttack;
    private float _attackElapsed;

    private int _spriteRow = EnemyConfig.BossSheetIdleRow;
    private int _spriteFrame;
    private float _spriteAnimTimer;

    public Action<ProjectileSpawnArgs> ProjectileSpawner { get; set; }
    public Action<Vector2, bool> SummonEnemy { get; set; }
    public Action<float> DamagePlayer { get; set; }

    public Vector2 ChaseTarget { get; set; }

    public float MoveSpeed { get; set; } = EnemyConfig.BossMoveSpeed;
    public float CollisionRadius { get; set; } = EnemyConfig.BossRadius;
    public float AttackPauseMin { get; set; } = EnemyConfig.BossAttackPauseMin;
    public float AttackPauseMax { get; set; } = EnemyConfig.BossAttackPauseMax;
    public float ContactDamage { get; set; } = 1f;

    public int SpikeBurstProjectileCount { get; set; } = EnemyConfig.BossSpikeBurstProjectileCount;
    public float SpikeBurstProjectileSpeed { get; set; } = EnemyConfig.BossSpikeBurstProjectileSpeed;
    public float SpikeBurstProjectileLifetime { get; set; } = EnemyConfig.BossSpikeBurstProjectileLifetime;
    public float SpikeBurstProjectileRadius { get; set; } = EnemyConfig.BossSpikeBurstProjectileRadius;
    public float SpikeBurstSpawnRadius { get; set; } = EnemyConfig.BossSpikeBurstSpawnRadius;

    public int SpinningSpikeCount { get; set; } = EnemyConfig.BossSpinningSpikeCount;
    public float SpinningSpikeOrbitRadius { get; set; } = EnemyConfig.BossSpinningSpikeOrbitRadius;
    public float SpinningSpikeAngularSpeed { get; set; } = EnemyConfig.BossSpinningSpikeAngularSpeed;
    public float SpinningSpikeOrbitDuration { get; set; } = EnemyConfig.BossSpinningSpikeOrbitDuration;
    public float SpinningSpikeReleaseSpeed { get; set; } = EnemyConfig.BossSpinningSpikeReleaseSpeed;

    public float BurrowTravelDuration { get; set; } = EnemyConfig.BossBurrowTravelDuration;
    public float BurrowTrailSpeed { get; set; } = EnemyConfig.BossBurrowTrailSpeed;
    public float BurrowStrikeRadius { get; set; } = EnemyConfig.BossBurrowStrikeRadius;
    public bool IsInvulnerableDuringBurrow { get; set; } = EnemyConfig.BossInvulnerableDuringBurrow;

    public int SummonMinCount { get; set; } = EnemyConfig.BossSummonMinCount;
    public int SummonMaxCount { get; set; } = EnemyConfig.BossSummonMaxCount;
    public float SummonSpawnRadius { get; set; } = EnemyConfig.BossSummonSpawnRadius;
    public float SummonShooterChance { get; set; } = EnemyConfig.BossSummonShooterChance;
    public float SummonAttackWeight { get; set; } = EnemyConfig.BossSummonAttackWeight;

    public override bool IsInvulnerable => IsInvulnerableDuringBurrow && _burrowPhase != BurrowPhase.None;
    public override bool CanDealContactDamage => _burrowPhase == BurrowPhase.None;

    public BossEnemy(
        Vector2 position,
        IWallCollisionChecker collision,
        float moveSpeed,
        int maxHealth,
        float collisionRadius)
        : base(position, maxHealth)
    {
        _collision = collision ?? throw new ArgumentNullException(nameof(collision));
        MoveSpeed = moveSpeed;
        CollisionRadius = collisionRadius;
        RecoilResistance = 0.9f;
        PenetrationRadius = 2f;
        RandomBehaviorChance = 0f;
    }

    public BossEnemy(Vector2 position, IWallCollisionChecker collision)
        : this(position, collision, EnemyConfig.BossMoveSpeed, EnemyConfig.BossMaxHealth, EnemyConfig.BossRadius)
    {
    }

    protected override void UpdateEnemy(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_burrowPhase != BurrowPhase.None)
        {
            UpdateBurrow(dt);
            UpdateBossSpriteAnimation(dt);
            return;
        }

        if (_isInAttack)
        {
            _attackElapsed += dt;
            _attackTimer -= dt;

            if (_currentAttack == BossAttackType.SpikeBurst && !_burstSpawned)
            {
                _burstSpawnDelay -= dt;
                if (_burstSpawnDelay <= 0f)
                {
                    ExecuteSpikeBurst();
                    _burstSpawned = true;
                }
            }

            if (_currentAttack == BossAttackType.SummonMinions)
            {
                _summonIntroRemaining -= dt;
                if (!_summonSpawned && _summonIntroRemaining <= 0f)
                {
                    ExecuteSummon();
                    _summonSpawned = true;
                    GameplayAudio.PlayBossStatic();
                }
            }

            UpdateVisualRowForAttack();
            UpdateBossSpriteAnimation(dt);

            if (_attackTimer <= 0f)
                EndAttackAndEnterCooldown();

            return;
        }

        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= dt;
            Velocity = Vector2.Zero;
            _spriteRow = EnemyConfig.BossSheetIdleRow;
            UpdateBossSpriteAnimation(dt);
            return;
        }

        MoveTowardPlayer(dt);
        _spriteRow = EnemyConfig.BossSheetIdleRow;
        UpdateBossSpriteAnimation(dt);
        StartAttack(ChooseNextAttack());
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || !IsActivated || spriteBatch == null)
            return;

        Texture2D sheet = SharedBossTexture;
        if (sheet != null)
        {
            if (_pixel == null)
            {
                _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }
            int fw = Math.Max(1, sheet.Width / Math.Max(1, EnemyConfig.BossSheetFramesCount));
            int fh = Math.Max(1, sheet.Height / Math.Max(1, EnemyConfig.BossSheetRowCount));
            int row = Math.Clamp(_spriteRow, 0, EnemyConfig.BossSheetRowCount - 1);
            int frameCount = row == EnemyConfig.BossSheetFliesAttackIdleRow
                ? EnemyConfig.BossSheetFliesAttckFramesCount
                : EnemyConfig.BossSheetCommonFramesCount;
            int frame = Math.Clamp(_spriteFrame, 0, frameCount - 1);
            var src = new Rectangle(frame * fw, row * fh, fw, fh);
            bool flip = ChaseTarget.X < Position.X - 2f;
            var effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float scale = (CollisionRadius * 2.1f) / Math.Max(fw, fh);
            Vector2 drawPos = Position;
            if (_burrowPhase == BurrowPhase.MovingTrail)
                drawPos = _burrowTrailPosition;
            else if (_burrowPhase == BurrowPhase.Windup)
                drawPos = Position;

            spriteBatch.Draw(
                sheet,
                drawPos,
                src,
                Color.White,
                0f,
                new Vector2(fw / 2f, fh / 2f),
                scale,
                effects,
                0f);

            if (_burrowPhase == BurrowPhase.MovingTrail && _pixel != null)
            {
                int radius = 12;
                Rectangle trail = new Rectangle((int)_burrowTrailPosition.X - radius, (int)_burrowTrailPosition.Y - radius, radius * 2, radius * 2);
                spriteBatch.Draw(_pixel, trail, new Color(135, 170, 85, 160));
            }

            DrawDebugOverlay(spriteBatch);
            return;
        }

        if (_pixel == null)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        Rectangle body = GetBounds();
        spriteBatch.Draw(_pixel, body, new Color(95, 45, 45, 240));
        spriteBatch.Draw(_pixel, new Rectangle(body.X + 6, body.Y + 6, body.Width - 12, body.Height - 12), new Color(170, 65, 65, 235));
        DrawDebugOverlay(spriteBatch);
    }

    public override Rectangle GetBounds()
    {
        int r = (int)CollisionRadius;
        return new Rectangle((int)Position.X - r, (int)Position.Y - r, r * 2, r * 2);
    }

    private void MoveTowardPlayer(float dt)
    {
        Vector2 toTarget = ChaseTarget - Position;
        if (toTarget.LengthSquared() < 4f)
        {
            Velocity = Vector2.Zero;
            return;
        }

        toTarget.Normalize();
        Position = ResolveWallCollision(Position, toTarget * MoveSpeed * dt);
        Velocity = Vector2.Zero;
    }

    private BossAttackType ChooseNextAttack()
    {
        (BossAttackType type, float weight)[] weighted =
        {
            (BossAttackType.SpikeBurst, 1f),
            (BossAttackType.SpinningSpikes, 1f),
            (BossAttackType.BurrowStrike, 1f),
            (BossAttackType.SummonMinions, SummonAttackWeight)
        };

        float total = 0f;
        for (int i = 0; i < weighted.Length; i++)
        {
            if (_lastAttack.HasValue && weighted[i].type == _lastAttack.Value)
                continue;

            total += weighted[i].weight;
        }

        if (total <= 0f)
            return BossAttackType.SpikeBurst;

        float roll = (float)_rng.NextDouble() * total;
        float cumulative = 0f;
        for (int i = 0; i < weighted.Length; i++)
        {
            if (_lastAttack.HasValue && weighted[i].type == _lastAttack.Value)
                continue;

            cumulative += weighted[i].weight;
            if (roll <= cumulative)
                return weighted[i].type;
        }

        return BossAttackType.SpikeBurst;
    }

    private void StartAttack(BossAttackType attack)
    {
        _isInAttack = true;
        _lastAttack = attack;
        _currentAttack = attack;
        Velocity = Vector2.Zero;
        _attackElapsed = 0f;
        _burstSpawned = false;
        _summonSpawned = false;

        float commonDur = EnemyConfig.BossCommonAnimFrameDurationSeconds;
        float rotateDur = EnemyConfig.BossSheetCommonFramesCount * commonDur;

        switch (attack)
        {
            case BossAttackType.SpikeBurst:
                _burstSpawnDelay = 2f * commonDur;
                _attackTimer = rotateDur + 6f * commonDur;
                break;

            case BossAttackType.SpinningSpikes:
                ExecuteSpinningSpikes();
                _attackTimer = SpinningSpikeOrbitDuration + rotateDur + 2f * commonDur;
                break;

            case BossAttackType.BurrowStrike:
                BeginBurrow();
                GameplayAudio.PlayBossBurrow();
                break;

            case BossAttackType.SummonMinions:
                _summonIntroRemaining = EnemyConfig.BossSheetCommonFramesCount * commonDur;
                _attackTimer = _summonIntroRemaining + GetSummonStaticDurationSeconds();
                break;
        }

        if (attack != BossAttackType.BurrowStrike)
            GameplayAudio.PlayBossAttack();
    }

    private static float GetSummonStaticDurationSeconds()
    {
        float idleLoop = EnemyConfig.BossSheetCommonFramesCount * EnemyConfig.BossIdleRowFrameDurationSeconds;
        float need = EnemyConfig.EnemyActivationDelaySeconds;
        int n = (int)Math.Ceiling(need / Math.Max(0.01f, idleLoop));
        return Math.Max(1, n) * idleLoop;
    }

    private void EndAttackAndEnterCooldown()
    {
        _isInAttack = false;
        _attackTimer = 0f;
        _cooldownTimer = NextRange(AttackPauseMin, AttackPauseMax);
    }

    private void UpdateVisualRowForAttack()
    {
        float commonDur = EnemyConfig.BossCommonAnimFrameDurationSeconds;
        float rotateEnd = EnemyConfig.BossSheetCommonFramesCount * commonDur;

        switch (_currentAttack)
        {
            case BossAttackType.SpikeBurst:
                if (_attackElapsed < rotateEnd)
                    _spriteRow = EnemyConfig.BossSheetRotateRow;
                else
                    _spriteRow = EnemyConfig.BossSheetAttackRow;
                break;

            case BossAttackType.SpinningSpikes:
                if (_attackElapsed < rotateEnd)
                    _spriteRow = EnemyConfig.BossSheetRotateRow;
                else
                    _spriteRow = EnemyConfig.BossSheetAttackRow;
                break;

            case BossAttackType.SummonMinions:
                if (_summonIntroRemaining > 0f)
                    _spriteRow = EnemyConfig.BossSheetAttackRow;
                else
                    _spriteRow = EnemyConfig.BossSheetFliesAttackIdleRow;
                break;

            default:
                _spriteRow = EnemyConfig.BossSheetIdleRow;
                break;
        }
    }

    private void UpdateBossSpriteAnimation(float dt)
    {
        Texture2D sheet = SharedBossTexture;
        if (sheet == null)
            return;

        int frameCount = _spriteRow == EnemyConfig.BossSheetFliesAttackIdleRow
            ? EnemyConfig.BossSheetFliesAttckFramesCount
            : EnemyConfig.BossSheetCommonFramesCount;

        float frameDur = _spriteRow == EnemyConfig.BossSheetFliesAttackIdleRow
            ? EnemyConfig.BossFlyIdleAnimFrameDurationSeconds
            : (_spriteRow == EnemyConfig.BossSheetIdleRow
                ? EnemyConfig.BossIdleRowFrameDurationSeconds
                : EnemyConfig.BossCommonAnimFrameDurationSeconds);

        if (_burrowPhase == BurrowPhase.Windup)
        {
            _spriteRow = EnemyConfig.BossSheetBurrowRow;
            frameCount = EnemyConfig.BossSheetCommonFramesCount;
            frameDur = EnemyConfig.BossCommonAnimFrameDurationSeconds;
        }
        else if (_burrowPhase == BurrowPhase.MovingTrail)
        {
            _spriteRow = EnemyConfig.BossSheetDiggingRow;
            frameCount = EnemyConfig.BossSheetCommonFramesCount;
            frameDur = EnemyConfig.BossCommonAnimFrameDurationSeconds;
        }

        _spriteAnimTimer += dt;
        if (_spriteAnimTimer >= frameDur)
        {
            _spriteAnimTimer = 0f;
            _spriteFrame = (_spriteFrame + 1) % Math.Max(1, frameCount);
        }
    }

    private void ExecuteSpikeBurst()
    {
        if (ProjectileSpawner == null)
            return;

        int count = Math.Max(8, SpikeBurstProjectileCount);
        float step = MathHelper.TwoPi / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * step;
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            ProjectileSpawner.Invoke(new ProjectileSpawnArgs
            {
                Position = Position + dir * SpikeBurstSpawnRadius,
                Direction = dir,
                Speed = SpikeBurstProjectileSpeed,
                Damage = ContactDamage,
                LifetimeSeconds = SpikeBurstProjectileLifetime,
                Radius = SpikeBurstProjectileRadius,
                RecoilForce = 0f,
                IsFriendlyToPlayer = false,
                IgnoreWallCollisions = true,
                Length = 30f
            });
        }

        SpawnBurstSpecialProjectiles();
    }

    private void SpawnBurstSpecialProjectiles()
    {
        if (ProjectileSpawner == null)
            return;

        int special = Math.Max(0, EnemyConfig.BossBurstSpecialProjectileCount);
        if (special == 0)
            return;

        Vector2 toPlayer = ChaseTarget - Position;
        if (toPlayer.LengthSquared() < 0.0001f)
            toPlayer = Vector2.UnitY;
        else
            toPlayer.Normalize();

        float spread = EnemyConfig.BossBurstSpecialSpreadHalfAngle;
        for (int i = 0; i < special; i++)
        {
            float t = special == 1 ? 0f : (i / (float)(special - 1)) * 2f - 1f;
            float ang = MathF.Atan2(toPlayer.Y, toPlayer.X) + t * spread;
            Vector2 dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
            ProjectileSpawner.Invoke(new ProjectileSpawnArgs
            {
                Position = Position + dir * (SpikeBurstSpawnRadius * 0.4f),
                Direction = dir,
                Speed = EnemyConfig.BossBurstSpecialProjectileSpeed,
                Damage = ContactDamage,
                LifetimeSeconds = EnemyConfig.BossBurstSpecialProjectileLifetime,
                Radius = EnemyConfig.BossBurstSpecialProjectileRadius,
                RecoilForce = 0f,
                IsFriendlyToPlayer = false,
                IgnoreWallCollisions = true,
                Length = EnemyConfig.BossBurstSpecialProjectileLength,
                HostileTextureOverride = EnemyConfig.BossLongProjectileTexture,
                HostileTextureFrameColumns = EnemyConfig.BossLongProjectileFrameColumns,
                HostileProjectileDrawSize = EnemyConfig.BossLongProjectileDrawSize
            });
        }
    }

    private void ExecuteSpinningSpikes()
    {
        if (ProjectileSpawner == null)
            return;

        int count = Math.Max(6, SpinningSpikeCount);
        float step = MathHelper.TwoPi / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * step;
            Vector2 outward = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            ProjectileSpawner.Invoke(new ProjectileSpawnArgs
            {
                Position = Position + outward * SpinningSpikeOrbitRadius,
                Direction = Vector2.Zero,
                Speed = SpinningSpikeReleaseSpeed,
                Damage = ContactDamage,
                LifetimeSeconds = SpinningSpikeOrbitDuration + 2f,
                Radius = SpikeBurstProjectileRadius,
                RecoilForce = 0f,
                IsFriendlyToPlayer = false,
                UseOrbitMotion = true,
                OrbitCenter = Position,
                OrbitRadius = SpinningSpikeOrbitRadius,
                OrbitStartAngle = angle,
                OrbitAngularSpeed = SpinningSpikeAngularSpeed,
                OrbitDurationSeconds = SpinningSpikeOrbitDuration,
                ReleaseAfterOrbit = true,
                ReleaseDirection = outward,
                IgnoreWallCollisions = true,
            });
        }
    }

    private void BeginBurrow()
    {
        _burrowPhase = BurrowPhase.Windup;
        _burrowTrailPosition = Position;
        _burrowWindupRemaining = EnemyConfig.BossSheetCommonFramesCount * EnemyConfig.BossCommonAnimFrameDurationSeconds;
        _attackTimer = _burrowWindupRemaining + BurrowTravelDuration;
        _spriteRow = EnemyConfig.BossSheetBurrowRow;
    }

    private void UpdateBurrow(float dt)
    {
        _attackTimer -= dt;

        if (_burrowPhase == BurrowPhase.Windup)
        {
            _burrowWindupRemaining -= dt;
            if (_burrowWindupRemaining <= 0f)
                _burrowPhase = BurrowPhase.MovingTrail;

            UpdateBossSpriteAnimation(dt);
            return;
        }

        if (_burrowPhase != BurrowPhase.MovingTrail)
            return;

        Vector2 toTarget = ChaseTarget - _burrowTrailPosition;
        if (toTarget.LengthSquared() > 1f)
        {
            Vector2 dir = Vector2.Normalize(toTarget);
            _burrowTrailPosition += dir * BurrowTrailSpeed * dt;
        }

        if (_attackTimer > 0f)
        {
            UpdateBossSpriteAnimation(dt);
            return;
        }

        Position = _burrowTrailPosition;
        _burrowPhase = BurrowPhase.None;
        GameplayAudio.PlayBossEmerge();
        TryBurrowStrikePlayer();
        EndAttackAndEnterCooldown();
    }

    private void TryBurrowStrikePlayer()
    {
        if (DamagePlayer == null)
            return;

        float dist = Vector2.Distance(Position, ChaseTarget);
        if (dist <= BurrowStrikeRadius)
            DamagePlayer.Invoke(ContactDamage);
    }

    private void ExecuteSummon()
    {
        if (SummonEnemy == null)
            return;

        int summonCount = _rng.Next(Math.Max(1, SummonMinCount), Math.Max(SummonMinCount + 1, SummonMaxCount + 1));
        for (int i = 0; i < summonCount; i++)
        {
            float angle = (float)_rng.NextDouble() * MathHelper.TwoPi;
            float radius = NextRange(SummonSpawnRadius * 0.55f, SummonSpawnRadius);
            Vector2 spawnPos = Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            bool spawnShooter = _rng.NextDouble() < SummonShooterChance;
            SummonEnemy.Invoke(spawnPos, spawnShooter);
        }
    }

    private Vector2 ResolveWallCollision(Vector2 oldPos, Vector2 delta)
    {
        Vector2 target = oldPos + delta;
        Vector2 final = target;
        float r = CollisionRadius;

        if (delta.X != 0f)
        {
            bool blockedX = _collision.IsPointBlockedByWall(new Vector2(target.X - r, oldPos.Y - r))
                || _collision.IsPointBlockedByWall(new Vector2(target.X + r, oldPos.Y - r))
                || _collision.IsPointBlockedByWall(new Vector2(target.X - r, oldPos.Y + r))
                || _collision.IsPointBlockedByWall(new Vector2(target.X + r, oldPos.Y + r));
            if (blockedX)
                final.X = oldPos.X;
        }

        if (delta.Y != 0f)
        {
            bool blockedY = _collision.IsPointBlockedByWall(new Vector2(final.X - r, target.Y - r))
                || _collision.IsPointBlockedByWall(new Vector2(final.X + r, target.Y - r))
                || _collision.IsPointBlockedByWall(new Vector2(final.X - r, target.Y + r))
                || _collision.IsPointBlockedByWall(new Vector2(final.X + r, target.Y + r));
            if (blockedY)
                final.Y = oldPos.Y;
        }

        return final;
    }

    private float NextRange(float min, float max)
    {
        if (max < min)
            (min, max) = (max, min);
        return min + (float)_rng.NextDouble() * (max - min);
    }

    protected override Rectangle? GetDebugAttackBounds()
    {
        int r = (int)BurrowStrikeRadius;
        return new Rectangle((int)Position.X - r, (int)Position.Y - r, r * 2, r * 2);
    }

    protected override void OnActivated()
    {
        GameplayAudio.PlayBossEntering();
        GameplayAudio.PlayMapBossEntry();
    }
}
