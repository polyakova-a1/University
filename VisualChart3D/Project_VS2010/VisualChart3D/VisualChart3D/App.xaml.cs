// This is a personal academic project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace VisualChart3D
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
    }
    /// <summary>
    /// Обработчик события отрисовки выбранного объекта
    /// </summary>
    public static class CallBackPoint
    {
        public delegate void callbackEvent(int objectNumber);
        public static callbackEvent callbackEventHandler;
    }
}
