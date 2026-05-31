using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vibe_Game.Core.Interfaces;
using Vibe_Game.Core.Settings;

namespace Vibe_Game.Scenes
{
    /// <summary>
    /// Сцена отображения финальных титров (экран победы).
    /// Выводит информацию об авторах игры и ожидает нажатия клавиши для возврата в главное меню.
    /// </summary>
    internal sealed class CreditsScene : BaseScene
    {
        private readonly IInputService _inputService;
        private SpriteFont _font;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CreditsScene"/>.
        /// </summary>
        /// <param name="game">Основной экземпляр игры MonoGame.</param>
        /// <param name="inputService">Сервис для проверки нажатия клавиш.</param>
        public CreditsScene(Game game, IInputService inputService)
            : base(game)
        {
            _inputService = inputService;
        }

        /// <inheritdoc />
        public override void LoadContent()
        {
            _font = GameInstance.Content.Load<SpriteFont>("room_font");
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            if (IsAnySkipActionPressed())
                ((Game1)GameInstance).ShowMainMenu();
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = GetSpriteBatch();
            Texture2D pixel = GetPixelTexture();
            if (spriteBatch == null || pixel == null || _font == null)
                return;

            Viewport viewport = GameInstance.GraphicsDevice.Viewport;
            Rectangle panelRect = new Rectangle(viewport.Width / 2 - 300, viewport.Height / 2 - 180, 600, 360);

            GameInstance.GraphicsDevice.Clear(GameColors.VictoryBackground);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            spriteBatch.Draw(pixel, panelRect, GameColors.VictoryPanel);
            DrawBorder(spriteBatch, pixel, panelRect);
            DrawCenteredText(spriteBatch, "VICTORY", new Vector2(viewport.Width / 2f, panelRect.Y + 76f), GameColors.VictoryText, 1.25f, GameColors.RoomLabelShadow);
            DrawCenteredText(spriteBatch, "created by", new Vector2(viewport.Width / 2f, panelRect.Y + 136f), GameColors.VictoryAccent, 0.62f);
            DrawCenteredText(spriteBatch, "Roman Akst, Yana Egorova-Ekimkova", new Vector2(viewport.Width / 2f, panelRect.Y + 174f), GameColors.VictoryText, 0.48f);
            DrawCenteredText(spriteBatch, "Kamilla Dzhumasheva, Michail Rodomakin", new Vector2(viewport.Width / 2f, panelRect.Y + 204f), GameColors.VictoryText, 0.48f);
            DrawCenteredText(spriteBatch, "PRESS E TO CONTINUE", new Vector2(viewport.Width / 2f, panelRect.Bottom - 78f), GameColors.VictoryText, 0.62f);

            spriteBatch.End();
        }

        /// <summary>Проверяет все игровые действия, которыми можно пропустить титры.</summary>
        private bool IsAnySkipActionPressed()
        {
            return  _inputService.IsActionPressed(InputAction.Interact);
        }

        /// <summary>Рисует рамку панели титров тем же стилем, что и остальные меню.</summary>
        private static void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel, Rectangle panelRect)
        {
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Y - 3, panelRect.Width + 6, 3), GameColors.VictoryOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Bottom, panelRect.Width + 6, 3), GameColors.VictoryOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.X - 3, panelRect.Y, 3, panelRect.Height), GameColors.VictoryOutline);
            spriteBatch.Draw(pixel, new Rectangle(panelRect.Right, panelRect.Y, 3, panelRect.Height), GameColors.VictoryOutline);
        }

        /// <summary>Рисует текст по центру выбранной точки с необязательной тенью.</summary>
        private void DrawCenteredText(SpriteBatch spriteBatch, string text, Vector2 center, Color color, float scale, Color? shadowColor = null)
        {
            Vector2 size = _font.MeasureString(text) * scale;
            Vector2 position = center - size / 2f;

            if (shadowColor.HasValue)
                spriteBatch.DrawString(_font, text, position + new Vector2(2f, 2f), shadowColor.Value, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            spriteBatch.DrawString(_font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
