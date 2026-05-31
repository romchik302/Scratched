using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;

namespace Vibe_Game.Gameplay.Entities.Player
{
    /// <summary>
    /// Компонент загрузки контента, отвечающий за импорт и хранение графических ассет-данных игрока.
    /// </summary>
    internal class PlayerContentLoader : IPlayerContentLoader
	{
        /// <summary>
        /// Указывает, были ли ресурсы игрока успешно загружены в память.
        /// </summary>
        public bool IsContentLoaded { get; private set; } = false;

        /// <summary>
        /// Спрайт-лист (текстура) со всеми анимациями персонажа игрока.
        /// </summary>
        public Texture2D PlayerTexture { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр загрузчика контента игрока.
        /// </summary>
        public PlayerContentLoader() { }

        /// <summary>
        /// Загружает текстуру игрока из графических ресурсов с помощью предоставленного менеджера контента.
        /// </summary>
        /// <param name="content">Менеджер игрового контента MonoGame/XNA.</param>
        public void LoadContent(ContentManager content)
		{
			PlayerTexture = content.Load<Texture2D>("player_sheet");
			IsContentLoaded = true;
		}
	}
}
