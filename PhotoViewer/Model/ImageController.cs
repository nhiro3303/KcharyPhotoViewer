﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Model
{
    public static class ImageController
    {
        /// <summary>
        /// Native image control method class.
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern int GetRawImageData(string path, ref ImageData imageData);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern int GetRawThumbnailImageData(string path, int longSideLength, ref ImageData imageData);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern int GetNormalImageData(string path, ref ImageData imageData);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern int GetNormalThumbnailImageData(string path, int longSideLength, ref ImageData imageData);

            [DllImport("ImageController.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern void FreeBuffer(IntPtr buffer);

            [StructLayout(LayoutKind.Sequential)]
            internal struct ImageData
            {
                public IntPtr buffer;
                public uint size;
                public int stride;
                public int width;
                public int height;
            }
        }

        /// <summary>
        /// Generate an image to be magnified.
        /// </summary>
        /// <param name="filePath">FilePath</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureViewImage(string filePath)
        {
            const int longSideLength = 2200;
            return DecodePicture(filePath, longSideLength);
        }

        /// <summary>
        /// Creating thumbnail images of still images.
        /// </summary>
        /// <param name="filePath">FilePath</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureThumbnailImage(string filePath)
        {
            const int longSideLength = 100;
            return DecodePicture(filePath, longSideLength);
        }

        /// <summary>
        /// Create thumbnail image for still image edit screen.
        /// </summary>
        /// <param name="filePath">FilePath</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreatePictureEditViewThumbnail(string filePath, out int defaultPictureWidth, out int defaultPictureHeight, out uint rotation)
        {
            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            sourceStream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(sourceStream);

            defaultPictureWidth = bitmapFrame.PixelWidth;
            defaultPictureHeight = bitmapFrame.PixelHeight;
            rotation = GetRotation(bitmapFrame.Metadata as BitmapMetadata);

            const int longSideLength = 240;
            return DecodePicture(filePath, longSideLength);
        }

        /// <summary>
        /// Create save image.
        /// </summary>
        /// <param name="filePath"><FilePath/param>
        /// <param name="scale">scale</param>
        /// <returns>BitmapSOurce</returns>
        public static BitmapSource CreateSavePicture(string filePath, double scale)
        {
            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            sourceStream.Seek(0, SeekOrigin.Begin);
            var bitmapFrame = BitmapFrame.Create(sourceStream);

            // Check the rotation information.
            var metaData = bitmapFrame.Metadata as BitmapMetadata;

            // Decode picture.
            BitmapSource saveImage = bitmapFrame;

            // Resize image.
            saveImage = new TransformedBitmap(saveImage, new ScaleTransform(scale, scale));

            // Rotate image.
            saveImage = RotateImage(metaData, saveImage);

            var decodeImage = new WriteableBitmap(saveImage);
            decodeImage.Freeze();

            return decodeImage;
        }

        /// <summary>
        /// Decode image from filePath.(If you set max width and max height, Image is resized.)
        /// </summary>
        /// <param name="filePath">FilePath</param>
        /// <param name="longSideLength">Long side Length of a resize picture</param>
        /// <returns>BitmapSource</returns>
        private static BitmapSource DecodePicture(string filePath, int longSideLength)
        {
            if (MediaChecker.CheckRawFileExtension(Path.GetExtension(filePath).ToLower()))
            {
                var imageData = new NativeMethods.ImageData();
                if (NativeMethods.GetRawThumbnailImageData(filePath, longSideLength, ref imageData) != 0)
                {
                    throw new FileFormatException("File format is wrong.");
                }

                var imgData = new byte[imageData.size];
                Marshal.Copy(imageData.buffer, imgData, 0, (int)imageData.size);
                NativeMethods.FreeBuffer(imageData.buffer);

                var bitmap = new WriteableBitmap(imageData.width, imageData.height, 96, 96, PixelFormats.Bgr24, null);
                bitmap.WritePixels(new Int32Rect(0, 0, imageData.width, imageData.height), imgData, imageData.stride, 0, 0);
                bitmap.Freeze();
                
                return bitmap;
            }
            else
            {
                var imageData = new NativeMethods.ImageData();
                if (NativeMethods.GetNormalThumbnailImageData(filePath, longSideLength, ref imageData) != 0)
                {
                    throw new FileFormatException("File format is wrong.");
                }

                var imgData = new byte[imageData.size];
                Marshal.Copy(imageData.buffer, imgData, 0, (int)imageData.size);
                NativeMethods.FreeBuffer(imageData.buffer);

                var bitmap = new WriteableBitmap(imageData.width, imageData.height, 96, 96, PixelFormats.Bgr24, null);
                bitmap.WritePixels(new Int32Rect(0, 0, imageData.width, imageData.height), imgData, imageData.stride, 0, 0);
                bitmap.Freeze();

                return bitmap;
            }
        }

        /// <summary>
        /// Rotate image
        /// </summary>
        /// <param name="metaData">Metadata</param>
        /// <param name="image">BitmapSource</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource RotateImage(BitmapMetadata metaData, BitmapSource image)
        {
            var rotation = GetRotation(metaData);

            return rotation switch
            {
                1 => image,
                3 => TransformBitmap(image, new RotateTransform(180)),
                6 => TransformBitmap(image, new RotateTransform(90)),
                8 => TransformBitmap(image, new RotateTransform(270)),
                2 => TransformBitmap(image, new ScaleTransform(-1, 1, 0, 0)),
                4 => TransformBitmap(image, new ScaleTransform(1, -1, 0, 0)),
                5 => TransformBitmap(TransformBitmap(image, new RotateTransform(90)), new ScaleTransform(-1, 1, 0, 0)),
                7 => TransformBitmap(TransformBitmap(image, new RotateTransform(270)), new ScaleTransform(-1, 1, 0, 0)),
                _ => image,
            };
        }

        /// <summary>
        /// Get image rotation information from image metadata.
        /// </summary>
        /// <param name="metaData">Metadata</param>
        /// <returns>rotation value</returns>
        private static uint GetRotation(BitmapMetadata metaData)
        {
            var _query = "/app1/ifd/exif:{uint=274}";
            if (!metaData.ContainsQuery(_query))
            {
                return 0;
            }

            return Convert.ToUInt32(metaData.GetQuery(_query));
        }

        /// <summary>
        /// Rotate the image.
        /// </summary>
        private static BitmapSource TransformBitmap(BitmapSource source, Transform transform)
        {
            var result = new TransformedBitmap();
            result.BeginInit();
            result.Source = source;
            result.Transform = transform;
            result.EndInit();
            return result;
        }
    }
}