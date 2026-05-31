using System;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Сервис управления глобальным состоянием игры.
    /// Предоставляет доступ к текущему состоянию и уведомляет подписчиков при его изменении.
    /// </summary>
    public interface IGameStateService
    {
        /// <summary>
        /// Возвращает текущее состояние игры.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Событие, которое вызывается при смене состояния игры.
        /// </summary>
        event EventHandler<GameState> StateChanged;

        /// <summary>
        /// Переключает игру в новое состояние.
        /// </summary>
        /// <param name="newState">Целевое состояние игры.</param>
        void ChangeState(GameState newState);
    }

    /// <summary>
    /// Перечисление возможных состояний игрового цикла.
    /// </summary>
    public enum GameState
    {
        /// <summary>Главное меню или экран выбора.</summary>
        Menu,
        /// <summary>Активный игровой процесс.</summary>
        Playing,
        /// <summary>Игра приостановлена.</summary>
        Paused,
        /// <summary>Экран проигрыша или завершения сессии.</summary>
        GameOver
    }
}