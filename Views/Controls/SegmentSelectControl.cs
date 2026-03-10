using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Descript.Utils;

namespace Descript.Views.Controls;

public class SegmentSelectControl : Control
{
    static SegmentSelectControl()
    {
        AffectsRender<SegmentSelectControl>(
            SelectionProperty,
            OutlineColorProperty, 
            AccentColorProperty);
    }
    
    public static readonly StyledProperty<int> SelectionProperty =
        AvaloniaProperty.Register<SegmentSelectControl, int>(nameof(Selection), defaultValue: 0);

    public int Selection
    {
        get => GetValue(SelectionProperty);
        set => SetValue(SelectionProperty, value);
    }
    
    public static readonly StyledProperty<ISolidColorBrush> OutlineColorProperty =
        AvaloniaProperty.Register<SegmentSelectControl, ISolidColorBrush>(nameof(OutlineColor), defaultValue: Brushes.DimGray);

    public ISolidColorBrush OutlineColor
    {
        get => GetValue(OutlineColorProperty);
        set => SetValue(OutlineColorProperty, value);
    }
    
    public static readonly StyledProperty<ISolidColorBrush> AccentColorProperty =
        AvaloniaProperty.Register<SegmentSelectControl, ISolidColorBrush>(nameof(AccentColor), defaultValue: Brushes.RoyalBlue);

    public ISolidColorBrush AccentColor
    {
        get => GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }
    
    private int HoverIndex { get; set; } = -1;

    private const double LineThickness = 12.0;
    private const double ArcThickness = 8.0;
    
    private readonly PathGeometry[] _clickZones;
    private readonly PathGeometry[] _segments;
    
    private readonly PathGeometry _segment1Or2;
    private readonly PathGeometry _outline;
    private readonly PathGeometry _base;
    
    public SegmentSelectControl()
    {
        Width = 200;
        Height = 256;
        
        _segments = new PathGeometry[12];
        _segments[0] = FromPoints(false, [new Point(27,54), new Point(27,200)]);
        _segments[1] = FromPoints(false, [new Point(100,99), new Point(100,9)]);
        _segments[2] = FromPoints(false, [new Point(100,245), new Point(100,155)]);
        _segments[3] = FromPoints(false, [new Point(27,54), new Point(100,9)]);
        _segments[4] = FromPoints(false, [new Point(173,54), new Point(100,9)]);
        _segments[5] = FromPoints(false, [new Point(27,54), new Point(100,99)]);
        _segments[6] = FromPoints(false, [new Point(173,54), new Point(100,99)]);
        _segments[7] = FromPoints(false, [new Point(27,200), new Point(100,155)]);
        _segments[8] = FromPoints(false, [new Point(173,200), new Point(100,155)]);
        _segments[9] = FromPoints(false, [new Point(27,200), new Point(100,245)]);
        _segments[10] = FromPoints(false, [new Point(173,200), new Point(100,245)]);
        _segments[11] = FromArcs(new Point(52,236),
        [
            new ArcSegment
            {
                Size = new Size(11.11, 11.11),
                RotationAngle = 0,
                IsLargeArc = true,
                SweepDirection = SweepDirection.Clockwise,
                Point = new Point(29,236),
            },
            new ArcSegment
            {
                Size = new Size(11.11, 11.11),
                RotationAngle = 0,
                IsLargeArc = true,
                SweepDirection = SweepDirection.Clockwise,
                Point = new Point(52,236),
            }
        ]);

        _segment1Or2 = FromPoints(false, [new Point(100,128), new Point(100,99)]);
        _base = FromPoints(false, [new Point(27, 128), new Point(173, 128)]);
        
        var outlineSegments = _segments
            .Append(_segment1Or2)
            .Where(p => 
                p.Figures is { Count: > 0 } && 
                p.Figures[0].Segments?.Count > 0 &&
                p.Figures[0].Segments?[0] is not ArcSegment)
            .ToArray();
        _outline = Merge(outlineSegments);

        _clickZones = new PathGeometry[12];
        _clickZones[0] = FromPoints(true, [new Point(27,54), new Point(52,99), new Point(52,155), new Point(27,200), new Point(2,155), new Point(2, 99)]);
        _clickZones[1] = FromPoints(true, [new Point(100,9), new Point(124,54), new Point(100,99), new Point(76,54)]);
        _clickZones[2] = FromPoints(true, [new Point(100,100), new Point(124,200), new Point(100,245), new Point(76,200)]);
        _clickZones[3] = FromPoints(true, [new Point(52,9), new Point(100,9), new Point(76,54), new Point(27,54)]);
        _clickZones[4] = FromPoints(true, [new Point(100,9), new Point(148,9), new Point(173,54), new Point(124,54)]);
        _clickZones[5] = FromPoints(true, [new Point(27,54), new Point(76,54), new Point(100,99), new Point(52,99)]);
        _clickZones[6] = FromPoints(true, [new Point(124,54), new Point(173,54), new Point(148,99), new Point(100,99)]);
        _clickZones[7] = FromPoints(true, [new Point(52,155), new Point(100,155), new Point(76,200), new Point(27,200)]);
        _clickZones[8] = FromPoints(true, [new Point(100,155), new Point(148,155), new Point(173,200), new Point(124,200)]);
        _clickZones[9] = FromPoints(true, [new Point(27,200), new Point(76,200), new Point(100,245), new Point(72,245), new Point(37,217)]);
        _clickZones[10] = FromPoints(true, [new Point(124,200), new Point(173,200), new Point(148,245), new Point(100,245)]);
        _clickZones[11] = FromPoints(true, [new Point(20,217), new Point(37,217), new Point(72,245), new Point(72,256), new Point(20,256)]);
    }
    
