using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
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
    public float Radius {  get; private set; }
    public float RecoilForce { get; }  // Сила отдачи при попадании
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
    public bool CanDealDamage => _canDealDamage; // Может ли наносить урон

    // Анимация и текстура
    private Texture2D _texture;
    private int _currentFrame = 0;
    private float _animationTimer = 0f;
    private bool _isImpacting = false;
    private float _rotation = 0f; // Угол поворота спрайта
    private bool _canDealDamage = true; // Может ли наносить урон

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
        float length = 0f)
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
        
        // Вычисляем угол поворота спрайта в направлении движения
        _rotation = (float)Math.Atan2(Direction.Y, Direction.X);
    }

    public override void LoadContent(ContentManager content)
    {
        base.LoadContent(content);
        
        if (IsFriendlyToPlayer)
        {
            _texture = content.Load<Texture2D>(WeaponConfig.PlayerProjectileTexture);
        }
        else
        {
            _texture = content.Load<Texture2D>(WeaponConfig.EnemyProjectileTexture);
        }
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Обновляем анимацию
        UpdateAnimation(dt);

        // Если не в состоянии анимации удара, продолжаем движение
        if (!_isImpacting)
        {
            if (IsOrbiting)
            {
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

            LifeLeft -= dt;
            if (LifeLeft <= 0f)
            {
                StartImpactAnimation();
            }
        }
        // Во время анимации удара проджектайл остается на месте
    }

    private void UpdateAnimation(float deltaTime)
    {
        _animationTimer += deltaTime;

        if (_animationTimer >= WeaponConfig.ProjectileAnimationSpeed)
        {
            if (_isImpacting)
            {
                // Во время анимации удара переходим к следующим кадрам
                if (_currentFrame < WeaponConfig.ProjectileFrameCount - 1)
                {
                    _currentFrame++;
                }
                else
                {
                    // Анимация завершена - удаляем проджектайл
                    IsAlive = false;
                }
            }
            else
            {
                // Во время полета остаемся на первом кадре
                _currentFrame = 0;
            }

            _animationTimer = 0f;
        }
    }

    public void StartImpactAnimation()
    {
        if (!_isImpacting)
        {
            if (IsFriendlyToPlayer)
            {
                _rotation = Random.Shared.NextSingle() * 2 * (float)Math.PI;
            }

            _isImpacting = true;
            _canDealDamage = false; // Блокируем урон
            Velocity = Vector2.Zero; // Останавливаем движение
            _currentFrame = 1; // Переходим ко второму кадру (начало анимации удара)
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
            // Вычисляем исходный прямоугольник для текущего кадра
            int frameWidth = _texture.Width / WeaponConfig.ProjectileFrameCount;
            Rectangle sourceRect = new Rectangle(
                _currentFrame * frameWidth,
                0,
                frameWidth,
                _texture.Height
            );

            // Вычисляем масштаб чтобы проджектайл соответствовал нужному размеру
            float scale = WeaponConfig.ProjectileSize / frameWidth;

            spriteBatch.Draw(_texture, Position, sourceRect, Color.White, 
                _rotation, new Vector2(frameWidth / 2f, _texture.Height / 2f), 
                scale, SpriteEffects.None, 0f);
        }
        else
        {
            // Fallback - отрисовка кругом
            var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
            if (pixel != null)
            {
                spriteBatch.Draw(pixel, new Rectangle((int)(Position.X - Radius), (int)(Position.Y - Radius), 
                    (int)(Radius * 2), (int)(Radius * 2)), 
                    IsFriendlyToPlayer ? Color.Yellow : Color.Red);
            }
        }
    }

    private Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
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
        Vector2 releaseDirection)
    {
        IsOrbiting = true;
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