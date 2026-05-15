using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Vibe_Game.Gameplay.Entities;
using Vibe_Game.Core.Settings;
using Vibe_Game.Core.Utilities;

namespace Vibe_Game.Gameplay.Projectiles;

public sealed class Projectile : Entity
{
    public Vector2 Direction { get; private set; }
    public float Speed { get; }
    public float Damage { get; }
    public float LifeLeft { get; private set; }
    public float Radius { get; private set; }
    public float RecoilForce { get; }
    public bool IsFriendlyToPlayer { get; }
    public bool IsOrbiting { get; private set; }
    public Vector2 OrbitCenter { get; private set; }
    public float OrbitRadius { get; private set; }
    public float OrbitAngle { get; private set; }
    public float OrbitAngularSpeed { get; private set; }
    public float OrbitTimeLeft { get; private set; }
    public bool ReleaseAfterOrbit { get; private set; }
    public Vector2 ReleaseDirection { get; private set; }
    public bool IgnoreWallCollisions { get; private set; }
    public float Length { get; private set; }
    public bool CanDealDamage => _canDealDamage;

    private Texture2D _texture;
    private int _currentFrame;
    private float _animationTimer;
    private bool _isImpacting;
    private float _rotation;
    private bool _canDealDamage = true;
    private Func<Vector2> _orbitCenterFollow;

    private readonly string _hostileTextureOverride;
    private int _sheetFrameColumns = WeaponConfig.ProjectileFrameCount;
    private float _drawSize = WeaponConfig.ProjectileSize;

    public Projectile(
        Vector2 position,
        Vector2 direction,
        float speed,
        float damage,
        float lifetimeSeconds,
        float radius,
        float recoilForce = 0f,
        bool isFriendlyToPlayer = true,
        bool ignoreWallCollisions = false,
        float length = 0f,
        string hostileTextureOverride = null,
        int hostileTextureFrameColumns = 0,
        float hostileProjectileDrawSize = 0f)
    {
        Position = position;
        Direction = direction.LengthSquared() > 0.0001f ? Vector2.Normalize(direction) : Vector2.UnitX;
        Speed = speed;
        Damage = damage;
        LifeLeft = lifetimeSeconds;
        Velocity = Direction * speed;
        Radius = radius;
        RecoilForce = recoilForce;
        IsFriendlyToPlayer = isFriendlyToPlayer;
        IgnoreWallCollisions = ignoreWallCollisions;
        Length = length;
        _hostileTextureOverride = hostileTextureOverride;

        if (!isFriendlyToPlayer && hostileTextureFrameColumns > 0)
            _sheetFrameColumns = hostileTextureFrameColumns;
        if (!isFriendlyToPlayer && hostileProjectileDrawSize > 0.01f)
            _drawSize = hostileProjectileDrawSize;

        _rotation = MathF.Atan2(Direction.Y, Direction.X);
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);

