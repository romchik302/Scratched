using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities;

namespace Vibe_Game.Gameplay.Entities.Collectables;

/// <summary>Предмет на пьедестале: управляет анимацией покачивания в покое, анимацией подбора и применением эффектов.</summary>
public sealed class CollectableEntity : Entity
{
    private enum CollectState
    {
        Idle,
        PickupAnimating,
        Consumed
    }

    private CollectState _state = CollectState.Idle;
    private float _idleAnimTimer;
    private float _pickupAnimTimer;
    private int _frameIndex;
    private float _bobPhase;

    public Point TilePositionInRoom { get; }
    public CollectableKind Kind { get; }

    /// <summary>Текущее вертикальное смещение предмета (эффект «парения» над пьедесталом).</summary>
    public float PedestalBobOffsetY { get; private set; }

    /// <summary>Инициализирует новый экземпляр класса CollectableEntity.</summary>
    /// <param name="tilePositionInRoom">Позиция плитки в комнате, где находится предмет.</param>
    /// <param name="kind">Тип предмета (определяет его визуальный ряд и эффект).</param>
    public CollectableEntity(Point tilePositionInRoom, CollectableKind kind)
    {
        TilePositionInRoom = tilePositionInRoom;
        Kind = kind;
        IsAlive = true;
    }

    /// <summary>Индекс текущего кадра анимации предмета.</summary>
    public int PedestalIdleFrameIndex => _frameIndex;

    /// <summary>Масштаб и альфа для совместной отрисовки с пьедесталом (1 → 0).</summary>
    public float VisualScale { get; private set; } = 1f;
    /// <summary>Текущая прозрачность предмета. Используется для плавной анимации исчезновения (1 → 0).</summary>
    public float VisualAlpha { get; private set; } = 1f;

