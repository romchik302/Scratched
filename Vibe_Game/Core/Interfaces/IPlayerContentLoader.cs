using Microsoft.Xna.Framework.Content;

namespace Vibe_Game.Core.Interfaces
{
    /// <summary>
    /// Контракт для сущностей, которым требуются внешние ресурсы (текстуры, звуки).
    /// Позволяет отделить логику загрузки от инициализации игрового объекта.
    /// </summary>
    public interface IPlayerContentLoader
    {
        /// <summary>
        /// Возвращает true, если все необходимые ресурсы были успешно загружены.
        /// </summary>
        bool IsContentLoaded { get; }

        /// <summary>
        /// Загружает ресурсы, необходимые объекту, используя предоставленный менеджер контента.
        /// </summary>
        /// <param name="content">Экземпляр ContentManager для загрузки ассетов.</param>
        void LoadContent(ContentManager content);
    }
}