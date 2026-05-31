using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>Летающий враг: преследует цель, игнорирует внутренние стены, использует проверку коллизий периметра и скользит вдоль препятствий.</summary>
public class FlyingEnemy : Enemy
{
    private readonly IFlyingCollisionChecker _collision;
    private readonly float _collisionRadius;
    private readonly float _moveSpeed;

    private Texture2D _pixel;
    private Texture2D _spriteSheet;
    private Rectangle _sourceRect;
    private int _frameWidth;
    private int _frameHeight;
    private int _frameCount;
    private int _frameIndex;
    private float _animTimer;
    private const float AnimFrameDuration = 0.07f;

    /// <summary>Цель преследования в мировых координатах (обычно позиция игрока), обновлять каждый кадр из сцены.</summary>
    public Vector2 ChaseTarget { get; set; }

    /// <summary>
    /// Инициализирует летающего врага с указанием логики коллизий для полета, скорости движения, здоровья и радиуса хитбокса.
    /// </summary>
    /// <param name="position">Начальная позиция врага в игровом мире.</param>
    /// <param name="collision">Сервис проверки коллизий, специфичный для летающих сущностей.</param>
    /// <param name="moveSpeed">Базовая скорость движения врага.</param>
    /// <param name="maxHealth">Максимальное (и начальное) количество здоровья.</param>
    /// <param name="collisionRadius">Радиус физического хитбокса врага.</param>
    /// <exception cref="ArgumentNullException">Бросается, если компонент <paramref name="collision"/> равен null.</exception>
    public FlyingEnemy(
        Vector2 position,
        IFlyingCollisionChecker collision,
        float moveSpeed,
        int maxHealth,
        float collisionRadius)
        : base(position, maxHealth)
    {
        _collision = collision ?? throw new System.ArgumentNullException(nameof(collision));
        _moveSpeed = moveSpeed;
        _collisionRadius = collisionRadius;
        Color = Color.White;
        RecoilResistance = 0.01f;  // Легко отскакивает (10% сопротивление)
    }

    /// <summary>
    /// Удобный конструктор, использующий базовые настройки из конфигурации <see cref="EnemyConfig"/>.
    /// </summary>
    /// <param name="position">Начальная позиция врага в игровом мире.</param>
    /// <param name="collision">Сервис проверки коллизий для летающих сущностей.</param>
    public FlyingEnemy(Vector2 position, IFlyingCollisionChecker collision)
        : this(
            position,
            collision,
            EnemyConfig.DefaultFlyingMoveSpeed,
            EnemyConfig.DefaultFlyingMaxHealth,
            EnemyConfig.DefaultFlyingRadius)
    {
        EnsureSpriteConfigured();
    }

    /// <summary>
    /// Пытается сдвинуть врага на дельту, разрешая скольжение по одной оси при блокировке по другой.
    /// </summary>
    /// <param name="oldPos">Исходная позиция до попытки перемещения.</param>
    /// <param name="delta">Планируемый вектор смещения за текущий кадр.</param>
    /// <returns>Новая скорректированная позиция врага с учетом скольжения вдоль стен.</returns>
    protected override Vector2 ResolveRecoilCollision(Vector2 oldPos, Vector2 newPos)
    {
        Vector2 delta = newPos - oldPos;
        return ResolveFlyingSlide(oldPos, delta);
    }

    /// <inheritdoc />
    protected override float GetCollisionRadius()
    {
        return _collisionRadius;
    }

    /// <inheritdoc />
    protected override void UpdateEnemy(GameTime gameTime)
    {
        EnsureSpriteConfigured();
        UpdateAnimation(gameTime);

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 toTarget = ChaseTarget - Position;
        if (toTarget.LengthSquared() < 2f)
        {
            Velocity = Vector2.Zero;
            return;
        }

        Vector2 moveDirection = GetMovementDirectionWithRandomBehavior(toTarget, dt, out float randomSpeedMultiplier);
        UpdateFacingFromDirection(moveDirection == Vector2.Zero ? toTarget : moveDirection, allowVertical: false);
        Vector2 delta = moveDirection * (_moveSpeed * randomSpeedMultiplier * dt);

        Position = ResolveFlyingSlide(Position, delta);
        Velocity = Vector2.Zero;
    }

