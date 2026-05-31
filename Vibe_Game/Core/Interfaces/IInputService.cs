using Microsoft.Xna.Framework.Input;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Сервис для обработки пользовательского ввода. 
    /// Преобразует сырые события ввода (клавиатура, геймпад) в высокоуровневые игровые действия.
    /// </summary>
    public interface IInputService
    {
        /// <summary>
        /// Возвращает true только в тот кадр, когда действие было инициировано (нажато).
        /// </summary>
        bool IsActionPressed(InputAction action);

        /// <summary>
        /// Возвращает true, пока действие удерживается (активно).
        /// </summary>
        bool IsActionDown(InputAction action);

        /// <summary>
        /// Возвращает true в кадр, когда действие было завершено (отпущено).
        /// </summary>
        bool IsActionUp(InputAction action);

        /// <summary>
        /// Обновляет состояние ввода. Должен вызываться один раз в начале каждого кадра игры.
        /// </summary>
        void Update();
    }

    /// <summary>
    /// Перечисление доступных действий игрока.
    /// </summary>
    public enum InputAction
    {
        MoveUp, MoveDown, MoveLeft, MoveRight,
        ShootUp, ShootDown, ShootLeft, ShootRight,
        Fire, Pause, Interact
    }
}