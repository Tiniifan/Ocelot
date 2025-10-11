using StudioElevenGUI.ViewModels;

namespace Ocelot.ViewModels
{
    public partial class OcelotViewModel : BaseViewModel
    {
        private string _boundingBoxText = "MMModelPos: Not loaded";
        /// <summary>
        /// Gets or sets the text displaying the current model's bounding box coordinates.
        /// </summary>
        public string BoundingBoxText
        {
            get => _boundingBoxText;
            set => SetProperty(ref _boundingBoxText, value);
        }

        private void UpdateBoundingBoxDisplay()
        {
            if (MapEnvironment?.MMModelPos != null)
            {
                var mmPos = MapEnvironment.MMModelPos;
                BoundingBoxText = $"MMModelPos: MinX={mmPos.MinX}, MinY={mmPos.MinY}, MaxX={mmPos.MaxX}, MaxY={mmPos.MaxY}";
            }
            else
            {
                BoundingBoxText = "MMModelPos: Not loaded";
            }
        }
    }
}