    /// <summary>Пытается сдвинуть врага на delta, разрешая скольжение по одной оси при блокировке по другой.</summary>
    private Vector2 ResolveFlyingSlide(Vector2 oldPos, Vector2 delta)
    {
        Vector2 target = oldPos + delta;
        Vector2 final = target;

        if (delta.X != 0f && HasFlyingBodyCollisionAt(new Vector2(target.X, oldPos.Y)))
            final.X = oldPos.X;

        if (delta.Y != 0f && HasFlyingBodyCollisionAt(new Vector2(final.X, target.Y)))
            final.Y = oldPos.Y;

        return final;
    }

    /// <summary>Проверяет, заблокирована ли указанная точка для полета (проверка четырех углов хитбокса).</summary>
    private bool HasFlyingBodyCollisionAt(Vector2 centerWorld)
    {
        float o = _collisionRadius;
        return _collision.IsFlyingBlocked(new Vector2(centerWorld.X - o, centerWorld.Y - o))
            || _collision.IsFlyingBlocked(new Vector2(centerWorld.X + o, centerWorld.Y - o))
            || _collision.IsFlyingBlocked(new Vector2(centerWorld.X - o, centerWorld.Y + o))
            || _collision.IsFlyingBlocked(new Vector2(centerWorld.X + o, centerWorld.Y + o));
    }

    /// <inheritdoc />
    public override Rectangle GetBounds()
    {
        int r = (int)_collisionRadius;
        int d = r * 2;
        return new Rectangle((int)Position.X - r, (int)Position.Y - r, d, d);
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || !IsActivated || spriteBatch == null)
            return;

        if (_spriteSheet != null)
        {
            spriteBatch.Draw(
                _spriteSheet,
                Position,
                _sourceRect,
                Color.White,
                0f,
                new Vector2(_frameWidth / 2f, _frameHeight / 2f),
                1f,
                GetHorizontalSpriteEffect(),
                0f
            );
            DrawDebugOverlay(spriteBatch);
            return;
        }

        if (_pixel == null)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        var rect = GetBounds();
        spriteBatch.Draw(_pixel, rect, new Color(200, 80, 200, 230));
        DrawDebugOverlay(spriteBatch);
    }

    /// <inheritdoc />
    protected override void EnsureSpriteConfigured()
    {
        if (_spriteSheet != null)
            return;

        _spriteSheet = SharedFlyingTexture;
        if (_spriteSheet == null)
            return;

        _frameHeight = _spriteSheet.Height / 2;
        _frameWidth = _frameHeight > 0 ? _frameHeight : _spriteSheet.Width / 2;
        _frameWidth = Math.Clamp(_frameWidth, 1, _spriteSheet.Width);
        _frameCount = Math.Max(1, _spriteSheet.Width / _frameWidth);
        _sourceRect = new Rectangle(0, 0, _frameWidth, _frameHeight);
    }

    /// <inheritdoc />
    protected override void UpdateAnimation(GameTime gameTime)
    {
        if (_spriteSheet == null)
            return;

        _animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_animTimer < AnimFrameDuration)
            return;

        _animTimer = 0f;
        _frameIndex = (_frameIndex + 1) % _frameCount;
        _sourceRect.X = _frameIndex * _frameWidth;
    }

    /// <summary>Срабатывает при активации врага, воспроизводя звук полета.</summary>
    protected override void OnActivated()
    {
        if (!ActivationSkippedDelay)
            GameplayAudio.PlayEnemyFly();
    }

    /// <inheritdoc />
    protected override float AmbientSoundInterval => 1.2f;

    /// <inheritdoc />
    protected override void PlayAmbientSound()
    {
        GameplayAudio.PlayEnemyFly();
    }
}