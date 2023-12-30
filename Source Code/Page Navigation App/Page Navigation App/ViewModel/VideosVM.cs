using Page_Navigation_App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Page_Navigation_App.ViewModel
{
    class VideosVM : Utilities.ViewModelBase
    {
        private readonly PageModel _pageModel;

        public string NameOfVideo
        {
            get { return _pageModel.VideosStatus; }
            set { _pageModel.VideosStatus = value; OnPropertyChanged(); }
        }
        public VideosVM()
        {
            _pageModel = new PageModel();
            NameOfVideo = string.Empty;
        }
    }
}
