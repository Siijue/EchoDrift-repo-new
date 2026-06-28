using UnityEngine;

public class RatPatrolZone
{
    public float centerX { get; }
    public float radius { get; }
    public float groundY { get; }


    public RatPatrolZone(float CenterX, float Radius, float GroundY)
    {
        centerX = CenterX;
        radius = Radius;
        groundY = GroundY;
    }

    public Vector2[] GeneratePoints(int count)
    {
        Vector2[] points = new Vector2[count];

        for (int i = 0; i < count;  i++)
        {
            float x = Random.Range(centerX - radius, centerX + radius);
            points[i] = new Vector2(x, groundY);
        }

        return points;
    }
}
