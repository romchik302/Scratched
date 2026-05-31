using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Интерфейс для управления привязками клавиш (Input Mapping).
    /// Позволяет абстрагировать игровые действия от конкретных физических клавиш,
    /// поддерживая переназначение и сохранение настроек управления.
    /// </summary>
    public interface IInputBindings
    {
        /// <summary>
        /// Возвращает список физических клавиш, назначенных на указанное игровое действие.
        /// </summary>
        /// <param name="action">Игровое действие (например, MoveUp, Jump).</param>
        IEnumerable<Keys> GetKeysForAction(InputAction action);

        /// <summary>
        /// Назначает новые клавиши для конкретного игрового действия.
        /// </summary>
        /// <param name="action">Игровое действие для изменения.</param>
        /// <param name="keys">Массив клавиш, которые будут активировать это действие.</param>
        void SetBinding(InputAction action, Keys[] keys);

        /// <summary>
        /// Загружает пользовательские настройки управления из файла (например, JSON или XML).
        /// </summary>
        /// <param name="path">Путь к файлу конфигурации.</param>
        void LoadFromFile(string path);

        /// <summary>
        /// Сохраняет текущие привязки клавиш в файл для последующего использования.
        /// </summary>
        /// <param name="path">Путь к файлу сохранения.</param>
        void SaveToFile(string path);
    }
}