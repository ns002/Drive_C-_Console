#pragma warning disable CA2211  //public non-const variables
using Axis = WindowsConsole.Input.Axis;
public class Player
{
    static bool touchedGrass = false;
    public static void TouchGrass() => touchedGrass = true;
    public static bool HasDied()
    {
        if (!touchedGrass) return false;
        touchedGrass = false; return true;
    }
    public ushort position = Game.windowWidth;  //set to something unrealistic like windowWidth (this will index out of bounds)
    public const ushort maxVelocity = 13;       //higher speeds make the game impossible
    public const ushort minVelocity = 4;
    public ushort velocity = minVelocity;

    //You will have to look at WindowsConsole.Input.StartInputListener() for the keys
    volatile public static Axis steerDirection = Axis.Neutral;
    volatile public static Axis speedDirection = Axis.Neutral;

    public void Update()
    {
        if (position == Game.windowWidth) 
        {
            //skip update in the first frame, the car doesnt exist yet
        }
        else
        {
            if (position > 1 && steerDirection == Axis.Negative) --position;
            else if (position < Game.windowWidth - 1 && steerDirection == Axis.Positive) ++position;

            if (velocity > minVelocity && speedDirection == Axis.Negative)
            {
                --velocity;
                if (velocity == 6) Console.Clear();
            }
            else if (velocity < maxVelocity && speedDirection == Axis.Positive) ++velocity;
        }
    }
}