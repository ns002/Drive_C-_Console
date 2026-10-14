using System.Runtime.InteropServices;

public static class RandomExtensions
{
    public static bool Chance(this Random rand, int percentage)
    {
        return rand.Next(100) < percentage;
    }
}

public sealed partial class WindowsConsole
{
    /*
     SetLastError = true: Windows functions don't usually throw C# exceptions if they fail.
     Instead, they store a failure code in a secret CPU register.
     Setting this to true tells the .NET runtime: "If this Windows function fails,
     capture that error code immediately so I can read it later in C# using Marshal.GetLastWin32Error()."
    */

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [LibraryImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    const int STD_INPUT_HANDLE = -10;
    const uint ENABLE_ECHO_INPUT = 0x0004;
    const uint ENABLE_LINE_INPUT = 0x0002;
    const uint ENABLE_QUICK_EDIT = 0x0040;

    public static void Main()
    {
        IntPtr hInput = GetStdHandle(STD_INPUT_HANDLE);
        bool success = GetConsoleMode(hInput, out uint mode); // Read the current settings of the console
        if (!success) return;
        mode &= ~(ENABLE_ECHO_INPUT | ENABLE_LINE_INPUT | ENABLE_QUICK_EDIT);
        SetConsoleMode(hInput, mode);
        Console.CursorVisible = false;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Input.StartInputListener();
        Game.Run();
    }

    public sealed partial class Input
    {
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial short GetAsyncKeyState(int vKey);

        public enum Key : int
        {
            ESCAPE = 0x1B,
            LEFT = 0x25, UP = 0x26, RIGHT = 0x27, DOWN = 0x28,
            A = 0x41, D = 0x44, R = 0x52, W = 0x57, S = 0x53
        }
        static volatile bool rIsPressed = false;
        public static bool WantToRestart() => rIsPressed;
        public enum Axis : sbyte { Neutral = 0, Negative = -1, Positive = 1 }
        public static Axis ProcessAxis(Key neg, Key pos, Key altNeg, Key altPos)
        {
            bool negPressed = GetAsyncKeyState((int)neg) < 0 || GetAsyncKeyState((int)altNeg) < 0;
            bool posPressed = GetAsyncKeyState((int)pos) < 0 || GetAsyncKeyState((int)altPos) < 0;
            if (negPressed && !posPressed) return Axis.Negative;
            else if (posPressed && !negPressed) return Axis.Positive;
            else return Axis.Neutral;
        }

        public static void StartInputListener() // An instance method that can freely read/write to the class instance fields
        {
            if (Interlocked.CompareExchange(ref GameState.running, true, false)) return; _ = Task.Run(() =>
            {
                while (GameState.running)
                {
                    if (GetAsyncKeyState((int)Key.ESCAPE) < 0)
                    {
                        GameState.running = false;
                        Console.WriteLine("Quit-Signal Registered.");
                        return;
                    }
                    if (GetAsyncKeyState((int)Key.R) < 0) 
                         rIsPressed = true;
                    else rIsPressed = false;

                    Player.steerDirection = ProcessAxis(Key.A, Key.D, Key.LEFT, Key.RIGHT);
                    Player.speedDirection = ProcessAxis(Key.S, Key.W, Key.DOWN, Key.UP);

                    //never again
                    //while (Console.KeyAvailable) Console.ReadKey(true);
                }
            });
        }
    }
}
