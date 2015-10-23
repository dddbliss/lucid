using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Lucid
{
    public static class StreamExtensions
    {
        public static BitmapImage GetBitmapImage(this MemoryStream @this)
        {
            BitmapImage bm = new BitmapImage();
            bm.BeginInit();
            @this.Seek(0, SeekOrigin.Begin);
            bm.StreamSource = @this;
            bm.EndInit();

            return bm;
        }
    }
}
