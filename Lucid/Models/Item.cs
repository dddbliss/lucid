using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPLib;
using System.Windows.Media.Imaging;
using NPLib.Models;

namespace Lucid.Models
{
    public class Item
    {
        private string _name;
        private Uri _image;
        private int _cost, _in_stock;

        public string Name
        {
            get
            {
                return _name;     
            }
        }

        //public Uri Image { get; set; }

        public string Cost
        {
            get
            {
                return string.Format("{0} NP", _cost.ToString("N0"));
            }
        }
        public string InStock
        {  
            get
            {
                return string.Format("{0} In Stock", _in_stock);
            }
        }

        public BitmapImage Bitmap { get; set; }

        public Item(MainShopItem item)
        {
            _name = item.Name;
            _cost = item.Cost;
            _in_stock = item.InStock;
            _image = item.Image;
        }
    }
}
