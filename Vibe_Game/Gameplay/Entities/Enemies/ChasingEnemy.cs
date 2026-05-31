using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>Враг, который преследует цель игрока, использует навигацию с учётом стен, поддерживает хитбоксы тела и атаки, а также анимацию спрайтов и систему обновления поведения.</summary>
public class ChasingEnemy : Enemy
{
    private readonly IWallCollisionChecker _collision;

    protected readonly float _collisionRadius;
    protected readonly float _moveSpeed;

    protected float _bodyRadius;
    protected float _attackRadius;

    protected Texture2D _pixel;
    protected Texture2D _spriteSheet;
    protected Rectangle _sourceRect;

    protected int _frameWidth;
    protected int _frameHeight;
    protected int _frameCount;
    protected int _frameIndex;

    protected float _animTimer;

    /// <summary>Продолжительность одного кадра анимации в секундах.</summary>
    protected virtual float AnimFrameDuration => 0.2f;

    /// <summary>Целевая позиция (обычно позиция игрока), к которой движется враг.</summary>
    public Vector2 ChaseTarget { get; set; }

    /// <summary>Множитель масштаба для физического хитбокса тела врага.</summary>
    public float BodyHitboxScale { get; set; } = 0.65f;

    /// <summary>Множитель масштаба для хитбокса атаки врага.</summary>
    public float AttackHitboxScale { get; set; } = 0.82f;

    /// <summary>Коэффициент смещения хитбокса по оси X.</summary>
    public float HitboxOffsetXFactor { get; set; } = 0f;

    /// <summary>Смещение хитбокса по оси Y в пикселях (для выравнивания относительно спрайта).</summary>
    public float HitboxOffsetYPixels { get; set; } = 0f;

    /// <summary>Инициализирует преследующего врага с полным набором параметров.</summary>
    public ChasingEnemy(
        Vector2 position,
        IWallCollisionChecker collision,
        float moveSpeed,
        int maxHealth,
        float collisionRadius)
        : base(position, maxHealth)
    {
        _collision = collision ?? throw new ArgumentNullException(nameof(collision));
        _moveSpeed = moveSpeed;
        _collisionRadius = collisionRadius;

        _bodyRadius = collisionRadius * BodyHitboxScale;
        _attackRadius = collisionRadius * AttackHitboxScale;

        Color = Color.White;
        RecoilResistance = 0.7f;
    }

    /// <summary>Инициализирует преследующего врага со стандартными параметрами из конфигурации.</summary>
    public ChasingEnemy(Vector2 position, IWallCollisionChecker collision)
        : this(
            position,
            collision,
            EnemyConfig.DefaultChasingMoveSpeed,
            EnemyConfig.DefaultChasingMaxHealth,
            EnemyConfig.DefaultChasingRadius)
    {
        EnsureSpriteConfigured();
    }

    /// <summary>Обрабатывает столкновения со стенами при отбрасывании врага.</summary>
    protected override Vector2 ResolveRecoilCollision(Vector2 oldPos, Vector2 newPos)
    {
        Vector2 delta = newPos - oldPos;
        return ResolveWallCollision(oldPos, delta);
    }

    /// <summary>Возвращает радиус физического хитбокса для обработки столкновений.</summary>
    protected override float GetCollisionRadius()
    {
        return _bodyRadius;
    }

    /// <summary>Обновляет логику поведения врага: расчет направления, движение к цели и обработку столкновений.</summary>
    protected override void UpdateEnemy(GameTime gameTime)
    {
        RefreshHitboxParameters();
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

        Position = ResolveWallCollision(Position, delta);
        Velocity = Vector2.Zero;
    }

    /// <summary>Корректирует позицию врага, предотвращая прохождение сквозь стены.</summary>
    protected Vector2 ResolveWallCollision(Vector2 oldPos, Vector2 delta)
    {
        Vector2 target = oldPos + delta;
        Vector2 final = target;

        Vector2 bodyCenter = GetBodyCenter(target);

        if (delta.X != 0f && HasWallCollisionAt(new Vector2(bodyCenter.X, oldPos.Y)))
            final.X = oldPos.X;

        bodyCenter = GetBodyCenter(new Vector2(final.X, target.Y));

        if (delta.Y != 0f && HasWallCollisionAt(new Vector2(final.X, bodyCenter.Y)))
            final.Y = oldPos.Y;

        return final;
    }

