//----------------------------------------------------------------------------
//  Copyright (C) 2004-2014 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

namespace Emgu.CV
{
   public partial class Mat
   {
      public WriteableBitmap ToWritableBitmap()
      {
         Size size = Size;
         WriteableBitmap bmp = new WriteableBitmap(size.Width, size.Height);
         byte[] buffer = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
         GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
         using (Image<Bgra, byte> resultImage = new Image<Bgra, byte>(bmp.PixelWidth, bmp.PixelHeight, bmp.PixelWidth * 4, handle.AddrOfPinnedObject()))
         {
            resultImage.ConvertFrom(this);
         }
         handle.Free();
         using (Stream resultStream = bmp.PixelBuffer.AsStream())
         {
            resultStream.Write(buffer, 0, buffer.Length);
         }
         return bmp;
      }
      
      public static async Task<Mat> FromStorageFile(StorageFile file)
      {
         using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
         {
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);

            Size s = new Size((int)decoder.PixelWidth, (int)decoder.PixelHeight);

            BitmapTransform transform = new BitmapTransform();               
            PixelDataProvider pixelData = await decoder.GetPixelDataAsync(
            BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, transform, ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage);

            byte[] sourcePixels = pixelData.DetachPixelData();
            GCHandle handle = GCHandle.Alloc(sourcePixels, GCHandleType.Pinned);
            using (Image<Bgra, Byte> img = new Image<Bgra, byte>(s.Width, s.Height, s.Width * 4, handle.AddrOfPinnedObject()))
            {
               Mat m = new Mat();
               CvInvoke.CvtColor(img, m, ColorConversion.Bgra2Bgr);
               handle.Free();
               return m;
            }
         }
      }

      public static async Task<Mat> FromMediaCapture(MediaCapture _mediaCapture)
      {
         using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
         {
            await _mediaCapture.CapturePhotoToStreamAsync(Windows.Media.MediaProperties.ImageEncodingProperties.CreateJpeg(), stream);
            stream.Seek(0);
            byte[] data = new byte[stream.Size];
            await stream.AsStreamForRead().ReadAsync(data, 0, data.Length);
            Mat result = new Mat();
            CvInvoke.Imdecode(data, LoadImageType.Color, result);
            return result;
         }
      }
   }
}

