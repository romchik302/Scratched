using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Vibe_Game.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Vibe_Game.Core.Services
{
    /// <summary>Сервис ввода, обеспечивающий обработку клавиатуры и мыши с поддержкой настраиваемых привязок действий.</summary>
    public class InputService : IInputService
    {
        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private readonly IInputBindings _bindings;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InputService"/> с использованием указанных привязок клавиш.
        /// </summary>
        /// <param name="bindings">Интерфейс привязок клавиш для сопоставления действий с вводом.</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если bindings равен null.</exception>
        public InputService(IInputBindings bindings)
        {
            _bindings = bindings ?? throw new System.ArgumentNullException(nameof(bindings));
        }

        /// <summary>Считывает и обновляет текущие состояния клавиатуры и мыши. Должен вызываться каждый кадр.</summary>
        public void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Проверяет, была ли клавиша, привязанная к указанному действию, нажата в текущем кадре (событие начала нажатия).
        /// </summary>
        /// <param name="action">Действие, для которого выполняется проверка.</param>
        /// <returns>True, если клавиша была нажата в этом кадре (ранее была отпущена).</returns>
        public bool IsActionPressed(InputAction action)
        {
            var keys = _bindings.GetKeysForAction(action);

            foreach (var key in keys)
            {
                if (_currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет, удерживается ли в данный момент любая из клавиш, привязанных к указанному действию.
        /// </summary>
        /// <param name="action">Действие, для которого выполняется проверка.</param>
        /// <returns>True, если хотя бы одна клавиша действия находится в состоянии нажатия.</returns>
        public bool IsActionDown(InputAction action)
        {
            var keys = _bindings.GetKeysForAction(action);

            foreach (var key in keys)
            {
                if (_currentKeyState.IsKeyDown(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет, была ли левая кнопка мыши нажата в текущем кадре (событие начала нажатия).
        /// </summary>
        /// <returns>True, если кнопка нажата сейчас и была отпущена в предыдущем кадре.</returns>
        public bool IsMouseButtonPressed()
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed &&
                   _previousMouseState.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Возвращает текущую позицию курсора мыши в экранных координатах.
        /// </summary>
        /// <returns>Вектор, содержащий X и Y координаты курсора.</returns>
        public Vector2 GetMousePosition()
        {
            return new Vector2(_currentMouseState.X, _currentMouseState.Y);
        }

        /// <summary>
        /// Проверяет, была ли клавиша, привязанная к указанному действию, отпущена в текущем кадре.
        /// </summary>
        /// <param name="action">Действие, для которого выполняется проверка.</param>
        /// <returns>True, если клавиша была отпущена в этом кадре (ранее была нажата).</returns>
        public bool IsActionUp(InputAction action)
        {
            var keys = _bindings.GetKeysForAction(action);

            foreach (var key in keys)
            {
                if (_currentKeyState.IsKeyUp(key) && _previousKeyState.IsKeyDown(key))
                    return true;
            }

            return false;
        }
    }
}