        if (IsFriendlyToPlayer)
        {
            _texture = content.Load<Texture2D>(WeaponConfig.PlayerProjectileTexture);
            _sheetFrameColumns = WeaponConfig.ProjectileFrameCount;
            _drawSize = WeaponConfig.ProjectileSize;
        }
        else
        {
            if (!string.IsNullOrEmpty(_hostileTextureOverride))
            {
                try
                {
                    _texture = content.Load<Texture2D>(_hostileTextureOverride);
                }
                catch
                {
                    _texture = null;
                }
            }

            if (_texture == null)
            {
                _texture = content.Load<Texture2D>(WeaponConfig.EnemyProjectileTexture);
                _sheetFrameColumns = WeaponConfig.ProjectileFrameCount;
                _drawSize = WeaponConfig.ProjectileSize;
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        UpdateAnimation(dt);

        if (!_isImpacting)
        {
            if (IsOrbiting)
            {
                if (_orbitCenterFollow != null)
                {
                    Vector2 trackedCenter = _orbitCenterFollow();
                    Vector2 delta = trackedCenter - OrbitCenter;
                    if (delta.LengthSquared() > 0f)
                    {
                        OrbitCenter = trackedCenter;
                        Position += delta;
                    }
                }

                OrbitAngle += OrbitAngularSpeed * dt;
                Position = OrbitCenter + new Vector2(MathF.Cos(OrbitAngle), MathF.Sin(OrbitAngle)) * OrbitRadius;
                OrbitTimeLeft -= dt;

                if (OrbitTimeLeft <= 0f)
                {
                    IsOrbiting = false;
                    if (ReleaseAfterOrbit)
                    {
                        Vector2 releaseDir = ReleaseDirection;
                        if (releaseDir.LengthSquared() < 0.0001f)
                            releaseDir = new Vector2(MathF.Cos(OrbitAngle), MathF.Sin(OrbitAngle));

                        ReleaseDirection = Vector2.Normalize(releaseDir);
                        Direction = ReleaseDirection;
                        Velocity = Direction * Speed;
                    }
                    else
                    {
                        IsAlive = false;
                    }
                }
            }
            else
            {
                base.Update(gameTime);
            }

            RefreshSpriteOrientation();

            LifeLeft -= dt;
            if (LifeLeft <= 0f)
                StartImpactAnimation();
        }
    }

    /// <summary>Поворот спрайта по фактическому направлению движения (в т.ч. орбита и полёт после выпуска).</summary>
    private void RefreshSpriteOrientation()
    {
        if (IsOrbiting)
        {
            Vector2 tangent = new Vector2(-MathF.Sin(OrbitAngle), MathF.Cos(OrbitAngle));
            if (OrbitAngularSpeed < 0f)
                tangent = -tangent;
            if (tangent.LengthSquared() > 0.0001f)
            {
                tangent.Normalize();
                _rotation = MathF.Atan2(tangent.Y, tangent.X);
            }
        }
        else if (Velocity.LengthSquared() > 0.0001f)
        {
            _rotation = MathF.Atan2(Velocity.Y, Velocity.X);
        }
    }

    private void UpdateAnimation(float deltaTime)
    {
        _animationTimer += deltaTime;

        if (_animationTimer < WeaponConfig.ProjectileAnimationSpeed)
            return;

        if (_isImpacting)
        {
            int maxImpactFrame = Math.Max(0, _sheetFrameColumns - 1);
            if (_currentFrame < maxImpactFrame)
                _currentFrame++;
            else
                IsAlive = false;
        }
        else
        {
            _currentFrame = 0;
        }

        _animationTimer = 0f;
    }

    public void StartImpactAnimation()
    {
        if (!_isImpacting)
        {
            if (IsFriendlyToPlayer)
                _rotation = Random.Shared.NextSingle() * 2 * MathF.PI;

            _isImpacting = true;
            _canDealDamage = false;
            Velocity = Vector2.Zero;
            _currentFrame = Math.Min(1, Math.Max(0, _sheetFrameColumns - 1));
            _animationTimer = 0f;
        }
    }

    public override Rectangle GetBounds()
    {
        int r = (int)Radius;
        return new Rectangle(
            (int)Position.X - r,
            (int)Position.Y - r,
            r * 2,
            r * 2
        );
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (_texture != null)
        {
            int cols = Math.Max(1, _sheetFrameColumns);
            int frameWidth = Math.Max(1, _texture.Width / cols);
            Rectangle sourceRect = new Rectangle(
                _currentFrame * frameWidth,
                0,
                frameWidth,
                _texture.Height
            );

            float scale = _drawSize / frameWidth;

            spriteBatch.Draw(_texture, Position, sourceRect, Color.White,
                _rotation, new Vector2(frameWidth / 2f, _texture.Height / 2f),
                scale, SpriteEffects.None, 0f);
        }
        else
        {
            Texture2D pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
            if (pixel != null)
            {
                spriteBatch.Draw(pixel, new Rectangle((int)(Position.X - Radius), (int)(Position.Y - Radius),
                    (int)(Radius * 2), (int)(Radius * 2)),
                    IsFriendlyToPlayer ? Color.Yellow : Color.Red);
            }
        }
    }

    private static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });
        return texture;
    }

    public void ConfigureOrbit(
        Vector2 center,
        float radius,
        float startAngle,
        float angularSpeed,
        float durationSeconds,
        bool releaseAfterOrbit,
        Vector2 releaseDirection,
        Func<Vector2> centerFollow = null)
    {
        IsOrbiting = true;
        _orbitCenterFollow = centerFollow;
        OrbitCenter = center;
        OrbitRadius = radius;
        OrbitAngle = startAngle;
        OrbitAngularSpeed = angularSpeed;
        OrbitTimeLeft = durationSeconds;
        ReleaseAfterOrbit = releaseAfterOrbit;
        ReleaseDirection = releaseDirection;
        Velocity = Vector2.Zero;
        Position = OrbitCenter + new Vector2(MathF.Cos(OrbitAngle), MathF.Sin(OrbitAngle)) * OrbitRadius;
    }
}
