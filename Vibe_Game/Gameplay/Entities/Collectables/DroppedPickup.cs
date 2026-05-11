using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities;

namespace Vibe_Game.Gameplay.Entities.Collectables;

/// <summary>Сердечко на полу (без анимации), подбирается при пересечении с игроком.</summary>
public sealed class DroppedPickup : Entity
{
    private readonly CollectibleVisualCache _visuals;
    private float _bobPhase;

    public DroppedPickup(Vector2 worldPosition, CollectableKind kind, CollectibleVisualCache visuals)
        : base()
    {
        Position = worldPosition;
        Kind = kind;
        _visuals = visuals;
        IsAlive = true;
    }

    public CollectableKind Kind { get; }

    public override Rectangle GetBounds()
    {
        int s = CollectibleConfig.FloorPickupSize;
        return new Rectangle((int)Position.X - s / 2, (int)Position.Y - s / 2, s, s);
    }

    public void Update(GameTime gameTime, global::Vibe_Game.Gameplay.Entities.Player.Player player)
    {
        if (!IsAlive || player == null)
            return;

        _bobPhase += (float)gameTime.ElapsedGameTime.TotalSeconds * CollectibleConfig.FloorPickupBobSpeed;

        if (GetBounds().Intersects(player.GetBounds()))
        {
            ApplyHeal(player);
            IsAlive = false;
        }
    }

    private void ApplyHeal(global::Vibe_Game.Gameplay.Entities.Player.Player player)
    {
        float amount = Kind == CollectableKind.HealthLarge ? 2f : 1f;
        player.Stats.Health = MathHelper.Min(player.Stats.MaxHealth, player.Stats.Health + amount);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsAlive || _visuals == null)
            return;

        Texture2D tex = _visuals.GetHealthTexture(Kind);
        if (tex == null)
            return;

        int s = CollectibleConfig.FloorPickupSize;
        float bob = MathF.Sin(_bobPhase) * CollectibleConfig.FloorPickupBobAmplitude;
        var dest = new Rectangle((int)Position.X - s / 2, (int)(Position.Y - s / 2 + bob), s, s);
        spriteBatch.Draw(tex, dest, Color.White * 0.95f);
    }
}
