using GongSolutions.Wpf.DragDrop;
using Imas.Business.Commands;
using Imas.Business.Queries;
using Imas.Common.Extensions;
using Imas.Domain.Entities;
using Imas.Mvvm;
using Imas.Mvvm.Activities;
using Imas.Office.Modules.Planning.Dialogs;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Excel = Microsoft.Office.Interop.Excel;

namespace Imas.Office.Modules.Planning.Portal
{
    public class PlanningViewModel : ListActivityBase<ProductionOrder>, IDropTarget
    {
        #region Members
        public const string Tag = "PlanningProductionOrders";
        public const string ESR2 = "ESR2";
        public const string ESR6 = "ESR6";
        #endregion

        #region Properties
        private ObservableCollection<ProductionOrder> AllPlannedItems { get; set; }
        public ObservableCollection<ProductionOrder> ReceivedItems { get; set; }
        public ObservableCollection<ProductionOrder> PlannedItems { get; set; }
        public GridViewColumnCollection PlannerColumnCollection { get; set; }

        private readonly ObservableCollection<string> dataSheetTooltipMessages = new ObservableCollection<string>(new string[2]);
        public ObservableCollection<string> DataSheetTooltipMessages
        {
            get
            {
                dataSheetTooltipMessages[0] = Loc("TooltipElectrodeNotPrepared");
                dataSheetTooltipMessages[1] = Loc("TooltipElectrodePrepared");
                return dataSheetTooltipMessages;
            }
        }

        private readonly ObservableCollection<string> operationCardTooltipMessages = new ObservableCollection<string>(new string[2]);
        public ObservableCollection<string> OperationCardTooltipMessages
        {
            get
            {
                operationCardTooltipMessages[0] = Loc("TooltipNoAdditionalData");
                operationCardTooltipMessages[1] = Loc("TooltipAdditionalData");
                return operationCardTooltipMessages;
            }
        }

        public bool FirstLoading { get; set; } = false;

        public bool ShowMaterialPool { get; set; } = true;

        public bool IsDirty { get; set; } = false;
        #endregion

        #region Ctor
        public PlanningViewModel(IModelFacade facade) : base(facade)
        {
            if (facade.Business().GetType().FullName.Contains("Shop"))
                ShowMaterialPool = false;

            PlannedItems = new ObservableCollection<ProductionOrder>();
            pageSize = int.MaxValue;
            IsDirty = false;
        }
        #endregion

        #region Overrides
        public override async void OnLeave()
        {
            if (IsDirty)
            {
                var settings = new MetroDialogSettings
                {
                    AffirmativeButtonText = Loc("Yes"),
                    NegativeButtonText = Loc("No")
                };

                var result = await facade.Dialog().ShowMessageAsync(this, Loc("UnsavedData"), Loc("Message_StandardSave"), MessageDialogStyle.AffirmativeAndNegative, settings);
                if (result == MessageDialogResult.Affirmative)
                {
                    await SaveProductionOrderListState();
                }
                base.OnLeave();
            }
        }

        protected override void OnLoaded()
        {
            FirstLoading = true;

            ReceivedItems = new ObservableCollection<ProductionOrder>(Entries.Where(m => m.ProductionOrderStatus.Name == ProductionOrderStatus.Received));
            PlannedItems = new ObservableCollection<ProductionOrder>(Entries.Where(m => m.ProductionOrderStatus.Name == ProductionOrderStatus.Unplanned ||
                                                                                        m.ProductionOrderStatus.Name == ProductionOrderStatus.Planned ||
                                                                                        m.ProductionOrderStatus.Name == ProductionOrderStatus.Blocked ||
                                                                                        m.ProductionOrderStatus.Name == ProductionOrderStatus.Production));
            PlannedItems.SortCollectionBy(m => m.Position);

            foreach (var item in PlannedItems)
            {
                if (!facade.Business().GetType().FullName.Contains("Shop") && facade.Business().Query<IsProductionOrderInOperationCard>().Execute(item))
                    item.HasAdditionalData = true;
            }

            ResetMaterialPoolSelectionState();
            ResetPlannerSelectionState();

            SetCollectionGrouping(ReceivedItems, "MotherHeat.Name");
            SetCollectionGrouping(PlannedItems, "Plant.Name");

            RaisePropertyChanged("ReceivedItems");
            RaisePropertyChanged("PlannedItems");

            FirstLoading = false;

            base.OnLoaded();
        }

