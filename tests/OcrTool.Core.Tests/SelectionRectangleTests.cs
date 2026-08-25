using OcrTool.Core;
using Xunit;

namespace OcrTool.Core.Tests;

public sealed class SelectionRectangleTests
{
    [Theory]
    [InlineData(10, 20, 4, 5, 4, 5, 6, 15)]
    [InlineData(4, 5, 10, 20, 4, 5, 6, 15)]
    public void FromPoints_normalizes_drag_direction(
        int startX,
        int startY,
        int endX,
        int endY,
        int expectedX,
        int expectedY,
        int expectedWidth,
        int expectedHeight)
    {
        SelectionRectangle actual = SelectionRectangle.FromPoints(
            startX,
            startY,
            endX,
            endY);

        Assert.Equal(
            new SelectionRectangle(expectedX, expectedY, expectedWidth, expectedHeight),
            actual);
    }
}
