using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using GLMap;
using Java.IO;
using Java.Lang;
using Java.Net;
using System;
using System.Collections.Generic;
using System.IO;
using static Android.Views.GestureDetector;
using static Android.Views.View;

namespace Xam_GLMap_Android_Demo
{
    class Pin
    {
        public MapPoint Pos;
        public int ImageVariant;
    }

    class ImageGroupCallback : Java.Lang.Object, IGLMapImageGroupCallback
    {
        List<Pin> pins;
        Bitmap[] images;

        public ImageGroupCallback(List<Pin> pins, Bitmap[] images)
        {
            this.pins = pins;
            this.images = images;
        }

        public int ImagesCount
        {
            get
            {
                return pins.Count;
            }
        }

        public int GetImageIndex(int i)
        {
            return pins[i].ImageVariant;
        }

        public MapPoint GetImagePos(int i)
        {
            return pins[i].Pos;
        }

        public void UpdateStarted()
        {
            Log.Info("GLMapImageGroupCallback", "Update started");
        }

        public void UpdateFinished()
        {
            Log.Info("GLMapImageGroupCallback", "Update finished");
        }

        public int ImageVariantsCount
        {
            get
            {
                return images.Length;
            }
        }

        public Bitmap GetImageVariantBitmap(int i)
        {
            return images[i];
        }

        public MapPoint GetImageVariantOffset(int i)
        {
            return new MapPoint(images[i].Width / 2, 0);
        }
    }

    [Activity(Label = "MapViewActivity")]
    public class MapViewActivity : Activity, GLMapView.IScreenCaptureCallback, GLMapManager.IStateListener
    {
        private GLMapImage image = null;
        private GLMapImageGroup imageGroup = null;
        private List<Pin> pins = new List<Pin>();
        private GestureDetector gestureDetector;
        private GLMapView mapView;
        private GLMapInfo mapToDownload = null;
        private Button btnDownloadMap;

        GLMapMarkerLayer markerLayer;
        GLMapLocaleSettings localeSettings;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.map);
            mapView = (GLMapView)FindViewById(Resource.Id.map_view);

            // Map list is updated, because download button depends on available map list and during first launch this list is empty
            GLMapManager.UpdateMapList(this, null);

            btnDownloadMap = (Button)this.FindViewById(Resource.Id.button_dl_map);
            btnDownloadMap.Click += (object sender, EventArgs e) =>
            {
                if (mapToDownload != null)
                {
                    GLMapDownloadTask task = GLMapManager.GetDownloadTask(mapToDownload);
                    if (task != null)
                    {
                        task.Cancel();
                    }
                    else
                    {
                        GLMapManager.CreateDownloadTask(mapToDownload, this).Start();
                    }
                    updateMapDownloadButtonText();
                }
                else
                {
                    Intent i = new Intent(Application.Context, typeof(DownloadActivity));

                    MapPoint pt = mapView.MapCenter;
                    i.PutExtra("cx", pt.X);
                    i.PutExtra("cy", pt.Y);
                    Application.Context.StartActivity(i);
                }
            };

            GLMapManager.AddStateListener(this);

            localeSettings = new GLMapLocaleSettings();
            mapView.LocaleSettings = localeSettings;
            mapView.LoadStyle(Assets, "DefaultStyle.bundle");
            mapView.SetUserLocationImages(mapView.ImageManager.Open("DefaultStyle.bundle/circle-new.svgpb", 1, 0), mapView.ImageManager.Open("DefaultStyle.bundle/arrow-new.svgpb", 1, 0));

            mapView.SetScaleRulerStyle(GLUnits.SI, GLMapPlacement.BottomCenter, new MapPoint(10, 10), 200);
            mapView.SetAttributionPosition(GLMapPlacement.TopCenter);

            CheckAndRequestLocationPermission();