        public override string Activity => Tag;

        public override string ItemActivity => null;

        protected override IQueryable<ProductionOrder> DeclareQuery(IQueryable<ProductionOrder> query) => query
                        .Include(m => m.Plant)
                        .Include(m => m.MotherHeat)
                        .Include(m => m.ElectrodeFormat)
                        .Include(m => m.Instructions)
                        .Include(m => m.Grade)
                        .Include(m => m.ProductionOrderStatus)
                        .Include(m => m.ScheduleItems)
                        .Where(m => m.ProductionOrderStatusID == 2 ||   // Received
                                    m.ProductionOrderStatusID == 3 ||   // Planned
                                    m.ProductionOrderStatusID == 4 ||   // Production
                                    m.ProductionOrderStatusID == 6 ||   // Unplanned
                                    m.ProductionOrderStatusID == 10)    // Blocked
                        .OrderBy(m => m.HeatNumber);

        protected override void AppJump(string plantName)
        {

        }
        #endregion

        #region IDropTarget Implementation
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Move;
            dropInfo.NotHandled = true;
        }

        async void IDropTarget.Drop(IDropInfo dropInfo)
        {
            List<ProductionOrder> draggedItems = new List<ProductionOrder>();
            if (dropInfo.Data is List<object> dropInfoData)
            {
                foreach (var item in dropInfoData)
                {
                    draggedItems.Add((ProductionOrder)item);
                }
            }
            else if (dropInfo.Data is ProductionOrder)
                draggedItems.Add((ProductionOrder)dropInfo.Data);
            else
                return;

            if (dropInfo.DragInfo.SourceCollection != dropInfo.TargetCollection)
            {
                if (((ListView)dropInfo.VisualTarget).Name == "PlannedItems")
                {
                    // Plant Assigment Dialogs
                    var diffMotherheatsSelected = ((IEnumerable<ProductionOrder>)draggedItems).DistinctBy(x => x.MotherHeat.Name);

                    foreach (ProductionOrder order in diffMotherheatsSelected)
                    {
                        // Determine if all selected orders for this motherheat have one (and the same) assigned ESR
                        var assignedPlantRows = draggedItems.Where(x => x.MotherHeat.Name == order.MotherHeat.Name).DistinctBy(y => y.PlantID);
                        string assignedPlant = assignedPlantRows.Count() == 1 ? assignedPlantRows.ElementAt(0).Plant.Name : null;

                        // Prompt User
                        await AssignToPlant(dropInfo, draggedItems, order.MotherHeat.Name, assignedPlant);
                    }
                }
                else // "ReceivedItems" is the only alternative
                {
                    int receivedStatusID = facade.Business().Query<AllProductionOrderStatus>().Execute().Where(x => x.Name == "Received").FirstOrDefault().ID;

                    foreach (ProductionOrder draggedOrder in draggedItems)
                    {
                        draggedOrder.ProductionOrderStatusID = receivedStatusID;
                        draggedOrder.ProductionOrderStatus.Name = ProductionOrderStatus.Received;
                        draggedOrder.IsOrderSelected = false;
                        ((IList)dropInfo.DragInfo.SourceCollection).Remove(draggedOrder);
                        ((IList)dropInfo.TargetCollection).Add(draggedOrder);
                    }

                    ((ObservableCollection<ProductionOrder>)dropInfo.TargetCollection).SortCollectionBy(m => m.HeatNumber);

                    RaisePropertyChanged("PlannedItems");
                    RaisePropertyChanged("ReceivedItems");
                }

                IsDirty = true;
            }
            else
            {
                // Only Items within the "Planner" are Reorderable
                if (((ListView)dropInfo.VisualTarget).Name == "PlannedItems")
                {
                    // Check the ESR of the dragged items
                    string selectedESR = null;
                    foreach (var order in draggedItems)
                    {
                        // If at least one selected order is in "Production", abort the dragging
                        if (order.ProductionOrderStatus.Name == ProductionOrderStatus.Production)
                        {
                            await facade.Dialog().ShowMessageAsync(this, Loc("DialogMessageChargesFromDiffPlantsTitle"), Loc("DialogMessageChargesInProductionMessage"));
                            return;
                        }

                        if (selectedESR == null)
                            selectedESR = order.Plant.Name;
                        else if (selectedESR != order.Plant.Name)
                        {
                            await facade.Dialog().ShowMessageAsync(this, Loc("DialogMessageChargesFromDiffPlantsTitle"), Loc("DialogMessageChargesFromDiffPlantsMessage"));
                            return;
                        }
                    }

                    if (draggedItems.Count == 1)
                    {
                        // When dragging only 1 item, approach based on the ObservableCollection<T>.Move() method
                        await ProcessSelectedOrderSwap(dropInfo, draggedItems[0], selectedESR);
                    }
                    else
                    {
                        // For more than 1 dragged item, approach based on the IDropTarget.Drop() default implementation (from the used drag-and-drop library)
                        await ProcessSelectedOrdersShift(dropInfo, draggedItems, selectedESR);
                    }

                    IsDirty = true;
                }
            }
        }
        #endregion

