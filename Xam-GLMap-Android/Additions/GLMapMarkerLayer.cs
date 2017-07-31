using System;
using System.Collections.Generic;
using Java.Lang;

namespace GLMap
{
    public partial class GLMapMarkerLayer
    {
        public void Modify(Java.Lang.Object[] markers, ICollection<Java.Lang.Object> p1, ICollection<Java.Lang.Object> p2, bool p3, Action action)
        {
            modify(markers, p1, p2, p3, new Runnable(action));
        }
    }
}