using Caliburn.Micro;
using Lucid.ViewModels;
using NPLib;
using NPLib.Events;
using NPLib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lucid
{
    [Export(typeof(IShell))]
    class AppViewModel : PropertyChangedBase, IShell
    {
        private const string WindowTitleDefault = "Lucid MSAB(a)";
        private string _windowTitle = WindowTitleDefault;


        private ImageSource _shopperImageSource = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/play_round_32px.png"));
        private User _currentUser = new User() { is_authenticated = false };
        private bool _shopperIsActive = false, IsBusyCheckingShop = false;

        private ObservableCollection<MainShopItem> _ShopStockList = new ObservableCollection<MainShopItem>() { };

        public ObservableCollection<MainShopItem> ShopStockList
        {
            get
            {
                return _ShopStockList;
            }
            set
            {
                _ShopStockList = value;
                NotifyOfPropertyChange(() => ShopStockList);
            }
        }

        public LogViewModel LogViewModel { get; set; }
        public SettingViewModel SettingsViewModel { get; set; }

        public BuyingItemViewModel BuyingItemViewModel { get; set; }
        private bool _isBuying = false;
        public bool IsBuying
        {
            get { return _isBuying; }
            set
            {
                _isBuying = value;
                NotifyOfPropertyChange(() => IsBuying);
            }
        }

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

        public bool ShopperIsActive
        {
            get
            {
                return _shopperIsActive;
            }
            set
            {
                _shopperIsActive = value;
                NotifyOfPropertyChange(() => ShopperIsActive);
            }
        }

        private readonly IWindowManager _windowManager;

        [ImportingConstructor]
        public AppViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;

            LogViewModel = new LogViewModel(_windowManager);
            SettingsViewModel = new SettingViewModel(_windowManager);
            BuyingItemViewModel = new BuyingItemViewModel(_windowManager);

            // Setup NPLib.
            var _cm = ClientManager.Instance;

            _cm.Settings = new ClientSettings()
            {
                GeneralWaitMax = Properties.Settings.Default.General_WaitDelayMax,
                GeneralWaitMin = Properties.Settings.Default.General_WaitDelayMin,
                OCRWaitMax = Properties.Settings.Default.General_OCRDelayMax,
                OCRWaitMin = Properties.Settings.Default.General_OCRDelayMin,
                PreHaggleWaitMax = Properties.Settings.Default.General_PreHaggleMax,
                PreHaggleWaitMin = Properties.Settings.Default.General_PreHaggleMin,
                UseProxy = Properties.Settings.Default.Web_ProxyEnabled,
                ProxyUri = new Uri(string.IsNullOrEmpty(Properties.Settings.Default.Web_ProxyUri) ? "http://www.google.com" : Properties.Settings.Default.Web_ProxyUri),
                ProxyUser = Properties.Settings.Default.Web_ProxyUsername,
                ProxyPass = Properties.Settings.Default.Web_ProxyPassword,
                UserAgent = Properties.Settings.Default.Web_UserAgent
            };

            _cm.RegisterMessageAction(new Action<LogMessage>((message) =>
            {
                LogViewModel.Add(message);
            }));

			_cm.RegisterEventHandler(new Action<Event>((ev) =>
			{
				if (ev.GetType().Equals(typeof(CurrencyEvent)))
				{
					var currencySet = (CurrencySet)ev.Object;
					CurrentUser.NP = currencySet.NP;
					CurrentUser.NC = currencySet.NC;
					NotifyOfPropertyChange(() => CurrentUser);
				}
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
                WinManager.ShowDialog(SettingsViewModel, null, new Dictionary<string, object>() { { "Title", (object)"Settings" }, { "ResizeMode", (object)System.Windows.ResizeMode.NoResize } });
            }));
        }

        public void OpenLog()
        {
            Execute.OnUIThread(new System.Action(() =>
            {
                WinManager.ShowDialog(LogViewModel, null, new Dictionary<string, object>() { { "Title", (object)"Log Viewer" }, { "ResizeMode", (object)System.Windows.ResizeMode.NoResize } });
            }));
        }

        public void ManageShopper()
        {

			ToggleShopperState();
 
            Task t = Task.Run(() =>
            {
                try
                {
                    MainShopManager _msm = new MainShopManager();
                    UserManager _um = new UserManager();
                    if (ShopperIsActive)
                    {
                        if (!_um.CurrentUser.is_authenticated)
                        {
                            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.Login_Username) && !string.IsNullOrWhiteSpace(Properties.Settings.Default.Login_Password))
                            {
                                _um.Login(Properties.Settings.Default.Login_Username, Properties.Settings.Default.Login_Password, new Action<bool>((login) =>
                                {
                                    try {
                                        if (!login)
                                        {
                                            ShopperIsActive = false;
                                        }
                                        else
                                        {
                                            CurrentUser = _um.CurrentUser;

                                            if (SettingsViewModel.ItemList.Count <= 0)
                                            {
                                                OpenSettings(3);

                                                LogViewModel.Add(new LogMessage() { Message = "Waiting for shopping list...", Level = LogLevel.Info, Date = DateTime.Now });
                                                while (SettingsViewModel.ItemList.Count <= 0)
                                                {
                                                    Task.Delay(100);
                                                }
                                            }

                                            while (ShopperIsActive)
                                            {
                                                if (!_um.CurrentUser.is_authenticated)
                                                {
                                                    // Not logged in.
                                                }
                                                else
                                                {
                                                    if (!IsBusyCheckingShop && !BuyingItemViewModel.IsBuying)
                                                    {
                                                        IsBusyCheckingShop = true;
                                                        _msm.GetItemsInShop(Properties.Settings.Default.MS_ShopID, new Action<List<MainShopItem>>((item_list) =>
                                                        {
                                                            try {
                                                                Execute.OnUIThread(new System.Action(() =>
                                                                {
                                                                    ShopStockList = new ObservableCollection<MainShopItem>(item_list);
                                                                }));

                                                            // Check for matching items.
                                                            if (!BuyingItemViewModel.IsBuying)
                                                                {
                                                                    var _buyItems = ShopStockList.Where(Item => SettingsViewModel.ItemList.Select(m => m.ToLower()).Contains(Item.Name.ToLower())).ToList();
                                                                    if (_buyItems.ToList().Count > 0)
                                                                    {
                                                                        var item = _buyItems.First();

                                                                        BuyingItemViewModel.Name = item.Name;
                                                                        BuyingItemViewModel.Image = item.Image;
                                                                        BuyingItemViewModel.Cost = item.Cost - Convert.ToInt32(item.Cost / (Properties.Settings.Default.General_HagglePercent));

                                                                        NotifyOfPropertyChange(() => BuyingItemViewModel);

                                                                        BuyingItemViewModel.IsBuying = true;
                                                                        _msm.BuyItem(item, BuyingItemViewModel.Cost, Properties.Settings.Default.General_HagglePercent, Properties.Settings.Default.General_HaggleAttempts, Properties.Settings.Default.General_HaggleAttemptPercent, new Action<MainShopTransaction>((trans) =>
                                                                        {
                                                                            LogViewModel.Add(trans);
                                                                            BuyingItemViewModel.IsBuying = false;
                                                                        }));
                                                                    }
                                                                }
                                                                IsBusyCheckingShop = false;
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                ex.ReportException();
                                                                ShopperIsActive = false;
                                                            }


                                                        }));
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        ex.ReportException();
                                        ShopperIsActive = false;
                                    }
                                }));
                            }
                            else
                            {
                                OpenSettings(0);

                                ShopperIsActive = false;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    ex.ReportException();
                    ShopperIsActive = false;
                }
            });           

            

        }

        public void ToggleShopperState()
        {
            ShopperIsActive = !ShopperIsActive;
        }
    }
}