        #region UI Requests
        public void ToggleAllInputMaterialGroupSelection(string motherHeatNumber)
        {
            var groupDataSet = ReceivedItems.Where(x => x.MotherHeat.Name == motherHeatNumber);
            bool intendedSelectionState = groupDataSet.Any(x => x.IsOrderSelected == false);

            foreach (ProductionOrder item in groupDataSet)
            {
                item.IsOrderSelected = intendedSelectionState;
            }
        }

        public void FilterDisplayedPlants(string plantReference)
        {
            if (plantReference == "All")
            {
                if (AllPlannedItems == null) return;
                PlannedItems = AllPlannedItems;
                AllPlannedItems = null;
            }
            else
            {
                if (AllPlannedItems == null) AllPlannedItems = PlannedItems;
                PlannedItems = new ObservableCollection<ProductionOrder>(AllPlannedItems.Where(m => m.Plant.Name.Contains(plantReference.Substring(3))));
                SetCollectionGrouping(PlannedItems, "Plant.Name");
            }
            RaisePropertyChanged("PlannedItems");
        }

        public async void ExportPlanner()
        {
            try
            {
                var dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true
                };
                CommonFileDialogResult result = dialog.ShowDialog();

                if (result == CommonFileDialogResult.Cancel || result == CommonFileDialogResult.None)
                    return;

                string selectedDirectory = dialog.FileName + @"\";

                DirectoryInfo di = new DirectoryInfo(selectedDirectory);

                if (di.Exists == false)
                    di.Create();

                // Delete previously created file
                var file = di.GetFiles().Where(x => x.Name.Contains("IMAS_Office_Planner_Export")).FirstOrDefault();
                if (file != null)
                    file.Delete();

                StreamWriter sw = new StreamWriter(selectedDirectory + "IMAS_Office_Planner_Export" + ".xls", false)
                {
                    AutoFlush = true
                };

                // HEADER
                sw.Write("\t" + Loc("Plant"));
                sw.Write("\t" + Loc("Count"));
                sw.Write("\t" + Loc("MotherHeatNumber"));
                sw.Write("\t" + Loc("SteelGrade"));
                sw.Write("\t" + Loc("Format"));
                sw.Write("\t" + Loc("DeliveryDate"));
                sw.Write("\t" + Loc("GlowGroup"));
                sw.Write("\t" + Loc("ChargeNumber"));
                sw.WriteLine("\t" + Loc("Comments"));

                // CONTENT
                for (int row = 0; row < PlannedItems.Count; row++)
                {
                    string st1 = "";
                    var rowItem = PlannedItems[row];

                    int chargesNumber = PlannedItems.Where(x => x.MotherHeatID == rowItem.MotherHeatID).Count();

                    st1 += "\t" + rowItem.Plant.Name;
                    st1 += "\t" + rowItem.Quantity;
                    st1 += "\t" + rowItem.MotherHeat.Name;
                    st1 += "\t" + rowItem.Grade.Name;
                    st1 += "\t" + rowItem.ElectrodeFormat.Name;
                    st1 += "\t" + rowItem.DeliveryDate;
                    st1 += "\t" + rowItem.GlowGroup;
                    st1 += "\t" + chargesNumber;
                    st1 += "\t" + rowItem.Comments;

                    sw.WriteLine(st1);
                }

                sw.Close();

                await facade.Dialog().ShowMessageAsync(this, Loc("SuccessTitle"), Loc("DialogExportToExcelSuccess"));
            }
            catch (Exception e)
            {
                await facade.Dialog().ShowMessageAsync(this, Loc("ErrorTitle"), e.Message);
            }

