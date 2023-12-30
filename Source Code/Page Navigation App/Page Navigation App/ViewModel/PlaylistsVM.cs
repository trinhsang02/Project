using Page_Navigation_App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Page_Navigation_App.ViewModel
{
    class PlaylistsVM : Utilities.ViewModelBase
    {
        private readonly PageModel _pageModel;

        public string NameOfPlaylists
        {
            get { return _pageModel.PLaylistsStatus; }
            set { _pageModel.PLaylistsStatus = value; OnPropertyChanged(); }
        }    

        public PlaylistsVM()
        {
            _pageModel = new PageModel();
            NameOfPlaylists = string.Empty;
        }    
    }
}
