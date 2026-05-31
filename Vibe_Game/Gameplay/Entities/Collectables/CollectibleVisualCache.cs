using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Gameplay.Entities.Collectables;

/// <summary>Все визуалы пьедестала и предметов — один спрайт-лист <see cref="CollectibleConfig.CollectablesTexture"/>.</summary>
public sealed class CollectibleVisualCache
{
    private Texture2D _sheet;

    /// <summary>
    /// Получить общую текстуру спрайт-листа.
    /// </summary>
    public Texture2D Sheet => _sheet;

    /// <summary>
    /// Загружает общий спрайт-лист предметов и элементов пьедестала в память.
    /// В случае ошибки загрузки создает процедурную текстуру-заглушку.
    /// </summary>
    /// <param name="content">Менеджер контента.</param>
    /// <param name="device">Графическое устройство.</param>
    public void Load(ContentManager content, GraphicsDevice device)
    {
        _sheet = TryLoadSheet(content, device);
    }

    /// <summary>
    /// Возвращает текстуру для отрисовки предмета.
    /// </summary>
    public Texture2D GetSheet(CollectableKind _) => _sheet;

    /// <summary>
    /// Возвращает текстуру, используемую для отображения выпадающих объектов здоровья.
    /// </summary>
    public Texture2D GetHealthTexture(CollectableKind _) => _sheet;

    /// <summary>
    /// Определяет номер строки в спрайт-листе на основе типа предмета.
    /// </summary>
    /// <param name="kind">Тип предмета.</param>
    /// <returns>Индекс строки.</returns>
    public static int GetSheetRow(CollectableKind kind) =>
        kind switch
        {
            CollectableKind.Totem => CollectibleConfig.CollectablesSheetTotemRow,
            CollectableKind.Feather => CollectibleConfig.CollectablesSheetFeatherRow,
            CollectableKind.Fang => CollectibleConfig.CollectablesSheetFangRow,
            CollectableKind.WeaponProjectile => CollectibleConfig.CollectablesSheetWeaponProjectileRow,
            CollectableKind.WeaponSword => CollectibleConfig.CollectablesSheetWeaponSwordRow,
            CollectableKind.HealthSmall => CollectibleConfig.CollectablesSheetHealthSmallRow,
            CollectableKind.HealthLarge => CollectibleConfig.CollectablesSheetHealthLargeRow,
            _ => CollectibleConfig.CollectablesSheetTotemRow
        };

    /// <summary>
    /// Вычисляет прямоугольник исходного изображения для отрисовки конкретного кадра предмета.
    /// </summary>
    /// <param name="kind">Тип предмета.</param>
    /// <param name="frameIndex">Индекс кадра (анимации).</param>
    public Rectangle GetSourceRect(CollectableKind kind, int frameIndex)
    {
        int row = GetSheetRow(kind);
        return GetSourceRect(row, frameIndex);
    }

    /// <summary>
    /// Вычисляет прямоугольник исходного изображения для основания пьедестала.
    /// </summary>
    public Rectangle GetPedestalBaseSourceRect(int frameIndex) =>
        GetSourceRect(CollectibleConfig.CollectablesSheetPedestalRow, frameIndex);

    /// <summary>
    /// Базовый метод для получения прямоугольника кадра.
    /// </summary>
    /// <param name="row">Строка в атласе.</param>
    /// <param name="frameIndex">Индекс кадра.</param>
    public Rectangle GetSourceRect(int row, int frameIndex)
    {
        if (_sheet == null)
            return Rectangle.Empty;

        int rows = Math.Max(1, CollectibleConfig.CollectablesSheetRowCount);
        int cols = Math.Max(1, CollectibleConfig.CollectablesSheetFramesPerRow);
        int fh = Math.Max(1, _sheet.Height / rows);
        int fw = Math.Max(1, _sheet.Width / cols);
        int f = Math.Abs(frameIndex) % cols;
        int r = Math.Clamp(row, 0, rows - 1);
        return new Rectangle(f * fw, r * fh, fw, fh);
    }

    private static Texture2D TryLoadSheet(ContentManager content, GraphicsDevice device)
    {
        if (content != null)
        {
            try
            {
                return content.Load<Texture2D>(CollectibleConfig.CollectablesTexture);
            }
            catch
            {
            }
        }

        return CreatePlaceholderCombinedSheet(device);
    }

    private static Texture2D CreatePlaceholderCombinedSheet(GraphicsDevice device)
    {
        int cols = CollectibleConfig.CollectablesSheetFramesPerRow;
        int rows = CollectibleConfig.CollectablesSheetRowCount;
        const int fw = 18;
        const int fh = 18;
        var tex = new Texture2D(device, fw * cols, fh * rows);
        Color[] data = new Color[fw * cols * fh * rows];

        for (int row = 0; row < rows; row++)
        {
            float hue = row / (float)Math.Max(1, rows - 1);
            Color baseColor = Color.Lerp(new Color(120, 90, 70), new Color(200, 180, 140), hue);

            for (int fi = 0; fi < cols; fi++)
            {
                for (int y = 0; y < fh; y++)
                {
                    for (int x = 0; x < fw; x++)
                    {
                        bool edge = x == 0 || y == 0 || x == fw - 1 || y == fh - 1;
                        int px = fi * fw + x;
                        int py = row * fh + y;
                        Color c = edge ? baseColor * 0.55f : baseColor * (0.75f + fi * 0.06f);
                        data[py * (fw * cols) + px] = c;
                    }
                }
            }
        }

        tex.SetData(data);
        return tex;
    }
}
