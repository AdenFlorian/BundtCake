using static SDL2.SDL;

namespace BundtCake
{
    public class SdlWrapper
    {
        public static SDL_Event PollEvent()
        {
            SDL_Event sdlEvent;
            SDL_PollEvent(out sdlEvent);
            return sdlEvent;
        }
    }
}