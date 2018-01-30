using System.Threading.Tasks;
using SkiaSharp;
using Xamarin.Forms;
using System;
#if __ANDROID__
using Xamarin.Forms.Platform.Android;
using Android.Graphics;
using SkiaSharp.Views.Android;
using Android.Content;
#else
using Xamarin.Forms.Platform.iOS;
using UIKit;
using SkiaSharp.Views.iOS;
using CoreGraphics;
#endif

namespace SkiaSharpDrawBitmapDemo
{
    public class ImageUtils
    {
        public ImageUtils()
        {
        }

#if __ANDROID__
        static Context context;

        public static void Init(Context _context) {
            context = _context;
        }
#endif

        static void CheckBytes(Byte[] bytes, int rowBytes = 0)
        {
            int nonEmptyCount = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != 0)
                {
                    if(rowBytes > 0) {
                        int position = i / 4;
                        int rgba = position % 4;
                        int row = position / rowBytes;
                        int column = position % rowBytes;
                        Console.WriteLine("non empty byte: index=" + i + ", row=" + row + ", column=" + column + ", rgba=" + rgba + ", value=" + bytes[i]);
                    }
                    nonEmptyCount++;
                }
            }
            Console.WriteLine("non empty bytes count: " + nonEmptyCount);
        }

        public async static Task<SKBitmap> BitmapFromNativeFile(string fileName)
        {
            var imageSource = ImageSource.FromFile(fileName);
            IImageSourceHandler imageLoader = new FileImageSourceHandler();
#if __ANDROID__
            if (context == null)
                throw new Exception("ImageLoader not initialized.");
            Bitmap image = await imageLoader.LoadImageAsync(imageSource, context);
            return image.ToSKBitmap();
#else
            UIImage image = await imageLoader.LoadImageAsync(imageSource);
            //return image.ToSKBitmap();

            CGImage cgImage = image.CGImage;
            var info = new SKImageInfo((int)cgImage.Width, (int)cgImage.Height);
            var bitmap = new SKBitmap(info);
            using (var pixmap = bitmap.PeekPixels())
            using (var colorSpace = CGColorSpace.CreateDeviceRGB())
            using (var context = new CGBitmapContext(pixmap.GetPixels(), pixmap.Width, pixmap.Height, 8, pixmap.RowBytes, colorSpace, CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big))
            {
                CheckBytes(bitmap.Bytes);
                SKCanvas canvas = new SKCanvas(bitmap);
                canvas.Clear();
                canvas.Flush();
                CheckBytes(bitmap.Bytes);
                context.DrawImage(new CGRect(0, 0, cgImage.Width, cgImage.Height), cgImage);
                CheckBytes(bitmap.Bytes);
            }
            return bitmap;
#endif
        }
    }
}
