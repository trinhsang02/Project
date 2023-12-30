using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Page_Navigation_App.Utilities;
using System.Windows.Input;

namespace Page_Navigation_App.ViewModel
{
    class NavigationVM : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand HomeCommand { get; set; }
        public ICommand SongsCommand { get; set; }
        public ICommand VideosCommand { get; set; }
        public ICommand PlaylistCommand { get; set; }
        public ICommand SettingsCommand { get; set; }

        private void Home(object obj) => CurrentView = new HomeVM();
        private void Songs(object obj) => CurrentView = new SongsVM();
        private void Videos(object obj) => CurrentView = new VideosVM();
        private void Playlists(object obj) => CurrentView = new PlaylistsVM();
        private void Setting(object obj) => CurrentView = new SettingVM();

        public NavigationVM()
        {
            HomeCommand = new RelayCommand(Home);
            SongsCommand = new RelayCommand(Songs);
            VideosCommand = new RelayCommand(Videos);
            PlaylistCommand = new RelayCommand(Playlists);
            SettingsCommand = new RelayCommand(Setting);

            // Startup Page
            CurrentView = new HomeVM();
        }
    }
}
