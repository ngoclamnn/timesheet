using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using TimeSheet.Business;
using TimeSheet.Business.Services;
using TimeSheet.Models;

namespace TimeSheet.Wpf.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private NotificationManager _notificationManager;
        private DispatcherTimer timer = null;
        private string _userId;
        public string UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                _userId = value;
                RaisePropertyChanged("UserId");
            }
        }
        private string _userFullName;
        public string UserFullName
        {
            get
            {
                return _userFullName;
            }
            set
            {
                _userFullName = value;
                RaisePropertyChanged("UserFullName");
            }
        }
        private string _displayTotalHour;
        public string DisplayTotalHour
        {
            get
            {
                return _displayTotalHour;
            }
            set
            {
                _displayTotalHour = value;
                RaisePropertyChanged("DisplayTotalHour");
            }
        }
        IDataService _serviceProxy;
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService serviceProxy)
        {
            if (IsInDesignMode)
            {
                Title = "Conexus timesheet monitor (Design)";
            }
            else
            {
                Title = "Conexus timesheet monitor";
            }
            _serviceProxy = serviceProxy;
            UserId = "00013";
            TimeSheetInfos = new ObservableCollection<TimeSheetInfoRow>();
            ReadAllCommand = new RelayCommand(() => GetData(UserId));
            _notificationManager = new NotificationManager();
            ReadAllCommand.Execute(null);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();

        }
        private bool PostPone = false;
        private void TimerTick(object send, EventArgs e)
        {
            ReadAllCommand.Execute(null);
            if (!IsInDesignMode)
            {
                var currentDateTimeSheet = _timeSheetInfos.FirstOrDefault(x => x.Info.IsCurrentDate);
                if (!PostPone && currentDateTimeSheet.Info.TotalHour >= 7)
                {
                    _notificationManager.Show(new NotificationContent
                    {
                        Title = "Conexus timesheet",
                        Message = "Can go home. Remember to check your finger.",
                        Type = NotificationType.Information,
                    }, onClose: () => PostPone = true);
                }
            }
        }

        public string Title { get; set; }

        ObservableCollection<TimeSheetInfoRow> _timeSheetInfos;

        public ObservableCollection<TimeSheetInfoRow> TimeSheetInfos
        {
            get { return _timeSheetInfos; }
            set
            {
                _timeSheetInfos = value;
                RaisePropertyChanged("TimeSheetInfos");
            }
        }
        public void GetData(string empId)
        {
            TimeSheetInfos.Clear();
            foreach (var item in _serviceProxy.GetData(empId))
            {
                TimeSheetInfos.Add(new TimeSheetInfoRow { Info = item });
            }
            UserFullName = "Name: " + TimeSheetInfos[0].Info.osdFullNameVN;
            var ts = TimeSpan.FromHours(TimeSheetInfos.Sum(x => x.Info.TotalHour));
            DisplayTotalHour = "Total until now: " + ((int)ts.TotalHours).ToString("D2") + ":" + ts.Minutes.ToString("D2");

        }
        public RelayCommand ReadAllCommand { get; set; }



    }

    public class TimeSheetInfoRow : ViewModelBase
    {
        private Color _backgroundColor; 
        public TimeSheetInfo Info { get; set; }
        public string Tooltip { get; set; }
        public Color MissingBackgroundColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString("Red");
            }
            set
            {
                _backgroundColor = value;
                RaisePropertyChanged("BackgroundColor");
            }
        }
    }
}