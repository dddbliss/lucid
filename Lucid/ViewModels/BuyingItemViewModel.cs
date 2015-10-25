using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.ComponentModel.Composition;

namespace Lucid.ViewModels
{
    [Export(typeof(BuyingItemViewModel))]
    class BuyingItemViewModel: PropertyChangedBase
    {
        private IWindowManager _windowManager;

        private string _name;
        private Uri _image;
        private int _cost;

        private bool _isBuying = false;
        public bool IsBuying
        {
            get
            {
                return _isBuying;
            }
            set
            {
                _isBuying = value;
                NotifyOfPropertyChange(() => IsBuying);
            }
        }

        [ImportingConstructor]
        public BuyingItemViewModel(IWindowManager _windowManager)
        {
            this._windowManager = _windowManager;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public Uri Image
        {
            get { return _image; }
            set
            {
                _image = value;
                NotifyOfPropertyChange(() => Image);
            }
        }

        public int Cost
        {
            get { return _cost; }
            set
            {
                _cost = value;
                NotifyOfPropertyChange(() => Cost);
            }
        }

        public string DisplayCost
        {
            get
            {
                return Cost.ToString("N0") + " NP";
            }
        }
    }
}
