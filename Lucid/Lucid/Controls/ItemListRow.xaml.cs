using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NPLib;
using System.IO;

namespace Lucid.Controls
{
    /// <summary>
    /// Interaction logic for ItemListRow.xaml
    /// </summary>
    public partial class ItemListRow : UserControl
    {
        public NPLib.Models.Item Item { get; set; }
        public BitmapImage ItemBitmap { get; set; }

        public ItemListRow()
        {
            InitializeComponent();

            ItemBitmap = new BitmapImage();
            ItemBitmap.BeginInit();

            var memoryStream = Item.Image.DownloadIntoMemory().Result;
            memoryStream.Seek(0, SeekOrigin.Begin);

            ItemBitmap.StreamSource = memoryStream;
            ItemBitmap.EndInit();
        }
    }
}
