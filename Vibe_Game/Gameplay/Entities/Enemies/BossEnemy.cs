using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>Главный босс игры, обладающий несколькими фазами атак (выброс шипов, вращающиеся снаряды, рывок под землей, призыв миньонов).</summary>
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
        MovingTrail,
        Emerging
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

    /// <summary>Делегат для спавна снарядов босса (инициализируется сценой).</summary>
    public Action<ProjectileSpawnArgs> ProjectileSpawner { get; set; }

    /// <summary>Делегат для спавна миньонов (передает позицию и флаг стрелка, инициализируется сценой).</summary>
    public Action<Vector2, bool> SummonEnemy { get; set; }

    /// <summary>Делегат для нанесения прямого урона игроку (например, при атаке из-под земли).</summary>
    public Action<float> DamagePlayer { get; set; }

    /// <summary>Целевая позиция игрока для отслеживания и атак.</summary>
    public Vector2 ChaseTarget { get; set; }

    /// <summary>Базовая скорость перемещения босса.</summary>
    public float MoveSpeed { get; set; } = EnemyConfig.BossMoveSpeed;

    /// <summary>Радиус физического хитбокса босса.</summary>
    public float CollisionRadius { get; set; } = EnemyConfig.BossRadius;

    /// <summary>Минимальная пауза между атаками в секундах.</summary>
    public float AttackPauseMin { get; set; } = EnemyConfig.BossAttackPauseMin;

    /// <summary>Максимальная пауза между атаками в секундах.</summary>
    public float AttackPauseMax { get; set; } = EnemyConfig.BossAttackPauseMax;

    /// <summary>Урон от прямого контакта с боссом или его атаки из-под земли.</summary>
    public float ContactDamage { get; set; } = 1f;

    /// <summary>Количество снарядов при базовой атаке взрывом шипов.</summary>
    public int SpikeBurstProjectileCount { get; set; } = EnemyConfig.BossSpikeBurstProjectileCount;

    /// <summary>Скорость полета снарядов при атаке взрывом шипов.</summary>
    public float SpikeBurstProjectileSpeed { get; set; } = EnemyConfig.BossSpikeBurstProjectileSpeed;

    /// <summary>Время жизни снарядов при атаке взрывом шипов.</summary>
    public float SpikeBurstProjectileLifetime { get; set; } = EnemyConfig.BossSpikeBurstProjectileLifetime;

    /// <summary>Радиус хитбокса снарядов при атаке взрывом шипов.</summary>
    public float SpikeBurstProjectileRadius { get; set; } = EnemyConfig.BossSpikeBurstProjectileRadius;

    /// <summary>Радиус от центра босса, на котором появляются снаряды при взрыве шипов.</summary>
    public float SpikeBurstSpawnRadius { get; set; } = EnemyConfig.BossSpikeBurstSpawnRadius;

    /// <summary>Количество шипов в атаке с вращающимися вокруг босса снарядами.</summary>
    public int SpinningSpikeCount { get; set; } = EnemyConfig.BossSpinningSpikeCount;

    /// <summary>Радиус орбиты, по которой вращаются шипы.</summary>
    public float SpinningSpikeOrbitRadius { get; set; } = EnemyConfig.BossSpinningSpikeOrbitRadius;

    /// <summary>Угловая скорость вращения шипов по орбите.</summary>
    public float SpinningSpikeAngularSpeed { get; set; } = EnemyConfig.BossSpinningSpikeAngularSpeed;

    /// <summary>Продолжительность фазы вращения шипов до их запуска.</summary>
    public float SpinningSpikeOrbitDuration { get; set; } = EnemyConfig.BossSpinningSpikeOrbitDuration;

    /// <summary>Скорость полета вращающихся шипов после срыва с орбиты.</summary>
    public float SpinningSpikeReleaseSpeed { get; set; } = EnemyConfig.BossSpinningSpikeReleaseSpeed;

    /// <summary>Длительность перемещения босса под землей во время атаки.</summary>
    public float BurrowTravelDuration { get; set; } = EnemyConfig.BossBurrowTravelDuration;

    /// <summary>Скорость перемещения следа от босса под землей.</summary>
    public float BurrowTrailSpeed { get; set; } = EnemyConfig.BossBurrowTrailSpeed;

    /// <summary>Радиус поражения при выныривании босса из-под земли.</summary>
    public float BurrowStrikeRadius { get; set; } = EnemyConfig.BossBurrowStrikeRadius;

    /// <summary>Определяет, неуязвим ли босс во время нахождения под землей.</summary>
    public bool IsInvulnerableDuringBurrow { get; set; } = EnemyConfig.BossInvulnerableDuringBurrow;

    /// <summary>Минимальное количество призываемых миньонов.</summary>
    public int SummonMinCount { get; set; } = EnemyConfig.BossSummonMinCount;

    /// <summary>Максимальное количество призываемых миньонов.</summary>
    public int SummonMaxCount { get; set; } = EnemyConfig.BossSummonMaxCount;

    /// <summary>Радиус, в пределах которого вокруг босса появляются миньоны.</summary>
    public float SummonSpawnRadius { get; set; } = EnemyConfig.BossSummonSpawnRadius;

    /// <summary>Шанс (от 0 до 1) того, что призванный миньон будет стрелком.</summary>
    public float SummonShooterChance { get; set; } = EnemyConfig.BossSummonShooterChance;

    /// <summary>Вес атаки призыва миньонов для системы случайного выбора (влияет на частоту применения).</summary>
    public float SummonAttackWeight { get; set; } = EnemyConfig.BossSummonAttackWeight;

    /// <summary>Указывает, неуязвим ли босс в данный момент (зависит от фазы нахождения под землей).</summary>
    public override bool IsInvulnerable => IsInvulnerableDuringBurrow && _burrowPhase != BurrowPhase.None;

    /// <summary>Указывает, может ли босс наносить контактный урон (отключено во время фазы закапывания).</summary>
    public override bool CanDealContactDamage => _burrowPhase == BurrowPhase.None;

    /// <summary>Инициализирует босса с полным набором параметров движения, здоровья и коллизий.</summary>
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

    /// <summary>Инициализирует босса со стандартными параметрами из конфигурации.</summary>
    public BossEnemy(Vector2 position, IWallCollisionChecker collision)
        : this(position, collision, EnemyConfig.BossMoveSpeed, EnemyConfig.BossMaxHealth, EnemyConfig.BossRadius)
    {
    }

    private Vector2 AttackOrigin => GetBossDrawAnchor();

    protected override void UpdateEnemy(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_burrowPhase != BurrowPhase.None)
        {
            UpdateBurrow(dt);
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

        Velocity = Vector2.Zero;
        _spriteRow = EnemyConfig.BossSheetIdleRow;
        UpdateBossSpriteAnimation(dt);
        StartAttack(ChooseNextAttack());
    }

    /// <inheritdoc />
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

            if (!TryGetBossSpriteLayout(sheet, out Vector2 anchor, out float scale, out int fw, out int fh))
                return;

            int row = Math.Clamp(_spriteRow, 0, EnemyConfig.BossSheetRowCount - 1);
            int frameCount = row == EnemyConfig.BossSheetFliesAttackIdleRow
                ? EnemyConfig.BossSheetFliesAttckFramesCount
                : EnemyConfig.BossSheetCommonFramesCount;
            int frame = Math.Clamp(_spriteFrame, 0, frameCount - 1);
            var src = new Rectangle(frame * fw, row * fh, fw, fh);
            bool flip = ChaseTarget.X < Position.X - 2f;
            var effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(
                sheet,
                anchor,
                src,
                Color.White,
                0f,
                new Vector2(fw / 2f, fh / 2f),
                scale,
                effects,
                0f);

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

    /// <summary>Возвращает текущий хитбокс босса, размер и положение которого зависят от активной фазы способности.</summary>
    public override Rectangle GetBounds()
    {
        Texture2D sheet = SharedBossTexture;
        if (sheet == null || !TryGetBossSpriteLayout(sheet, out Vector2 anchor, out float scale, out int fw, out int fh))
        {
            int r = (int)CollisionRadius;
            return new Rectangle((int)Position.X - r, (int)Position.Y - r, r * 2, r * 2);
        }

        float fullW = fw * scale;
        float fullH = fh * scale;
        float hitH = fullH * EnemyConfig.BossHitboxVisibleHeightFraction;
        float bottomY = anchor.Y + fullH * 0.5f;
        float topY = bottomY - hitH;
        int x = (int)MathF.Floor(anchor.X - fullW * 0.5f);
        int y = (int)MathF.Floor(topY);
        int w = Math.Max(1, (int)MathF.Ceiling(fullW));
        int h = Math.Max(1, (int)MathF.Ceiling(hitH));
        return new Rectangle(x, y, w, h);
    }

    private Vector2 GetBossDrawAnchor()
    {
        if (_burrowPhase == BurrowPhase.MovingTrail)
            return _burrowTrailPosition;
        return Position;
    }

    private static void GetBossCellPixelSize(Texture2D sheet, out int fw, out int fh)
    {
        fw = Math.Max(1, sheet.Width / Math.Max(1, EnemyConfig.BossSheetFramesCount));
        fh = Math.Max(1, sheet.Height / Math.Max(1, EnemyConfig.BossSheetRowCount));
    }

    private bool TryGetBossSpriteLayout(Texture2D sheet, out Vector2 anchor, out float scale, out int fw, out int fh)
    {
        anchor = default;
        scale = 0f;
        fw = fh = 0;
        if (sheet == null)
            return false;

        GetBossCellPixelSize(sheet, out fw, out fh);
        anchor = GetBossDrawAnchor();
        scale = ((CollisionRadius * 2.1f) / Math.Max(fw, fh)) * 2f;
        return true;
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
                _summonIntroRemaining = rotateDur;
                _attackTimer = GetSummonAttackDurationSeconds();
                _spriteRow = EnemyConfig.BossSheetAttackRow;
                _spriteFrame = 0;
                _spriteAnimTimer = 0f;
                break;
        }

        if (attack != BossAttackType.BurrowStrike)
            GameplayAudio.PlayBossAttack();
    }

    private static float GetSummonAttackDurationSeconds()
    {
        float intro = EnemyConfig.BossSheetCommonFramesCount * EnemyConfig.BossCommonAnimFrameDurationSeconds;
        float fliesLoop = EnemyConfig.BossSheetFliesAttckFramesCount * EnemyConfig.BossFlyIdleAnimFrameDurationSeconds;
        int loops = Math.Max(1, EnemyConfig.BossSummonFliesAnimLoopCount);
        return intro + loops * fliesLoop;
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
                _spriteRow = _summonSpawned
                    ? EnemyConfig.BossSheetFliesAttackIdleRow
                    : EnemyConfig.BossSheetAttackRow;
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
        else if (_burrowPhase == BurrowPhase.Emerging)
        {
            _spriteRow = EnemyConfig.BossSheetBurrowRow;
            frameCount = EnemyConfig.BossSheetCommonFramesCount;
            frameDur = EnemyConfig.BossCommonAnimFrameDurationSeconds;
        }

        _spriteAnimTimer += dt;

        if (_spriteAnimTimer >= frameDur)
        {
            _spriteAnimTimer = 0f;

            if (_burrowPhase == BurrowPhase.Emerging)
            {
                _spriteFrame--;

                if (_spriteFrame <= 0)
                {
                    _spriteFrame = 0;
                    _burrowPhase = BurrowPhase.None;
                    EndAttackAndEnterCooldown();
                }
            }
            else
            {
                _spriteFrame = (_spriteFrame + 1) % frameCount;
            }
        }
    }

    private void ExecuteSpikeBurst()
    {
        if (ProjectileSpawner == null)
            return;

        Vector2 origin = AttackOrigin;
        int count = Math.Max(8, SpikeBurstProjectileCount);
        float step = MathHelper.TwoPi / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * step;
            Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            ProjectileSpawner.Invoke(new ProjectileSpawnArgs
            {
                Position = origin + dir * SpikeBurstSpawnRadius,
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

        Vector2 origin = AttackOrigin;
        Vector2 toPlayer = ChaseTarget - origin;
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
                Position = origin + dir * (SpikeBurstSpawnRadius * 0.4f),
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

        Vector2 origin = AttackOrigin;
        int count = Math.Max(6, SpinningSpikeCount);
        float step = MathHelper.TwoPi / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * step;
            Vector2 outward = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            ProjectileSpawner.Invoke(new ProjectileSpawnArgs
            {
                Position = origin + outward * SpinningSpikeOrbitRadius,
                Direction = Vector2.Zero,
                Speed = SpinningSpikeReleaseSpeed,
                Damage = ContactDamage,
                LifetimeSeconds = SpinningSpikeOrbitDuration + 2f,
                Radius = SpikeBurstProjectileRadius,
                RecoilForce = 0f,
                IsFriendlyToPlayer = false,
                UseOrbitMotion = true,
                OrbitCenter = origin,
                OrbitCenterFollow = () => AttackOrigin,
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

        _spriteFrame = 0;
        _spriteAnimTimer = 0f;

        _burrowTrailPosition = AttackOrigin;
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
        if (_burrowPhase == BurrowPhase.MovingTrail)
        {
            Vector2 toTarget = ChaseTarget - _burrowTrailPosition;

            if (toTarget.LengthSquared() > 1f)
            {
                Vector2 dir = Vector2.Normalize(toTarget);
                _burrowTrailPosition = ResolveWallCollision(_burrowTrailPosition, dir * BurrowTrailSpeed * dt);
            }

            if (_attackTimer <= 0f)
            {
                Position = _burrowTrailPosition;
                _burrowPhase = BurrowPhase.Emerging;
                _spriteRow = EnemyConfig.BossSheetBurrowRow;
                _spriteFrame = EnemyConfig.BossSheetCommonFramesCount - 1;
                _spriteAnimTimer = 0f;
                GameplayAudio.PlayBossEmerge();
                TryBurrowStrikePlayer();
            }
            UpdateBossSpriteAnimation(dt);
            return;
        }
        if (_burrowPhase == BurrowPhase.Emerging)
        {
            UpdateBossSpriteAnimation(dt);
        }
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

        _spriteRow = EnemyConfig.BossSheetFliesAttackIdleRow;
        _spriteFrame = 0;
        _spriteAnimTimer = 0f;

        int summonCount = _rng.Next(SummonMinCount, SummonMaxCount + 1);
        summonCount = Math.Clamp(summonCount, SummonMinCount, SummonMaxCount);
        for (int i = 0; i < summonCount; i++)
        {
            float angle = (float)_rng.NextDouble() * MathHelper.TwoPi;
            float radius = NextRange(SummonSpawnRadius * 0.55f, SummonSpawnRadius);
            Vector2 spawnPos = AttackOrigin + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
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
        GameplayAudio.PlayBossEmerge();
        GameplayAudio.PlayMapBossEntry();
    }
}