using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>
/// Враг, который увеличивает радиус преследования (агрессии) 
/// при обнаружении игрока или получении урона.
/// </summary>
internal class AdaptiveChasingEnemy : ChasingEnemy
{
    /// <inheritdoc />
    protected override float AnimFrameDuration =>
        EnemyConfig.AdaptiveChasingAnimationSpeed;
    private bool _isPlayerCurrentlyInRadius;

    private readonly float _initialChaseRadius;
    private readonly float _expandedChaseRadius;
    private bool _hasPlayerEnteredRadius = false;
    private float _currentChaseRadius;

    /// <summary>
    /// Инициализирует новый экземпляр адаптивного врага с заданными параметрами.
    /// </summary>
    /// <param name="position">Начальная позиция врага.</param>
    /// <param name="collision">Сервис для проверки столкновений со стенами.</param>
    /// <param name="moveSpeed">Скорость перемещения врага.</param>
    /// <param name="maxHealth">Максимальное количество здоровья.</param>
    /// <param name="collisionRadius">Радиус физической коллизии врага.</param>
    /// <param name="initialChaseRadius">Начальный (спокойный) радиус обнаружения игрока.</param>
    /// <param name="expandedChaseRadius">Расширенный (агрессивный) радиус преследования.</param>
    public AdaptiveChasingEnemy(
        Vector2 position,
        IWallCollisionChecker collision,
        float moveSpeed,
        int maxHealth,
        float collisionRadius,
        float initialChaseRadius,
        float expandedChaseRadius)
        : base(position, collision, moveSpeed, maxHealth, collisionRadius)
    {
        _initialChaseRadius = initialChaseRadius;
        _expandedChaseRadius = expandedChaseRadius;
        _currentChaseRadius = initialChaseRadius;
        RecoilResistance = 0.8f;  // Тяжело отскакивает (80% сопротивление)
    }

    /// <summary>
    /// Инициализирует новый экземпляр адаптивного врага, используя стандартные настройки из конфига.
    /// </summary>
    /// <param name="position">Начальная позиция врага.</param>
    /// <param name="collision">Сервис для проверки столкновений со стенами.</param>
    public AdaptiveChasingEnemy(Vector2 position, IWallCollisionChecker collision)
        : this(
            position,
            collision,
            EnemyConfig.AdaptiveChasingMoveSpeed,
            EnemyConfig.AdaptiveChasingMaxHealth,
            EnemyConfig.AdaptiveChasingRadius,
            EnemyConfig.AdaptiveChasingInitialRadius,
            EnemyConfig.AdaptiveChasingExpandedRadius)
    {
    }

    /// <inheritdoc />
    protected override void UpdateEnemy(GameTime gameTime)
    {
        EnsureSpriteConfigured();
        UpdateAnimation(gameTime);

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float distanceToPlayer = Vector2.Distance(Position, ChaseTarget);

        _isPlayerCurrentlyInRadius =
            distanceToPlayer <= _currentChaseRadius;

        CheckPlayerEnteredRadius(distanceToPlayer);

        if (Health < MaxHealth && !_hasPlayerEnteredRadius)
        {
            _hasPlayerEnteredRadius = true;
            _currentChaseRadius = _expandedChaseRadius;
        }

        if (distanceToPlayer > _currentChaseRadius)
        {
            Velocity = Vector2.Zero;
            return;
        }

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


    /// <summary>
    /// Проверяет дистанцию до игрока и расширяет радиус преследования, если игрок оказался слишком близко.
    /// </summary>
    /// <param name="distanceToPlayer">Текущая дистанция между врагом и игроком.</param>
    private void CheckPlayerEnteredRadius(float distanceToPlayer)
    {
        if (distanceToPlayer <= _currentChaseRadius && !_hasPlayerEnteredRadius)
        {
            _hasPlayerEnteredRadius = true;
            _currentChaseRadius = _expandedChaseRadius;
        }
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || !IsActivated || spriteBatch == null)
            return;

        EnsureSpriteConfigured();

        if (_spriteSheet != null)
        {
            int currentRow = _isPlayerCurrentlyInRadius ? 0 : 1;

            _sourceRect = new Rectangle(
                _frameIndex * _frameWidth,
                currentRow * _frameHeight,
                _frameWidth,
                _frameHeight
            );

            spriteBatch.Draw(
                _spriteSheet,
                Position,
                _sourceRect,
                Color.White,
                0f,
                new Vector2(_frameWidth / 2f, _frameHeight / 4f),
                1f,
                GetHorizontalSpriteEffect(),
                0f
            );

            DrawDebugOverlay(spriteBatch);
            return;
        }

        // Fallback при отсутствии текстуры
        if (_pixel == null)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        var rect = GetBounds();
        Color enemyColor = _hasPlayerEnteredRadius
            ? new Color(255, 30, 30, 230)
            : new Color(200, 80, 80, 230);

        spriteBatch.Draw(_pixel, rect, enemyColor);
        DrawDebugOverlay(spriteBatch);
    }


    /// <summary>
    /// Сбрасывает состояние агрессии врага, возвращая его к изначальному (спокойному) радиусу поиска.
    /// </summary>
    public void ResetRadiusState()
    {
        _hasPlayerEnteredRadius = false;
        _currentChaseRadius = _initialChaseRadius;
    }

    /// <summary>
    /// Указывает, вошел ли игрок в радиус агрессии врага хотя бы один раз.
    /// </summary>
    public bool HasPlayerEnteredRadius => _hasPlayerEnteredRadius;
    /// <summary>
    /// Текущий радиус преследования врага.
    /// </summary>
    public float CurrentChaseRadius => _currentChaseRadius;

    /// <inheritdoc />
    protected override float? GetDebugAggroRadius()
    {
        return _currentChaseRadius;
    }

    /// <summary>
    /// Гарантирует создание отладочной текстуры только один раз за сессию игры.
    /// </summary>
    protected override void EnsureSpriteConfigured()
    {
        if (_spriteSheet != null)
            return;

        _spriteSheet = SharedAdaptiveTexture ?? SharedChasingTexture;
        if (_spriteSheet == null)
            return;

        _frameCount = EnemyConfig.AdaptiveChasingFrameCount;

        _frameWidth = _spriteSheet.Width / _frameCount;

        _frameHeight =
            _spriteSheet.Height / EnemyConfig.AdaptiveChasingAnimationRows;

        _sourceRect = new Rectangle(0, 0, _frameWidth, _frameHeight);
    }

    /// <inheritdoc />
    protected override float AmbientSoundInterval => 0.8f;

    /// <inheritdoc />
    protected override void PlayAmbientSound()
    {
        if(_hasPlayerEnteredRadius) GameplayAudio.PlayEnemyTreant();
    }

}