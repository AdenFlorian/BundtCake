using System;
using System.Drawing;
using SDL2;

namespace BundtCake
{
    public class Window
    {
        public IntPtr Win32hInstance => Win32Api.GetModuleHandle(null);
        public IntPtr Win32hWnd
        {
            get
            {
                var info = new SDL.SDL_SysWMinfo();
                SDL.SDL_GetWindowWMInfo(_sdlWindowPtr, ref info);
                return info.info.win.window;
            }
        }

        IntPtr _sdlWindowPtr;

        public Window(string title, int xPosition, int yPosition, int width, int height)
        {
            _sdlWindowPtr = SDL.SDL_CreateWindow(title, xPosition, yPosition, width, height, 0);
        }

        public Rectangle GetSize()
        {
            int width, height;
            SDL.SDL_GetWindowSize(_sdlWindowPtr, out width, out height);
            return new Rectangle(0, 0, width, height);
        }
    }
}