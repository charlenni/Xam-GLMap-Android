using Android.App;
using Android.OS;
using Android.Views;
using GLMap;

namespace Xam_GLMap_Android_Demo
{
    [Activity(Label = "MapTextureViewActivity")]
    public class MapTextureViewActivity : Activity
    {
        GLMapView mapView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.texture_view_map);

            TextureView textureView = (TextureView)FindViewById(Resource.Id.texture_view);
            mapView = new GLMapView(this, textureView);

            mapView.LoadStyle(Assets, "DefaultStyle.bundle");
            mapView.SetScaleRulerStyle(GLUnits.SI, GLMapPlacement.BottomCenter, new MapPoint(10, 10), 200);
            mapView.SetAttributionPosition(GLMapPlacement.TopCenter);
        }
    }
}