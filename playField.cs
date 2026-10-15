using System.Text;
public class PlayField
{
    readonly static string COLOR_RESET = "\e[0m";
    readonly static string COLOR_RED = "\e[31m";
    readonly static string COLOR_GREEN = "\e[32m";
    readonly static string COLOR_YELLOW = "\e[33m";
    readonly static string COLOR_MAGENTA = "\e[35m";
    readonly static string COLOR_WHITE = "\e[37m";

    const char roadsideChar = '▓';
    const char roadChar = ' ';
    const char playerChar = '▲';
    const char grassChar = '░';
    const char coinChar = '*';

    private static short GetRandomDirection() => (short)Random.Shared.Next(-1, 2); //get's the random direction the road will move to

    bool roadSideColorDecider = true;
    private string GetRoadsideColored()
    {
        return roadSideColorDecider
               ? $"{COLOR_WHITE}{roadsideChar}"
               : $"{COLOR_RED}{roadsideChar}";
    }

    const ushort roadWidth = 12;
    const ushort minRoadSP = 1;
    const ushort maxRoadSP = Game.windowWidth - roadWidth - 3;
    short roadStartingPoint = (short)Random.Shared.Next(minRoadSP, maxRoadSP);
    StringBuilder[] lines = [];
    
    public void Update()
    {
    /* remembering the discarded line to move it back to the top to reduce GC collection
     * keep in mind that you need to clear the old string before adding the new one to it though
     */ StringBuilder lastLine = lines[Game.windowHeight - 1].Clear();
        
        //shifting the lines
        for (int i = Game.windowHeight - 1; i > 0; --i)
        {
            int j = i - 1;
            lines[i] = lines[j];
        }

        lines[0] = lastLine;
        CreateLine(lines[0]);   //the newly generated line will always be at the top of the stack
    }

    ushort roadCenterFinder = 0;
    const short halfRoadWidth = roadWidth / 2;
    public void LateUpdate(
        ref ushort playerPosition,
        ref ulong coinsCollected,
        ref ulong distanceTravelled)
    {
        // Spawn coins with a 1% chance on every road space
        int length = lines[0].Length;
        for (int i = 0; i < length; ++i)
        {
            if (lines[0][i] == roadChar && Random.Shared.Chance(1))
            {
                lines[0][i] = coinChar;
                length = lines[0].Insert(i, COLOR_YELLOW).Length;
                i += COLOR_YELLOW.Length;
            }
        }

        // Checking the state of the final line below
        ushort visibleSpotCounter = 0;
        length = lines[29].Length;
        for (int i = 0; i < length; ++i)
        {
            char currentChar = lines[29][i];
            switch (currentChar)
            {
                case roadsideChar or grassChar: break;
                case roadChar or coinChar: // Find the road center if the finder is active
                    if (playerPosition == Game.windowWidth && ++roadCenterFinder == halfRoadWidth)
                        playerPosition = (ushort)(visibleSpotCounter + 1);
                    break;
                default: continue;  //skip noise characters, like color codes entirely
            }

            // Placing the player & checking collision
            if (++visibleSpotCounter == playerPosition)
            {
                lines[29][i] = playerChar;
                if (currentChar == coinChar) ++coinsCollected;
                else if (currentChar == grassChar) {
                    Player.TouchGrass();
                    lines[29].Insert(i, COLOR_MAGENTA);
                    lines[29].Insert(i + COLOR_MAGENTA.Length + 1, COLOR_GREEN); //don't forget the grass doesn't append unecessary color flags
                    return;
                }
                lines[29].Insert(i, COLOR_MAGENTA);
                ++distanceTravelled;
                return;
            }
        }
    }

    public void Render()
    {
        string canvas = string.Empty;
        Console.SetCursorPosition(0, 0); //avoid Console.Clear() to prevent flashing and/or artefacts
        for (int i = 0; i < Game.windowHeight; ++i) canvas += (lines[i].ToString() + '\n');
        Console.Write(canvas);
    }

    public void Create()
    {
        lines = new StringBuilder[Game.windowHeight];
        for (int i = Game.windowHeight - 1; i >= 0; --i)
        {
            lines[i] = new();
            CreateLine(lines[i]);
        }
    }
    ulong lineNumber = 0;
    private void CreateLine(StringBuilder lineReference)
    {
        short direction = GetRandomDirection(); //either -1 (left), 0 (straight) or 1 (right)
        if ((direction < 0 && roadStartingPoint > minRoadSP) || //move road left?
            (direction > 0 && roadStartingPoint < maxRoadSP))   //move road right?
            roadStartingPoint += direction;

        short grassRightLength = (short)(Game.windowWidth - roadStartingPoint - roadWidth - 2);
        roadSideColorDecider = !roadSideColorDecider; //switch behaviour Red & White roadside every time

        lineReference.Append(COLOR_GREEN).Append(grassChar, roadStartingPoint); //grass left
        lineReference.Append(GetRoadsideColored());                             //roadside left
        lineReference.Append(roadChar, roadWidth);                              //road
        lineReference.Append(GetRoadsideColored());                             //roadside right
        lineReference.Append(COLOR_GREEN).Append(grassChar, grassRightLength);  //grass right
        lineReference.Append(COLOR_RESET);                                      //safety
        ++lineNumber; //just keeping track for debugging purposes
    }
}
