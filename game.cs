public class GameState
{
    public GameState()
    {
        distance = coins = 0;
    }
    public ulong distance;
    public ulong coins;

    //Do not reset these in the constructor
    volatile public static bool running = false;
}

public sealed class Game
{
    static Player? player = null;
    static PlayField? field = null;
    static GameState? state = null;

    public const ushort windowHeight = 30;
    public const ushort windowWidth = 50;

    static bool WaitingForInput() => !WindowsConsole.Input.WantToRestart() && GameState.running;

    public static void Run()
    {
        if (!GameState.running) return; //no need to initialize new memory if user wants to quit

        Console.Clear();
        state = new();
        player = new();
        field = new();
        field.Create();

        while (GameState.running) 
        {
            field.Update();
            
            player.Update();

            field.LateUpdate(ref player.position, ref state.coins, ref state.distance);

            field.Render();
            // TODO: make a renderer class that handles field.Render() plus a new GUI class
            // You just need to ensure that the unused trailing space of every row is explicitly padded with spaces out to the edge of the screen.
            // And also important, try to force the console to open with a forced size so the game doesn't break when there isn't enough space
            Console.WriteLine($"Speed: {(player.velocity + 3) * 10}km/h");
            Console.WriteLine($"Distance: {state.distance}m");
            Console.WriteLine($"Coins: {state.coins}");
            Console.WriteLine("\nPress escape to quit...");
            if (Player.HasDied()) 
            {
                Console.WriteLine("You touched grass. Press 'R' to play again.");
                while (WaitingForInput()) Thread.Sleep(1);
                Run(); //The loop will break whenever the input listener disconnects.
                return;
            }
            Thread.Sleep(150 - player.velocity * 10);
        }
    }
}