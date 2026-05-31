using Vibe_Game.Gameplay.Entities.Player;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для эффектов предметов, которые могут изменять характеристики игрока.
    /// Используется для реализации бонусов, дебаффов и временных усилений.
    /// </summary>
    public interface IItemEffect
    {
        /// <summary>Отображаемое имя эффекта (например, "Сила великана").</summary>
        string Name { get; }

        /// <summary>Краткое описание эффекта (например, "Увеличивает урон на 10%").</summary>
        string Description { get; }

        /// <summary>
        /// Применяет модификатор к характеристикам игрока.
        /// </summary>
        /// <param name="stats">Объект характеристик игрока.</param>
        void Apply(PlayerStats stats);

        /// <summary>
        /// Отменяет действие эффекта, возвращая характеристики к исходному состоянию.
        /// </summary>
        /// <param name="stats">Объект характеристик игрока.</param>
        void Remove(PlayerStats stats);
    }
}