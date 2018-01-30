using System;
using System.IO;
using System.Reflection;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDrawBitmapDemo
{
    public partial class MainPage : ContentPage
    {
        SKBitmap resourceBitmap, nativeBitmap;

        public MainPage()
        {
            InitializeComponent();

            LoadBitmap();
        }

        async void LoadBitmap()
        {
            //load a bitmap from embeded resource
#if __ANDROID__
            string prefix = "SkiaSharpDrawBitmapDemo.Droid.";
#elif __IOS__
            string prefix = "SkiaSharpDrawBitmapDemo.iOS.";
#endif
            string resourceID = prefix + "assets.message.png";
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            using (SKManagedStream skStream = new SKManagedStream(stream))
            {
                resourceBitmap = SKBitmap.Decode(skStream);
            }

            //load a bitmap from native projects
            nativeBitmap = await ImageUtils.BitmapFromNativeFile("message.png");

            //the follwing codes compare the two bitmaps
            byte[] resourceBytes = resourceBitmap.Bytes;
            byte[] nativeBytes = nativeBitmap.Bytes;
            int length = resourceBytes.Length;
            int sameCount = 0;
            for (int i = 0; i < length; i++)
            {
                if (resourceBytes[i] == nativeBytes[i])
                    sameCount++;
            }
            Console.WriteLine("bitmap bytes comparing: total=" + length + ", same=" + sameCount + ", ratio=" + (float)sameCount / length);

            canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            SKImageInfo info = args.Info;
            int canvasWidth = info.Width;
            int canvasHight = info.Height;

            canvas.Clear(SKColors.Black);

            if (resourceBitmap != null)
                canvas.DrawBitmap(resourceBitmap, canvasWidth / 4, canvasHight / 2);

            if (nativeBitmap != null)
                canvas.DrawBitmap(nativeBitmap, canvasWidth / 2, canvasHight / 2);
        }
    }
}
