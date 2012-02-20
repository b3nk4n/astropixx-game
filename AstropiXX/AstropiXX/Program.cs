using System;

namespace AstropiXX
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Astropixx game = new Astropixx())
            {
                game.Run();
            }
        }
    }
#endif
}

