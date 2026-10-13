public class Player()
{
    public ushort position = Game.windowWidth;  //set to something unrealistic like windowWidth (this will index out of bounds)
    public const ushort maxVelocity = 13;       //higher speeds make the game impossible
    public const ushort minVelocity = 4;
    public ushort velocity = minVelocity;

    volatile public static Input.Movement steerDirection = Input.Movement.NONE;
    volatile public static Input.Movement speedDirection = Input.Movement.NONE;

    public void Update()
    {
        if (position == Game.windowWidth) 
        {
            //skip update in the first frame, the car doesnt exist yet
        }
        else
        {
            if (position > 1 && steerDirection == Input.Movement.LEFT) --position;
            else if (position < Game.windowWidth - 1 && steerDirection == Input.Movement.RIGHT) ++position;

            if (velocity > Player.minVelocity && speedDirection == Input.Movement.SLOWER)
            {
                --velocity;
                if (velocity == 6) Console.Clear();
            }
            else if (velocity < Player.maxVelocity && speedDirection == Input.Movement.FASTER) ++velocity;
        }
    }
}