
public struct LidarPoint
{
    public string Id;
    public int mAngle;
    public int mDistance;

    public LidarPoint(string id, string distance, string angle)
    {
        Id = id;
        mDistance = int.Parse(distance);
        mAngle = int.Parse(angle);
    }
}
