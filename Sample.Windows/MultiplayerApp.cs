using Stride.Engine;

namespace Multiplayer
{
    class MultiplayerApp
    {
        static void Main(string[] args)
        {
            using var game = new Game();
            game.TreatNotFocusedLikeMinimized = false;
            game.Run();
        }
    }
}
