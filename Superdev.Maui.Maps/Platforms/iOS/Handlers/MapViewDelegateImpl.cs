using Foundation;
using MapKit;

namespace Superdev.Maui.Maps.Platforms.Handlers
{
    public delegate MKAnnotationView? GetViewForAnnotationDelegate(MauiMKMapView mapView, NSObject annotation);

    public delegate void DidSelectAnnotationViewDelegate(MKMapView mapView, MKAnnotationView annotationView);

    public delegate void RegionDidChangeAnimatedDelegate(MKMapView mapView, bool animated);

    public delegate void DidFinishRenderingMapDelegate(MKMapView mapView, bool fullyRendered);

    public delegate void DidFinishLoadingMapDelegate(MKMapView mapView);

    public delegate MKOverlayRenderer RendererForOverlayDelegate(MKMapView mapView, IMKOverlay overlay);

    public class MapViewDelegateImpl : MKMapViewDelegate
    {
        public RendererForOverlayDelegate? RendererForOverlayDelegate { get; set; }

        [Export("mapView:rendererForOverlay:")]
        public new MKOverlayRenderer? OverlayRenderer(MKMapView mapView, IMKOverlay overlay)
        {
            return this.RendererForOverlayDelegate?.Invoke(mapView, overlay);
        }

        public RegionDidChangeAnimatedDelegate? RegionDidChangeAnimatedDelegate { get; set; }

        [Export("mapView:regionDidChangeAnimated:")]
        public new void RegionChanged(MKMapView mapView, bool animated)
        {
            this.RegionDidChangeAnimatedDelegate?.Invoke(mapView, animated);
        }

        public DidFinishRenderingMapDelegate? DidFinishRenderingMapDelegate { get; set; }

        [Export("mapViewDidFinishRenderingMap:fullyRendered:")]
        public new void DidFinishRenderingMap(MKMapView mapView, bool fullyRendered)
        {
            this.DidFinishRenderingMapDelegate?.Invoke(mapView, fullyRendered);
        }

        public DidFinishLoadingMapDelegate? DidFinishLoadingMapDelegate { get; set; }

        [Export("mapViewDidFinishLoadingMap:")]
        public void DidFinishLoadingMap(MKMapView mapView)
        {
            this.DidFinishLoadingMapDelegate?.Invoke(mapView);
        }

        public DidSelectAnnotationViewDelegate? DidSelectAnnotationViewDelegate { get; set; }

        [Export("mapView:didSelectAnnotationView:")]
        public new void DidSelectAnnotationView(MKMapView mapView, MKAnnotationView view)
        {
            this.DidSelectAnnotationViewDelegate?.Invoke(mapView, view);
        }

        public GetViewForAnnotationDelegate? GetViewForAnnotationDelegate { get; set; }

        [Export("mapView:viewForAnnotation:")]
        public MKAnnotationView? GetViewForAnnotation(MauiMKMapView mapView, NSObject annotation)
        {
            return this.GetViewForAnnotationDelegate?.Invoke(mapView, annotation);
        }
    }
}