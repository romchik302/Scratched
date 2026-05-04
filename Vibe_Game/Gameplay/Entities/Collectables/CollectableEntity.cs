using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Settings;
using Vibe_Game.Gameplay.Entities;

namespace Vibe_Game.Gameplay.Entities.Collectables
{
    /// <summary>Заготовка под предмет на пьедестале, Роман.</summary>
    public sealed class CollectableEntity : Entity
    {
        public CollectableEntity(Point tilePosition)
        {
            TilePosition = tilePosition;
        }

        public Point TilePosition { get; }

        /// <summary>Рисует временную метку предмета поверх пьедестала.</summary>
        public void DrawOnPedestal(SpriteBatch spriteBatch, Texture2D pixel, Rectangle pedestalBounds)
        {
            if (pixel == null || !IsAlive)
                return;

            Rectangle itemBounds = new Rectangle(
                pedestalBounds.Center.X - WorldConfig.TileSize / 8,
                pedestalBounds.Y + WorldConfig.TileSize / 5,
                WorldConfig.TileSize / 4,
                WorldConfig.TileSize / 4);

            spriteBatch.Draw(pixel, itemBounds, GameColors.CollectablePlaceholder);
        }
    }
}
