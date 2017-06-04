using System;
using BundtCommon;
using SDL2;
using static SDL2.SDL;

namespace BundtCake
{
    public class Input
    {
        public static bool A;
        public static bool ADown;
        static bool ADownLock;
        public static bool AUp;
        public static bool W;
        public static bool S;
        public static bool D;
        public static bool LeftShift;

        public static int MouseX;
        public static int MouseY;

        static MyLogger _logger = new MyLogger(nameof(Input));

        internal static void Start()
        {
        }

        internal static void Update()
        {
        }

        internal static void Clear()
        {
            ADown = false;
            AUp = false;
            MouseX = 0;
            MouseY = 0;
        }

        public static void SetKeyDown(SDL_Keycode keycode)
        {
            _logger.LogInfo("SetKeyDown " + keycode);

            switch (keycode)
            {
                case SDL_Keycode.SDLK_a:
                    A = true;
                    if (ADownLock == false)
                    {
                        ADown = true;
                        ADownLock = true;
                    }
                    break;
                case SDL_Keycode.SDLK_d: D = true; break;
                case SDL_Keycode.SDLK_w: W = true; break;
                case SDL_Keycode.SDLK_s: S = true; break;
                case SDL_Keycode.SDLK_LSHIFT: LeftShift = true; break;
                default: break;
            }
        }

        public static void SetKeyUp(SDL_Keycode keycode)
        {

            switch (keycode)
            {
                case SDL_Keycode.SDLK_a:
                    A = false;
                    AUp = true;
                    ADownLock = false;
                    break;
                case SDL_Keycode.SDLK_d: D = false; break;
                case SDL_Keycode.SDLK_w: W = false; break;
                case SDL_Keycode.SDLK_s: S = false; break;
                case SDL_Keycode.SDLK_LSHIFT: LeftShift = false; break;
                default: break;
            }
        }

        internal static void OnMouseMotion(SDL_MouseMotionEvent motion)
        {
            MouseX += motion.xrel;
            MouseY += motion.yrel;
        }
    }
}