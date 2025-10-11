using Ocelot.Models;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ocelot.Views.Panels
{
    public partial class AppearPanel : UserControl
    {
        private NPCAppear _appear;

        public AppearPanel()
        {
            InitializeComponent();
        }

        public void LoadData(NPCAppear appear)
        {
            _appear = appear;

            if (_appear == null) return;

            LocationXTextBox.Text = _appear.LocationX.ToString();
            LocationYTextBox.Text = _appear.LocationY.ToString();
            LocationZTextBox.Text = _appear.LocationZ.ToString();
            RotationTextBox.Text = _appear.Rotation.ToString();
            StandAnimationTextBox.Text = _appear.StandAnimation;
            TalkAnimationTextBox.Text = _appear.TalkAnimation;
            UnkAnimationTextBox.Text = _appear.UnkAnimation;
            LookAtPlayerCheckBox.IsChecked = _appear.LookAtThePlayer == 2;
            PhaseAppearTextBox.Text = _appear.PhaseAppear;

            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            LocationXTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null && float.TryParse(LocationXTextBox.Text, out float result))
                    _appear.LocationX = result;
            };

            LocationYTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null && float.TryParse(LocationYTextBox.Text, out float result))
                    _appear.LocationY = result;
            };

            LocationZTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null && float.TryParse(LocationZTextBox.Text, out float result))
                    _appear.LocationZ = result;
            };

            RotationTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null && float.TryParse(RotationTextBox.Text, out float result))
                    _appear.Rotation = result;
            };

            StandAnimationTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null)
                    _appear.StandAnimation = StandAnimationTextBox.Text;
            };

            TalkAnimationTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null)
                    _appear.TalkAnimation = TalkAnimationTextBox.Text;
            };

            UnkAnimationTextBox.TextChanged += (s, e) =>
            {
                if (_appear != null)
                    _appear.UnkAnimation = UnkAnimationTextBox.Text;
            };

            LookAtPlayerCheckBox.Checked += (s, e) =>
            {
                if (_appear != null)
                    _appear.LookAtThePlayer = 2;
            };

            LookAtPlayerCheckBox.Unchecked += (s, e) =>
            {
                if (_appear != null)
                    _appear.LookAtThePlayer = 0;
            };
        }


        private void FloatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;

            var fullText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            e.Handled = !float.TryParse(fullText,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.CurrentCulture,
                out _);
        }
    }
}