    /// <summary>Указывает, был ли предмет полностью поглощен (анимация подбора завершена).</summary>
    public bool IsPedestalGone => _state == CollectState.Consumed;

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (!IsAlive || _state == CollectState.Consumed)
            return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_state == CollectState.Idle)
        {
            _bobPhase += dt * CollectibleConfig.FloorPickupBobSpeed;
            PedestalBobOffsetY = MathF.Sin(_bobPhase) * CollectibleConfig.FloorPickupBobAmplitude;

            _idleAnimTimer += dt;
            float frameDur = 1f / Math.Max(1f, PedestalConfig.IdleAnimFps);
            if (_idleAnimTimer >= frameDur)
            {
                _idleAnimTimer = 0f;
                _frameIndex = (_frameIndex + 1) % PedestalConfig.SpriteFrameCount;
            }
        }
        else if (_state == CollectState.PickupAnimating)
        {
            _bobPhase += dt * CollectibleConfig.FloorPickupBobSpeed;
            PedestalBobOffsetY = MathF.Sin(_bobPhase) * CollectibleConfig.FloorPickupBobAmplitude;

            _pickupAnimTimer += dt;
            float t = MathHelper.Clamp(_pickupAnimTimer / PedestalConfig.PickupAnimDurationSeconds, 0f, 1f);
            VisualScale = 1f - t;
            VisualAlpha = 1f - t;
            if (t >= 1f)
            {
                _state = CollectState.Consumed;
                IsAlive = false;
            }
        }
    }

    /// <summary>Проверяет пересечение с игроком и запускает анимацию подбора.</summary>
    /// <param name="playerBounds">Хитбокс игрока для проверки коллизии.</param>
    /// <param name="roomGrid">Координаты комнаты в сетке мира для вычисления абсолютной позиции.</param>
    public bool TryBeginPickupIfOverlapping(Rectangle playerBounds, Point roomGrid)
    {
        if (_state != CollectState.Idle)
            return false;

        Rectangle pickup = GetWorldPickupBounds(roomGrid);
        if (!pickup.Intersects(playerBounds))
            return false;

        _state = CollectState.PickupAnimating;
        _pickupAnimTimer = 0f;
        VisualScale = 1f;
        VisualAlpha = 1f;
        return true;
    }

    /// <summary>Применяет баффы или эффекты предмета к статистике игрока.</summary>
    /// <param name="player">Экземпляр игрока, на которого применяется эффект.</param>
    public void ApplyEffect(global::Vibe_Game.Gameplay.Entities.Player.Player player)
    {
        if (player?.Stats == null)
            return;

        switch (Kind)
        {
            case CollectableKind.Totem:
                player.Stats.ExtraLives++;
                break;
            case CollectableKind.Feather:
                player.Stats.SpeedMultiplier += CollectibleConfig.FeatherSpeedMultiplierBonus;
                player.Stats.ProjectileSpeedMultiplier += CollectibleConfig.FeatherProjectileSpeedMultiplierBonus;
                player.Stats.SwordCooldownMultiplier *= CollectibleConfig.FeatherSwordCooldownMultiplierPerStack;
                break;
            case CollectableKind.Fang:
                player.Stats.BonusWeaponDamage += CollectibleConfig.FangBonusDamage;
                break;
            case CollectableKind.WeaponProjectile:
            case CollectableKind.WeaponSword:
                break;
        }
    }

    /// <summary>Вычисляет хитбокс предмета в мировых координатах для обработки подбора.</summary>
    /// <param name="roomGrid">Координаты комнаты в сетке мира.</param>
    public Rectangle GetWorldPickupBounds(Point roomGrid)
    {
        float wx = roomGrid.X * WorldConfig.RoomWidthPx + TilePositionInRoom.X * WorldConfig.TileSize;
        float wy = roomGrid.Y * WorldConfig.RoomHeightPx + TilePositionInRoom.Y * WorldConfig.TileSize;
        int margin = WorldConfig.TileSize / 5;
        int size = WorldConfig.TileSize - margin * 2;
        return new Rectangle((int)wx + margin, (int)wy + margin, size, size);
    }

    /// <summary>Отрисовывает предмет над пьедесталом с учетом текущей анимации.</summary>
    /// <param name="spriteBatch">Экземпляр SpriteBatch для отрисовки.</param>
    /// <param name="visuals">Кэш текстур для получения нужного кадра анимации.</param>
    /// <param name="pixel">Текстура пикселя 1x1 для отрисовки частиц (spark).</param>
    /// <param name="tileBounds">Область плитки, на которой стоит пьедестал.</param>
    public void DrawOnPedestal(SpriteBatch spriteBatch, CollectibleVisualCache visuals, Texture2D pixel, Rectangle tileBounds)
    {
        if (!IsAlive || _state == CollectState.Consumed || visuals == null)
            return;

        Texture2D sheet = visuals.GetSheet(Kind);
        if (sheet == null)
            return;

        Rectangle source = visuals.GetSourceRect(Kind, _frameIndex);
        int frameW = source.Width;
        int frameH = source.Height;

        Vector2 center = tileBounds.Center.ToVector2()
            + new Vector2(PedestalConfig.CollectableOnPedestalOffsetXPixels, -PedestalConfig.CollectableOnPedestalOffsetYUpPixels + PedestalBobOffsetY);
        float scale = PedestalConfig.CollectableOnPedestalScaleMultiplier * VisualScale * (WorldConfig.TileSize / (float)Math.Max(frameW, frameH));
        Color tint = Color.White * VisualAlpha;

        spriteBatch.Draw(
            sheet,
            center,
            source,
            tint,
            0f,
            new Vector2(frameW / 2f, frameH / 2f),
            scale * 0.9f,
            SpriteEffects.None,
            0f);

        if (pixel != null && _state == CollectState.PickupAnimating)
        {
            int spark = (int)(4 * (1f - VisualScale));
            if (spark > 0)
                spriteBatch.Draw(pixel, new Rectangle(tileBounds.Center.X - spark, tileBounds.Center.Y - spark, spark * 2, spark * 2), Color.White * (0.35f * VisualAlpha));
        }
    }
}