            Bundle b = Intent.Extras;
            Samples example = (Samples)b.GetInt("example");
            switch (example)
            {
                case Samples.MAP_EMBEDD:
                    if (!GLMapManager.AddMap(Assets, "Montenegro.vm", null))
                    {
                        //Failed to unpack to caches. Check free space.
                    }
                    zoomToPoint();
                    break;
                case Samples.MAP_ONLINE:
                    GLMapManager.SetAllowedTileDownload(true);
                    break;
                case Samples.MAP_ONLINE_RASTER:
                    mapView.RasterTileSources = new GLMapRasterTileSource[] { new OSMTileSource(this) };
                    break;
                case Samples.ZOOM_BBOX:
                    zoomToBBox();
                    break;
                case Samples.FLY_TO:
                    {
                        mapView.SetMapCenter(MapPoint.CreateFromGeoCoordinates(37.3257, -122.0353), false);
                        mapView.SetMapZoom(14, false);

                        Button btn = (Button)FindViewById(Resource.Id.button_action);
                        btn.Visibility = ViewStates.Visible;
                        btn.Text = "Fly";
                        btn.Click += (object sender, EventArgs e) =>
                        {
                            double min_lat = 33;
                            double max_lat = 48;
                            double min_lon = -118;
                            double max_lon = -85;

                            double lat = min_lat + (max_lat - min_lat) * new Random(1).NextDouble();
                            double lon = min_lon + (max_lon - min_lon) * new Random(2).NextDouble();

                            MapGeoPoint geoPoint = new MapGeoPoint(lat, lon);

                            mapView.FlyTo(geoPoint, 15, 0, 0);
                        };
                        GLMapManager.SetAllowedTileDownload(true);
                        break;
                    }
                case Samples.OFFLINE_SEARCH:
                    GLMapManager.AddMap(Assets, "Montenegro.vm", null);
                    zoomToPoint();
                    offlineSearch();
                    break;
                case Samples.MARKERS:
                    mapView.LongClickable = true;

                    gestureDetector = new GestureDetector(this, new SimpleOnGestureListenerAnonymousInnerClassHelper(this));

                    mapView.SetOnTouchListener(new TouchListener(gestureDetector));

                    addMarkers();
                    GLMapManager.SetAllowedTileDownload(true);
                    break;
                case Samples.MARKERS_MAPCSS:
                    addMarkersWithMapcss();

                    gestureDetector = new GestureDetector(this, new SimpleOnGestureListenerAnonymousInnerClassHelper2(this));

                    mapView.SetOnTouchListener(new TouchListener(gestureDetector));

                    GLMapManager.SetAllowedTileDownload(true);
                    break;
                case Samples.MULTILINE:
                    addMultiline();
                    break;
                case Samples.POLYGON:
                    addPolygon();
                    break;
                case Samples.CAPTURE_SCREEN:
                    zoomToPoint();
                    captureScreen();
                    break;
                case Samples.IMAGE_SINGLE:
                    {
                        Button btn = (Button)this.FindViewById(Resource.Id.button_action);
                        btn.Visibility = ViewStates.Visible;
                        delImage(btn, null);
                        break;
                    }
                case Samples.IMAGE_MULTI:
                    mapView.LongClickable = true;
                    
                    gestureDetector = new GestureDetector(this, new SimpleOnGestureListenerAnonymousInnerClassHelper3(this));

                    mapView.SetOnTouchListener(new TouchListener(gestureDetector));
                    break;
                case Samples.GEO_JSON:
                    loadGeoJSON();
                    break;
                case Samples.STYLE_LIVE_RELOAD:
                    styleLiveReload();
                    break;
            }

            mapView.SetCenterTileStateChangedCallback(() =>
            {
                RunOnUiThread(() =>
                {
                    updateMapDownloadButton();
                });
            });

