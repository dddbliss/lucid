using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.Collections.ObjectModel;

namespace Lucid.ViewModels
{
    [Export(typeof(LogViewModel))]
    class LogViewModel : PropertyChangedBase
    {
        private ObservableCollection<string> _log = new ObservableCollection<string>();

        public string LastMessage
        {
            get
            {
                return _log.LastOrDefault();
            }
        }

        public ObservableCollection<string> AllMessages
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

        public void Add(string Message)
        {
            // Prune.
            if(AllMessages.Count > 200)
            {
                AllMessages = new ObservableCollection<string>(AllMessages.Take(199).ToList());
            }

            AllMessages.Add(Message);
            NotifyOfPropertyChange(() => AllMessages);
            NotifyOfPropertyChange(() => LastMessage);
        }
        
    }
}