            /*****************************************************************************************
             *  
             *  PURE MICROSOFT EXCEL APPROACH
             *
             *  Requirement: The running machine must have MS Office (Excel) installed
             *
             *  Comment: The code was not tested due to the aforementioned requirement not being met
             *  
             *****************************************************************************************/
            /*try
            {
                Excel.Application app = new Excel.Application
                {
                    Visible = true
                };

                Excel.Workbook wb = app.Workbooks.Add(1);
                Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets[1];

                // HEADER
                ws.Cells[1, 1] = Loc("Plant");
                ws.Cells[1, 2] = Loc("Count");
                ws.Cells[1, 3] = Loc("MotherHeatNumber");
                ws.Cells[1, 4] = Loc("SteelGrade");
                ws.Cells[1, 5] = Loc("Format");
                ws.Cells[1, 6] = Loc("DeliveryDate");
                ws.Cells[1, 7] = Loc("GlowGroup");
                ws.Cells[1, 8] = Loc("ChargeNumber");
                ws.Cells[1, 9] = Loc("Comments");

                for (int row = 0; row < PlannedItems.Count; row++)
                {
                    var rowItem = PlannedItems[row];

                    // CONTENT
                    ws.Cells[row + 2, 1] = rowItem.Plant.Name;
                    ws.Cells[row + 2, 2] = rowItem.Quantity;
                    ws.Cells[row + 2, 3] = rowItem.MotherHeatNumber;
                    ws.Cells[row + 2, 4] = rowItem.Grade.Name;
                    ws.Cells[row + 2, 5] = rowItem.RawElectrodeFormat;
                    ws.Cells[row + 2, 6] = rowItem.DeliveryDate;
                    ws.Cells[row + 2, 7] = rowItem.GlowGroup;
                    ws.Cells[row + 2, 8] = rowItem.ChargesNumber;
                    ws.Cells[row + 2, 9] = rowItem.Comments;
                }

                // AutoSet Cell Widths to Content Size
                ws.Cells.Select();
                ws.Cells.EntireColumn.AutoFit();

                wb.Save();

                await facade.Dialog().ShowMessageAsync(this, Loc("SuccessTitle"), Loc("DialogExportToExcelSuccess"));
            }
            catch (Exception e)
            {
                await facade.Dialog().ShowMessageAsync(this, Loc("ErrorTitle"), e.Message);
            }*/
        }

        public async Task SaveProductionOrderListState()
        {
            for (int i = 0; i < PlannedItems.Count; i++)
            {
                PlannedItems[i].Position = i;
            }

            await facade.Business().Command<UpdateProductionOrderState>().ExecuteAsync(PlannedItems);
            await facade.Business().Command<UpdateProductionOrderState>().ExecuteAsync(ReceivedItems);

            IsDirty = false;
        }
        #endregion

        #region Private Methods
        private void ResetMaterialPoolSelectionState()
        {
            foreach (var item in ReceivedItems)
            {
                item.IsOrderSelected = false;
            }
        }

        private void ResetPlannerSelectionState()
        {
            foreach (var item in PlannedItems)
            {
                item.IsOrderSelected = false;
            }
        }

        private async Task AssignToPlant(IDropInfo dropInfo, List<ProductionOrder> selectedOrders, string motherheatNumber, string assignedPlant = null)
        {
            try
            {
                var customDialog = new CustomDialog { Title = Loc("PlantAssignment") };

                var dataContext = new AssignProductionOrderToPlantDialogViewModel(
                    motherheatNumber,
                    assignedPlant,
                    facade.Business().Query<AllPlants>().Execute(),
                    async save =>
                    {
                        try
                        {
                            int unplannedID = facade.Business().Query<AllProductionOrderStatus>().Execute().Where(x => x.Name == "Unplanned").FirstOrDefault().ID;

                            foreach (ProductionOrder draggedOrder in selectedOrders.Where(x => x.MotherHeat.Name == motherheatNumber))
                            {
                                draggedOrder.PlantID = save.Plant.ID;
                                draggedOrder.Plant = save.Plant;
                                draggedOrder.ProductionOrderStatusID = unplannedID;
                                draggedOrder.ProductionOrderStatus.Name = ProductionOrderStatus.Unplanned;
                                draggedOrder.IsOrderSelected = false;
                                ((IList)dropInfo.DragInfo.SourceCollection).Remove(draggedOrder);
                                ((IList)dropInfo.TargetCollection).Add(draggedOrder);
                            }

                            ((ObservableCollection<ProductionOrder>)dropInfo.TargetCollection).SortCollectionBy(m => m.Plant.Name);

                            RaisePropertyChanged("PlannedItems");
                            RaisePropertyChanged("ReceivedItems");

                            await facade.Dialog().HideMetroDialogAsync(this, customDialog);
                        }
                        catch (Exception ex)
                        {
                            facade.Oops(ex);
                        }
                    },
                    async close => await facade.Dialog().HideMetroDialogAsync(this, customDialog));

                customDialog.Content = new AssignProductionOrderToPlantDialogView { DataContext = dataContext };
                await facade.Dialog().ShowMetroDialogAsync(this, customDialog);
            }
            catch (Exception e)
            {
                facade.Oops(e);
            }
        }

        private async Task ProcessSelectedOrderSwap(IDropInfo dropInfo, ProductionOrder selectedItem, string selectedESR)
        {
            int oldIndex = PlannedItems.IndexOf(selectedItem);

            int newIndex = dropInfo.InsertIndex;
            if (dropInfo.InsertPosition == RelativeInsertPosition.AfterTargetItem || dropInfo.InsertPosition.Equals(RelativeInsertPosition.AfterTargetItem | RelativeInsertPosition.TargetItemCenter))
                newIndex = dropInfo.InsertIndex - 1; // Hack to guarantee we don't insert after the Target Item position

            // Safety measure considering the "index" is provided by the drag-and-drop used library
            if (newIndex >= PlannedItems.Count)
                newIndex = PlannedItems.Count - 1;

            ProductionOrder targetLocationOrder = PlannedItems[newIndex];

            // Block Swaps that are not allowed
            bool allowed = await CheckIfSwapIsAllowed(selectedESR, targetLocationOrder);
            if (!allowed) return;

            // Update the dragged ProductionOrder to the new ESR
            selectedItem.PlantID = targetLocationOrder.PlantID;
            selectedItem.Plant = targetLocationOrder.Plant;

            // Updating the "Planner"
            UnsetCollectionGroupings(PlannedItems);
            PlannedItems.Move(oldIndex, newIndex);
            SetCollectionGrouping(PlannedItems, "Plant.Name");
            ResetPlannerSelectionState();
            PlannedItems.SortCollectionBy(m => m.Plant.Name);
            RaisePropertyChanged("PlannedItems");
        }

        private async Task ProcessSelectedOrdersShift(IDropInfo dropInfo, List<ProductionOrder> draggedItems, string selectedESR)
        {
            int insertIndex = (dropInfo.InsertIndex != dropInfo.UnfilteredInsertIndex) ? dropInfo.UnfilteredInsertIndex : dropInfo.InsertIndex;
            if (dropInfo.VisualTarget is ItemsControl itemsControl)
            {
                IEditableCollectionView editableItems = itemsControl.Items;
                if (editableItems != null)
                {
                    NewItemPlaceholderPosition newItemPlaceholderPosition = editableItems.NewItemPlaceholderPosition;
                    if (newItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning && insertIndex == 0)
                    {
                        insertIndex++;
                    }
                    else if (newItemPlaceholderPosition == NewItemPlaceholderPosition.AtEnd && insertIndex == itemsControl.Items.Count)
                    {
                        insertIndex--;
                    }
                }

                // Hack to guarantee we don't insert after the Target Item position
                if (dropInfo.InsertPosition == RelativeInsertPosition.AfterTargetItem || dropInfo.InsertPosition.Equals(RelativeInsertPosition.AfterTargetItem | RelativeInsertPosition.TargetItemCenter))
                    insertIndex = insertIndex - 1;

                ProductionOrder targetLocationOrder = PlannedItems[insertIndex];

                // Block Swaps that are not allowed
                bool allowed = await CheckIfSwapIsAllowed(selectedESR, targetLocationOrder);
                if (!allowed) return;

                // Removing the "Planner" Groupings
                UnsetCollectionGroupings(PlannedItems);

                // Removal of the Selected Rows
                foreach (ProductionOrder selectedItem in draggedItems)
                {
                    int oldIndex = PlannedItems.IndexOf(selectedItem);
                    PlannedItems.RemoveAt(oldIndex);
                    if (oldIndex < insertIndex)
                    {
                        insertIndex--;
                    }
                }

                // Insertion of the Selected Rows in the new positions
                foreach (ProductionOrder selectedItem in draggedItems)
                {
                    // Update the dragged ProductionOrder to the new ESR before Insertion
                    selectedItem.PlantID = targetLocationOrder.PlantID;
                    selectedItem.Plant = targetLocationOrder.Plant;
                    PlannedItems.Insert(insertIndex++, selectedItem);
                }

                // Updating the "Planner"
                SetCollectionGrouping(PlannedItems, "Plant.Name");
                ResetPlannerSelectionState();
                PlannedItems.SortCollectionBy(m => m.Plant.Name);
                RaisePropertyChanged("PlannedItems");
            }
        }

        private async Task<bool> CheckIfSwapIsAllowed(string selectedESR, ProductionOrder targetLocationOrder)
        {
            if ((selectedESR == ESR2 || selectedESR == ESR6) && targetLocationOrder.Plant.Name != ESR2 && targetLocationOrder.Plant.Name != ESR6)
            {
                await facade.Dialog().ShowMessageAsync(this, Loc("DialogMessageChargesFromDiffPlantsTitle"), Loc("DialogMessageChargesFromDiffPlantsMessageESR26"));
                return false;
            }
            else if (selectedESR != ESR2 && selectedESR != ESR6 && (targetLocationOrder.Plant.Name == ESR2 || targetLocationOrder.Plant.Name == ESR6))
            {
                await facade.Dialog().ShowMessageAsync(this, Loc("DialogMessageChargesFromDiffPlantsTitle"), Loc("DialogMessageChargesFromDiffPlantsMessageESR345"));
                return false;
            }

            return true;
        }

        private void SetCollectionGrouping(ICollection listSource, string groupingProperty)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription(groupingProperty));
        }

        private void UnsetCollectionGroupings(ICollection listSource)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listSource);
            for (int index = view.GroupDescriptions.Count - 1; index >= 0; index--)
            {
                view.GroupDescriptions.RemoveAt(index);
            }
        }
        #endregion
    }
}
