﻿using Caliburn.Micro;
using Lucid.ViewModels;
using NPLib;
using NPLib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lucid
{
    [Export(typeof(AppViewModel))]
    class AppViewModel : PropertyChangedBase
    {
        private const string WindowTitleDefault = "Lucid MSAB(a)";
        private string _windowTitle = WindowTitleDefault;


        private ImageSource _shopperImageSource = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/next_25px.png"));
        private User _currentUser = new User() { is_authenticated = false };
        private bool _shopperIsActive = false;

        private ObservableCollection<MainShopItem> ShopStockList { get; set; }

        public LogViewModel LogViewModel { get; set; }
        public SettingViewModel SettingsViewModel { get; set; }

        public IWindowManager WinManager {
            get
            {
                return _windowManager;
            }
        }


        // NP objects.
        public User CurrentUser
        {
            get
            {
                return _currentUser;
            }
            set
            {
                _currentUser = value;
                NotifyOfPropertyChange(() => CurrentUser);
            }
        }

        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                NotifyOfPropertyChange(() => WindowTitle);
            }
        }

        public ImageSource ManageShopperImage
        {
            get
            {
                return _shopperImageSource;
            }
            set
            {
                _shopperImageSource = value;
                NotifyOfPropertyChange(() => ManageShopperImage);
            }
        }

        public bool ShopperIsActive
        {
            get
            {
                return _shopperIsActive;
            }
            set
            {
                _shopperIsActive = value;

                if (!value)
                {
                    ManageShopperImage = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/close_window_25px.png"));
                }
                else
                {
                    ManageShopperImage = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/next_25px.png"));
                }

                NotifyOfPropertyChange(() => ShopperIsActive);
                NotifyOfPropertyChange(() => ManageShopperImage);
            }
        }

        private readonly IWindowManager _windowManager;

        [ImportingConstructor]
        public AppViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;

            LogViewModel = new LogViewModel();
            SettingsViewModel = new SettingViewModel(_windowManager);

            // Setup NPLib.
            var _cm = ClientManager.Instance;
            _cm.RegisterMessageAction(new Action<string>((message) =>
            {
                LogViewModel.Add(message);
            }));
        }

        public AppViewModel()
        {
        }

        public void OpenSettings(int TabIndex)
        {
            Execute.OnUIThread(new System.Action(() =>
            {
                SettingsViewModel.ActiveTabIndex = TabIndex;
                WinManager.ShowWindow(SettingsViewModel, null, new Dictionary<string, object>() { { "Title", (object)"Settings" }, { "ResizeMode", (object)System.Windows.ResizeMode.NoResize } });
            }));
        }

        public void ManageShopper()
        {

            if (!ShopperIsActive)
                LogViewModel.Add("Beginning to shop.");
            else
                LogViewModel.Add("All done shopping.");

            ToggleShopperState();

            Task t = Task.Run(() =>
            {
                MainShopManager _msm = new MainShopManager();
                UserManager _um = new UserManager();
                if(ShopperIsActive)
                {
                    if(!_um.CurrentUser.is_authenticated)
                    {
                        if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.Login_Username) && !string.IsNullOrWhiteSpace(Properties.Settings.Default.Login_Password))
                        {
                            LogViewModel.Add("Attempting to log into Neopets.");
                            _um.Login(Properties.Settings.Default.Login_Username, Properties.Settings.Default.Login_Password, new Action<bool>((login) =>
                            {
                                if(!login)
                                {
                                    ShopperIsActive = false;
                                    LogViewModel.Add("Could not log in to Neopets.");
                                }
                                else
                                {
                                    CurrentUser = _um.CurrentUser;
                                    LogViewModel.Add("Sucessfully completed log in into Neopets.");
                                    Task.Delay(5000).Wait();

                                    if (SettingsViewModel.ItemList.Count <= 0)
                                    {
                                        OpenSettings(2);

                                        LogViewModel.Add("Waiting for shopping list...");
                                        while (SettingsViewModel.ItemList.Count <= 0)
                                        {
                                            Task.Delay(100);
                                        }
                                    }

                                    LogViewModel.Add("Lets see what the shop has...");
                                    while (ShopperIsActive)
                                    {
                                        if (!_um.CurrentUser.is_authenticated)
                                        {
                                            // Not logged in.
                                        }
                                        else
                                        {
                                            if (ShopStockList == null)
                                                _msm.GetItemsInShop(Properties.Settings.Default.MS_ShopID, new Action<List<MainShopItem>>((item_list) =>
                                                {
                                                    ShopStockList = new ObservableCollection<MainShopItem>(item_list);
                                                    NotifyOfPropertyChange(() => ShopStockList);
                                                }));
                                            else
                                                LogViewModel.Add("Not null..");
                                            
                                        }

                                    }

                                    LogViewModel.Add("All done shopping..");
                                }
                            }));
                        }
                        else
                        {
                            OpenSettings(0);

                            LogViewModel.Add("Waiting for authentication to be configured.");
                            while(!string.IsNullOrWhiteSpace(Properties.Settings.Default.Login_Username) && !string.IsNullOrWhiteSpace(Properties.Settings.Default.Login_Password))
                            {
                                Task.Delay(100).Wait();
                            }
                        }
                    } 
                }
            });           

            

        }

        public void ToggleShopperState()
        {
            ShopperIsActive = !ShopperIsActive;
        }
    }
}
