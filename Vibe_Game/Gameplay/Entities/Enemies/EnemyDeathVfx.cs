using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Enemies;

/// <summary>Анимация смерти врага или босса: кадры из соответствующего спрайт-листа и затухание по альфе.</summary>
public sealed class EnemyDeathVfx
{
    public Vector2 Position { get; }
    public float DisplayRadius { get; }
    public bool IsBossDeath { get; }

    private float _elapsed;

    public EnemyDeathVfx(Vector2 position, float displayRadius, bool isBossDeath = false)
    {
        Position = position;
        DisplayRadius = MathHelper.Max(4f, displayRadius);
        IsBossDeath = isBossDeath;
    }

    private int FrameCount =>
        IsBossDeath ? EnemyConfig.BossDeathAnimationFramesCount : EnemyConfig.DeathAnimationFramesCount;

    private float FrameDuration =>
        IsBossDeath ? EnemyConfig.BossDeathAnimationFrameDurationSeconds : EnemyConfig.DeathAnimationFrameDurationSeconds;

    public bool IsFinished =>
        _elapsed >= FrameCount * FrameDuration;

    public void Update(float deltaSeconds)
    {
        if (!IsFinished)
            _elapsed += deltaSeconds;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D normalDeathSheet, Texture2D bossSheet)
    {
        if (spriteBatch == null)
            return;

        Texture2D sheet = IsBossDeath ? bossSheet : normalDeathSheet;
        if (sheet == null)
            return;

        int fc = Math.Max(1, FrameCount);
        float frameDur = Math.Max(0.01f, FrameDuration);

        Rectangle src;
        if (IsBossDeath)
        {
            int fw = Math.Max(1, sheet.Width / Math.Max(1, EnemyConfig.BossSheetFramesCount));
            int fh = Math.Max(1, sheet.Height / Math.Max(1, EnemyConfig.BossSheetRowCount));
            int row = Math.Clamp(EnemyConfig.BossSheetDeathRow, 0, EnemyConfig.BossSheetRowCount - 1);
            int frame = Math.Min(fc - 1, (int)(_elapsed / frameDur));
            src = new Rectangle(frame * fw, row * fh, fw, fh);
        }
        else
        {
            int fw = Math.Max(1, sheet.Width / fc);
            int fh = sheet.Height;
            int frame = Math.Min(fc - 1, (int)(_elapsed / frameDur));
            src = new Rectangle(frame * fw, 0, fw, fh);
        }

        float t = Math.Clamp(_elapsed / (fc * frameDur), 0f, 1f);
        float alpha = MathHelper.Lerp(EnemyConfig.DeathAnimationStartOpacity, EnemyConfig.DeathAnimationEndOpacity, t);
        alpha = MathHelper.Clamp(alpha, 0f, 1f);

        int srcW = src.Width;
        int srcH = src.Height;
        float scale = (DisplayRadius * 2.2f) / Math.Max(srcW, srcH);
        var color = Color.White * alpha;

        spriteBatch.Draw(
            sheet,
            Position,
            src,
            color,
            0f,
            new Vector2(srcW / 2f, srcH / 2f),
            scale,
            SpriteEffects.None,
            0f);
    }
}
