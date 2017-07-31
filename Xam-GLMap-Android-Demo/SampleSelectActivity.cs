using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using GLMap;
using System;

namespace Xam_GLMap_Android_Demo
{
    public enum Samples
    {
        MAP,
        MAP_EMBEDD,
        MAP_ONLINE,
        MAP_ONLINE_RASTER,
        MAP_TEXTURE_VIEW,
        ZOOM_BBOX,
        OFFLINE_SEARCH,
        MARKERS,
        MARKERS_MAPCSS,
        IMAGE_SINGLE,
        IMAGE_MULTI,
        MULTILINE,
        POLYGON,
        GEO_JSON,
        CALLBACK_TEST,
        CAPTURE_SCREEN,
        FLY_TO,
        STYLE_LIVE_RELOAD,
        DOWNLOAD_MAP,
        SVG_TEST,
        CRASH_NDK,
    }

    [Activity(Label = "SampleSelectActivity", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Black")]
    public class SampleSelectActivity : ListActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.sample_select);

            if (!GLMapManager.Initialize(this, this.GetString(Resource.String.api_key), null))
            {
                //Error caching resources. Check free space for world database (~25MB)			
            }

            String[] values = new String[] {
                "Open offline map",
                "Open embedd map",
                "Open online map",
                "Open online raster map",
                "GLMapView in TextureView",
                "Zoom to bbox",
                "Offline Search",
                "Markers",
                "Markers using mapcss",
                "Display single image",
                "Display image group",
                "Add multiline",
                "Add polygon",
                "Load GeoJSON",
                "Callback test",
                "Capture screen",
                "Fly to",
                "Style live reload",
                "Download Map",
                "SVG Test",
                "Crash NDK",
                };

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, values);

            ListAdapter = adapter;
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            Intent i;

            if ((Samples)position == Samples.MAP_TEXTURE_VIEW)
            {
                i = new Intent(this, typeof(MapTextureViewActivity));
			    i.PutExtra("cx", 27.0);
			    i.PutExtra("cy", 53.0);
			    this.StartActivity(i);
			    return;
		    }

            if ((Samples)position == Samples.DOWNLOAD_MAP)
            {
                i = new Intent(this, typeof(DownloadActivity));
        	    i.PutExtra("cx", 27.0);
        	    i.PutExtra("cy", 53.0);

                this.StartActivity(i);    	
			    return;
            }

            if ((Samples)position == Samples.SVG_TEST)
            {
                i = new Intent(this, typeof(DisplayImageActivity));

                this.StartActivity(i);
        	    return;
            }

            if ((Samples)position == Samples.CRASH_NDK)
            {
                GLMapView.CrashNDK2();
                return;
            }

            i = new Intent(this, typeof(MapViewActivity));
            Bundle b = new Bundle();
            b.PutInt("example", position);
            i.PutExtras(b);

            this.StartActivity(i);
        }
    }
}