using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using Vibe_Game.Core.Utilities;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Weapons;

public sealed class SwordWeapon : WeaponBase
{
    public override WeaponFireMode FireMode => WeaponFireMode.DirectionHeldPlusButtonPress;

    /// <summary>Базовая отдача меча (сила толчка врага при ударе).</summary>
    public override float BaseRecoil => WeaponConfig.SwordRecoilForce;

    private Texture2D _swordTexture;
    private Texture2D _trailTexture;
    private readonly List<SwordTrailParticle> _trailParticles = new();

    private float _swordLength = WeaponConfig.SwordLength;
    private float _swordWidth = WeaponConfig.SwordWidth;
    private readonly float _attackAngle = WeaponConfig.SwordAttackAngle;
    private readonly float _attackDuration = WeaponConfig.SwordAttackDuration;
    private readonly int _damage = WeaponConfig.SwordDamage;
    private readonly float _baseCooldownSeconds;

    private float _attackTimer;
    private Vector2 _attackDirection;
    private Vector2 _currentPlayerPosition;
    private float _startAngle;
    private float _endAngle;
    private float _handleOffsetFromBottom = 13f;

    // Кто уже получил урон в этой атаке
    private HashSet<object> _hitEnemies = new();

    // Публичные свойства для изменения размера оружия в рантайме
    public float SwordLength
    {
        get => _swordLength;
        set
        {
            _swordLength = Math.Max(10f, value);
        }
    }

    public float SwordWidth
    {
        get => _swordWidth;
        set
        {
            _swordWidth = Math.Max(2f, value);
        }
    }

    public int ExternalDamageBonus { get; set; }

    public int Damage
    {
        get => _damage;
    }

    
    public SwordWeapon(float cooldownSeconds = 0.4f)
        : base("Sword", cooldownSeconds)
    {
        _baseCooldownSeconds = cooldownSeconds;
    }

    public void SetCooldownMultiplier(float multiplier)
    {
        CooldownSeconds = Math.Max(0.04f, _baseCooldownSeconds * multiplier);
    }

    public void LoadContent(ContentManager content)
    {
        _swordTexture = content.Load<Texture2D>(WeaponConfig.SwordTexture);
        _trailTexture = content.Load<Texture2D>(WeaponConfig.SwordTrailTexture);
    }

    public override void Update(GameTime gameTime, IAttackContext context)
    {
        base.Update(gameTime, context);

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _attackTimer -= dt;

        // Обновляем частицы
        UpdateTrailParticles(dt);

        if (_attackTimer <= 0) return;

        // Прогресс анимации (от 0 до 1)
        float totalTime = _attackDuration;
        float elapsedTime = totalTime - _attackTimer;
        float progress = Math.Clamp(elapsedTime / totalTime, 0f, 1f);

        // ПРОВЕРЯЕМ УРОН КАЖДЫЙ КАДР во время всей анимации
        CheckAndDealDamage(context, progress);

        // Создаем частицы следа
        CreateTrailParticles(progress);

        // Сброс после окончания анимации
        if (_attackTimer <= 0)
        {
            _hitEnemies.Clear();
            // Частицы не очищаем - они должны исчезнуть только по истечению лайфтайма
        }
    }

    private float GetCurrentAngle(float progress)
    {
        // Интерполяция угла от startAngle до endAngle
        return MathHelper.Lerp(_startAngle, _endAngle, progress);
    }

    private void CheckAndDealDamage(IAttackContext context, float progress)
    {
        // Получаем текущий угол меча
        float currentAngle = GetCurrentAngle(progress);

        // Получаем прямоугольник хитбокса меча
        Rectangle swordBounds = GetSwordBounds(currentAngle);

        // Находим всех врагов в этой области
        var enemies = context.GetEnemiesInArea(swordBounds);

        foreach (var enemy in enemies)
        {
            if (!_hitEnemies.Contains(enemy))
            {
                _hitEnemies.Add(enemy);
                context.DamageEnemy(enemy, _damage + ExternalDamageBonus);

                // Применяем отдачу врагу в направлении удара
                context.ApplyRecoilToEnemy(enemy, _attackDirection, WeaponConfig.SwordRecoilForce);
            }
        }
    }