            mapView.SetMapDidMoveCallback(() =>
            {
                if (example == Samples.CALLBACK_TEST)
                {
                    Log.Warn("GLMapView", "Did move");
                }
                RunOnUiThread(() =>
                {
                    updateMapDownloadButtonText();
                });
            });
        }

        private class TouchListener : Java.Lang.Object, IOnTouchListener
        {
            GestureDetector gestureDetector;

            public TouchListener(GestureDetector gestureDetector)
            {
                this.gestureDetector = gestureDetector;
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                return gestureDetector.OnTouchEvent(e);
            }
        }

        private class SimpleOnGestureListenerAnonymousInnerClassHelper : SimpleOnGestureListener
        {
            private readonly MapViewActivity outerInstance;

            public SimpleOnGestureListenerAnonymousInnerClassHelper(MapViewActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override bool OnSingleTapConfirmed(MotionEvent e)
            {
                outerInstance.deleteMarker(e.GetX(), e.GetY());
                return true;
            }

            public override void OnLongPress(MotionEvent e)
            {
                outerInstance.addMarker(e.GetX(), e.GetY());
            }
        }

        private class SimpleOnGestureListenerAnonymousInnerClassHelper2 : SimpleOnGestureListener
        {
            private readonly MapViewActivity outerInstance;

            public SimpleOnGestureListenerAnonymousInnerClassHelper2(MapViewActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override bool OnSingleTapConfirmed(MotionEvent e)
            {
                outerInstance.deleteMarker(e.GetX(), e.GetY());
                return true;
            }

            public override void OnLongPress(MotionEvent e)
            {
                outerInstance.addMarkerAsVectorObject(e.GetX(), e.GetY());
            }
        }

        private class SimpleOnGestureListenerAnonymousInnerClassHelper3 : SimpleOnGestureListener
        {
            private readonly MapViewActivity outerInstance;

            public SimpleOnGestureListenerAnonymousInnerClassHelper3(MapViewActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override bool OnSingleTapConfirmed(MotionEvent e)
            {
                outerInstance.deletePin(e.GetX(), e.GetY());
                return true;
            }

            public override void OnLongPress(MotionEvent e)
            {
                outerInstance.addPin(e.GetX(), e.GetY());
            }
        }

        public virtual void CheckAndRequestLocationPermission()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation }, 0);
            }
            else
            {
                mapView.SetShowsUserLocation(true);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case 0:
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                        {
                            mapView.SetShowsUserLocation(true);
                        }
                        break;
                    }
                default:
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                    break;
            }
        }

        protected override void OnDestroy()
        {
            GLMapManager.RemoveStateListener(this);
            if (markerLayer != null)
            {
                markerLayer.Dispose();
                markerLayer = null;
            }
            base.OnDestroy();
        }

        public bool OnCreateOptionsMenu(Menu menu)
        {
            MapPoint pt = new MapPoint(mapView.Width / 2, mapView.Height / 2);
            mapView.ChangeMapZoom(-1, pt, true);
            return false;
        }

        public void OnStartDownloading(GLMapInfo map)
        {

        }

        public void OnDownloadProgress(GLMapInfo map)
        {
            updateMapDownloadButtonText();
        }

        public void OnFinishDownloading(GLMapInfo map)
        {
            mapView.ReloadTiles();
        }

        public void OnStateChanged(GLMapInfo map)
        {
            updateMapDownloadButtonText();
        }

        internal virtual void updateMapDownloadButtonText()
        {
            if (btnDownloadMap.Visibility == ViewStates.Visible)
            {
                MapPoint center = mapView.MapCenter;

                mapToDownload = GLMapManager.MapAtPoint(center);

                if (mapToDownload != null)
                {
                    string text;
                    if (mapToDownload.State == GLMapInfoState.InProgress)
                    {
                        text = string.Format("Downloading {0} {1}%", mapToDownload.GetLocalizedName(localeSettings), (int)(mapToDownload.DownloadProgress * 100));
                    }
                    else
                    {
                        text = string.Format("Download {0}", mapToDownload.GetLocalizedName(localeSettings));
                    }
                    btnDownloadMap.Text = text;
                }
                else
                {
                    btnDownloadMap.Text = "Download maps";
                }
            }
        }

        internal virtual void updateMapDownloadButton()
        {
            switch (mapView.CenterTileState)
            {
                case GLMapTileState.NoData:
                    {
                        if (btnDownloadMap.Visibility == ViewStates.Invisible)
                        {
                            btnDownloadMap.Visibility = ViewStates.Visible;
                            btnDownloadMap.Parent.RequestLayout();
                            updateMapDownloadButtonText();
                        }
                        break;
                    }

                case GLMapTileState.Loaded:
                    {
                        if (btnDownloadMap.Visibility == ViewStates.Visible)
                        {
                            btnDownloadMap.Visibility = ViewStates.Invisible;
                        }
                        break;
                    }
                case GLMapTileState.Unknown:
                    break;
            }
        }

        private static GLSearchCategories searchCategories;
        //Example how to load search categories.
        public static GLSearchCategories GetSearchCategories(Resources resources)
        {
            if (searchCategories == null)
            {
                byte[] raw = null;
                byte[] icuData = null;
                try
                {
                    //Read prepared categories
                    System.IO.Stream stream = resources.OpenRawResource(Resource.Raw.categories);
                    raw = new byte[21793];
                    stream.Read(raw, 0, raw.Length);
                    stream.Close();

                    //Read icu collation data
                    stream = resources.OpenRawResource(Resource.Raw.icudt56l);
                    icuData = new byte[1018032];
                    stream.Read(icuData, 0, icuData.Length);
                    stream.Close();
                }
                catch (Java.IO.IOException e)
                {
                    System.Console.WriteLine(e.ToString());
                    System.Console.Write(e.StackTrace);
                }

                //Construct categories
                searchCategories = GLSearchCategories.CreateFromBytes(raw, icuData);
            }
            return searchCategories;
        }

        internal virtual void offlineSearch()
        {
            GLSearchCategories categories = GetSearchCategories(Resources);

            GLSearchOffline searchOffline = new GLSearchOffline();
            searchOffline.SetCategories(categories); //Set categories to use for search
            searchOffline.SetCenter(MapPoint.CreateFromGeoCoordinates(42.4341, 19.26)); //Set center of search
            searchOffline.SetLimit(20); //Set maximum number of results. By default is is 100
            searchOffline.SetLocaleSettings(mapView.LocaleSettings); //Locale settings to give bonus for results that match to user language

            GLSearchCategory[] category = categories.GetStartedWith(new string[] { "food" }, localeSettings); //find categories by name
            if (category.Length != 0)
            {
                searchOffline.AddCategoryFilter(category[0]); //Filter results by category
            }
            //searchOffline.addNameFilter("cali"); //Add filter by name

            searchOffline.Start(new GLMapSearchCompletionAnonymousInnerClassHelper(this));
        }

        private class GLMapSearchCompletionAnonymousInnerClassHelper : Java.Lang.Object, GLSearchOffline.IGLMapSearchCompletion
        {
            private readonly MapViewActivity outerInstance;

            public GLMapSearchCompletionAnonymousInnerClassHelper(MapViewActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public void OnResults(Java.Lang.Object[] objects)
            {
                this.outerInstance.RunOnUiThread(() =>
                {
                    outerInstance.displaySearchResults(objects);
                });
            }
        }

        internal virtual void displaySearchResults(Java.Lang.Object[] objects)
        {
            GLMapMarkerStyleCollection style = new GLMapMarkerStyleCollection();
            style.AddStyle(new GLMapMarkerImage("marker", mapView.ImageManager.Open("cluster.svgpb", 0.2f, unchecked((int)0xFFFF0000))));
            style.SetDataCallback(new GLMapMarkerStyleCollectionDataCallbackAnonymousInnerClassHelper(this));
            GLMapMarkerLayer layer = new GLMapMarkerLayer(objects, style);
            layer.SetClusteringEnabled(false);
            mapView.DisplayMarkerLayer(layer);

            //Zoom to results
            if (objects.Length != 0)
            {
                //Calculate bbox
                GLMapBBox bbox = new GLMapBBox();
                foreach (object @object in objects)
                {
                    if (@object is GLMapVectorObject)
                    {
                        bbox.AddPoint(((GLMapVectorObject)@object).Point());
                    }
                }
                //Zoom to bbox
                mapView.SetMapCenter(bbox.Center(), false);
                mapView.SetMapZoom(mapView.MapZoomForBBox(bbox, mapView.Width, mapView.Height), false);
            }
        }

        private class GLMapMarkerStyleCollectionDataCallbackAnonymousInnerClassHelper : GLMapMarkerStyleCollectionDataCallback
        {
            private readonly MapViewActivity outerInstance;

            public GLMapMarkerStyleCollectionDataCallbackAnonymousInnerClassHelper(MapViewActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void FillUnionData(int markersCount, long nativeMarker)
            {
                //Not called if clustering is off
            }
            public override void FillData(Java.Lang.Object marker, long nativeMarker)
            {
                if (marker is GLMapVectorObject)
                {
                    GLMapVectorObject obj = (GLMapVectorObject)marker;
                    GLMapMarkerStyleCollection.SetMarkerLocationFromVectorObject(nativeMarker, obj);
                }
                GLMapMarkerStyleCollection.SetMarkerStyle(nativeMarker, 0);
            }
        }

        // Example how to calculate zoom level for some bbox
        internal virtual void zoomToBBox()
        {
            mapView.DoWhenSurfaceCreated(() =>
            {
                GLMapBBox bbox = new GLMapBBox();
                bbox.AddPoint(MapPoint.CreateFromGeoCoordinates(52.5037, 13.4102)); // Berlin
                bbox.AddPoint(MapPoint.CreateFromGeoCoordinates(53.9024, 27.5618)); // Minsk

                mapView.SetMapCenter(bbox.Center(), false);
                mapView.SetMapZoom(mapView.MapZoomForBBox(bbox, mapView.Width, mapView.Height), false);
            });
        }

        internal virtual void zoomToPoint()
        {
            //New York
            //MapPoint pt = new MapPoint(-74.0059700 , 40.7142700	);

            //Belarus
            //MapPoint pt = new MapPoint(27.56, 53.9);
            //;

            // Move map to the Montenegro capital
            MapPoint pt = MapPoint.CreateFromGeoCoordinates(42.4341, 19.26);
            GLMapView mapView = (GLMapView)this.FindViewById(Resource.Id.map_view);
            mapView.SetMapCenter(pt, false);
            mapView.SetMapZoom(16, false);
        }

        internal virtual void addPin(float touchX, float touchY)
        {
            if (imageGroup == null)
            {
                Bitmap[] images = new Bitmap[3];
                images[0] = mapView.ImageManager.Open("1.svgpb", 1, unchecked((int)0xFFFF0000));
                images[1] = mapView.ImageManager.Open("2.svgpb", 1, unchecked((int)0xFF00FF00));
                images[2] = mapView.ImageManager.Open("3.svgpb", 1, unchecked((int)0xFF0000FF));

                imageGroup = mapView.CreateImageGroup(new ImageGroupCallback(pins, images));
            }

            MapPoint pt = mapView.ConvertDisplayToInternal(new MapPoint(touchX, touchY));

            Pin pin = new Pin();
            pin.Pos = pt;
            pin.ImageVariant = pins.Count % 3;
            pins.Add(pin);
            imageGroup.SetNeedsUpdate();
        }


        internal virtual void deletePin(float touchX, float touchY)
        {
            for (int i = 0; i < pins.Count; ++i)
            {
                MapPoint pos = pins[i].Pos;
                MapPoint screenPos = mapView.ConvertInternalToDisplay(new MapPoint(pos));

                Rect rt = new Rect(-40, -40, 40, 40);
                rt.Offset((int)screenPos.X, (int)screenPos.Y);
                if (rt.Contains((int)touchX, (int)touchY))
                {
                    pins.Remove(pins[i]);
                    imageGroup.SetNeedsUpdate();
                    break;
                }
            }
        }

        internal virtual void deleteMarker(float x, float y)
        {
            if (markerLayer != null)
            {
                Java.Lang.Object[] markersToRemove = markerLayer.ObjectsNearPoint(mapView, mapView.ConvertDisplayToInternal(new MapPoint(x, y)), 30);
                if (markersToRemove != null && markersToRemove.Length == 1)
                {
                    markerLayer.Modify(null, new List<Java.Lang.Object> { markersToRemove[0] }, null, true, () =>
                    {
                        Log.Debug("MarkerLayer", "Marker deleted");
                    });
                }
            }
        }

        internal virtual void addMarker(float x, float y)
        {
            if (markerLayer != null)
            {
                MapPoint[] newMarkers = new MapPoint[1];
                newMarkers[0] = mapView.ConvertDisplayToInternal(new MapPoint(x, y));

                markerLayer.Modify(newMarkers, null, null, true, () =>
                {
                    Log.Debug("MarkerLayer", "Marker added");
                });
            }
        }

        internal virtual void addMarkerAsVectorObject(float x, float y)
        {
            if (markerLayer != null)
            {
                GLMapVectorObject[] newMarkers = new GLMapVectorObject[1];
                newMarkers[0] = GLMapVectorObject.CreatePoint(mapView.ConvertDisplayToInternal(new MapPoint(x, y)));

                markerLayer.Modify(newMarkers, null, null, true, () =>
                {
                    Log.Debug("MarkerLayer", "Marker added");
                });
            }
        }

        private static int[] unionColours = new int[] { Color.Argb(255, 33, 0, 255), Color.Argb(255, 68, 195, 255), Color.Argb(255, 63, 237, 198), Color.Argb(255, 15, 228, 36), Color.Argb(255, 168, 238, 25), Color.Argb(255, 214, 234, 25), Color.Argb(255, 223, 180, 19), Color.Argb(255, 255, 0, 0) };

        internal virtual void addMarkersWithMapcss()
        {
            GLMapMarkerStyleCollection styleCollection = new GLMapMarkerStyleCollection();
            for (int i = 0; i < unionColours.Length; i++)
            {
                float scale = (float)(0.2 + 0.1 * i);
                int index = styleCollection.AddStyle(new GLMapMarkerImage("marker" + scale, mapView.ImageManager.Open("cluster.svgpb", scale, unionColours[i])));
                styleCollection.SetStyleName(i, "uni" + index);
            }

            GLMapVectorCascadeStyle style = GLMapVectorCascadeStyle.CreateStyle("node{icon-image:\"uni0\"; text:eval(tag(\"name\")); text-color:#2E2D2B; font-size:12; font-stroke-width:1pt; font-stroke-color:#FFFFFFEE;}" + "node[count>=2]{icon-image:\"uni1\"; text:eval(tag(\"count\"));}" + "node[count>=4]{icon-image:\"uni2\";}" + "node[count>=8]{icon-image:\"uni3\";}" + "node[count>=16]{icon-image:\"uni4\";}" + "node[count>=32]{icon-image:\"uni5\";}" + "node[count>=64]{icon-image:\"uni6\";}" + "node[count>=128]{icon-image:\"uni7\";}");

            new AsyncTaskAnonymousInnerClassHelper(this, styleCollection, style).Execute();
        }

        private class AsyncTaskAnonymousInnerClassHelper : AsyncTask<Java.Lang.Void, Java.Lang.Void, GLMapMarkerLayer>
        {
            private readonly MapViewActivity outerInstance;

            private GLMapMarkerStyleCollection styleCollection;
            private GLMapVectorCascadeStyle style;

            public AsyncTaskAnonymousInnerClassHelper(MapViewActivity outerInstance, GLMapMarkerStyleCollection styleCollection, GLMapVectorCascadeStyle style)
            {
                this.outerInstance = outerInstance;
                this.styleCollection = styleCollection;
                this.style = style;
            }

            protected override GLMapMarkerLayer RunInBackground(params Java.Lang.Void[] voids)
            {
                GLMapMarkerLayer rv;
                try
                {
                    Log.Warn("GLMapView", "Start parsing");
                    GLMapVectorObjectList objects = GLMapVectorObject.CreateFromGeoJSONStream(outerInstance.Assets.Open("cluster_data.json"));
                    Log.Warn("GLMapView", "Finish parsing");

                    GLMapBBox bbox = objects.BBox;
                    outerInstance.RunOnUiThread(() =>
                    {
                        outerInstance.mapView.SetMapCenter(bbox.Center(), false);
                        outerInstance.mapView.SetMapZoom(outerInstance.mapView.MapZoomForBBox(bbox, outerInstance.mapView.Width, outerInstance.mapView.Height), false);
                    });

                    Log.Warn("GLMapView", "Start creating layer");
                    rv = new GLMapMarkerLayer(objects, style, styleCollection);
                    Log.Warn("GLMapView", "Finish creating layer");
                    objects.Dispose();
                }
                catch (System.Exception)
                {
                    rv = null;
                }
                return rv;
            }

            protected override void OnPostExecute(GLMapMarkerLayer layer)
            {
                if (layer != null)
                {
                    outerInstance.markerLayer = layer;
                    outerInstance.mapView.DisplayMarkerLayer(layer);
                }
            }
        }

        internal virtual void addMarkers()
        {
            GLMapMarkerStyleCollection style = new GLMapMarkerStyleCollection();
            int[] unionCounts = new int[] { 1, 2, 4, 8, 16, 32, 64, 128 };
            for (int i = 0; i < unionCounts.Length; i++)
            {
                float scale = (float)(0.2 + 0.1 * i);
                style.AddStyle(new GLMapMarkerImage("marker" + scale, mapView.ImageManager.Open("cluster.svgpb", scale, unionColours[i])));
            }

            GLMapVectorStyle textStyle = GLMapVectorStyle.CreateStyle("{text-color:black;font-size:12;font-stroke-width:1pt;font-stroke-color:#FFFFFFEE;}");
            style.SetDataCallback(new GLMapMarkerStyleCollectionDataCallbackAnonymousInnerClassHelper2(this, unionCounts, textStyle));

            new AsyncTaskAnonymousInnerClassHelper2(this, style).Execute();
        }

        private class GLMapMarkerStyleCollectionDataCallbackAnonymousInnerClassHelper2 : GLMapMarkerStyleCollectionDataCallback
        {
            private readonly MapViewActivity outerInstance;

            private int[] unionCounts;
            private GLMapVectorStyle textStyle;

            public GLMapMarkerStyleCollectionDataCallbackAnonymousInnerClassHelper2(MapViewActivity outerInstance, int[] unionCounts, GLMapVectorStyle textStyle)
            {
                this.outerInstance = outerInstance;
                this.unionCounts = unionCounts;
                this.textStyle = textStyle;
            }

            public override void FillUnionData(int markersCount, long nativeMarker)
            {
                for (int i = unionCounts.Length - 1; i >= 0; i--)
                {
                    if (markersCount > unionCounts[i])
                    {
                        GLMapMarkerStyleCollection.SetMarkerStyle(nativeMarker, i);
                        break;
                    }
                }
                GLMapMarkerStyleCollection.SetMarkerText(nativeMarker, Convert.ToString(markersCount), new Point(0, 0), textStyle);
            }

            public override void FillData(Java.Lang.Object marker, long nativeMarker)
            {
                if (marker is MapPoint)
                {
                    GLMapMarkerStyleCollection.SetMarkerLocation(nativeMarker, (MapPoint)marker);
                    GLMapMarkerStyleCollection.SetMarkerText(nativeMarker, "Test", new Point(0, 0), textStyle);
                }
                else if (marker is GLMapVectorObject)
                {
                    GLMapVectorObject obj = (GLMapVectorObject)marker;
                    GLMapMarkerStyleCollection.SetMarkerLocationFromVectorObject(nativeMarker, obj);
                    string name = obj.ValueForKey("name");
                    if (!string.ReferenceEquals(name, null))
                    {
                        GLMapMarkerStyleCollection.SetMarkerText(nativeMarker, name, new Point(0, 15 / 2), textStyle);
                    }
                }
                GLMapMarkerStyleCollection.SetMarkerStyle(nativeMarker, 0);
            }
        }

        private class AsyncTaskAnonymousInnerClassHelper2 : AsyncTask<Java.Lang.Void, Java.Lang.Void, GLMapMarkerLayer>
        {
            private readonly MapViewActivity outerInstance;

            private GLMapMarkerStyleCollection style;

            public AsyncTaskAnonymousInnerClassHelper2(MapViewActivity outerInstance, GLMapMarkerStyleCollection style)
            {
                this.outerInstance = outerInstance;
                this.style = style;
            }

            protected override GLMapMarkerLayer RunInBackground(params Java.Lang.Void[] voids)
            {
                GLMapMarkerLayer rv;
                try
                {
                    Log.Warn("GLMapView", "Start parsing");
                    GLMapVectorObjectList objects = GLMapVectorObject.CreateFromGeoJSONStream(outerInstance.Assets.Open("cluster_data.json"));
                    Log.Warn("GLMapView", "Finish parsing");

                    GLMapBBox bbox = objects.BBox;
                    outerInstance.RunOnUiThread(() =>
                    {
                        outerInstance.mapView.SetMapCenter(bbox.Center(), false);
                        outerInstance.mapView.SetMapZoom(outerInstance.mapView.MapZoomForBBox(bbox, outerInstance.mapView.Width, outerInstance.mapView.Height), false);
                    });

                    Log.Warn("GLMapView", "Start creating layer");
                    rv = new GLMapMarkerLayer(objects.ToArray(), style);
                    Log.Warn("GLMapView", "Finish creating layer");
                    objects.Dispose();
                }
                catch (Java.Lang.Exception)
                {
                    rv = null;
                }
                return rv;
            }

            protected override void OnPostExecute(GLMapMarkerLayer layer)
            {
                if (layer != null)
                {
                    outerInstance.markerLayer = layer;
                    outerInstance.mapView.DisplayMarkerLayer(layer);
                }
            }

        }

        internal virtual void addImage(object btn, EventArgs e)
        {
            Bitmap bmp = mapView.ImageManager.Open("arrow-maphint.svgpb", 1, 0);
            image = mapView.DisplayImage(bmp);
            image.Offset = new MapPoint(bmp.Width, bmp.Height / 2);
            image.RotatesWithMap = true;
            image.Angle = (float)new Random(360).NextDouble();

            image.Position = mapView.MapCenter;

            ((Button)btn).Text = "Move image";
            ((Button)btn).Click -= addImage;
            ((Button)btn).Click += moveImage;
        }

        internal virtual void moveImage(object btn, EventArgs e)
        {
            image.Position = mapView.MapCenter;
            ((Button)btn).Text = "Remove image";
            ((Button)btn).Click -= moveImage;
            ((Button)btn).Click += delImage;
        }

        internal virtual void delImage(object btn, EventArgs e)
        {
            if (image != null)
            {
                mapView.RemoveImage(image);
                image.Dispose();
                image = null;
            }
            ((Button)btn).Text = "Add image";
            ((Button)btn).Click -= delImage;
            ((Button)btn).Click += addImage;
        }

        internal virtual void addMultiline()
        {
            MapPoint[] line1 = new MapPoint[5];
            line1[0] = MapPoint.CreateFromGeoCoordinates(53.8869, 27.7151); // Minsk
            line1[1] = MapPoint.CreateFromGeoCoordinates(50.4339, 30.5186); // Kiev
            line1[2] = MapPoint.CreateFromGeoCoordinates(52.2251, 21.0103); // Warsaw
            line1[3] = MapPoint.CreateFromGeoCoordinates(52.5037, 13.4102); // Berlin
            line1[4] = MapPoint.CreateFromGeoCoordinates(48.8505, 2.3343); // Paris

            MapPoint[] line2 = new MapPoint[3];
            line2[0] = MapPoint.CreateFromGeoCoordinates(52.3690, 4.9021); // Amsterdam
            line2[1] = MapPoint.CreateFromGeoCoordinates(50.8263, 4.3458); // Brussel
            line2[2] = MapPoint.CreateFromGeoCoordinates(49.6072, 6.1296); // Luxembourg

            MapPoint[][] multiline = new MapPoint[][] { line1, line2 };
            GLMapVectorObject obj = GLMapVectorObject.CreateMultiline(multiline);
            // style applied to all lines added. Style is string with mapcss rules. Read more in manual.
            mapView.AddVectorObjectWithStyle(obj, GLMapVectorCascadeStyle.CreateStyle("line{width: 2pt;color:green;layer:100;}"), null);
        }

        internal virtual void addPolygon()
        {
            int pointCount = 25;
            MapGeoPoint[] outerRing = new MapGeoPoint[pointCount];
            MapGeoPoint[] innerRing = new MapGeoPoint[pointCount];

            float rOuter = 20, rInner = 10;
            float cx = 30, cy = 30;

            // let's display circle
            for (int i = 0; i < pointCount; i++)
            {
                outerRing[i] = new MapGeoPoint(cx + System.Math.Sin(2 * System.Math.PI / pointCount * i) * rOuter, cy + System.Math.Cos(2 * System.Math.PI / pointCount * i) * rOuter);

                innerRing[i] = new MapGeoPoint(cx + System.Math.Sin(2 * System.Math.PI / pointCount * i) * rInner, cy + System.Math.Cos(2 * System.Math.PI / pointCount * i) * rInner);
            }

            MapGeoPoint[][] outerRings = new MapGeoPoint[][] { outerRing };
            MapGeoPoint[][] innerRings = new MapGeoPoint[][] { innerRing };

            GLMapVectorObject obj = GLMapVectorObject.CreatePolygonGeo(outerRings, innerRings);
            mapView.AddVectorObjectWithStyle(obj, GLMapVectorCascadeStyle.CreateStyle("area{fill-color:#10106050; fill-color:#10106050; width:4pt; color:green;}"), null); // #RRGGBBAA format
        }

        private void styleLiveReload()
        {
            EditText editText = (EditText)this.FindViewById(Resource.Id.edit_text);
            editText.Visibility = ViewStates.Visible;

            Button btn = (Button)this.FindViewById(Resource.Id.button_action);
            btn.Visibility = ViewStates.Visible;
            btn.Text = "Reload";
            btn.Click += (object sender, EventArgs e) =>
            {
                new AsyncTaskAnonymousInnerClassHelper3(this).Execute(editText.Text.ToString());
            };
        }

        private class AsyncTaskAnonymousInnerClassHelper3 : AsyncTask<string, string, byte[]>
        {
            private readonly MapViewActivity outerInstance;

            public AsyncTaskAnonymousInnerClassHelper3(MapViewActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            protected override byte[] RunInBackground(params string[] strings)
            {
                byte[] rv;
                try
                {
                    URLConnection connection = (new URL(strings[0])).OpenConnection();
                    connection.Connect();
                    System.IO.Stream inputStream = connection.InputStream;

                    System.IO.MemoryStream buffer = new System.IO.MemoryStream();
                    int nRead;
                    byte[] data = new byte[16384];
                    while ((nRead = inputStream.Read(data, 0, data.Length)) != -1)
                    {
                        buffer.Write(data, 0, nRead);
                    }
                    buffer.Flush();
                    rv = buffer.ToArray();
                    buffer.Close();
                    inputStream.Close();
                }
                catch (Java.Lang.Exception)
                {
                    rv = null;
                }
                return rv;
            }

            protected override void OnPostExecute(byte[] newStyleData)
            {
                outerInstance.mapView.LoadStyle(new ResourceLoadCallbackAnonymousInnerClassHelper(outerInstance, newStyleData));
            }
        }

        private class ResourceLoadCallbackAnonymousInnerClassHelper : Java.Lang.Object, GLMapView.IResourceLoadCallback
        {
            private readonly MapViewActivity outerInstance;

            private byte[] newStyleData;

            public ResourceLoadCallbackAnonymousInnerClassHelper(MapViewActivity outerInstance, byte[] newStyleData)
            {
                this.outerInstance = outerInstance;
                this.newStyleData = newStyleData;
            }

            public byte[] LoadResource(string name)
            {
                byte[] rv;
                if (name.Equals("Style.mapcss"))
                {
                    rv = newStyleData;
                }
                else
                {
                    try
                    {
                        System.IO.Stream stream = outerInstance.Assets.Open("DefaultStyle.bundle/" + name);
                        rv = new byte[stream.Length];
                        if (stream.Read(rv, 0, rv.Length) < rv.Length)
                        {
                            rv = null;
                        }
                        stream.Close();
                    }
                    catch (Java.IO.IOException)
                    {
                        rv = null;
                    }
                }
                return rv;
            }
        }

        private void loadGeoJSON()
        {
            GLMapVectorObjectList objects = GLMapVectorObject.CreateFromGeoJSON(
                    "[{\"type\": \"Feature\", \"geometry\": {\"type\": \"Point\", \"coordinates\": [30.5186, 50.4339]}, \"properties\": {\"id\": \"1\", \"text\": \"test1\"}},"
                    + "{\"type\": \"Feature\", \"geometry\": {\"type\": \"Point\", \"coordinates\": [27.7151, 53.8869]}, \"properties\": {\"id\": \"2\", \"text\": \"test2\"}},"
                    + "{\"type\":\"LineString\",\"coordinates\": [ [27.7151, 53.8869], [30.5186, 50.4339], [21.0103, 52.2251], [13.4102, 52.5037], [2.3343, 48.8505]]},"
                    + "{\"type\":\"Polygon\",\"coordinates\":[[ [0.0, 10.0], [10.0, 10.0], [10.0, 20.0], [0.0, 20.0] ],[ [2.0, 12.0], [ 8.0, 12.0], [ 8.0, 18.0], [2.0, 18.0] ]]}]");

            GLMapVectorCascadeStyle style = GLMapVectorCascadeStyle.CreateStyle(
                    "node[id=1]{icon-image:\"bus.svgpb\";icon-scale:0.5;icon-tint:green;text:eval(tag('text'));text-color:red;font-size:12;text-priority:100;}"
                    + "node|z-9[id=2]{icon-image:\"bus.svgpb\";icon-scale:0.7;icon-tint:blue;text:eval(tag('text'));text-color:red;font-size:12;text-priority:100;}"
                    + "line{linecap: round; width: 5pt; color:blue;}"
                    + "area{fill-color:green; width:1pt; color:red;}");

            mapView.AddVectorObjectsWithStyle(objects.ToArray(), style);
        }

        void captureScreen()
        {
            GLMapView mapView = (GLMapView)this.FindViewById(Resource.Id.map_view);
            mapView.CaptureFrameWhenFinish(this);
        }

        public void ScreenCaptured(Bitmap bmp)
        {
            this.RunOnUiThread(new Runnable(() =>
            {
                System.IO.MemoryStream bytes = new System.IO.MemoryStream();
                bmp.Compress(Bitmap.CompressFormat.Png, 100, bytes);
                try
                {
                    System.IO.Stream fo = OpenFileOutput("screenCapture", FileCreationMode.Private);
                    fo.Write(bytes.ToArray(), 0, (int)bytes.Length);
                    fo.Close();

                    Java.IO.File file = new Java.IO.File(GetExternalFilesDir(null), "Test.jpg");
                    Java.IO.FileOutputStream fo2 = new FileOutputStream(file);
                    fo2.Write(bytes.ToArray());
                    fo2.Close();
                }
                catch (Java.IO.IOException e)
                {
                    e.PrintStackTrace();
                }

                Intent intent = new Intent(this, typeof(DisplayImageActivity));
                Bundle b = new Bundle();
                b.PutString("imageName", "screenCapture");
                intent.PutExtras(b);

                StartActivity(intent);
            }));
        }
    }
}