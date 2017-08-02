using System;
using System.Collections.Generic;
using Java.Lang;

namespace GLMap
{
    public partial class GLMapMarkerLayer
    {
        public void Modify(Java.Lang.Object[] newMarkers, ICollection<Java.Lang.Object> markersToRemove, ICollection<Java.Lang.Object> markersToReload, bool animated, Action complete)
        {
            modify(newMarkers, markersToRemove, markersToReload, animated, new Runnable(complete));
        }
    }
}