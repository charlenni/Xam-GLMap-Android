using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GLMap;
using Android.Util;
using Java.IO;

namespace Xam_GLMap_Android_Demo
{
    public class OSMTileSource : GLMapRasterTileSource
    {
        string[] mirrors;

        public OSMTileSource(Activity activity) : base(CachePath(activity))
        {
            mirrors = new string[3];
            mirrors[0] = @"https://a.tile.openstreetmap.org/{0}/{1}/{2}.png";
            mirrors[1] = @"https://b.tile.openstreetmap.org/{0}/{1}/{2}.png";
            mirrors[2] = @"https://c.tile.openstreetmap.org/{0}/{1}/{2}.png";

            //Set as valid zooms all levels from 0 to 19
            SetValidZoomMask((1 << 20) - 1);

            DisplayMetrics metrics = new DisplayMetrics();
            activity.WindowManager.DefaultDisplay.GetMetrics(metrics);
            //For devices with high screen density we can make tile size a bit bigger.
            if (metrics.ScaledDensity >= 2)
            {
                SetTileSize(192);
            }

            SetAttributionText("© OpenStreetMap contributors");
        }

        public static string CachePath(Activity activity)
        {
            File filesDir = new File(activity.FilesDir.AbsolutePath, "RasterCache");
            filesDir.Mkdir();
            return new File(filesDir.AbsolutePath, "osm.db").AbsolutePath;
        }

        public override string UrlForTilePos(int x, int y, int z)
        {
            string mirror = mirrors[new Random().Next(2)];
            string rv = string.Format(mirror, z, x, y);
            Log.Info("OSMTileSource", rv);
            return rv;
        }
    }
}