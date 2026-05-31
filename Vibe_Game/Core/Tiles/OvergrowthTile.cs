using Microsoft.Xna.Framework;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>
    /// Представляет декоративный тайл с густой травой или ветками.
    /// Является проходимым, но запрещает спавн врагов на этом участке.
    /// </summary>
    public sealed class OvergrowthTile : Tile
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса OvergrowthTile.
        /// </summary>
        /// <param name="gridPosition">Позиция тайла в сетке уровня.</param>
        /// <param name="visualFrame">Опциональный индекс визуального кадра для вариативности спрайта.</param>
        public OvergrowthTile(Point gridPosition, int? visualFrame = null) : base(gridPosition)
        {
            VisualFrame = visualFrame;
        }

        /// <summary>
        /// Индекс кадра или варианта текстуры. Если null, используется текстура по умолчанию.
        /// </summary>
        public int? VisualFrame { get; }

        /// <inheritdoc />
        public override bool CanHostEnemy => false;

        /// <inheritdoc />
        public override Color Tint => GameColors.Overgrowth;
    }
}