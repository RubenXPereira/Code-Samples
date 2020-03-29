using System;
using System.Windows;
using System.Windows.Controls;
using Imas.LocExtension;

namespace Shop.Freital.Modules.Melting.Portal
{
    /// <summary>
    /// Interaction logic for MeltingView.xaml
    /// </summary>
    public partial class MeltingView : UserControl
    {
        private readonly MeltingViewModel _viewModel;

        public MeltingView()
        {
            InitializeComponent();
            _viewModel = (MeltingViewModel)DataContext;

            UpdateButtonState(ESR2);

            TranslationManager.Instance.LanguageChanged += Instance_LanguageChanged;
        }

        // Hack to Update each Column's Width after changing the Language
        private void Instance_LanguageChanged(object sender, EventArgs e)
        {
            var gridView = (GridView)MeltingList.View;

            foreach (var column in gridView.Columns)
            {
                if (double.IsNaN(column.Width))
                { column.Width = column.ActualWidth; }

                column.Width = double.NaN;
            }
        }

        private void Button_Click_ESU_Filter(object sender, RoutedEventArgs e)
        {
            UpdateButtonState((Button)sender);
        }

        private void UpdateButtonState(Button button)
        {
            UnselectTextButtons();

            SetButtonStyle(button, true);
            _viewModel.FilterDisplayedPlant(button.Name);
        }

        private void UnselectTextButtons()
        {
            SetButtonStyle(ESR2);
            SetButtonStyle(ESR3);
            SetButtonStyle(ESR4);
            SetButtonStyle(ESR5);
            SetButtonStyle(ESR6);
        }

        private void SetButtonStyle(Button button, bool isSelectedMode = false)
        {
            button.Style = Resources[
                _viewModel.IsMelting(button.Name) ?
                    (isSelectedMode ? "GreenButtonSelectedStyle" : "GreenButtonStyle") :
                    (isSelectedMode ? "BlueButtonSelectedStyle" : "BlueButtonStyle")
                ] as Style;
        }
    }
}
