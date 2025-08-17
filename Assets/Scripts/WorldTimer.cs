public static class WorldTimer
{
    private static float totalTime = 0f;

    public static void AddLevelTime(float time)
    {
        totalTime += time;
    }

    public static float GetTotalTime()
    {
        return totalTime;
    }

    public static void Reset()
    {
        totalTime = 0f;
    }
}
