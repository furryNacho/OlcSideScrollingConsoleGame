using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OlcSideScrollingConsoleGame.Global.GlobalNamespace
{
    //static class StaticRes
    //{ }
    public static class SoundRef
    {
        public static string Jump { get { return "jump_01.wav"; } } // jump_01.wav
        public static string Land { get { return "jump_02.wav"; } } // jump_02.wav
        public static string Damage { get { return "jumpland.wav"; } } // jumpland.wav
        public static string DamageHero { get { return "monster.wav"; } }
        public static string PickUp { get { return "key01.wav"; } } // key01.wav // coin01.wav

        public static string BGSoundWorld { get { return "uno.wav"; } } //uno.wav 
        public static string BGSoundGame { get { return "puttekong.wav"; } } // puttekong.wav 
        public static string BGSoundFinalStage { get { return "uno.wav"; } }
        public static string BGSoundEnd { get { return "uno.wav"; } }
    }

    public static class SplashScreenRef
    {
        public static string Start { get { return "splashstart"; } }
        public static string End { get { return "splashend"; } }
        public static string AltEnd { get { return "splashaltend"; } }
        public static string SuperAltEnd { get { return "splashsuperaltend"; } }
    }

}