    private static PathGeometry FromArcs(Point p1, ArcSegment[] arcs)
    {
        PathFigure path = new()
        {
            IsClosed = true,
            StartPoint = p1,
            Segments = []
        };

        foreach (ArcSegment arcSegment in arcs)
        {
            path.Segments.Add(arcSegment);
        }

        return new PathGeometry
        {
            Figures = [path]
        };
    }

    private static PathGeometry Merge(PathGeometry[] paths)
    {
        return paths.Aggregate((p1, p2) =>
        {
            PathFigure[] figures = (p1.Figures ?? []).Concat(p2.Figures ?? []).ToArray();

            PathGeometry result = new()
            {
                Figures = []
            };
            result.Figures.AddRange(figures);

            return result;
        });
    }
    
    private static PathGeometry FromPoints(bool isClosed, Point[] points)
    {
        PathFigure path = new()
        {
            IsClosed = isClosed,
            StartPoint = points[0],
            Segments = []
        };

        for (int i = 1; i < points.Length; i++)
        {
            path.Segments.Add(new LineSegment {Point = points[i]});
        }

        return new PathGeometry
        {
            Figures = [path]
        };
    }

    public sealed override void Render(DrawingContext context)
    {
        // Click Zones
        foreach (PathGeometry clickZone in _clickZones)
        {
            context.DrawGeometry(Brushes.Transparent, null, clickZone);
        }
        
        // Outline
        Pen outlinePenLine = new(OutlineColor.WithOpacity(0.5), LineThickness, lineCap: PenLineCap.Round);
        context.DrawGeometry(null, outlinePenLine, _outline);
        
        Pen outlinePenArc = new(OutlineColor.WithOpacity(0.5), ArcThickness, lineCap: PenLineCap.Round);
        context.DrawGeometry(null, outlinePenArc, _segments[^1]);

        // Current Selection
        for (int i = 0; i < _segments.Length; i++)
        {
            double thickness = i != (_segments.Length - 1) ? 12.0 : 8.0;
            if ((Selection & (1 << i)) == (1 << i))
            {
                context.DrawGeometry(null, new Pen(AccentColor, thickness, lineCap: PenLineCap.Round), _segments[i]);
            }

            if ((Selection & (1 << 1)) == (1 << 1) || (Selection & (1 << 2)) == (1 << 2))
            {
                context.DrawGeometry(null, new Pen(AccentColor, thickness, lineCap: PenLineCap.Round), _segment1Or2);
            }
        }
        
        context.DrawGeometry(null, new Pen(AccentColor, 12, lineCap: PenLineCap.Square), _base);
        
        // Hover Highlight
        for (int i = 0; i < _segments.Length; i++)
        {
            if (HoverIndex != i)
            {
                continue;
            }
            
            double thickness = i != (_segments.Length - 1) ? 12.0 : 8.0;

            HsvColor hsv = AccentColor.Color.ToHsv();
            HsvColor newHsv = new(hsv.A, hsv.H, hsv.S + 0.2, hsv.V - 0.05);
            SolidColorBrush brush = new(newHsv.ToRgb());
            Pen pen = new(brush, thickness, lineCap: PenLineCap.Round);
            
            context.DrawGeometry(null, pen, _segments[i]);
        }
            
        base.Render(context);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        PointerPoint point = e.GetCurrentPoint(this);
        for (int i = 0; i < _clickZones.Length; i++)
        {
            if (_clickZones[i].FillContains(point.Position))
            {
                Selection ^= 1 << i;
                break;
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        
        PointerPoint point = e.GetCurrentPoint(this);
        for (int i = 0; i < _clickZones.Length; i++)
        {
            if (_clickZones[i].FillContains(point.Position) && HoverIndex != i)
            {
                HoverIndex = i;
                InvalidateVisual();
                break;
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        
        PointerPoint point = e.GetCurrentPoint(this);
        for (int i = 0; i < _clickZones.Length; i++)
        {
            if (_clickZones[i].FillContains(point.Position) && HoverIndex != i)
            {
                HoverIndex = i;
                InvalidateVisual();
                return;
            }

            if (_clickZones[i].FillContains(point.Position) && HoverIndex == i)
            {
                return;
            }

            HoverIndex = -1;
        }
        InvalidateVisual();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        
        HoverIndex = -1;
        InvalidateVisual();
    }
}