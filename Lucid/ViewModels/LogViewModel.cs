using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using NPLib.Models;

namespace Lucid.ViewModels
{
    [Export(typeof(LogViewModel))]
    class LogViewModel : PropertyChangedBase
    {
        private ObservableCollection<LogMessage> _log = new ObservableCollection<LogMessage>();
        private ObservableCollection<MainShopTransaction> _trans = new ObservableCollection<MainShopTransaction>();

        public LogMessage LastMessage
        {
            get
            {
                return _log.FirstOrDefault();
            }
        }

        public ObservableCollection<LogMessage> AllMessages
        {
            get
            {
                return _log;
            }
            set
            {
                _log = value;
                NotifyOfPropertyChange(() => AllMessages);
                NotifyOfPropertyChange(() => LastMessage);
            }
        }

        public MainShopTransaction LastTransaction
        {
            get
            {
                return _trans.FirstOrDefault();
            }
        }

        public ObservableCollection<MainShopTransaction> AllTransactions
        {
            get
            {
                return _trans;
            }
            set
            {
                _trans = value;
                NotifyOfPropertyChange(() => AllTransactions);
                NotifyOfPropertyChange(() => LastTransaction);
            }
        }

        public void Add(LogMessage Message)
        {
            // Prune.
            if (AllMessages.Count > 200)
            {
                AllMessages = new ObservableCollection<LogMessage>(AllMessages.Take(199).ToList());
            }

            Execute.OnUIThread(new System.Action(() => AllMessages.Add(Message)));            

            AllMessages = new ObservableCollection<LogMessage>(AllMessages.OrderByDescending(m => m.Date));

            NotifyOfPropertyChange(() => AllMessages);
            NotifyOfPropertyChange(() => LastMessage);
        }

        public void Add(MainShopTransaction Transaction)
        {
            Execute.OnUIThread(new System.Action(() => AllTransactions.Add(Transaction)));

            AllTransactions = new ObservableCollection<MainShopTransaction>(AllTransactions.OrderByDescending(m => m.Date));

            NotifyOfPropertyChange(() => AllTransactions);
            NotifyOfPropertyChange(() => LastTransaction);
        }

    }
}