    /// <summary>Проверяет наличие столкновения со стеной в заданных мировых координатах для всех четырех углов хитбокса.</summary>
    private bool HasWallCollisionAt(Vector2 centerWorld)
    {
        float o = _bodyRadius;

        return _collision.IsPointBlockedByWall(new Vector2(centerWorld.X - o, centerWorld.Y - o))
            || _collision.IsPointBlockedByWall(new Vector2(centerWorld.X + o, centerWorld.Y - o))
            || _collision.IsPointBlockedByWall(new Vector2(centerWorld.X - o, centerWorld.Y + o))
            || _collision.IsPointBlockedByWall(new Vector2(centerWorld.X + o, centerWorld.Y + o));
    }

    /// <summary>Вычисляет центр тела врага с учетом вертикального смещения для указанной позиции.</summary>
    protected Vector2 GetBodyCenter(Vector2 basePos)
    {
        return basePos + new Vector2(0, HitboxOffsetYPixels);
    }

    /// <summary>Вычисляет центр тела врага с учетом вертикального смещения для текущей позиции.</summary>
    protected Vector2 GetBodyCenter()
    {
        return GetBodyCenter(Position);
    }

    /// <inheritdoc />
    public override Rectangle GetBounds()
    {
        var center = GetBodyCenter();

        int r = (int)_bodyRadius;
        int d = r * 2;

        return new Rectangle((int)center.X - r, (int)center.Y - r, d, d);
    }

    /// <summary>Возвращает область нанесения контактного урона. Может отличаться от физического хитбокса врага.</summary>
    public Rectangle GetAttackBounds()
    {
        var center = GetBodyCenter();

        int r = (int)_attackRadius;
        int d = r * 2;

        return new Rectangle((int)center.X - r, (int)center.Y - r, d, d);
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || !IsActivated || spriteBatch == null)
            return;

        if (_spriteSheet != null)
        {
            float headX = 52f;

            var origin = Facing == FacingDirection.Right
                ? new Vector2(headX, _frameHeight / 2f)
                : new Vector2(_frameWidth - headX, _frameHeight / 2f);

            spriteBatch.Draw(
                _spriteSheet,
                Position,
                _sourceRect,
                Color.White,
                0f,
                origin,
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
        spriteBatch.Draw(_pixel, rect, new Color(255, 50, 50, 230));
        DrawDebugOverlay(spriteBatch);
    }

    /// <summary>Возвращает хитбокс атаки для отрисовки в режиме отладки.</summary>
    protected override Rectangle? GetDebugAttackBounds()
    {
        return GetAttackBounds();
    }

    /// <summary>Инициализирует спрайт-лист и настраивает параметры кадров анимации, если они еще не загружены.</summary>
    protected override void EnsureSpriteConfigured()
    {
        if (_spriteSheet != null)
            return;

        _spriteSheet = SharedChasingTexture;
        if (_spriteSheet == null)
            return;

        _frameHeight = _spriteSheet.Height;

        _frameCount = 4;
        _frameWidth = _spriteSheet.Width / _frameCount;

        _sourceRect = new Rectangle(0, 0, _frameWidth, _frameHeight);
    }

    /// <summary>Обновляет таймер анимации и переключает кадры спрайта.</summary>
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

    /// <summary>Срабатывает при активации врага в комнате, воспроизводя звук появления.</summary>
    protected override void OnActivated()
    {
        if (!ActivationSkippedDelay)
            GameplayAudio.PlayEnemySlime();
    }

    /// <summary>Интервал времени между воспроизведением фоновых звуков перемещения врага.</summary>
    protected override float AmbientSoundInterval => 1.15f;

    /// <summary>Воспроизводит фоновый звук перемещения врага.</summary>
    protected override void PlayAmbientSound()
    {
        GameplayAudio.PlayEnemySlime();
    }

    /// <summary>Пересчитывает радиусы хитбоксов тела и атаки на основе базового радиуса и множителей.</summary>
    private void RefreshHitboxParameters()
    {
        _bodyRadius = _collisionRadius * BodyHitboxScale;
        _attackRadius = _collisionRadius * AttackHitboxScale;
    }
}