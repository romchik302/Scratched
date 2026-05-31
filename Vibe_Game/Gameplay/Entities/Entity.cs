using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Vibe_Game.Gameplay.Entities
{
    /// <summary>Базовый игровой объект. Предоставляет общую функциональность для всех сущностей игры: позицию, движение, отрисовку, загрузку ресурсов и коллизии.</summary>
    public abstract class Entity
    {
        /// <summary>Текущая позиция объекта в мировых координатах.</summary>
        public Vector2 Position { get; set; }

        /// <summary>Текущая скорость объекта. Используется для расчета перемещения между кадрами.</summary>
        public Vector2 Velocity { get; set; }

        /// <summary>Показывает, активен ли объект. Если значение равно false, объект считается уничтоженным.</summary>
        public bool IsAlive { get; set; } = true;

        /// <summary>Основная текстура объекта. Используется базовой реализацией отрисовки.</summary>
        protected Texture2D Texture { get; set; }

        /// <summary>Цветовой множитель, применяемый при отрисовке. Может использоваться для эффектов урона, прозрачности и других визуальных состояний.</summary>
        protected Color Color { get; set; } = Color.White;

        /// <summary>Точка привязки спрайта при отрисовке. По умолчанию находится в начале координат текстуры.</summary>
        protected Vector2 Origin = Vector2.Zero;

        /// <summary>Обновляет состояние объекта. Базовая реализация выполняет перемещение на основе текущей скорости.</summary>
        /// <param name="gameTime">Время, прошедшее с момента последнего обновления.</param>
        public virtual void Update(GameTime gameTime)
        {
            // Базовая физика: позиция += скорость * время
            if (Velocity != Vector2.Zero)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                Position += Velocity * deltaTime;
            }
        }

        /// <summary>Отрисовывает объект на экране. Использует текстуру, позицию и цвет сущности.</summary>
        /// <param name="spriteBatch">Экземпляр SpriteBatch, используемый для отрисовки графики.</param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null && IsAlive)
            {
                spriteBatch.Draw(Texture, Position, null, Color, 0f, Origin, 1f, SpriteEffects.None, 0);
            }
        }

        /// <summary>Загружает ресурсы объекта. Переопределяется наследниками при необходимости.</summary>
        /// <param name="content">Менеджер контента MonoGame для загрузки графических ресурсов.</param>
        public virtual void LoadContent(ContentManager content) { }

        /// <summary>Возвращает область столкновения объекта. Используется системой коллизий и проверкой попаданий.</summary>
        public virtual Rectangle GetBounds()
        {
            if (Texture == null)
                return new Rectangle((int)Position.X, (int)Position.Y, 16, 16);

            return new Rectangle(
                (int)(Position.X - Origin.X),
                (int)(Position.Y - Origin.Y),
                Texture.Width,
                Texture.Height
            );
        }
    }
}