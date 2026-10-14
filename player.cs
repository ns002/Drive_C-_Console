using Axis = WindowsConsole.Input.Axis;
public class Player
{
    static bool died = false;
    public static void Kill() => died = true;
    public static bool HasDied()
    {
        if (!died) return false;
        died = false; return true;
    }
    public ushort position = Game.windowWidth;  //set to something unrealistic like windowWidth (this will index out of bounds)
    public const ushort maxVelocity = 13;       //higher speeds make the game impossible
    public const ushort minVelocity = 4;
    public ushort velocity = minVelocity;

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