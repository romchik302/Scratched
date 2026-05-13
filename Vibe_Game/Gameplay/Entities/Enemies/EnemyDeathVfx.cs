using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>Общая анимация смерти врага: кадры из спрайт-листа и затухание по альфе.</summary>
public sealed class EnemyDeathVfx
{
    public Vector2 Position { get; }
    public float DisplayRadius { get; }

    private float _elapsed;

    public EnemyDeathVfx(Vector2 position, float displayRadius)
    {
        Position = position;
        DisplayRadius = MathHelper.Max(4f, displayRadius);
    }

    public bool IsFinished =>
        _elapsed >= EnemyConfig.DeathAnimationFramesCount * EnemyConfig.DeathAnimationFrameDurationSeconds;

    public void Update(float deltaSeconds)
    {
        if (!IsFinished)
            _elapsed += deltaSeconds;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D sheet)
    {
        if (sheet == null || spriteBatch == null)
            return;

        int fc = Math.Max(1, EnemyConfig.DeathAnimationFramesCount);
        int fw = Math.Max(1, sheet.Width / fc);
        int fh = sheet.Height;

        float frameDur = Math.Max(0.01f, EnemyConfig.DeathAnimationFrameDurationSeconds);
        int frame = Math.Min(fc - 1, (int)(_elapsed / frameDur));
        var src = new Rectangle(frame * fw, 0, fw, fh);

        float t = Math.Clamp(_elapsed / (fc * frameDur), 0f, 1f);
        float alpha = MathHelper.Lerp(EnemyConfig.DeathAnimationStartOpacity, EnemyConfig.DeathAnimationEndOpacity, t);
        alpha = MathHelper.Clamp(alpha, 0f, 1f);

        float scale = (DisplayRadius * 2.2f) / Math.Max(fw, fh);
        var color = Color.White * alpha;

        spriteBatch.Draw(
            sheet,
            Position,
            src,
            color,
            0f,
            new Vector2(fw / 2f, fh / 2f),
            scale,
            SpriteEffects.None,
            0f);
    }
}
