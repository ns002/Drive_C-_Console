using System.ComponentModel.Design;
using System.Text;
public static class GlobalEvent
{
    public static void Reset()
    {
        distance = coinsCollected = 0;
        playerDies = false;
    }
    public static ulong distance;
    public static ulong coinsCollected;
    public static bool playerDies;
}

public class Game
{
    readonly Player player = new();
    readonly PlayField field = new();

    public const ushort windowHeight = 30;
    public const ushort windowWidth = 50;
    volatile public static bool running = true;

    public static void Main()
    {
        Console.Clear();
        GlobalEvent.Reset();
        Game game = new();
        game.field.Create();
        game.Run();
    }
    
    public void Run()
    {
        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.UTF8;
        Input.StartInputListener(); //start the background input listener

        do {/* The order of operations is very important
             * First The playfield is updated so the lines in memory look as they will be rendered
             * Then other gameobjects will update
             * Then the field has to do a late update... in order to account for these objects in the scene
             */

            field.Update();
            
            player.Update();

            field.LateUpdate(ref player.position);

            field.Render();

            Console.WriteLine($"Speed: {(player.velocity + 3) * 10}km/h");
            Console.WriteLine($"Distance: {GlobalEvent.distance}m");
            Console.WriteLine($"Coins: {GlobalEvent.coinsCollected}");
            Console.WriteLine("Press escape to quit...");
            if (GlobalEvent.playerDies) 
            {
                if (TooBad()) Main();
                else running = false;
            };

            Thread.Sleep(150 - player.velocity * 10);

        } while (running);
    }

    public bool TooBad()
    {
        Console.WriteLine("Play Again? press: x");

        ConsoleKeyInfo answer = Console.ReadKey();
        if (answer.Key == ConsoleKey.X)
            return true;
        else
            return TooBad();

    }
}