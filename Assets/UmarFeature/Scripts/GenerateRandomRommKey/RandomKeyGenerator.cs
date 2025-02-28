using System.Linq;
public static class RoomKeyGenerator
{
    public static string GenerateRoomKey(int length = 6)
    {
        const string chars = "ACDEFHJKLMNPQRTUVWXY3479";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
