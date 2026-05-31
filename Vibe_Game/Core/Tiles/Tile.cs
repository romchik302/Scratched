using Microsoft.Xna.Framework;

namespace Vibe_Game.Core.Tiles
{
    /// <summary>
    /// Абстрактный базовый класс для всех тайлов на сетке игрового уровня. 
    /// Хранит состояние тайла, его положение и правила взаимодействия с другими сущностями.
    /// </summary>
    public abstract class Tile
    {
        /// <summary>
        /// Инициализирует новый экземпляр тайла.
        /// </summary>
        /// <param name="gridPosition">Позиция тайла в сетке (колонна, строка).</param>
        protected Tile(Point gridPosition)
        {
            GridPosition = gridPosition;
        }

        /// <summary>Координаты тайла в системе сетки уровня.</summary>
        public Point GridPosition { get; }

        /// <summary>Указывает, находится ли в данный момент враг на этом тайле.</summary>
        public bool HasEnemy { get; set; }

        /// <summary>Определяет, может ли игрок или враг пройти через этот тайл. По умолчанию true.</summary>
        public virtual bool IsWalkable => true;

        /// <summary>Указывает, является ли этот тайл кнопкой (активируемым объектом). По умолчанию false.</summary>
        public virtual bool HasButton => false;

        /// <summary>
        /// Вычисляемое свойство: может ли тайл содержать врага. 
        /// Основано на проходимости и отсутствии объектов взаимодействия (кнопок).
        /// </summary>
        public virtual bool CanHostEnemy => IsWalkable && !HasButton;

        /// <summary>Цвет отрисовки тайла. Переопределяется наследниками для уникальных текстур/цветов.</summary>
        public abstract Color Tint { get; }

        /// <summary>Указывает, снижает ли этот тайл трение (например, эффект льда). Влияет на физику движения.</summary>
        public bool ReducesFriction { get; set; } = false;
    }
}
