using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using GLMap;
using static GLMap.GLMapManager;
using Java.Lang;
using static Android.Widget.AdapterView;

namespace Xam_GLMap_Android_Demo
{
    [Activity(Label = "DownloadActivity")]
    public class DownloadActivity : ListActivity, GLMapManager.IStateListener
    {
        private enum ContextItems
        {
            Delete
        }

        private MapPoint center;
        private GLMapInfo selectedMap = null;
        private GLMapLocaleSettings localeSettings = new GLMapLocaleSettings();
        private ListView listView;

        internal class MapsAdapter : BaseAdapter, IListAdapter
        {
            private GLMapInfo[] maps;
            private Context context;
            private GLMapLocaleSettings localeSettings;

            public MapsAdapter(GLMapInfo[] maps, Context context, GLMapLocaleSettings localeSettings)
            {
                this.maps = maps;
                this.context = context;
                this.localeSettings = localeSettings;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                GLMapInfo map = maps[position];
                TextView txtDescription, txtHeaderName;
                if (convertView == null)
                {
                    convertView = LayoutInflater.From(context).Inflate(Resource.Layout.map_name, null);
                }
                txtHeaderName = ((TextView)convertView.FindViewById(Android.Resource.Id.Text1));
                txtDescription = ((TextView)convertView.FindViewById(Android.Resource.Id.Text2));

                string str = map.GetLocalizedName(localeSettings);

                txtHeaderName.SetText(str.ToCharArray(), 0, str.Length);

                if (map.IsCollection)
                {
                    str = "Collection";
                }
                else if (map.State == GLMapInfoState.Downloaded)
                {
                    str = "Downloaded";
                }
                else if (map.State == GLMapInfoState.NeedUpdate)
                {
                    str = "Need update";
                }
                else if (map.State == GLMapInfoState.NeedResume)
                {
                    str = "Need resume";
                }
                else if (map.State == GLMapInfoState.InProgress)
                {
                    str = string.Format("Download {0:0.00}%", map.DownloadProgress * 100);
                }
                else
                {
                    str = NumberFormatter.FormatSize(map.Size);
                }

                txtDescription.SetText(str.ToCharArray(), 0, str.Length);

                return convertView;
            }

            public GLMapInfo[] Maps
            {
                get
                {
                    return maps;
                }
            }

            public override int Count
            {
                get
                {
                    return maps.Length;
                }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return maps[position];
            }

            public override long GetItemId(int position)
            {
                return position;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.download);

            listView = (ListView)FindViewById(Android.Resource.Id.List);
            RegisterForContextMenu(listView);
            IEnumerable<GLMapDownloadTask> tasks = GLMapManager.MapDownloadTasks;
            GLMapManager.AddStateListener((IStateListener)this);

            Intent i = Intent;
            center = new MapPoint(i.GetDoubleExtra("cx", 0.0), i.GetDoubleExtra("cy", 0.0));
            long collectionID = i.GetLongExtra("collectionID", 0);
            if (collectionID != 0)
            {
                GLMapInfo collection = GLMapManager.GetMapWithID(collectionID);
                if (collection != null)
                {
                    UpdateAllItems(collection.GetMaps());
                }
            }
            else
            {
                UpdateAllItems(GLMapManager.GetMaps());
                GLMapManager.UpdateMapList(this, new Runnable(() => UpdateAllItems(GLMapManager.GetMaps())));
    		}
        }

        protected override void OnDestroy()
        {
            GLMapManager.RemoveStateListener((IStateListener)this);
            base.OnDestroy();
        }

        public void OnStartDownloading(GLMapInfo map)
        {

        }

        public void OnDownloadProgress(GLMapInfo map)
        {
            ((MapsAdapter)listView.Adapter).NotifyDataSetChanged();
        }

        public void OnFinishDownloading(GLMapInfo map)
        {

        }

        public void OnStateChanged(GLMapInfo map)
        {
            ((MapsAdapter)listView.Adapter).NotifyDataSetChanged();
        }


        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);

            if (selectedMap != null)
            {
                menu.SetHeaderTitle(selectedMap.GetLocalizedName(localeSettings));
                menu.Add(0, (int)ContextItems.Delete, (int)ContextItems.Delete, "Delete");
            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            ContextItems selected = (ContextItems)item.ItemId;
            switch (selected)
            {
                case ContextItems.Delete:
                    selectedMap.DeleteFiles();
                    ListView listView = (ListView)FindViewById(Android.Resource.Id.List);
                    ((MapsAdapter)listView.Adapter).NotifyDataSetChanged();
                    break;

                default:
                    return base.OnOptionsItemSelected(item);
            }
            return true;
        }

        public void UpdateAllItems(GLMapInfo[] maps)
        {
            if (maps == null)
                return;

            GLMapManager.SortMaps(maps, center);
            ListView listView = (ListView)FindViewById(Android.Resource.Id.List);
            listView.Adapter = new MapsAdapter(maps, this, localeSettings);
            listView.ItemClick += (object sender, ItemClickEventArgs e) => {
                GLMapInfo info = (GLMapInfo)listView.Adapter.GetItem(e.Position);
                if (info.IsCollection)
                {
                    Intent intent = new Intent(this, typeof(DownloadActivity));
                    intent.PutExtra("collectionID", info.MapID);
                    intent.PutExtra("cx", center.X);
                    intent.PutExtra("cy", center.Y);
                    StartActivity(intent);
                }
                else
                {
                    GLMapDownloadTask task = GLMapManager.GetDownloadTask(info);
                    if (task != null)
                    {
                        task.Cancel();
                    }
                    else if (info.State != GLMapInfoState.Downloaded)
                    {
                        GLMapManager.CreateDownloadTask(info, this).Start();
                    }
                }
            };

            listView.ItemLongClick += (object sender, ItemLongClickEventArgs e) => {
                GLMapInfo info = ((MapsAdapter)this.listView.Adapter).Maps[e.Position];
                GLMapInfoState state = info.State;
                if (state == GLMapInfoState.Downloaded || state == GLMapInfoState.NeedResume || state == GLMapInfoState.NeedUpdate)
                {
                    selectedMap = info;
                    //return false;
                }
                else
                {
                    //return true;
                }
            };
        }    
    }
}