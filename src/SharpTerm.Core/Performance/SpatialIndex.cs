namespace SharpTerm.Core.Performance;

/// <summary>
/// Quadtree-based spatial index for fast widget hit testing.
/// </summary>
public class SpatialIndex
{
    private readonly QuadTreeNode _root;
    private readonly int _maxDepth;
    private readonly int _maxItemsPerNode;

    public SpatialIndex(Rectangle bounds, int maxDepth = 6, int maxItemsPerNode = 4)
    {
        _root = new QuadTreeNode(bounds, 0, maxDepth);
        _maxDepth = maxDepth;
        _maxItemsPerNode = maxItemsPerNode;
    }

    /// <summary>
    /// Inserts a widget into the spatial index.
    /// </summary>
    public void Insert(Widget widget)
    {
        _root.Insert(widget, _maxItemsPerNode);
    }

    /// <summary>
    /// Removes a widget from the spatial index.
    /// </summary>
    public void Remove(Widget widget)
    {
        _root.Remove(widget);
    }

    /// <summary>
    /// Finds all widgets at the specified point.
    /// </summary>
    public List<Widget> Query(int x, int y)
    {
        var results = new List<Widget>();
        _root.Query(x, y, results);
        return results;
    }

    /// <summary>
    /// Finds all widgets intersecting the specified rectangle.
    /// </summary>
    public List<Widget> Query(Rectangle bounds)
    {
        var results = new List<Widget>();
        _root.Query(bounds, results);
        return results;
    }

    /// <summary>
    /// Clears all widgets from the index.
    /// </summary>
    public void Clear()
    {
        _root.Clear();
    }

    /// <summary>
    /// Rebuilds the entire index (useful after many removals).
    /// </summary>
    public void Rebuild(IEnumerable<Widget> widgets)
    {
        Clear();
        foreach (var widget in widgets)
        {
            Insert(widget);
        }
    }

    private class QuadTreeNode
    {
        private readonly Rectangle _bounds;
        private readonly int _depth;
        private readonly int _maxDepth;
        private List<Widget>? _items;
        private QuadTreeNode[]? _children;

        public QuadTreeNode(Rectangle bounds, int depth, int maxDepth)
        {
            _bounds = bounds;
            _depth = depth;
            _maxDepth = maxDepth;
        }

        public void Insert(Widget widget, int maxItemsPerNode)
        {
            if (!_bounds.IntersectsWith(widget.Bounds))
                return;

            if (_children != null)
            {
                // Already subdivided, insert into children
                foreach (var child in _children)
                {
                    child.Insert(widget, maxItemsPerNode);
                }
            }
            else
            {
                // Add to this node
                _items ??= new List<Widget>();
                _items.Add(widget);

                // Subdivide if necessary
                if (_items.Count > maxItemsPerNode && _depth < _maxDepth)
                {
                    Subdivide();

                    // Move items to children
                    var itemsToMove = _items;
                    _items = null;

                    foreach (var item in itemsToMove)
                    {
                        foreach (var child in _children!)
                        {
                            child.Insert(item, maxItemsPerNode);
                        }
                    }
                }
            }
        }

        public void Remove(Widget widget)
        {
            if (!_bounds.IntersectsWith(widget.Bounds))
                return;

            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.Remove(widget);
                }
            }
            else if (_items != null)
            {
                _items.Remove(widget);
            }
        }

        public void Query(int x, int y, List<Widget> results)
        {
            if (!_bounds.Contains(x, y))
                return;

            if (_items != null)
            {
                foreach (var item in _items)
                {
                    if (item.Bounds.Contains(x, y))
                    {
                        results.Add(item);
                    }
                }
            }

            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.Query(x, y, results);
                }
            }
        }

        public void Query(Rectangle bounds, List<Widget> results)
        {
            if (!_bounds.IntersectsWith(bounds))
                return;

            if (_items != null)
            {
                foreach (var item in _items)
                {
                    if (item.Bounds.IntersectsWith(bounds))
                    {
                        results.Add(item);
                    }
                }
            }

            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.Query(bounds, results);
                }
            }
        }

        public void Clear()
        {
            _items?.Clear();
            _items = null;

            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.Clear();
                }
                _children = null;
            }
        }

        private void Subdivide()
        {
            var halfWidth = _bounds.Width / 2;
            var halfHeight = _bounds.Height / 2;
            var x = _bounds.X;
            var y = _bounds.Y;

            _children = new QuadTreeNode[4];
            _children[0] = new QuadTreeNode(new Rectangle(x, y, halfWidth, halfHeight), _depth + 1, _maxDepth);
            _children[1] = new QuadTreeNode(new Rectangle(x + halfWidth, y, halfWidth, halfHeight), _depth + 1, _maxDepth);
            _children[2] = new QuadTreeNode(new Rectangle(x, y + halfHeight, halfWidth, halfHeight), _depth + 1, _maxDepth);
            _children[3] = new QuadTreeNode(new Rectangle(x + halfWidth, y + halfHeight, halfWidth, halfHeight), _depth + 1, _maxDepth);
        }
    }
}

/// <summary>
/// Extension methods for spatial indexing.
/// </summary>
public static class SpatialIndexExtensions
{
    /// <summary>
    /// Finds the topmost widget at the specified point using spatial indexing.
    /// </summary>
    public static Widget? FindWidgetAt(this SpatialIndex index, int x, int y)
    {
        var widgets = index.Query(x, y);

        // Return the last visible widget (topmost in z-order)
        for (int i = widgets.Count - 1; i >= 0; i--)
        {
            if (widgets[i].Visible)
                return widgets[i];
        }

        return null;
    }
}
