// IsaacClone.Game/Program.cs
using System;

namespace Vibe_Game
{
    /// <summary>
    /// Статический класс, содержащий точку входа в приложение.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Главная точка входа в программу. Инициализирует игровой экземпляр и управляет его жизненным циклом.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
            {
                try
                {
                    game.Run();
                }
                catch (Exception ex)
                {
                    // Логирование ошибки
                    LogError(ex);

#if DEBUG
                    // В дебаге показываем окно с ошибкой
                    System.Windows.Forms.MessageBox.Show(
                        $"Game startup error:\n{ex.Message}\n\n{ex.StackTrace}",
                        "Error",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );
#endif
                }
            }   
        }

        /// <summary>
        /// Записывает детали возникшего исключения в файл "error.log".
        /// </summary>
        /// <param name="ex">Объект исключения, которое необходимо залогировать.</param>
        static void LogError(Exception ex)
        {
            // Запись ошибки в файл
            string logPath = "error.log";
            string logMessage = $"[{DateTime.Now}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n\n";

            try
            {
                System.IO.File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Не удалось записать лог - игнорируем
            }
        }
    }
}
