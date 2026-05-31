using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Core.Engine
{
    /// <summary>Реализация привязок клавиш по умолчанию.</summary>
    public class DefaultInputBindings : IInputBindings
    {
        /// <summary>Словарь, хранящий соответствие игровых действий массивам назначенных клавиш.</summary>
        private readonly Dictionary<InputAction, Keys[]> _bindings;

        /// <summary>Инициализирует новый экземпляр класса DefaultInputBindings и задаёт привязки клавиш по умолчанию.</summary>
        public DefaultInputBindings()
        {
            _bindings = new Dictionary<InputAction, Keys[]>
            {
                [InputAction.MoveUp] = new[] { Keys.W },
                [InputAction.MoveDown] = new[] { Keys.S },
                [InputAction.MoveLeft] = new[] { Keys.A },
                [InputAction.MoveRight] = new[] { Keys.D },
                [InputAction.ShootUp] = new[] { Keys.Up },
                [InputAction.ShootDown] = new[] { Keys.Down },
                [InputAction.ShootLeft] = new[] { Keys.Left },
                [InputAction.ShootRight] = new[] { Keys.Right },
                [InputAction.Fire] = new[] { Keys.Space },
                [InputAction.Pause] = new[] { Keys.Escape },
                [InputAction.Interact] = new[] { Keys.E }
            };
        }

        /// <summary>Возвращает перечисление клавиш, привязанных к указанному игровому действию.</summary>
        public IEnumerable<Keys> GetKeysForAction(InputAction action)
        {
            return _bindings.TryGetValue(action, out var keys)
                ? keys
                : Enumerable.Empty<Keys>();
        }

        /// <summary>Назначает массив клавиш для определённого игрового действия.</summary>
        /// <param name="action">Игровое действие, для которого изменяется привязка.</param>
        /// <param name="keys">Массив клавиш, которые будут назначены на данное действие.</param>
        public void SetBinding(InputAction action, Keys[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new System.ArgumentException("Keys array cannot be null or empty", nameof(keys));

            _bindings[action] = keys;
        }

        /// <summary>Загружает настройки привязок клавиш из файла конфигурации.</summary>
        /// <param name="path">Путь к файлу конфигурации (JSON/XML).</param>
        public void LoadFromFile(string path)
        {
            // TODO: Реализовать загрузку из JSON/XML
            // Пример структуры JSON:
            // {
            //   "MoveUp": ["W", "Up"],
            //   "MoveDown": ["S", "Down"],
            //   ...
            // }
            // Для прототипа оставляем привязки по умолчанию и игнорируем файл.
        }

        /// <summary>Сохраняет текущие настройки привязок клавиш в файл конфигурации.</summary>
        /// <param name="path">Путь к файлу, в который будут записаны настройки.</param>
        public void SaveToFile(string path)
        {
            // TODO: Реализовать сохранение в JSON/XML
            // Для прототипа ничего не сохраняем.
        }

        IEnumerable<Keys> IInputBindings.GetKeysForAction(InputAction action) => GetKeysForAction(action);
    }
}