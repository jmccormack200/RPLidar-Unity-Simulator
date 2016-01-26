
public struct LidarPoint
{
    public int Id;
    public int X;
    public int Y;

    public LidarPoint(string id, string x, string y)
    {
        Id = int.Parse(id);
        X = int.Parse(x);
        Y = int.Parse(y);
    }
}
