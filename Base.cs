using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shell;

namespace BaseChromeWindow;

[ContentProperty(nameof(Body))]
public class Base : Window
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(Base),
            new PropertyMetadata(null));

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty BodyProperty =
        DependencyProperty.Register(nameof(Body), typeof(object), typeof(Base), new PropertyMetadata(null));
    public object Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public ContentPresenter ContentArea { get; }

    private (double left, double top) _prevPosition;
    private bool _headerMouseDown;
    private Point _mouseDownPoint;
    private bool _isDragging;

    public Base()
    {
        Title = "BaseWindow";
        ResizeMode = ResizeMode.CanResizeWithGrip;
        WindowStyle = WindowStyle.None;
        SnapsToDevicePixels = true;
        Width = 800;
        Height = 450;

        var chrome = new WindowChrome
        {
            CaptionHeight = 0,
            CornerRadius = new CornerRadius(0),
            GlassFrameThickness = new Thickness(0),
            ResizeBorderThickness = new Thickness(8),
            UseAeroCaptionButtons = false
        };
        WindowChrome.SetWindowChrome(this, chrome);

        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
        root.RowDefinitions.Add(new RowDefinition());

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        headerGrid.MinHeight = 40;

        var headerControl = new ContentControl();
        headerControl.SetBinding(
            ContentControl.ContentProperty,
            new Binding(nameof(Header)) { Source = this });
        headerControl.MouseLeftButtonDown += Header_MouseLeftButtonDown;
        headerControl.MouseMove += Header_MouseMove;
        headerControl.MouseLeftButtonUp += Header_MouseLeftButtonUp;
        Grid.SetColumn(headerControl, 0);
        headerGrid.Children.Add(headerControl);

        Grid.SetRow(headerGrid, 0);
        root.Children.Add(headerGrid);

        ContentArea = new ContentPresenter();
        ContentArea.SetBinding(
            ContentPresenter.ContentProperty,
            new Binding(nameof(Body)) { Source = this });
        Grid.SetRow(ContentArea, 1);
        root.Children.Add(ContentArea);

        Content = root;
    }

    #region Button Handlers

    public virtual void MinimizeClick(object s, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    public virtual void MaximizeClick(object s, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            _prevPosition = (Left, Top);
            var mi = MonitorHelper.GetMonitorInfoForWindow(this);
            var work = mi.rcWork;
            Left = work.Left;
            Top = work.Top;
            MaxWidth = mi.rcWork.Right;
            Width = mi.rcWork.Right;
            MaxHeight = (mi.rcMonitor.Bottom - mi.cbSize) / MonitorHelper.GetDevicePixelRatio(this);
            Height = (mi.rcMonitor.Bottom - mi.cbSize) / MonitorHelper.GetDevicePixelRatio(this);
            WindowState = WindowState.Maximized;
        }
        else
        {
            var mi = MonitorHelper.GetMonitorInfoForWindow(this);
            MaxWidth = mi.rcWork.Right;
            Width = mi.rcWork.Right;
            MaxHeight = (mi.rcMonitor.Bottom - mi.cbSize) / MonitorHelper.GetDevicePixelRatio(this);
            Height = (mi.rcMonitor.Bottom - mi.cbSize) / MonitorHelper.GetDevicePixelRatio(this);
            WindowState = WindowState.Normal;
            Left = _prevPosition.left;
            Top = _prevPosition.top;
        }
    }

    public virtual void CloseClick(object s, RoutedEventArgs e)
    {
        Close();
    }

    #endregion

    #region Dragging Header

    public virtual void Header_MouseLeftButtonDown(object s, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;

        _headerMouseDown = true;
        _isDragging = false;
        _mouseDownPoint = e.GetPosition(this);

        if (e.ClickCount == 2)
        {
            MaximizeClick(s, null);
            _headerMouseDown = false;
        }
    }

    protected virtual void Header_MouseMove(object s, MouseEventArgs e)
    {
        if (!_headerMouseDown || _isDragging || e.LeftButton != MouseButtonState.Pressed)
            return;

        var current = e.GetPosition(this);
        const double threshold = 3;
        if (Math.Abs(current.X - _mouseDownPoint.X) < threshold &&
            Math.Abs(current.Y - _mouseDownPoint.Y) < threshold)
            return;

        _isDragging = true;

        if (WindowState == WindowState.Maximized)
        {
            var relX = _mouseDownPoint.X / ActualWidth;
            var screenPt = PointToScreen(_mouseDownPoint);

            MaxWidth = double.PositiveInfinity;
            MaxHeight = double.PositiveInfinity;
            WindowState = WindowState.Normal;

            Width = RestoreBounds.Width;
            Height = RestoreBounds.Height;
            Left = screenPt.X - Width * relX;
            Top = screenPt.Y - _mouseDownPoint.Y;
        }

        DragMove();
    }



    protected virtual void Header_MouseLeftButtonUp(object s, MouseButtonEventArgs e)
    {
        _headerMouseDown = false;
        _isDragging = false;
    }

    #endregion
}
