using System.Collections.ObjectModel;
using System.Windows.Controls;
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

        private ContextMenu _contextMenu;
        public ContextMenu ContextMenu
        {
            get => _contextMenu;
            set => SetProperty(ref _contextMenu, value);
        }

        public TreeViewItemViewModel()
        {
            Children = new ObservableCollection<TreeViewItemViewModel>();
        }
    }
}
