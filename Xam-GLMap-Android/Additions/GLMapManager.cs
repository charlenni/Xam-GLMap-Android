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

namespace GLMap
{
    public partial class GLMapManager
    {
        public static IEnumerable<GLMapDownloadTask> MapDownloadTasks
        {
            get
            {
                List<GLMapDownloadTask> output = new List<GLMapDownloadTask>();
                var input = getMapDownloadTasks();

                for (int i = 0; i < input.Count; i++)
                {
                    output.Add(input[i]);
                }

                return output;
            }
        }
    }
}