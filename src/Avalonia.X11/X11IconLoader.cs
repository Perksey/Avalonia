using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Utilities;
using Avalonia.Visuals.Media.Imaging;
using static Avalonia.X11.XLib;
namespace Avalonia.X11
{
    class X11IconLoader : IPlatformIconLoader
    {
        private readonly X11Info _x11;

        public X11IconLoader(X11Info x11)
        {
            _x11 = x11;
        }
        
        IWindowIconImpl LoadIcon(Bitmap bitmap) => new X11IconData(bitmap);

        public IWindowIconImpl LoadIcon(string fileName) => LoadIcon(new Bitmap(fileName));

        public IWindowIconImpl LoadIcon(Stream stream) => LoadIcon(new Bitmap(stream));

        public IWindowIconImpl LoadIcon(IBitmapImpl bitmap)
        {
            var ms = new MemoryStream();
            bitmap.Save(ms);
            ms.Position = 0;
            return LoadIcon(ms);
        }
    }
    
    unsafe class X11IconData : IWindowIconImpl, IFramebufferPlatformSurface
    {
        private readonly Bitmap _bitmap;
        private int _width;
        private int _height;
        private uint[] _bdata;
        public IntPtr[]  Data { get; }
        
        public X11IconData(Bitmap bitmap)
        {
            _bitmap = bitmap;
            _width = Math.Min(_bitmap.PixelSize.Width, 128);
            _height = Math.Min(_bitmap.PixelSize.Height, 128);
            _bdata = new uint[_width * _height];
            fixed (void* ptr = _bdata)
            {
                var iptr = (int*)ptr;
                iptr[0] = _width;
                iptr[1] = _height;
            }
            using(var rt = AvaloniaLocator.Current.GetService<IPlatformRenderInterface>().CreateRenderTarget(new[]{this}))
            using (var ctx = rt.CreateDrawingContext(null))
                ctx.DrawImage(bitmap.PlatformImpl, 1, new Rect(bitmap.Size),
                    new Rect(0, 0, _width, _height));
            Data = new IntPtr[_width * _height + 2];
            Data[0] = new IntPtr(_width);
            Data[1] = new IntPtr(_height);
            for (var y = 0; y < _height; y++)
            {
                var r = y * _width;
                for (var x = 0; x < _width; x++)
                    Data[r + x] = new IntPtr(_bdata[r + x]);
            }
        }

        public void Save(Stream outputStream)
        {
            _bitmap.Save(outputStream);
        }

        public ILockedFramebuffer Lock()
        {
            var h = GCHandle.Alloc(_bdata, GCHandleType.Pinned);
            return new LockedFramebuffer(h.AddrOfPinnedObject(), new PixelSize(_width, _height), _width * 4,
                new Vector(96, 96), PixelFormat.Bgra8888,
                () => h.Free());
        }
    }
}
