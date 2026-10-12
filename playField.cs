using System.Text;
public class PlayField()
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

    private short GetRandomDirection() => (short)randomNumberGenerator.Next(-1, 2); //get's the random direction the road will move to

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
    short roadStartingPoint = (short)randomNumberGenerator.Next(minRoadSP, maxRoadSP);

    static Random randomNumberGenerator = new Random();
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
    ushort? roadCenterFinder = 0;
    const short halfRoadWidth = roadWidth / 2;
    ushort visibleSpotCounter;
    public bool LateUpdate(ref ushort playerPosition)
    {
        //TODO: place coins in line[0]
        visibleSpotCounter = 0;
        for (int i = 0; i < lines[29].Length; ++i)
        {
            switch(lines[29][i])   //inspecting each character in line 29
            {
            default: continue; //if criteria not hit it skips the character; it goes immediately to the next 'i'
            case roadChar or coinChar:
                if (roadCenterFinder != null && ++roadCenterFinder == halfRoadWidth)
                {
                    roadCenterFinder = null;
                    playerPosition = (ushort)(visibleSpotCounter + 1);
                }
                goto criteriaMatches;
            case roadsideChar or grassChar: criteriaMatches:
            /* Emplace the player character, make sure that during the first frame
             * when the player doesn't exist yet... playerPosition must be an unrealistic large number
             */ if (++visibleSpotCounter == playerPosition)   
                {
                    if (lines[29][i] == grassChar)
                    {
                        int length = lines[29].Length;
                        lines[29].Clear();
                        lines[29].Length = length;
                        for(int it = 0; it < length; it++)
                        {
                            lines[29][it] = ' ';
                            if (it == length / 2 - 1) lines[29][it] = 'd';
                            if (it == length / 2) lines[29][it] = 'e';
                            if (it == length / 2 + 1) lines[29][it] = 'a';
                            if (it == length / 2 + 2) lines[29][it] = 'd';
                        }
                        return true;
                    }
                            
                    lines[29][i] = playerChar;
                    //this will insert the color sequence, the player character is now after the color seq.
                    lines[29].Insert(i, COLOR_MAGENTA);
                }
                break;
            }
        }
        ++GlobalEvent.distance;
        return false;
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
