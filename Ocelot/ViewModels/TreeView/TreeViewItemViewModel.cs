using System.Collections.ObjectModel;
using StudioElevenGUI.ViewModels;

namespace Ocelot.ViewModels.TreeView
{
    public class TreeViewItemViewModel : BaseViewModel
    {
        private string _header;
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        private object _tag;
        public object Tag
        {
            get => _tag;
            set => SetProperty(ref _tag, value);
        }

        private ObservableCollection<TreeViewItemViewModel> _children;
        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get => _children;
            set => SetProperty(ref _children, value);
        }

        public TreeViewItemViewModel()
        {
            Children = new ObservableCollection<TreeViewItemViewModel>();
        }
    }
}
