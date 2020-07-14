using OpenTK;

public class Rect
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public Rect(float x, float y, float w, float h)
    {
        X = x;
        Y = y;
        Width = w;
        Height = h;
    }

    public bool IsPointInside(Vector2 mousePos)
    {
        return mousePos.X > X && mousePos.X < X + Width &&
               mousePos.Y > Y && mousePos.Y < Y + Height;
    }
}
