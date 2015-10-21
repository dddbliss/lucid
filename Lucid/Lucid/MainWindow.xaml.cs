using Lucid.Models;
using MahApps.Metro.Controls;
using NPLib;
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

namespace Lucid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public Dictionary<int, string> MainShopList { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            MainShopList = MainShopManager.GetAllMainShops();
            cmbMainShop.ItemsSource = MainShopList;
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            UserManager _um = new UserManager();

            if(await _um.Login(txtUsername.Text, txtPassword.Password))
            {
                lblNP.Content = _um.CurrentUser.NP.ToString() + " NP";
            }
        }

        private async void btnCheckMainShop_Click(object sender, RoutedEventArgs e)
        {
            MainShopManager _msm = new MainShopManager();

            var _main_shop_items = await _msm.GetItemsInShop((int)cmbMainShop.SelectedValue);
            List<Item> _my_shop_items = new List<Item>();
            _main_shop_items.ForEach(item => _my_shop_items.Add(new Item(item)));

            dgMainShopStock.ItemsSource = _my_shop_items;
        }
    }
}
