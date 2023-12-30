using Page_Navigation_App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Page_Navigation_App.ViewModel
{
    class SongsVM : Utilities.ViewModelBase
    {
        private readonly PageModel _pageModel;

        public string NameOfSong
        {
            get { return _pageModel.SongsStatus; }
            set {  _pageModel.SongsStatus = value; OnPropertyChanged(); }
        }
        public SongsVM()
        {
            _pageModel = new PageModel();
            NameOfSong = string.Empty;
        }
    }
}
