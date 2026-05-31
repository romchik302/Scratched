using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Engine;

namespace Vibe_Game.Scenes
{
    /// <summary>
    /// Базовый абстрактный класс для всех игровых сцен (главное меню, игровой процесс, экран паузы, загрузка и т.д.).
    /// Обеспечивает управление жизненным циклом сцены и предоставляет удобный доступ к системным сервисам игры.
    /// </summary>
    public abstract class BaseScene
    {
        /// <summary>
        /// Ссылка на основной экземпляр игры MonoGame.
        /// </summary>
        protected Game GameInstance { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BaseScene"/>.
        /// </summary>
        /// <param name="game">Основной экземпляр игры MonoGame.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если переданный экземпляр игры равен null.</exception>
        protected BaseScene(Game game)
        {
            GameInstance = game ?? throw new ArgumentNullException(nameof(game));
        }

        /// <summary>
        /// Возвращает зарегистрированный глобальный сервис игры по его типу через Service Locator.
        /// </summary>
        /// <typeparam name="T">Тип запрашиваемого сервиса (должен быть ссылочным типом).</typeparam>
        /// <returns>Экземпляр запрошенного сервиса или null, если сервис не зарегистрирован.</returns>
        protected T GetService<T>() where T : class
        {
            return GameInstance.Services.GetService<T>();
        }

        /// <summary>
        /// Вспомогательный метод для быстрого получения сервиса отрисовки спрайтов.
        /// </summary>
        /// <returns>Глобальный экземпляр <see cref="SpriteBatch"/>.</returns>
        protected SpriteBatch GetSpriteBatch() => GetService<SpriteBatch>();

        /// <summary>
        /// Вспомогательный метод для быстрого получения сервиса управления игровой камерой.
        /// </summary>
        /// <returns>Глобальный экземпляр <see cref="Camera"/>.</returns>
        protected Camera GetCamera() => GetService<Camera>();

        /// <summary>
        /// Вспомогательный метод для быстрого получения однопиксельной текстуры, используемой для отрисовки примитивов или заливки фона.
        /// </summary>
        /// <returns>Однопиксельная текстура <see cref="Texture2D"/>.</returns>
        protected Texture2D GetPixelTexture() => GetService<Texture2D>();

        /// <summary>
        /// Вызывается при создании или смене сцены для инициализации неподгружаемых компонентов, переменных и базовой логики.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Вызывается для загрузки графических, звуковых и других ассет-ресурсов, специфичных для данной сцены.
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Вызывается при закрытии или смене сцены для корректного освобождения занятых ресурсов и отписки от событий.
        /// </summary>
        public virtual void UnloadContent() { }

        /// <summary>
        /// Обновляет внутреннюю игровую логику сцены (ввод, физику, состояние сущностей). Вызывается каждый кадр.
        /// </summary>
        /// <param name="gameTime">Текущее игровое время.</param>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Отрисовывает визуальные элементы сцены на экране. Вызывается каждый кадр после метода Update.
        /// </summary>
        /// <param name="gameTime">Текущее игровое время.</param>
        public virtual void Draw(GameTime gameTime) { }
    }
}
