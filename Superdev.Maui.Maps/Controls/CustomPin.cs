using System.Windows.Input;
using Microsoft.Maui.Controls.Maps;

namespace Superdev.Maui.Maps.Controls
{
    public class CustomPin : Pin
    {
        public int Id { get; set; }

        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(CustomPin));

        public ImageSource ImageSource
        {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }

        public static readonly BindableProperty AnchorProperty = BindableProperty.Create(
            nameof(Anchor),
            typeof(Point),
            typeof(CustomPin),
            new Point(0.5d, 0.5d));

        public Point Anchor
        {
            get => (Point)this.GetValue(AnchorProperty);
            set => this.SetValue(AnchorProperty, value);
        }

        public static readonly BindableProperty MarkerClickedCommandProperty = BindableProperty.Create(
            nameof(MarkerClickedCommand),
            typeof(ICommand),
            typeof(CustomPin)
        );

        public ICommand MarkerClickedCommand
        {
            get => (ICommand)this.GetValue(MarkerClickedCommandProperty);
            set => this.SetValue(MarkerClickedCommandProperty, value);
        }

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(CustomPin),
            defaultBindingMode: BindingMode.OneWayToSource
        );

        public bool IsSelected
        {
            get => (bool)this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        internal CustomMap Map { get; set; }
    }
}