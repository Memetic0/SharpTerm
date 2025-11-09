namespace SharpTerm.Core.Layout;

/// <summary>
/// Grid cell definition for positioning widgets.
/// </summary>
public record GridCell(int Row, int Column, int RowSpan = 1, int ColumnSpan = 1);

/// <summary>
/// Arranges child widgets in a grid with rows and columns.
/// </summary>
public class GridLayout : ILayoutManager
{
    private readonly List<Widget> _children = new();
    private readonly Dictionary<Widget, GridCell> _cellPositions = new();
    private readonly List<int> _rowHeights = new();
    private readonly List<int> _columnWidths = new();

    public int Rows { get; set; }
    public int Columns { get; set; }
    public int RowSpacing { get; set; } = 0;
    public int ColumnSpacing { get; set; } = 0;

    public IReadOnlyList<Widget> Children => _children.AsReadOnly();

    public GridLayout(int rows, int columns)
    {
        Rows = Math.Max(1, rows);
        Columns = Math.Max(1, columns);
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        _rowHeights.Clear();
        _columnWidths.Clear();

        for (int i = 0; i < Rows; i++)
            _rowHeights.Add(0);

        for (int i = 0; i < Columns; i++)
            _columnWidths.Add(0);
    }

    /// <summary>
    /// Adds a child widget to the grid at the specified cell.
    /// </summary>
    public void AddChild(Widget widget, int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        if (!_children.Contains(widget))
        {
            _children.Add(widget);
            _cellPositions[widget] = new GridCell(row, column, rowSpan, columnSpan);
        }
    }

    public void AddChild(Widget widget)
    {
        // Add to first available cell
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                if (!IsCellOccupied(row, col))
                {
                    AddChild(widget, row, col);
                    return;
                }
            }
        }
    }

    public void RemoveChild(Widget widget)
    {
        _children.Remove(widget);
        _cellPositions.Remove(widget);
    }

    private bool IsCellOccupied(int row, int column)
    {
        return _cellPositions.Values.Any(cell =>
            row >= cell.Row && row < cell.Row + cell.RowSpan &&
            column >= cell.Column && column < cell.Column + cell.ColumnSpan);
    }

    public (int Width, int Height) Measure(int availableWidth, int availableHeight)
    {
        CalculateCellSizes(availableWidth, availableHeight);

        int totalWidth = _columnWidths.Sum() + ColumnSpacing * (Columns - 1);
        int totalHeight = _rowHeights.Sum() + RowSpacing * (Rows - 1);

        return (Math.Min(totalWidth, availableWidth), Math.Min(totalHeight, availableHeight));
    }

    private void CalculateCellSizes(int availableWidth, int availableHeight)
    {
        // Reset sizes
        for (int i = 0; i < Rows; i++)
            _rowHeights[i] = 0;
        for (int i = 0; i < Columns; i++)
            _columnWidths[i] = 0;

        // Calculate minimum sizes based on content
        foreach (var (widget, cell) in _cellPositions)
        {
            if (!widget.Visible)
                continue;

            // For single-cell widgets, update row/column sizes
            if (cell.RowSpan == 1 && cell.Row < Rows)
            {
                _rowHeights[cell.Row] = Math.Max(_rowHeights[cell.Row], widget.Bounds.Height);
            }

            if (cell.ColumnSpan == 1 && cell.Column < Columns)
            {
                _columnWidths[cell.Column] = Math.Max(_columnWidths[cell.Column], widget.Bounds.Width);
            }
        }

        // Distribute available space evenly if sizes are not set
        int totalUsedWidth = _columnWidths.Sum();
        int totalUsedHeight = _rowHeights.Sum();

        if (totalUsedWidth < availableWidth - ColumnSpacing * (Columns - 1))
        {
            int remainingWidth = availableWidth - ColumnSpacing * (Columns - 1);
            int widthPerColumn = remainingWidth / Columns;

            for (int i = 0; i < Columns; i++)
            {
                _columnWidths[i] = Math.Max(_columnWidths[i], widthPerColumn);
            }
        }

        if (totalUsedHeight < availableHeight - RowSpacing * (Rows - 1))
        {
            int remainingHeight = availableHeight - RowSpacing * (Rows - 1);
            int heightPerRow = remainingHeight / Rows;

            for (int i = 0; i < Rows; i++)
            {
                _rowHeights[i] = Math.Max(_rowHeights[i], heightPerRow);
            }
        }
    }

    public void Arrange(Rectangle bounds)
    {
        CalculateCellSizes(bounds.Width, bounds.Height);

        foreach (var (widget, cell) in _cellPositions)
        {
            if (!widget.Visible)
                continue;

            // Calculate position
            int x = bounds.X;
            for (int col = 0; col < cell.Column && col < Columns; col++)
            {
                x += _columnWidths[col] + ColumnSpacing;
            }

            int y = bounds.Y;
            for (int row = 0; row < cell.Row && row < Rows; row++)
            {
                y += _rowHeights[row] + RowSpacing;
            }

            // Calculate size
            int width = 0;
            for (int col = cell.Column; col < cell.Column + cell.ColumnSpan && col < Columns; col++)
            {
                width += _columnWidths[col];
                if (col < cell.Column + cell.ColumnSpan - 1)
                    width += ColumnSpacing;
            }

            int height = 0;
            for (int row = cell.Row; row < cell.Row + cell.RowSpan && row < Rows; row++)
            {
                height += _rowHeights[row];
                if (row < cell.Row + cell.RowSpan - 1)
                    height += RowSpacing;
            }

            widget.Bounds = new Rectangle(x, y, width, height);
        }
    }
}
