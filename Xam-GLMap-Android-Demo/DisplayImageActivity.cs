using Android.App;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using Java.IO;
using GLMap;

namespace Xam_GLMap_Android_Demo
{
    [Activity(Label = "DisplayImageActivity")]
    public class DisplayImageActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.display_image);

            Bundle b = this.Intent.Extras;
            string imgPath = b != null ? b.GetString("imageName") : null;
            if (imgPath != null)
            {
                try
                {
                    Bitmap bmp = BitmapFactory.DecodeStream(OpenFileInput(imgPath));
                    ImageView imageView = (ImageView)this.FindViewById(Resource.Id.image_view);
                    imageView.SetMinimumWidth((int)(bmp.Width * 0.5));
                    imageView.SetMinimumHeight((int)(bmp.Height * 0.5));
                    imageView.SetMaxWidth((int)(bmp.Width * 0.5));
                    imageView.SetMaxHeight((int)(bmp.Height * 0.5));
                    imageView.SetImageBitmap(bmp);
                }
                catch (FileNotFoundException e)
                {
                }
            }
            else
            {
                ImageManager mgr = new ImageManager(this.Assets, 1);
                Bitmap bmp = mgr.Open("DefaultStyle.bundle/theme_park.svgpb", 4, unchecked((int)0xFF800000));
                //Bitmap bmp = mgr.open("star.svgpb", 4, 0xFFFFFFFF);
                ImageView imageView = (ImageView)this.FindViewById(Resource.Id.image_view);
                imageView.SetMinimumWidth(bmp.Width * 2);
                imageView.SetMinimumHeight(bmp.Height * 2);
                imageView.SetMaxWidth(bmp.Width * 2);
                imageView.SetMaxHeight(bmp.Height * 2);
                imageView.SetImageBitmap(bmp);
            }
        }
    }
}