using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Services;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities;

namespace Vibe_Game.Gameplay.Entities.Collectables;

/// <summary>
/// Представляет предмет, лежащий на полу (например, сердечки или другие бонусы).
/// Отвечает за проигрывание idle-анимации, покачивание и логику подбора игроком.
/// </summary>
public sealed class DroppedPickup : Entity
{
    private readonly CollectibleVisualCache _visuals;
    private float _bobPhase;
    private float _idleAnimTimer;
    private int _frameIndex;

    /// <summary>
    /// Инициализирует новый экземпляр класса DroppedPickup.
    /// </summary>
    /// <param name="worldPosition">Позиция появления в мире.</param>
    /// <param name="kind">Тип предмета.</param>
    /// <param name="visuals">Кэш текстур для отрисовки.</param>
    public DroppedPickup(Vector2 worldPosition, CollectableKind kind, CollectibleVisualCache visuals)
        : base()
    {
        Position = worldPosition;
        Kind = kind;
        _visuals = visuals;
        IsAlive = true;
    }

    /// <summary>
    /// Тип подбираемого предмета.
    /// </summary>
    public CollectableKind Kind { get; }

    /// <inheritdoc />
    public override Rectangle GetBounds()
    {
        int s = CollectibleConfig.FloorPickupSize;
        return new Rectangle((int)Position.X - s / 2, (int)Position.Y - s / 2, s, s);
    }

    /// <summary>
    /// Обновляет анимацию покачивания и проверяет пересечение с игроком.
    /// </summary>
    /// <param name="gameTime">Текущее время игры.</param>
    /// <param name="player">Ссылка на игрока для проверки коллизий.</param>
    public void Update(GameTime gameTime, global::Vibe_Game.Gameplay.Entities.Player.Player player)
    {
        if (!IsAlive || player == null)
            return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _bobPhase += dt * CollectibleConfig.FloorPickupBobSpeed;

        _idleAnimTimer += dt;
        float frameDur = 1f / Math.Max(1f, CollectibleConfig.FloorPickupIdleAnimFps);
        if (_idleAnimTimer >= frameDur)
        {
            _idleAnimTimer = 0f;
            int fc = Math.Max(1, CollectibleConfig.CollectablesSheetFramesPerRow);
            _frameIndex = (_frameIndex + 1) % fc;
        }

        if (GetBounds().Intersects(player.GetBounds()))
        {
            player.TryPlayPickupAnimation();
            ApplyHeal(player);
            IsAlive = false;
        }
    }

    private void ApplyHeal(global::Vibe_Game.Gameplay.Entities.Player.Player player)
    {
        float amount = Kind == CollectableKind.HealthLarge ? 2f : 1f;
        player.Stats.Health = MathHelper.Min(player.Stats.MaxHealth, player.Stats.Health + amount);
        GameplayAudio.PlayPlayerHeal();
    }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || _visuals == null)
            return;

        Texture2D tex = _visuals.GetSheet(Kind);
        if (tex == null)
            return;

        Rectangle src = _visuals.GetSourceRect(Kind, _frameIndex);
        int s = CollectibleConfig.FloorPickupSize;
        float bob = MathF.Sin(_bobPhase) * CollectibleConfig.FloorPickupBobAmplitude;
        var dest = new Rectangle((int)Position.X - s / 2, (int)(Position.Y - s / 2 + bob), s, s);
        spriteBatch.Draw(tex, dest, src, Color.White * 0.95f);
    }
}
