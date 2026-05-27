using System;

namespace WaferFlow.Helpers
{
    public static class AppEvents
    {
        public static event EventHandler WaferAdded;

        public static void OnWaferAdded()
        {
            WaferAdded?.Invoke(null, EventArgs.Empty);
        }
    }
}