using Auxil.Pathfinding.Engine;

namespace Auxil.Tests.Pathfinding;

/// <summary>Wiring check for the test project; real black-box tests arrive with the public API.</summary>
public class GridSmokeTests
{
    [Fact]
    public void TryStep_OnEmptyGrid_AllowsEveryInPlaneDirection()
    {
        var cells = new byte[9];
        Array.Fill(cells, Grid.Open);
        var grid = new Grid(3, 3, 1, cells);
        int center = grid.Index(1, 1, 0);

        for (int direction = 0; direction < Grid.InPlaneDirectionCount; direction++)
        {
            grid.TryStep(center, 1, 1, 0, direction, out _).ShouldBeTrue();
        }
    }

    [Fact]
    public void TryStep_AgainstAWall_IsRejected()
    {
        var cells = new byte[9];
        Array.Fill(cells, Grid.Open);
        cells[1 * 3 + 2] = Grid.Blocked; // wall east of the center
        var grid = new Grid(3, 3, 1, cells);
        int center = grid.Index(1, 1, 0);

        grid.TryStep(center, 1, 1, 0, 0, out _).ShouldBeFalse();
    }
}
