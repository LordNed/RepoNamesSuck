using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor
{
    public static class Time
    {
        /// <summary> The time it took to render the last frame (in seconds). </summary>
        public static float DeltaTime { get; private set; }
        /// <summary> The time since the start of the application run (in seconds). </summary>
        public static float TimeSinceStart { get; private set; }

        internal static void Internal_UpdateTime(float deltaTime)
        {
            DeltaTime = deltaTime;
            TimeSinceStart += deltaTime;
        }
    }
}
