using System;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Core.Services
{
    /// <summary>Управления состоянием игры, отвечает за хранение текущего состояния и уведомление  о его изменении.</summary>
    public class GameStateService : IGameStateService
    {
        /// <summary>Текущее активное состояние игры.</summary>
        public GameState CurrentState { get; private set; }

        /// <summary>Событие, возникающее при смене состояния игры.</summary>
        public event EventHandler<GameState> StateChanged;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="GameStateService"/> с заданным начальным состоянием.
        /// </summary>
        /// <param name="initialState">Начальное состояние игры.</param>
        public GameStateService(GameState initialState)
        {
            CurrentState = initialState;
        }

        /// <summary>Изменяет текущее состояние игры на новое и вызывает событие смены состояния, если оно изменилось.</summary>
        /// <param name="newState">Целевое состояние.</param>
        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState)
                return;

            CurrentState = newState;
            StateChanged?.Invoke(this, newState);
        }
    }
}