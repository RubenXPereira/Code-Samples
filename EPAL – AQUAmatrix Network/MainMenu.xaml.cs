using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Maps.MapControl;
using MiraObjectsClassLibrary;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Globalization;
using ETELibrary;

namespace ETEPrototipo
{
	public partial class MainMenu
    {
        private List<Location> Locations { get; set; }
        private double DefaultZoomLevel { get; set; }
        private bool AllowTimersTick { get; set; }
        private bool ShowAllPins { get; set; }
        private bool IsZoomed { get; set; }
        private Pushpin SelectedPushpin { get; set; }
        private long ID_Suspension;
        private bool AllowSuspensionDetail { get; set; }

        public int SelectedYear { get; set; }
        public event EventHandler NavigateHome;

        private bool IsLoaded { get; set; }
        public double MaxGrapshWidth { get; set; }
        public double MaxGrapshHeight { get; set; }

        private MapLayer m_PushpinLayer;
        private MapLayer m_ControlLayer;

        private DispatcherTimer DrawMapTimer { get; set; }
        private DispatcherTimer RefreshMapTimer { get; set; }

        private LocationRect CurrentLocation { get; set; }

        private LinearGradientBrush PinBackgroundColorNormal;
        private LinearGradientBrush PinBackgroundColorApproved;
        private LinearGradientBrush PinBackgroundColorExecution;
        private LinearGradientBrush PinBackgroundColorConcluded;

        private List<EPL_WS.GET_Suspensions_FullDetailed_Result> PinsArray { get; set; }

        private Guid SpecificRamal;

        private bool isSpecificRamalCalled { get; set; }
        public double AuxZoomLevel;

        public MainMenu()
		{
			// Required to initialize variables
			InitializeComponent();
		}

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AllowSuspensionDetail = false;
            DefaultZoomLevel = 13;
            IsZoomed = false;
            IsLoaded = true;
            isSpecificRamalCalled = false;
            SpecificRamal = Guid.Empty;

            PinsArray = new List<EPL_WS.GET_Suspensions_FullDetailed_Result>();

            SB_Breadcrumb_Title_Close.Completed += new EventHandler(SB_Breadcrumb_Title_Close_Completed);

            TB_SubTitle.Text = DateHandler.GetDateFormatted(DateTime.Now).ToUpper();

            ShowAllPins = true;

            MaxGrapshWidth = 1356;
            MaxGrapshHeight = 640;

            // ========== "LOAD" COLORS ========== 
            PinBackgroundColorNormal = new LinearGradientBrush
            {
                EndPoint = new Point(0.5, 1),
                StartPoint = new Point(0.5, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = ColorHandler.HexColor("#3D8939"), Offset = 0 },
                    new GradientStop { Color = ColorHandler.HexColor("#068700"), Offset = 1 }
                }
            };

