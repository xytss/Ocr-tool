namespace OcrTool.Core;

public readonly record struct SelectionRectangle(int X, int Y, int Width, int Height)
{
    public static SelectionRectangle FromPoints(int startX, int startY, int endX, int endY)
    {
        return new SelectionRectangle(
            Math.Min(startX, endX),
            Math.Min(startY, endY),
            Math.Abs(endX - startX),
            Math.Abs(endY - startY));
    }
}
