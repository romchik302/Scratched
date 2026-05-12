using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Enemies;

internal class AdaptiveChasingEnemy : ChasingEnemy
{
    protected override float AnimFrameDuration =>
        EnemyConfig.AdaptiveChasingAnimationSpeed;
    private bool _isPlayerCurrentlyInRadius;

    private readonly float _initialChaseRadius;
    private readonly float _expandedChaseRadius;
    private bool _hasPlayerEnteredRadius = false;
    private float _currentChaseRadius;

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

    private void CheckPlayerEnteredRadius(float distanceToPlayer)
    {
        if (distanceToPlayer <= _currentChaseRadius && !_hasPlayerEnteredRadius)
        {
            _hasPlayerEnteredRadius = true;
            _currentChaseRadius = _expandedChaseRadius;
        }
    }


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

    public void ResetRadiusState()
    {
        _hasPlayerEnteredRadius = false;
        _currentChaseRadius = _initialChaseRadius;
    }

    public bool HasPlayerEnteredRadius => _hasPlayerEnteredRadius;
    public float CurrentChaseRadius => _currentChaseRadius;

    protected override float? GetDebugAggroRadius()
    {
        return _currentChaseRadius;
    }

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
}