    private Rectangle GetSwordBounds(float angle)
    {
        Vector2 handle = GetSwordHandle();
        Vector2 swordDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        Vector2 perpendicular = new Vector2(-swordDir.Y, swordDir.X);

        Vector2 tip = handle + swordDir * _swordLength;

        // Вычисляем 4 угла прямоугольника меча
        Vector2 p1 = handle + perpendicular * (_swordWidth / 2);
        Vector2 p2 = tip + perpendicular * (_swordWidth / 2);
        Vector2 p3 = tip - perpendicular * (_swordWidth / 2);
        Vector2 p4 = handle - perpendicular * (_swordWidth / 2);

        // Находим мин/макс координаты для прямоугольника
        float minX = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
        float minY = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));
        float maxX = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
        float maxY = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));

        return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
    }

    public override bool TryPrimaryAttack(IAttackContext context, Vector2 ownerPosition, Vector2 facingDirection)
    {
        if (facingDirection == Vector2.Zero) return false;
        if (_attackTimer > 0) return false;
        if (!TryStartCooldown()) return false;

        _currentPlayerPosition = ownerPosition;
        _attackDirection = Vector2.Normalize(facingDirection);
        _attackTimer = _attackDuration;
        _hitEnemies.Clear();

        // Вычисляем углы атаки
        float directionAngle = (float)Math.Atan2(_attackDirection.Y, _attackDirection.X);
        _startAngle = directionAngle - _attackAngle / 2;
        _endAngle = directionAngle + _attackAngle / 2;

        return true;
    }

    public void UpdateOwnerPosition(Vector2 ownerPosition)
    {
        _currentPlayerPosition = ownerPosition;
    }

    private Vector2 GetSwordHandle()
    {
        // Смещаем рукоять меча к краю игрока в направлении атаки
        float handleOffset = 15f; // Расстояние от центра игрока до рукояти
        return _currentPlayerPosition + _attackDirection * handleOffset;
    }

    private Vector2 GetSwordTip(float angle)
    {
        Vector2 swordDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        return GetSwordHandle() + swordDir * _swordLength;
    }

    public override void Draw(SpriteBatch spriteBatch, IAttackContext context)
    {
        // Рисуем меч только во время атаки
        if (_attackTimer > 0)
        {
            float totalTime = _attackDuration;
            float elapsedTime = totalTime - _attackTimer;
            float progress = Math.Clamp(elapsedTime / totalTime, 0f, 1f);

            float currentAngle = GetCurrentAngle(progress);

            Vector2 handle = GetSwordHandle();
            Vector2 tip = GetSwordTip(currentAngle);

            if (Vector2.Distance(handle, tip) < 0.1f) return;

            // Альфа: 0 -> 1 -> 0
            float alpha;
            if (progress <= 0.5f)
                alpha = progress * 2f;
            else
                alpha = 2f - (progress * 2f);
            alpha = Math.Clamp(alpha, 0f, 1f);

            // Цвет меча
            Color swordColor;
            if (progress <= 0.5f)
                swordColor = Color.Lerp(Color.White, Color.LightBlue, progress * 2f);
            else
                swordColor = Color.Lerp(Color.LightBlue, Color.White, (progress - 0.5f) * 2f);
            swordColor *= alpha;

            // Рисуем меч текстурой если доступна
            if (_swordTexture != null)
            {
                Vector2 swordDir = tip - handle;
                float swordAngle = (float)Math.Atan2(swordDir.Y, swordDir.X);
                float swordLength = swordDir.Length();

                spriteBatch.Draw(
                    _swordTexture,
                    handle,
                    null,
                    swordColor,
                    swordAngle + MathHelper.PiOver2,

                    // origin
                    new Vector2(
                        _swordTexture.Width / 2f,
                        _swordTexture.Height - _handleOffsetFromBottom
                    ),

                    // scale
                    new Vector2(
                        _swordLength / _swordTexture.Height,
                        _swordWidth / _swordTexture.Width
                    ),

                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                // Fallback на отрисовку линиями
                var pixel = GetPixelTexture(spriteBatch);
                if (pixel != null)
                {
                    DrawLine(spriteBatch, pixel, handle, tip, swordColor, _swordWidth);
                }
            }

#if DEBUG
            var dbgPixel = GetPixelTexture(spriteBatch);
            Rectangle bounds = GetSwordBounds(currentAngle);
            spriteBatch.Draw(dbgPixel, bounds, Color.Cyan * 0.3f);
            spriteBatch.DrawRectangle(dbgPixel, bounds, Color.Cyan, 1);
            Vector2 center = (handle + tip) / 2;
            spriteBatch.Draw(dbgPixel, new Rectangle((int)center.X - 2, (int)center.Y - 2, 4, 4), Color.Yellow);
            Vector2 debugEnd = handle + _attackDirection * _swordLength;
            DrawLine(spriteBatch, dbgPixel, handle, debugEnd, Color.Green * 0.3f, 1f);
#endif
        }

        // Рисуем частицы следа всегда
        DrawTrailParticles(spriteBatch);
    }

    private Texture2D _pixelTexture;
    private Texture2D GetPixelTexture(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null || _pixelTexture.IsDisposed)
        {
            _pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        return _pixelTexture;
    }

    private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        float length = edge.Length();

        if (length <= 0.01f) return;

        spriteBatch.Draw(texture, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
    }

    private void CreateTrailParticles(float progress)
    {
        if (_trailTexture == null) return;

        // Создаем частицы вдоль меча
        Vector2 handle = GetSwordHandle();
        float currentAngle = GetCurrentAngle(progress);
        Vector2 tip = GetSwordTip(currentAngle);
        
        // Случайное количество частиц от 1 до 5
        int particleCount = Random.Shared.Next(1, WeaponConfig.SwordTrailParticleCount + 1);
        
        for (int i = 0; i < particleCount; i++)
        {
            // Случайная позиция от рукояти к острию по вертикальной оси меча
            float t = Random.Shared.NextSingle(); // от 0 до 1
            Vector2 position = Vector2.Lerp(handle, tip, t);
            
            // Добавляем случайное смещение для более естественного вида
            Vector2 offset = new Vector2(
                (float)(Random.Shared.NextDouble() - 0.5) * WeaponConfig.SwordTrailParticleSize * 0.5f,
                (float)(Random.Shared.NextDouble() - 0.5) * WeaponConfig.SwordTrailParticleSize * 0.5f
            );

            float brightness =
                1f +
                Random.Shared.NextSingle() *
                WeaponConfig.SwordTrailBrightnessVariation;

            _trailParticles.Add(new SwordTrailParticle
            {
                Position = position + offset,
                CurrentFrame = 0,
                Row = Random.Shared.Next(0, 2),
                Timer = 0f,
                Lifetime = WeaponConfig.SwordTrailParticleLifetime,
                Size = WeaponConfig.SwordTrailParticleSize,
                Brightness = brightness
            });
        }
    }

    private void UpdateTrailParticles(float deltaTime)
    {
        for (int i = _trailParticles.Count - 1; i >= 0; i--)
        {
            var particle = _trailParticles[i];
            particle.Lifetime -= deltaTime;
            particle.Timer += deltaTime;

            // Переходим к следующему кадру анимации
            if (particle.Timer >= WeaponConfig.SwordTrailAnimationSpeed)
            {
                particle.CurrentFrame++;
                particle.Timer = 0f;
            }

            // Удаляем частицу если анимация закончилась или время жизни истекло
            if (particle.CurrentFrame >= WeaponConfig.SwordTrailFrameCount || particle.Lifetime <= 0)
            {
                _trailParticles.RemoveAt(i);
            }
        }
    }

    private void DrawTrailParticles(SpriteBatch spriteBatch)
    {
        if (_trailTexture == null) return;

        foreach (var particle in _trailParticles)
        {
            float alpha = Math.Clamp(particle.Lifetime / WeaponConfig.SwordTrailParticleLifetime, 0f, 1f) * 0.3f;
            Color baseColor = Color.Yellow;

            // 0 = обычный цвет
            // 1 = полностью белый
            float brightnessFactor = particle.Brightness - 1f;

            Color brightColor = Color.Lerp(
                baseColor,
                Color.White,
                brightnessFactor);

            Color color = brightColor * alpha;

            // Вычисляем исходный прямоугольник для текущего кадра
            int frameWidth = _trailTexture.Width / 8; // 8 кадров в строке
            int frameHeight = _trailTexture.Height / 2; // 2 строки
            Rectangle sourceRect = new Rectangle(
                particle.CurrentFrame * frameWidth, 
                particle.Row * frameHeight, 
                frameWidth, 
                frameHeight
            );

            spriteBatch.Draw(_trailTexture, particle.Position, sourceRect, color, 
                0f, new Vector2(frameWidth / 2f, frameHeight / 2f), 
                particle.Size / frameWidth, SpriteEffects.None, 0f);
        }
    }
}

public class SwordTrailParticle
{
    public Vector2 Position;
    public int CurrentFrame;
    public int Row; // Строка в спрайт листе (0 или 1)
    public float Timer;
    public float Lifetime;
    public float Size;

    public float Brightness;
}