            PinBackgroundColorApproved = new LinearGradientBrush
            {
                EndPoint = new Point(0.5, 1),
                StartPoint = new Point(0.5, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = ColorHandler.HexColor("#FF8C0A"), Offset = 0 },
                    new GradientStop { Color = ColorHandler.HexColor("#FFFF5D00"), Offset = 1 }
                }
            };

            PinBackgroundColorExecution = new LinearGradientBrush
            {
                EndPoint = new Point(0.5, 1),
                StartPoint = new Point(0.5, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = ColorHandler.HexColor("#FFD40000"), Offset = 0 },
                    new GradientStop { Color = ColorHandler.HexColor("#FF800000"), Offset = 1 }
                }
            };

            PinBackgroundColorConcluded = new LinearGradientBrush
            {
                EndPoint = new Point(0.5, 1),
                StartPoint = new Point(0.5, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop { Color = ColorHandler.HexColor("#3D8939"), Offset = 0 },
                    new GradientStop { Color = ColorHandler.HexColor("#068700"), Offset = 1 }
                }
            };
            // =================================== 

            m_PushpinLayer = new MapLayer();
            m_ControlLayer = new MapLayer();

            // Draw on Map
            MiraMap.Children.Add(m_PushpinLayer);
            MiraMap.Children.Add(m_ControlLayer);

            // Default Zoom Level
            MiraMap.ZoomLevel = 13.0;

            // Default Center - EPAL
            MiraMap.Center = new Location(38.722584, -9.158435);

            // Timer to Reload 'Ramais' after map changed events
            DrawMapTimer = new DispatcherTimer();
            DrawMapTimer.Interval = new TimeSpan(0, 0, 0, 2, 0);
            DrawMapTimer.Tick += DrawMapTimer_CallBack;

            // Timer to Reload 'Ramais' after a certain timespan
            RefreshMapTimer = new DispatcherTimer();
            RefreshMapTimer.Interval = new TimeSpan(0, 0, 0, 30, 0);
            RefreshMapTimer.Tick += RefreshMapTimer_CallBack;

            AllowTimersTick = false;

            if (GetParam("Type") != null)
            {
                int type = 0;
                int.TryParse(GetParam("Type"), out type);
                CB_Year.SelectedIndex = type;
            }

            int frameType = 0;

            if (GetParam("Frame") != null && int.TryParse(GetParam("Frame"), out frameType))
            {
                SP_Content.Margin = new Thickness(2, -3, 2, 2);
                BRD_Top.Visibility = Visibility.Collapsed;
                BRD_Graphs.BorderThickness = new Thickness(0);
                MiraMap.CopyrightVisibility = Visibility.Collapsed;
            }

            int enabled = 0;

            if (GetParam("Enabled") != null && int.TryParse(GetParam("Enabled"), out enabled))
            {
                MiraMap.IsEnabled = enabled == 1;
            }

            int nav = 0;

            if (GetParam("Nav") != null && int.TryParse(GetParam("Nav"), out nav))
            {
                MiraMap.NavigationVisibility = (nav == 0) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }

            ID_Suspension = 0;

            if (GetParam("ID") != null && long.TryParse(GetParam("ID"), out ID_Suspension))
            {
                AllowSuspensionDetail = true;
                TB_Title.Text = string.Format("SUSPENSÃO {0}", ID_Suspension);
                CB_Year.IsEnabled = false;
                TB_Search.IsEnabled = false;
                SP_FilterSearch.Visibility = Visibility.Collapsed;
            }

            GetSuspensions();
        }

        private string GetParam(string p)
        {
            if (App.Current.Resources[p] != null)
                return App.Current.Resources[p].ToString();
            else
                return string.Empty;
        }

        private void CB_Year_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                SP_Breadcrumb.Visibility = Visibility.Visible;
                SB_Breadcrumb_Title_Close.Begin();

                SelectedPushpin = null;

                foreach (Pushpin item in m_PushpinLayer.Children)
                {
                    if (CB_Year.SelectedIndex == 0)
                        item.Visibility = Visibility.Visible;
                    else
                    {
                        string tag = item.Tag.ToString();
                        int index = tag.IndexOf("_");

                        string ID_Suspension = tag.Substring(0, index);

                        item.Visibility = GetItemStatus(ID_Suspension).Equals(CB_Year.SelectedIndex) ? Visibility.Visible : Visibility.Collapsed;
                    }

                    item.RenderTransform = new ScaleTransform
                    {
                        CenterX = item.Width / 2,
                        CenterY = item.Height / 2,
                        ScaleX = 0.75,
                        ScaleY = 0.75
                    };
                }

                MiraMap.SetView(new LocationRect(Locations));
                m_ControlLayer.Children.Clear();
                MiraMap.ZoomLevel = DefaultZoomLevel;
                AllowTimersTick = true;
            }
        }

        private int GetItemStatus(string id)
        {
            foreach (EPL_WS.GET_Suspensions_FullDetailed_Result item in PinsArray)
            {
                if (item.ID.ToString().Equals(id))
                {
                    return item.Status.Value;
                }
            }

            return -1;
        }

        private void SB_Breadcrumb_Title_Close_Completed(object sender, EventArgs e)
        {
            TB_Title.Text = ((ComboBoxItem)CB_Year.SelectedItem).Content.ToString();
            SB_Breadcrumb_Title_Open.Begin();
        }

        private void GetSuspensions()
        {
            try
            {
                EPL_WS.HZN_WSSoapClient soapClient = new EPL_WS.HZN_WSSoapClient("HZN_WSSoap");
                soapClient.GET_Suspensions_FullDetailedCompleted += new EventHandler<EPL_WS.GET_Suspensions_FullDetailedCompletedEventArgs>(soapClient_GET_Suspensions_FullDetailedCompleted);
                soapClient.GET_Suspensions_FullDetailedAsync();
                soapClient.CloseAsync();
            }
            catch
            {
            }
        }

        private void soapClient_GET_Suspensions_FullDetailedCompleted(object sender, EPL_WS.GET_Suspensions_FullDetailedCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null && e.Result.Length > 0)
                {
                    Locations = new List<Location>();
                    m_PushpinLayer.Children.Clear();
                    m_ControlLayer.Children.Clear();

                    PinsArray.Clear();

                    string Latitude;
                    string Longitude;

                    for (int i = 0; i < e.Result.Length; i++)
                    {
                        if (!AllowSuspensionDetail || (AllowSuspensionDetail && e.Result.ElementAt(i).ID.ToString().Equals(ID_Suspension.ToString())))
                        {
                            LinearGradientBrush PinBackgroundColor;

                            switch (e.Result.ElementAt(i).Status)
                            {
                                case 0:
                                    PinBackgroundColor = PinBackgroundColorNormal;
                                    break;

                                case 1:
                                    PinBackgroundColor = PinBackgroundColorApproved;
                                    break;

                                case 2:
                                    PinBackgroundColor = PinBackgroundColorExecution;
                                    break;

                                case 3:
                                    PinBackgroundColor = PinBackgroundColorConcluded;
                                    break;

                                default:
                                    PinBackgroundColor = PinBackgroundColorNormal;
                                    break;
                            }

                            Latitude = e.Result.ElementAt(i).Coord_Lat == null ? "0.0" : e.Result.ElementAt(i).Coord_Lat.ToString();
                            Longitude = e.Result.ElementAt(i).Coord_Long == null ? "0.0" : e.Result.ElementAt(i).Coord_Long.ToString();

                            if (AllowSuspensionDetail || (!AllowSuspensionDetail && e.Result.ElementAt(i).Status > 0))
                            {
                                PinsArray.Add(e.Result.ElementAt(i));

                                Pushpin pushpin = new Pushpin()
                                {
                                    Background = PinBackgroundColor,
                                    Tag = string.Format("{0}_{1}", e.Result.ElementAt(i).ID, e.Result.ElementAt(i).ID_Ramal),
                                    Location = new Location { Latitude = double.Parse(Latitude), Longitude = double.Parse(Longitude) },
                                    Cursor = Cursors.Hand
                                };

                                if (CB_Year.SelectedIndex == 0)
                                    pushpin.Visibility = Visibility.Visible;
                                else
                                {
                                    pushpin.Visibility = e.Result.ElementAt(i).Status.Equals(CB_Year.SelectedIndex) ? Visibility.Visible : Visibility.Collapsed;
                                }

                                pushpin.MouseLeftButtonDown += new MouseButtonEventHandler(pushpin_MouseLeftButtonDown);

                                pushpin.RenderTransform = new ScaleTransform
                                {
                                    CenterX = pushpin.Width / 2,
                                    CenterY = pushpin.Height / 2,
                                    ScaleX = 0.75,
                                    ScaleY = 0.75
                                };

                                m_PushpinLayer.AddChild(pushpin, new Location(double.Parse(Latitude), double.Parse(Longitude)), PositionOrigin.BottomCenter);

                                Locations.Add(pushpin.Location);
                            }
                        }
                    }

                    if (ShowAllPins)
                    {
                        MiraMap.SetView(new LocationRect(Locations));

                        if (MiraMap.ZoomLevel > 16)
                            MiraMap.ZoomLevel = 16;

                        DefaultZoomLevel = MiraMap.ZoomLevel;
                        ShowAllPins = false;
                    }

                    RefreshMapTimer.Start();
                }
                else
                {
                    MiraMap.ZoomLevel = 13;
                }
            }
            catch
            {
            }
        }

        private void pushpin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Pushpin pushPin = sender as Pushpin;
            
            e.Handled = true;

            RefreshSelectedPushpin();

            SelectedPushpin = pushPin;

            pushPin.RenderTransform = new ScaleTransform
            {
                CenterX = pushPin.Width / 2,
                CenterY = pushPin.Height,
                ScaleX = 1.5,
                ScaleY = 1.5
            };

            Canvas.SetZIndex(SelectedPushpin, 1000);

            foreach (EPL_WS.GET_Suspensions_FullDetailed_Result item in PinsArray)
            {
                if (string.Format("{0}_{1}", item.ID, item.ID_Ramal).Equals(pushPin.Tag.ToString()))
                {
                    ShowSuspensionDetail(pushPin, item);
                }
            }
        }

        private void ShowSuspensionDetail(Pushpin pushPin, EPL_WS.GET_Suspensions_FullDetailed_Result item)
        {
            if (item != null)
            {
                IsZoomed = true;
                AllowTimersTick = false;
                ShowAllPins = true;
                m_ControlLayer.Children.Clear();

                pushPin.Visibility = Visibility.Visible;

                Suspension_Detail detail = new Suspension_Detail
                {
                    Margin = new Thickness(27, 75, 0, 0),
                    ID = item.ID.ToString(),
                    ID_Ramal = item.ID_Ramal,
                    ID_Status = item.StatDesc,
                    
                    Date_Prev_Begin = String.Format("{0:ddd, dd-MM-yyyy HH:mm}", item.Date_Pred_Begin),
                    Date_Prev_End = String.Format("{0:ddd, dd-MM-yyyy HH:mm}", item.Date_Pred_End),
                    Date_Real_Begin = String.Format("{0:ddd, dd-MM-yyyy HH:mm}", item.Date_Real_Begin),
                    Date_Real_End = String.Format("{0:ddd, dd-MM-yyyy HH:mm}", item.Date_Real_End),

                    Observations = item.Description,
                    Description = item.Label
                };

                detail.CloseWindow += new Suspension_Detail.CloseWindowHandler(detail_CloseWindow);

                m_ControlLayer.AddChild(detail, new Location(pushPin.Location.Latitude, pushPin.Location.Longitude), PositionOrigin.CenterLeft);
                MiraMap.Center = pushPin.Location;

                MiraMap.ZoomLevel = Math.Max(13, MiraMap.ZoomLevel);
            }
            else
            {
                m_ControlLayer.Children.Clear();
            }
        }

        private void detail_CloseWindow(object sender, EventArgs e)
        {
            CloseDetailWindow();
        }

        private void RefreshSelectedPushpin()
        {
            foreach (Pushpin pushpin in m_PushpinLayer.Children)
            {
                pushpin.RenderTransform = new ScaleTransform
                {
                    CenterX = pushpin.Width / 2,
                    CenterY = pushpin.Height / 2,
                    ScaleX = 0.75,
                    ScaleY = 0.75
                };

                Canvas.SetZIndex(pushpin, 0);
            }
        }

        private void CloseDetailWindow()
        {
            IsZoomed = false;
            AllowTimersTick = true;
            m_ControlLayer.Children.Clear();
            RefreshSelectedPushpin();
        }

        private void IMG_Home_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (NavigateHome == null) return;

            NavigateHome(this, e);
        }

        private void DrawMapTimer_CallBack(object sender, EventArgs e)
        {
            if (!AllowTimersTick) return;

            DrawMapTimer.Stop();
            RefreshMapTimer.Start();
            GetSuspensions();
        }

        private void RefreshMapTimer_CallBack(object sender, EventArgs e)
        {
            if (!AllowTimersTick) return;

            ShowAllPins = false;
            DrawMapTimer.Stop();
            GetSuspensions();
        }

        private void MiraMap_ViewChangeEnd(object sender, MapEventArgs e)
        {
            if (!IsZoomed)
            {
                AllowTimersTick = true;

                if (RefreshMapTimer != null)
                    RefreshMapTimer.Start();
            }
        }

        private void MiraMap_MouseClick(object sender, MapMouseEventArgs e)
        {
            CloseDetailWindow();
        }

        #region Event Handlers

        private void TB_Search_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    StartSearch();
                    break;
                case Key.Escape:
                    TB_Search.Text = string.Empty;
                    break;
                default:
                    break;
            }
        }

        private void StartSearch()
        {
            bool found = false;
            AllowTimersTick = false;
            RefreshMapTimer.Start();

            List<Location> locations = new List<Location>();
            EPL_WS.GET_Suspensions_FullDetailed_Result item = new EPL_WS.GET_Suspensions_FullDetailed_Result();

            foreach (Pushpin pushpin in m_PushpinLayer.Children)
            {
                string tag = pushpin.Tag.ToString();
                int index = tag.IndexOf("_");

                string ID_Suspension = tag.Substring(0, index);
                string ID_Ramal = tag.Substring(index + 1, tag.Length - index - 1);

                if (ID_Suspension.Equals(TB_Search.Text))
                {
                    pushpin.RenderTransform = new ScaleTransform
                    {
                        CenterX = pushpin.Width / 2,
                        CenterY = pushpin.Height / 2,
                        ScaleX = 1.5,
                        ScaleY = 1.5
                    };

                    Canvas.SetZIndex(pushpin, 1000);

                    locations.Add(pushpin.Location);

                    if (!found)
                    {
                        item = PinsArray.Where(e => e.ID.ToString().Equals(ID_Suspension)).ElementAt(0);
                        SelectedPushpin = pushpin;
                        found = true;
                    }
                }
                else
                {
                    pushpin.RenderTransform = new ScaleTransform
                    {
                        CenterX = pushpin.Width / 2,
                        CenterY = pushpin.Height / 2,
                        ScaleX = 0.75,
                        ScaleY = 0.75
                    };

                    Canvas.SetZIndex(pushpin, 0);
                }
            }

            if (found)
            {
                MiraMap.SetView(new LocationRect(locations));
                ShowSuspensionDetail(SelectedPushpin, item);
            }
        }

        private void IMG_Search_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StartSearch();
        }

        private void ScrollContentPresenter_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!TB_Search.Text.ToString().Equals(string.Empty))
            {
                TB_Search.Text = string.Empty;
                TB_Search.FontStyle = FontStyles.Normal;
            }
        }

        private void TB_Search_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TB_Search.Text.Equals(string.Empty))
            {
                AllowTimersTick = true;
                ShowAllPins = true;

                GetSuspensions();
            }
        }

        #endregion
	}
}