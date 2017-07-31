using Java.Lang;
using System;

namespace GLMap
{
    public partial class GLMapView
    {
        public GLMap.GLMapTileState CenterTileState
        {
            get
            {
                return (GLMap.GLMapTileState)getCenterTileState().Ordinal();
            }
        }

        public GLMapRasterTileSource[] RasterTileSources
        {
            get
            {
                return getRasterTileSources();

            }
            set
            {
                setRasterTileSources(value);
            }
        }

        public void SetAttributionPosition(GLMap.GLMapPlacement placement)
        {
            setAttributionPosition(GLMap.GLMapView.GLMapPlacement.Values()[(int)placement]);
        }

        public void SetScaleRulerStyle(GLMap.GLUnits units, GLMap.GLMapPlacement placement, MapPoint paddings, double maxWidth)
        {
            setScaleRulerStyle(GLMap.GLMapView.GLUnits.Values()[(int)units], GLMap.GLMapView.GLMapPlacement.Values()[(int)placement], paddings, maxWidth);
        }

        public void SetMapDidMoveCallback(Action action)
        {
            setMapDidMoveCallback(new Runnable(action));
        }

        public void SetCenterTileStateChangedCallback(Action action)
        {
            setCenterTileStateChangedCallback(new Runnable(action));
        }

        public void DoWhenSurfaceCreated(Action action)
        {
            doWhenSurfaceCreated(new Runnable(action));
        }
    }
}