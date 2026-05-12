using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Collectables;

/// <summary>Текстуры предметов: при наличии PNG в Content подхватываются, иначе создаются простые полоски кадров.</summary>
public sealed class CollectibleVisualCache
{
    private Texture2D _totemSheet;
    private Texture2D _featherSheet;
    private Texture2D _fangSheet;
    private Texture2D _weaponProjectileSheet;
    private Texture2D _weaponSwordSheet;
    private Texture2D _health1;
    private Texture2D _health2;

    public void Load(ContentManager content, GraphicsDevice device)
    {
        _totemSheet = TryLoadOrCreateStrip(content, device, PedestalConfig.TotemTextureAsset, new Color(180, 120, 40), new Color(200, 140, 55), new Color(160, 100, 30), new Color(190, 130, 45));
        _featherSheet = TryLoadOrCreateStrip(content, device, PedestalConfig.FeatherTextureAsset, new Color(220, 220, 255), new Color(200, 200, 250), new Color(240, 240, 255), new Color(210, 210, 245));
        _fangSheet = TryLoadOrCreateStrip(content, device, PedestalConfig.FangTextureAsset, new Color(200, 80, 80), new Color(220, 100, 100), new Color(180, 60, 60), new Color(210, 90, 90));
        _weaponProjectileSheet = TryLoadOrCreateStrip(content, device, PedestalConfig.WeaponProjectilePedestalTextureAsset, new Color(120, 200, 255), new Color(100, 180, 240), new Color(140, 220, 255), new Color(110, 190, 245));
        _weaponSwordSheet = TryLoadOrCreateStrip(content, device, PedestalConfig.WeaponSwordPedestalTextureAsset, new Color(200, 200, 220), new Color(220, 220, 240), new Color(180, 180, 200), new Color(210, 210, 230));
        _health1 = TryLoadOrCreateSolid(content, device, "pickup_health_1", new Color(255, 90, 110));
        _health2 = TryLoadOrCreateSolid(content, device, "pickup_health_2", new Color(255, 50, 80));
    }

    public Texture2D GetSheet(CollectableKind kind) =>
        kind switch
        {
            CollectableKind.Totem => _totemSheet,
            CollectableKind.Feather => _featherSheet,
            CollectableKind.Fang => _fangSheet,
            CollectableKind.WeaponProjectile => _weaponProjectileSheet,
            CollectableKind.WeaponSword => _weaponSwordSheet,
            _ => _health1
        };

    public Texture2D GetHealthTexture(CollectableKind kind) =>
        kind == CollectableKind.HealthLarge ? _health2 : _health1;

    private static Texture2D TryLoadOrCreateStrip(ContentManager content, GraphicsDevice device, string assetName, Color a, Color b, Color c, Color d)
    {
        if (content != null)
        {
            try
            {
                return content.Load<Texture2D>(assetName);
            }
            catch
            {
                // ignore
            }
        }

        return CreateFourFrameStrip(device, a, b, c, d);
    }

    private static Texture2D TryLoadOrCreateSolid(ContentManager content, GraphicsDevice device, string assetName, Color color)
    {
        if (content != null)
        {
            try
            {
                return content.Load<Texture2D>(assetName);
            }
            catch
            {
            }
        }

        return CreateSolid(device, 20, 20, color);
    }

    private static Texture2D CreateFourFrameStrip(GraphicsDevice device, Color c0, Color c1, Color c2, Color c3)
    {
        const int fw = 18;
        const int fh = 18;
        int fc = PedestalConfig.SpriteFrameCount;
        var tex = new Texture2D(device, fw * fc, fh);
        Color[] data = new Color[fw * fc * fh];
        Color[] frames = { c0, c1, c2, c3 };

        for (int fi = 0; fi < fc; fi++)
        {
            for (int y = 0; y < fh; y++)
            {
                for (int x = 0; x < fw; x++)
                {
                    bool edge = x == 0 || y == 0 || x == fw - 1 || y == fh - 1;
                    data[y * (fw * fc) + fi * fw + x] = edge ? frames[fi] * 0.55f : frames[fi];
                }
            }
        }

        tex.SetData(data);
        return tex;
    }

    private static Texture2D CreateSolid(GraphicsDevice device, int w, int h, Color color)
    {
        var tex = new Texture2D(device, w, h);
        Color[] data = new Color[w * h];
        for (int i = 0; i < data.Length; i++)
            data[i] = color;
        tex.SetData(data);
        return tex;
    }
}
