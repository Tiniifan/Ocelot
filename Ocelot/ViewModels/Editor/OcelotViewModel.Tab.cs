using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using StudioElevenGUI.ViewModels;

namespace Ocelot.ViewModels
{
    public partial class OcelotViewModel : BaseViewModel
    {
        private string _activeTabName;
        /// <summary>
        /// Nom du tab actif : "Minimap" ou "View3D"
        /// </summary>
        public string ActiveTabName
        {
            get => _activeTabName;
            set => SetProperty(ref _activeTabName, value);
        }

        public ICommand SetTabCommand { get; private set; }

        /// <summary>
        /// Initialise la commande pour changer le tab actif.
        /// </summary>
        public void InitializeTabCommand()
        {
            SetTabCommand = new RelayCommand(SetTab);
        }

        /// <summary>
        /// Fonction exécutée par la commande SetTabCommand.
        /// </summary>
        /// <param name="tabName">Nom du tab à activer</param>
        private void SetTab(object tabName)
        {
            ActiveTabName = tabName.ToString();
        }
    }
}
