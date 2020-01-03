
namespace OlcSideScrollingConsoleGame
{
    public class Enum
    {
        public enum State
        {
            SplashScreen,
            Menu,
            WorldMap,
            GameMap,
            Pause,
            Settings,
            GameOver,
            End,
            HighScore
        };

        public enum Direction
        {
            SOUTH = 0,
            WEST = 1,
            NORTH = 2,
            EAST = 3
        }

        public enum GraphicsState
        {
            Standing,
            Walking,
            Celebrating,
            Dead,
            Falling,
            Jumping,
            TakingDamage
        }


        public enum PlayerOrientation
        {
            Right = 0,
            Left = 1
        }

        public enum NATURE
        {
            TALK,
            WALK
        }

    }
  
}
