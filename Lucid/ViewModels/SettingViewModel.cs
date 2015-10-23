using Caliburn.Micro;
using NPLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;

namespace Lucid.ViewModels
{
    [Export(typeof(SettingViewModel))]
    class SettingViewModel : Screen
    {
        private readonly IWindowManager _windowManager;

        private Dictionary<int, string> _mainShopList = MainShopManager.GetAllMainShops();
        private int _itemSelectedIndex = -1, _activeTabIndex = 0;
        private ObservableCollection<string> _itemList = new ObservableCollection<string>();
        private string _addItemName = "";

        public string Password { get; set; }

        public string AddItemName
        {
            get
            {
                return _addItemName;
            }
            set
            {
                _addItemName = value;
                NotifyOfPropertyChange(() => AddItemName);
                NotifyOfPropertyChange(() => CanAddItem);
            }
        }

        public int ActiveTabIndex
        {
            get
            {
                return _activeTabIndex;
            }
            set
            {
                _activeTabIndex = value;
                NotifyOfPropertyChange(() => ActiveTabIndex);
            }
        }

        public int ItemSelectedIndex
        {
            get
            {
                return _itemSelectedIndex;
            }
            set
            {
                _itemSelectedIndex = value;
                NotifyOfPropertyChange(() => ItemSelectedIndex);
                NotifyOfPropertyChange(() => CanRemoveItem);
            }
        }

        public ObservableCollection<string> ItemList
        {
            get
            {
                return _itemList;
            }
            set
            {
                _itemList = value;
                NotifyOfPropertyChange(() => ItemList);
                NotifyOfPropertyChange(() => CanExportList);
            }
        }


        public Dictionary<int, string> ShopList
        {
            get {
                return _mainShopList;
            }
            set
            {
                _mainShopList = value;
                NotifyOfPropertyChange(() => ShopList);
            }
        }

        [ImportingConstructor]
        public SettingViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void Save()
        {
            Properties.Settings.Default.Login_Password = Password;
            Properties.Settings.Default.Save();
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }

        public void ImportList()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var text = File.ReadAllLines(openFileDialog.FileName);
                foreach(var item in text)
                {
                    ItemList.Add(item);
                    NotifyOfPropertyChange(() => ItemList);
                    NotifyOfPropertyChange(() => CanExportList);
                }
            }
        }

        public bool CanExportList
        {
            get
            {
                return (ItemList.Count > 0);
            }
        }

        public void ExportList()
        {

        }

        public bool CanAddItem
        {
            get
            {
                return !string.IsNullOrWhiteSpace(AddItemName);
            }
        }
        public void AddItem()
        {
            ItemList.Add(AddItemName);
            AddItemName = string.Empty;
            NotifyOfPropertyChange(() => ItemList);
            NotifyOfPropertyChange(() => CanExportList);
        }

        public bool CanRemoveItem
        {
            get
            {
                return (_itemSelectedIndex >= 0 && _itemSelectedIndex < _itemList.Count);
            }
        }

        public void RemoveItem()
        {
            ItemList.RemoveAt(ItemSelectedIndex);
            ItemSelectedIndex = -1;
            NotifyOfPropertyChange(() => ItemList);
            NotifyOfPropertyChange(() => CanExportList);
        }
    }
}
