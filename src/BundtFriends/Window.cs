using System;
using System.Drawing;
using static SDL2.SDL;

namespace BundtCake
{
    public class Window
    {
        public IntPtr Win32hInstance => Win32Api.GetModuleHandle(null);
        public IntPtr Win32hWnd
        {
            get
            {
                var info = new SDL_SysWMinfo();
                SDL_GetWindowWMInfo(_sdlWindowPtr, ref info);
                return info.info.win.window;
            }
        }
        public uint Id => SDL_GetWindowID(_sdlWindowPtr);
        public string Title
        {
            get
            {
                return SDL_GetWindowTitle(_sdlWindowPtr);
            }
            set
            {
                SDL_SetWindowTitle(_sdlWindowPtr, value);
            }
        }

        IntPtr _sdlWindowPtr;

        public Window(string title, int xPosition, int yPosition, int width, int height, SDL_WindowFlags flags = 0)
        {
            _sdlWindowPtr = SDL_CreateWindow(title, xPosition, yPosition, width, height, flags);
        }

        public Rectangle GetSize()
        {
            int width, height;
            SDL_GetWindowSize(_sdlWindowPtr, out width, out height);
            return new Rectangle(0, 0, width, height);
        }

        public void Dispose()
        {
            SDL_DestroyWindow(_sdlWindowPtr);
        }
    }
}