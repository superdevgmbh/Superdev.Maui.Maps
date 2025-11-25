using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Superdev.Maui.Maps.Controls
{
    public class Pin : Microsoft.Maui.Controls.Maps.Pin
    {
        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(Pin));

        public ImageSource ImageSource
        {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }

        public static readonly BindableProperty AnchorProperty = BindableProperty.Create(
            nameof(Anchor),
            typeof(Point),
            typeof(Pin),
            new Point(0.5d, 0.5d));

        public Point Anchor
        {
            get => (Point)this.GetValue(AnchorProperty);
            set => this.SetValue(AnchorProperty, value);
        }

        public static readonly BindableProperty MarkerClickedCommandProperty = BindableProperty.Create(
            nameof(MarkerClickedCommand),
            typeof(ICommand),
            typeof(Pin)
        );

        public ICommand MarkerClickedCommand
        {
            get => (ICommand)this.GetValue(MarkerClickedCommandProperty);
            set => this.SetValue(MarkerClickedCommandProperty, value);
        }

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(Pin),
            defaultBindingMode: BindingMode.OneWayToSource
        );

        public bool IsSelected
        {
            get => (bool)this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        internal WeakReference<Map>? Map { private get; set; }

        internal bool TryGetMap([NotNullWhen(true)] out Map? map)
        {
            if (this.Map is WeakReference<Map> mr && mr.TryGetTarget(out var m))
            {
                map = m;
                return true;
            }

            map = null;
            return false;
        }
    }
}