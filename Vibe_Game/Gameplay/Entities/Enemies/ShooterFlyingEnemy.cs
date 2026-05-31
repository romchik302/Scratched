using System;
using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Weapons;

namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>
/// Летающий стреляющий враг: преследует цель в воздухе, а при сближении на расстояние 
/// радиуса агрессии останавливается и атакует игрока снарядами с заданной периодичностью.
/// </summary>
public sealed class ShooterFlyingEnemy : FlyingEnemy
{
    private float _shotCooldownLeft;
    /// <summary>
    /// Делегат (колбэк) для создания, настройки и спавна вражеских снарядов в игровом мире.
    /// </summary>
    public Action<ProjectileSpawnArgs> ProjectileSpawner { get; set; }

    public float AggroRadius { get; set; } = EnemyConfig.ShooterAggroRadius;
    public float ShotIntervalSeconds { get; set; } = EnemyConfig.ShooterShotIntervalSeconds;
    /// <summary>Задержка перед первым выстрелом при повторном переходе в режим атаки (входе в радиус агрессии).</summary>
    public float ReentryShotCooldownSeconds { get; set; } = EnemyConfig.ShooterReentryShotCooldownSeconds;
    public float ShotSpeed { get; set; } = EnemyConfig.ShooterProjectileSpeed;
    /// <summary>Время жизни выпущенного снаряда в секундах до его автоматического деспавна.</summary>
    public float ShotLifetimeSeconds { get; set; } = EnemyConfig.ShooterProjectileLifetime;
    public float ShotRadius { get; set; } = EnemyConfig.ShooterProjectileRadius;
    public float ShotRecoilForce { get; set; } = EnemyConfig.ShooterProjectileRecoilForce;
    public int ShotDamage { get; set; } = EnemyConfig.ShooterProjectileDamage;

    /// <summary>
    /// Инициализирует новый экземпляр летающего стреляющего врага с явным указанием всех характеристик.
    /// </summary>
    /// <param name="position">Начальная позиция спавна врага в игровом мире.</param>
    /// <param name="collision">Сервис проверки коллизий для летающих сущностей.</param>
    /// <param name="moveSpeed">Базовая скорость перемещения врага.</param>
    /// <param name="maxHealth">Максимальный запас очков здоровья.</param>
    /// <param name="collisionRadius">Радиус физического хитбокса врага.</param>
    public ShooterFlyingEnemy(
        Vector2 position,
        IFlyingCollisionChecker collision,
        float moveSpeed,
        int maxHealth,
        float collisionRadius)
        : base(position, collision, moveSpeed, maxHealth, collisionRadius)
    {
    }

    /// <summary>
    /// Удобный конструктор, использующий базовые конфигурационные настройки стрелка из конфигурации <see cref="EnemyConfig"/>.
    /// </summary>
    /// <param name="position">Начальная позиция спавна врага в игровом мире.</param>
    /// <param name="collision">Сервис проверки коллизий для летающих сущностей.</param>
    public ShooterFlyingEnemy(Vector2 position, IFlyingCollisionChecker collision)
        : this(
            position,
            collision,
            EnemyConfig.ShooterMoveSpeed,
            EnemyConfig.ShooterMaxHealth,
            EnemyConfig.ShooterRadius)
    {
        EnsureSpriteConfigured();
    }

    /// <inheritdoc />
    protected override void UpdateEnemy(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float distanceToPlayer = Vector2.Distance(Position, ChaseTarget);
        bool isInsideAggro = distanceToPlayer <= AggroRadius;

        if (isInsideAggro)
        {
            // Прокручиваем базовую анимацию, но без движения.
            Vector2 cachedTarget = ChaseTarget;
            ChaseTarget = Position;
            base.UpdateEnemy(gameTime);
            ChaseTarget = cachedTarget;

            UpdateAttackMode(dt);
            return;
        }

        base.UpdateEnemy(gameTime);
    }

    /// <summary>
    /// Обновляет логику поведения врага в режиме атаки: обнуляет скорость, сбрасывает 
    /// случайные флуктуации ИИ, поворачивает врага к игроку и отсчитывает таймер перезарядки.
    /// </summary>
    /// <param name="dt">Время, прошедшее с предыдущего кадра, в секундах (DeltaTime).</param>
    private void UpdateAttackMode(float dt)
    {
        Vector2 toPlayer = ChaseTarget - Position;
        UpdateFacingFromDirection(toPlayer, allowVertical: false);

        Velocity = Vector2.Zero;
        ResetRandomMovementBehavior();
        _shotCooldownLeft -= dt;
        if (_shotCooldownLeft <= 0f)
        {
            TryShootAtPlayer();
            _shotCooldownLeft = MathF.Max(
                0.05f,
                MathF.Max(ShotIntervalSeconds, ReentryShotCooldownSeconds)
            );
        }
    }

    private void TryShootAtPlayer()
    {
        if (ProjectileSpawner == null)
            return;

        Vector2 toPlayer = ChaseTarget - Position;
        if (toPlayer.LengthSquared() < 0.0001f)
            return;

        Vector2 direction = Vector2.Normalize(toPlayer);
        ProjectileSpawner.Invoke(new ProjectileSpawnArgs
        {
            Position = Position,
            Direction = direction,
            Speed = ShotSpeed,
            Damage = ShotDamage,
            LifetimeSeconds = ShotLifetimeSeconds,
            Radius = ShotRadius,
            RecoilForce = ShotRecoilForce,
            IsFriendlyToPlayer = false
        });
    }

    /// <inheritdoc />
    protected override float? GetDebugAggroRadius()
    {
        return AggroRadius;
    }
}
