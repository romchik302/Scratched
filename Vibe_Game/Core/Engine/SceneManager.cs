using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Vibe_Game.Scenes;

namespace Vibe_Game.Core.Engine
{
    /// <summary>
    /// Менеджер сцен игры. Отвечает за хранение, инициализацию, переключение и 
    /// делегирование обновлений (Update) и отрисовки (Draw) активной игровой сцены.
    /// </summary>
    public class SceneManager : DrawableGameComponent
    {
        private readonly Dictionary<string, BaseScene> _scenes = new();
        private BaseScene _currentScene;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="SceneManager"/>.
        /// </summary>
        /// <param name="game">Базовый объект игры Microsoft XNA/MonoGame.</param>
        public SceneManager(Microsoft.Xna.Framework.Game game) : base(game)
        {
            System.Diagnostics.Debug.WriteLine("SceneManager created");
        }

        /// <summary>
        /// Регистрирует новую сцену в менеджере и вызывает её метод инициализации.
        /// </summary>
        /// <param name="name">Уникальное строковое имя сцены для последующего доступа.</param>
        /// <param name="scene">Экземпляр класса сцены.</param>
        public void AddScene(string name, BaseScene scene)
        {
            _scenes[name] = scene;
            scene.Initialize();
        }

        /// <summary>
        /// Выполняет переключение на указанную сцену. 
        /// Освобождает ресурсы текущей сцены и загружает ресурсы новой.
        /// </summary>
        /// <param name="name">Имя сцены, на которую необходимо переключиться.</param>
        public void SwitchTo(string name)
        {
            if (_scenes.TryGetValue(name, out var scene))
            {
                _currentScene?.UnloadContent();
                _currentScene = scene;
                _currentScene.LoadContent();
            }
        }

        /// <summary>
        /// Вызывает метод Update текущей активной сцены, если она активна.
        /// </summary>
        /// <param name="gameTime">Снимок времени игры.</param>
        public override void Update(GameTime gameTime)
        {
            if (_currentScene != null && Enabled)
                _currentScene.Update(gameTime);
        }

        /// <summary>
        /// Вызывает метод Draw текущей активной сцены, если она видима.
        /// </summary>
        /// <param name="gameTime">Снимок времени игры.</param>
        public override void Draw(GameTime gameTime)
        {
            if (_currentScene != null && Visible)
                _currentScene.Draw(gameTime);
        }
    }
}