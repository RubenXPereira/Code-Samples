using Imas.LocExtension;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Imas.Office.Modules.Planning.Portal
{
    /// <summary>
    /// Interaction logic for PlanningView.xaml
    /// </summary>
    public partial class PlanningView : UserControl
    {
        #region Members
        private GridViewColumnHeader lastHeaderClicked = null;
        private ListSortDirection lastDirection = ListSortDirection.Ascending;
        #endregion

        #region Ctor
        public PlanningView()
        {
            InitializeComponent();

            TranslationManager.Instance.LanguageChanged += Instance_LanguageChanged;
        }
        #endregion

        #region Events
        private void Button_Click_ESR_Filter(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            UnselectTextButtons();
            clickedButton.Style = Resources["TextButtonSelectedStyle"] as Style;
            ((PlanningViewModel)DataContext).FilterDisplayedPlants(clickedButton.Name);
            PlannedItems.UnselectAll();
        }

        private void OnMaterialPool_Loaded(object sender, RoutedEventArgs e)
        {
            MaterialPool.UnselectAll();
            ((PlanningViewModel)DataContext).PlannerColumnCollection = ((GridView)PlannedItems.View).Columns;
        }

        private void OnMaterialPoolHeader_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is GridViewColumnHeader ch)) return;
            if (ch.Column == null) return;
            if (!(sender is ListView lv)) return;

            var dir = ListSortDirection.Ascending;
            if (ch == lastHeaderClicked && lastDirection == ListSortDirection.Ascending)
                dir = ListSortDirection.Descending;

            Sort(ch, lv, dir);

            ch.Column.HeaderTemplate = dir == ListSortDirection.Ascending ? Resources["HeaderTemplateArrowUp"] as DataTemplate : Resources["HeaderTemplateArrowDown"] as DataTemplate;

            // Removing the arrow from a previously sorted header
            if (lastHeaderClicked != null && lastHeaderClicked != ch)
                lastHeaderClicked.Column.HeaderTemplate = null;

            lastHeaderClicked = ch;
            lastDirection = dir;
        }

        private void OnCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialPool.Items.Count == MaterialPool.SelectedItems.Count)
                MaterialPool.UnselectAll();
            else
                MaterialPool.SelectAll();
        }

        private void OnGroupCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ((PlanningViewModel)DataContext).ToggleAllInputMaterialGroupSelection(((CheckBox)sender).Tag.ToString());
        }

        private void OnMaterialPool_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Hack to force 'UnselectedAll' after loading (default: 1st list item selected)
            if (((PlanningViewModel)DataContext).FirstLoading)
            {
                MaterialPool.UnselectAll();
                return;
            }

            if (MaterialPool.Items.Count > 0 && MaterialPool.Items.Count == MaterialPool.SelectedItems.Count)
                HeaderListViewCheckbox.IsChecked = true;
            else
                HeaderListViewCheckbox.IsChecked = false;
        }

        private void OnPlannedItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Hack to force 'UnselectedAll' after loading (default: 1st list item selected)
            if (((PlanningViewModel)DataContext).FirstLoading)
            {
                PlannedItems.UnselectAll();
            }
        }

        // Hack to Update each Column's Width after changing the Language
        private void Instance_LanguageChanged(object sender, EventArgs e)
        {
            var gridView1 = (GridView)MaterialPool.View;
            var gridView2 = (GridView)PlannedItems.View;

            if(MaterialPool.Visibility == Visibility.Visible)
                UpdateGridColumnWidth(gridView1);

            // The PlannedItems are always Visible
            UpdateGridColumnWidth(gridView2);
        }

        private void Button_Click_Excel_Export(object sender, RoutedEventArgs e)
        {
            ((PlanningViewModel)DataContext).ExportPlanner();
        }

        private async void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            await ((PlanningViewModel)DataContext).SaveProductionOrderListState();
        }
        #endregion

        #region Methods
        private void Sort(GridViewColumnHeader ch, ListView lv, ListSortDirection dir)
        {
            if(ch.Column != null)
            {
                var bn = (ch.Column.DisplayMemberBinding as Binding)?.Path.Path;
                bn = bn ?? ch.Column.Header as string;
                var dv = CollectionViewSource.GetDefaultView(lv.ItemsSource);
                dv.SortDescriptions.Clear();
                var sd = new SortDescription(bn, dir);
                dv.SortDescriptions.Add(sd);
                dv.Refresh();
            }            
        }

        private void UnselectTextButtons()
        {
            All.Style = Resources["TextButtonStyle"] as Style;
            ESR2.Style = Resources["TextButtonStyle"] as Style;
            ESR3.Style = Resources["TextButtonStyle"] as Style;
            ESR4.Style = Resources["TextButtonStyle"] as Style;
            ESR5.Style = Resources["TextButtonStyle"] as Style;
            ESR6.Style = Resources["TextButtonStyle"] as Style;
        }

        private void UpdateGridColumnWidth(GridView gv)
        {
            foreach (var column in gv.Columns)
            {
                if (double.IsNaN(column.Width))
                { column.Width = column.ActualWidth; }

                column.Width = double.NaN;
            }
        }
        #endregion
    }
}
