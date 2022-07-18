using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using PaparazziGroundControlStation.Communication;
using PaparazziGroundControlStation.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;


namespace PaparazziGroundControlStation
{

    public partial class Main : Form
    {
        private System.Timers.Timer ZoomCenterRefresh;
        private BinaryWriter RecordFileStream;
        private BinaryReader PlayBackFileStream;
        private bool IsRecording;
        private bool KeyIsDown;
        private bool IsPlaying;
        private bool IsTrackBarMoving;
        private int currentLogTrackBarPos;
        private Recorder RecorderStream;
        private System.Timers.Timer uiUpdaterTimer;
        private System.Timers.Timer DownLinkRateTimer;
        static Main cMain;
        private uint[] FP_MAX_STAGES = {
            1 ,
            4 ,
            1 ,
            3 ,
            4 ,
            2 ,
            7 ,
            1 ,
            3 ,
            1 ,
            3 ,
            1 ,
            3 ,
            2 ,
            2 ,
            2
           } ;
        static string[] GpsStatus = { "Not fixed", "Not fixed", "2D", "3D", "?", "?", "?", "?" };
        static string[] ApModeTypesString = {"MANUAL","AUTO1", "AUTO2", "HOME", "NOGPS", "FAILSAFE"};
        static string[] Ap_gaz_mode = { "MANUAL" , "AUTO_THROTTLE" , "AUTO_CLIMB" , "AUTO_ALT" };
        static string[] Ap_lateral_mode = { "MANUAL", "ROLL_RATE", "ROLL", "COURSE" };
        static string[] Ap_horizontal_mode = { "WAYPOINT", "ROUTE", "CIRCLE"};
        static string[] mcu1_status = { "LOST", "OK", "REALLY_LOST" };
        static string[] rc_status = { "OK", "LOST", "REALLY_LOST" };
        static string[] state_filter_mode = { "UNKNOWN", "INIT", "ALIGN", "OK", "GPS_LOST", "IMU_LOST", "COV_ERR", "IR_CONTRAST", "ERROR" };

        private System.Timers.Timer planeMovingTimer;
        bool IsMouseOnMarker = false;
        GMapMarker SelectedMarker = null;
        GMapMarker DragingMarker = null;
        Point StartDragingPos = new Point();
        Point StartDragingMouse = new Point();
        bool IsDragingMarker = false;
        public List<PointLatLng> PlaneRoute = new List<PointLatLng>();

        private Point NewMarkerPos;
        private Point NewMarkerPosDblClick;

        private bool IsConnected = false;
        DataStream CommSerial;


        GMapMarker currentMarker;
        SettingParser SettingItems;

        PlansParser PlansData;
        MessagesParser MessagesItems;

        // Controling layers
        public GMapOverlay MarkersOverlay;
        public GMapOverlay PlaneOverlay;

        // Monitoring layers
        public GMapOverlay MPlaneRoutesOverlay;
        public GMapOverlay MPlaneOverlay;
        public GMapOverlay MMarkersOverlay;

        private byte recvExpectDlMsgIndex;
        private bool recvNewDlMsg;

        private byte recvExpectWpMoveMsgId;
        private double recvExpectWpMoveMsgLat;
        private double recvExpectWpMoveMsgLon;
        private bool recvNewWpMoveMsg;
        public uint recvBytesCnt;
        private System.Timers.Timer PingTransmitter;
        private Stopwatch PingTime;
        private GMapOverlay standbyCircleOverlay;
        public Main()
        {
            InitializeComponent();
        }
        public void GetSetting(byte settingType)
        {
            CommSerial.GetSetting((Setting.PPrzSettingList)settingType);
        }
        public void SetSetting(byte settingType, float value)
        {
            CommSerial.SetSetting((Setting.PPrzSettingList)settingType, value);
            recvNewDlMsg = false;
            recvExpectDlMsgIndex = settingType;
            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender,e) => {
                if (!recvNewDlMsg)
                {
                    statusAckMap.Text = "Error in send setting.";
                }
                ((System.Timers.Timer)(sender)).Stop();
                ((System.Timers.Timer)(sender)).Dispose();
            };
            timer.Start();
        }
        internal void DlReceived(byte index, float p)
        {
            cMain.BeginInvoke((ThreadStart)delegate
            {
                UpdateSettingItems(index, p);
            });
            if (!recvNewDlMsg && index == recvExpectDlMsgIndex)
            {
                recvNewDlMsg = true;
                statusAckMap.Text = String.Format("Setting '{0}' set to '{1}'", Setting.GetSettingTitle(index), p);
            }
        }
        public void UpdateSettingItems(byte index, float value)
        {
            SettingItems.SettingsUpdate(index,value);
        }
        public void SetWaypointsPos(double lat, double lon, double alt, byte index)
        {
            gMapControl.BeginInvoke((ThreadStart)delegate
            {
                (MarkersOverlay.Markers[index] as GMapStationMarker).Position = new PointLatLng(lat, lon);
                (MarkersOverlay.Markers[index] as GMapStationMarker).Alt = alt;
                gMapControl.UpdateMarkerLocalPosition(MarkersOverlay.Markers[index]);
                gMapControl.Invalidate();
                gMapControl.Refresh();
            });
            if (!recvNewWpMoveMsg && recvExpectWpMoveMsgId != index)
                recvNewWpMoveMsg = true;
            if(!recvNewWpMoveMsg && recvExpectWpMoveMsgId == index)
            if(recvExpectWpMoveMsgLat == lat && recvExpectWpMoveMsgLon == lon)
            {
                recvNewWpMoveMsg = true;
                statusAckMap.Text = String.Format("Waypoint '{0}' Moved!", PlansData.GetWaypoint(index)); ;
            }
        }

        public void WpMoved(byte index, double lat, double lon, double alt)
        {
            CommSerial.MoveWp(index, lat, lon, alt*1000);
            recvNewWpMoveMsg = false;
            recvExpectWpMoveMsgId = index;
            recvExpectWpMoveMsgLat = lat;
            recvExpectWpMoveMsgLon = lon;

            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, e) =>
            {
                if (!recvNewWpMoveMsg)
                {
                    //statusAckMap.Text = "Error in set Waypoint.";
                }
                ((System.Timers.Timer)(sender)).Stop();
                ((System.Timers.Timer)(sender)).Dispose();
            };
            timer.Start();
        }
        static void UiUpdater(object sender, ElapsedEventArgs e,Main MainObj)
        {
            try
            {
                Parameters.uiUpdater.WaitOne();

                MainObj.BeginInvoke((ThreadStart)delegate
                {
                    UpdateGauges(MainObj);
                    UpdatePlane(MainObj);
                    UpdateLabels(MainObj);
                    UpdateFailsIndicatores(MainObj);
                    MainObj.UpdateList();
                });

                //Parameters.linkQuality = 100.0f * (Parameters.DownLinkRate / Parameters.DataLinkReport.downlink_rate);
                Parameters.linkQuality = 100.0f * (Parameters.DownLinkRate / 950);
                Parameters.linkQuality = Parameters.linkQuality > 90 ? 100 : Parameters.linkQuality;
                Parameters.AGL = Math.Abs((Parameters.Estimator.z) - (Parameters.NavigationRef.ground_alt));
                Parameters.uiUpdater.Release();
            }
            catch(Exception exc)
            {

            }
            finally
            {
                if (Parameters.uiUpdater != null)
                    Parameters.uiUpdater.Release();
            }
        }

        static void UpdatePlane(Main MainObj)
        {            
            Util.UTMConvertor utmConv = new Util.UTMConvertor("WGS 84");
            Util.UTMConvertor.LatLng latLon = utmConv.convertUtmToLatLng(((double)Parameters.Gps.utm_east) / 100.0, ((double)Parameters.Gps.utm_north) / 100.0, Parameters.Gps.utm_zone, "N");
            Util.UTMConvertor.LatLng latLon0 = utmConv.convertUtmToLatLng(((double)Parameters.WpMoved[1].utm_east), ((double)Parameters.WpMoved[1].utm_north), Parameters.WpMoved[1].utm_zone, "N");
            Parameters.Pbearing =bearing(latLon0.Lat, latLon0.Lng, latLon.Lat, latLon.Lng);
            MainObj.PlaneOverlay.Markers[0].Position = new PointLatLng(latLon.Lat, latLon.Lng);
            ((GMapPlane)(MainObj.PlaneOverlay.Markers[0])).Angle = Parameters.Gps.course / 10.0;
            MainObj.gMapControl.Invalidate();
            MainObj.gMapControl.Refresh();

            MainObj.MPlaneOverlay.Markers.Clear();
            MainObj.MPlaneOverlay.Markers.Add(new GMapPlane(new PointLatLng(latLon.Lat, latLon.Lng), Properties.Resources.airplane,true));
            ((GMapPlane)(MainObj.MPlaneOverlay.Markers[0])).Angle = Parameters.Gps.course / 10.0;
            MainObj.gMapMonitor.Invalidate();
            MainObj.gMapMonitor.Refresh();
        }
        static void UpdateGauges(Main MainObj)
        {
            MainObj.status3D1.Roll = -Parameters.Attitude.phi;
            MainObj.status3D1.Pitch = -Parameters.Attitude.theta;
            MainObj.status3D1.Yaw = Parameters.Attitude.psi;
            MainObj.status3D1.Update();
            //MainObj.hud.displayrollpitch = false;
            MainObj.attitudeIndicatorInstrumentControl1.SetAttitudeIndicatorParameters((float)((Parameters.Attitude.theta / Math.PI) * 180.0f), (float)((Parameters.Attitude.phi / Math.PI) * 180.0f));
            double psi = (Parameters.Attitude.psi / Math.PI) * 180;
            if (psi < 0)
                psi =Math.Abs(psi) + 180;
            MainObj.headingIndicatorInstrumentControl.SetHeadingIndicatorParameters((int)psi);

            MainObj.airSpeedIndicatorInstrumentControl1.SetAirSpeedIndicatorParameters(Parameters.AirData.airspeed * 3.6f);
            MainObj.altimeterInstrumentControl.SetAlimeterParameters((float)Parameters.AGL);
            DateTime dt = DateTime.MinValue;
            DateTime dtfommls = dt.AddMilliseconds(Parameters.Bat.flight_time * 1000);
            MainObj.altimeterInstrumentControl.SetAlimeterParameters(Parameters.Gps.alt / 1000);
            MainObj.turnCoordinatorInstrumentControl1.SetTurnCoordinatorParameters((float)(Parameters.ImuGyro.gr / Math.PI) * 180, 0);
        }
        static void UpdateFailsIndicatores(Main MainObj)
        {
            MainObj.RollFail.Image = (Parameters.Parachute.trigger & 0x02) != 0 ? Properties.Resources.ind_fault_red : Properties.Resources.ind_fault_green;

            MainObj.PitchFail.Image = (Parameters.Parachute.trigger & 0x04) != 0 ? Properties.Resources.ind_fault_red : Properties.Resources.ind_fault_green;

            MainObj.EngineFail.Image = (Parameters.Parachute.trigger & 0x08) != 0 ? Properties.Resources.ind_fault_red : Properties.Resources.ind_fault_green;

            MainObj.Parachute.Image = (Parameters.Parachute.trigger & 0x01) != 0 ? Properties.Resources.ind_fault_red : Properties.Resources.ind_fault_green;

            MainObj.CutoffFail.Image = (Parameters.Parachute.trigger & 0x10) != 0 ? Properties.Resources.ind_fault_red : Properties.Resources.ind_fault_green;

            MainObj.BatteryFail.Image = (Parameters.Parachute.trigger & 0x20) != 0 ? Properties.Resources.ind_fault_red : Properties.Resources.ind_fault_green;

            MainObj.LinkFail.Image = Parameters.linkQuality < 50 ? Properties.Resources.ind_fault_red : Properties.Resources.ind_fault_green;

            MainObj.ModeStatus.Image = (Parameters.PprzMode.ap_mode == 5) ? Properties.Resources.ind_fault_red : Properties.Resources.ind_fault_green;

            MainObj.ModeStatus.Text = ApModeTypesString[Parameters.PprzMode.ap_mode];

        }
        static void UpdateLabels(Main MainObj)
        {
            Util.UTMConvertor utmConv = new Util.UTMConvertor("WGS 84");
            Util.UTMConvertor.LatLng latLon = utmConv.convertUtmToLatLng(((double)Parameters.Gps.utm_east) / 100.0, ((double)Parameters.Gps.utm_north) / 100.0, Parameters.Gps.utm_zone, "N");
            MainObj.RollLabel.Text = ((Parameters.Attitude.phi / Math.PI) * 180).ToString("F1");
            MainObj.PitchLabel.Text = ((Parameters.Attitude.theta / Math.PI) * 180).ToString("F1");
            double psi = (Parameters.Attitude.psi / Math.PI) * 180;
            if (psi < 0)
                psi = Math.Abs(psi) + 180;
            MainObj.YawLabel.Text = psi.ToString("F1");
            MainObj.LaserAltLabel.Text = (Parameters.LaserAlt.alt).ToString("F1");
            MainObj.IASLabel.Text = (Parameters.AirData.airspeed*3.6).ToString("F1");           
            MainObj.EngineRPMLabel.Text = (Parameters.Motor.rpm).ToString("F1");
            MainObj.BatteryVoltageLabel.Text = (Parameters.Bat.voltage/10).ToString("F1") + "v";
            MainObj.AltitudeLable.Text = (Parameters.Estimator.z).ToString("F1");
            MainObj.AGLlable.Text =Math.Abs(((Parameters.Estimator.z) - (Parameters.NavigationRef.ground_alt))).ToString("F1");

            MainObj.GpsAltLabel.Text = (Parameters.Gps.alt/1000).ToString("F1");
            MainObj.GpsFixStateLabel.Text = GpsStatus[Parameters.Gps.mode];
            MainObj.GpsHeadingLabel.Text = (Parameters.Gps.course / 10.0).ToString("F1");
            MainObj.GpsLatLabel.Text = latLon.Lat.ToString("F5");
            MainObj.GpsLngLabel.Text = latLon.Lng.ToString("F5");
            MainObj.GpsNumberOfSatellitesLabel.Text = Parameters.GpsSol.numSV.ToString();
            MainObj.GpsSpeedLabel.Text = ((Parameters.Gps.speed/100)*3.6).ToString("F1");
            MainObj.bearinglabel.Text = Parameters.Pbearing.ToString("F1");

            MainObj.VerticalSpeedLabel.Text = (Parameters.Gps.climb/100).ToString();
            MainObj.GazModelabel.Text = Ap_gaz_mode[Parameters.PprzMode.ap_gaz];
            MainObj.LateralModelabel.Text = Ap_lateral_mode[Parameters.PprzMode.ap_lateral];
            MainObj.FilterModelabel.Text = state_filter_mode[Parameters.StateFilterStatus.state_filter_mode];
            MainObj.rcmodelabel.Text = rc_status[Parameters.FbwStatus.rc_status];
            
            
            if (Parameters.Gps.mode == 3) //GPS in 3D mode
            {
                GMapRoute route = new GMapRoute(MainObj.PlaneRoute, "Plane Path");
                MainObj.PlaneRoute.Add(new PointLatLng(latLon.Lat, latLon.Lng));
                route.Stroke = new Pen(Color.White, 1);
                MainObj.MPlaneRoutesOverlay.Routes.Clear();
                MainObj.MPlaneRoutesOverlay.Routes.Add(route);
            }

            if (MainObj.RecorderStream.LogLengthMs > 0 && MainObj.IsTrackBarMoving == false)
            {
                int value = (int)(MainObj.RecorderStream.CurrentPosTimeRelativity * 100) / (int)MainObj.RecorderStream.LogLengthMs;
                MainObj.trackBarLog.Value = value > 100 ? 100 : value;
                MainObj.labelLogTimer.Text = TimeSpan.FromMilliseconds((float)MainObj.RecorderStream.CurrentPosTimeRelativity).ToString(@"hh\:mm\:ss");
                MainObj.currentLogTrackBarPos = MainObj.trackBarLog.Value;
            }
        }
        public void MessageReceived(byte cmd, byte[] data)
        {
            recvBytesCnt += (uint)data.Length + 6;
            MessagesItems.MessageArrived(cmd, data.ToList());
            if (IsRecording)
            {
                RecorderStream.AddNewData(cmd, data);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Hide();
            RecorderStream = new Recorder();
            IsRecording = false;
            IsPlaying = false;
            KeyIsDown = false;
            planeMovingTimer = new System.Timers.Timer();
            NewMarkerPos = new Point();
            Splash splashScreen = new Splash();
            splashScreen.Show(this);
            gMapMonitor.MapProvider = BingSatelliteMapProvider.Instance;
            gMapControl.MapProvider = BingSatelliteMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;
            gMapControl.SetPositionByKeywords("Iran , Tehran");
            gMapControl.DragButton = MouseButtons.Left;
            gMapControl.Zoom = 10;
            gMapMonitor.SetPositionByKeywords("Iran , Tehran");
            gMapMonitor.DragButton = MouseButtons.Left;
            gMapMonitor.Zoom = 10;
            gMapMonitor.CacheLocation = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            gMapMonitor.Manager.Mode = AccessMode.ServerAndCache;
            
            mainSplitContainer_SplitterMoved(null, null);
            InitComm();
            PlaneOverlay = new GMapOverlay("PlaneCnt");
            MarkersOverlay = new GMapOverlay("markers");

            MMarkersOverlay = new GMapOverlay("markers");
            MPlaneOverlay = new GMapOverlay("Plane");
            MPlaneRoutesOverlay = new GMapOverlay("routes");
            standbyCircleOverlay = new GMapOverlay("StandbyCircle");

            currentMarker = new GMapPlane(gMapControl.Position, Properties.Resources.airplane,true);
            trackBar1.Value = 5;
            gMapControl.Overlays.Add(standbyCircleOverlay);
            MPlaneOverlay.Markers.Add(currentMarker);
            PlaneOverlay.Markers.Add(currentMarker);
            gMapControl.Overlays.Add(MarkersOverlay);
            gMapControl.Overlays.Add(PlaneOverlay);
            

            gMapMonitor.Overlays.Add(MPlaneOverlay);
            gMapMonitor.Overlays.Add(MPlaneRoutesOverlay);
            gMapMonitor.Overlays.Add(MMarkersOverlay);

            gMapControl.Invalidate();
            gMapControl.Refresh();

            UpdateMonitorMap();
            comboBoxMapProvider.SelectedIndex = 0;
            cMain = this;
            uiUpdaterTimer = new System.Timers.Timer(500);  // Interval of UI timer
            uiUpdaterTimer.Elapsed += (timerSender, timerEvt) => UiUpdater(timerSender, timerEvt, this);
            uiUpdaterTimer.Start();

            ZoomCenterRefresh = new System.Timers.Timer(20000);
            ZoomCenterRefresh.Elapsed += ZoomCenterRefresh_Elapsed;
            ZoomCenterRefresh.Start();

            DownLinkRateTimer = new System.Timers.Timer(2000);  // Interval of DownLink data rate timer
            DownLinkRateTimer.Elapsed += (timerRateSender, TimerRateEvt) => DownlinkRateUpdate(timerRateSender, TimerRateEvt, this);
            DownLinkRateTimer.Start();


            recvExpectDlMsgIndex = 255;
            recvNewDlMsg = false;

            recvExpectWpMoveMsgId = 255;
            recvNewWpMoveMsg = false;
            

            String fileMessagesXml = Directory.GetCurrentDirectory() + "\\messages.xml";
            String fileSettingsXml = Directory.GetCurrentDirectory() + "\\settings.xml";
            String fileFlightPlansXml = Directory.GetCurrentDirectory() + "\\flight_plans.xml";
            PlansData = new PlansParser(fileFlightPlansXml);
            MessagesItems = new MessagesParser(fileMessagesXml,ledList1,checkBoxTuning,zedGraphControlTuning,treeViewMsgFields);
            SettingItems = new SettingParser(tabPageSetting, fileSettingsXml, SetSetting, GetSetting);
            for (int i = 0; i < PlansData.GetBlocksCount();i++ )
            {
                Commands.Rows.Add(new object[] { "", PlansData.GetBlock(i), "-" });
            }
            InsertAllWaypoints();
            PingTransmitter = new System.Timers.Timer(2000);
            PingTransmitter.Elapsed += PingTransmitter_Elapsed;
            PingTransmitter.Start();
            PingTime = new Stopwatch();
            splashScreen.Hide();
            Show();
        }
        void ZoomCenterRefresh_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CommSerial.IsOpen())
            gMapMonitor.BeginInvoke((ThreadStart)delegate
            {
                double zoom = gMapMonitor.Zoom;
                gMapMonitor.ZoomAndCenterMarkers("Plane");
                gMapMonitor.Zoom = zoom;
            });
        }
        void PingTransmitter_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CommSerial.IsOpen())
            {
                if (CommSerial.SendPing())
                {
                    PingTime.Reset();
                    PingTime.Start();
                }
            }
        }

        public static void DownlinkRateUpdate(object timerRateSender, ElapsedEventArgs TimerRateEvt, Main MainObj)
        {
            Parameters.DownLinkRate = MainObj.recvBytesCnt / 2;
            MainObj.recvBytesCnt = 0;
        }

        private void DrawCircleOnMap(PointLatLng center, double radius)
        {
            double lat = (center.Lat * Math.PI) / 180;
            double lng = (center.Lng * Math.PI) / 180;
            double d = radius / 6367000;
            List<PointLatLng> gpollist = new List<PointLatLng>();
            for (var x = 0; x <= 360; x++)
            {
                // Calculate the coordinates of the point
                double brng = (Math.PI * x)/180;
                double latRadians = Math.Asin((Math.Sin(lat) * Math.Cos(d)) + (Math.Cos(lat) * Math.Sin(d) * Math.Cos(brng)));
                double lngRadians = lng
                                    + Math.Atan2(
                                        Math.Sin(brng) * Math.Sin(d) * Math.Cos(lat),
                                        Math.Cos(d) - (Math.Sin(lat) * Math.Sin(latRadians)));

                PointLatLng gpoi = new PointLatLng((latRadians * 180) / Math.PI, (lngRadians * 180) / Math.PI);
                gpollist.Add(gpoi);
            }

            GMapPolygon gpol = new GMapPolygon(gpollist, "pol");
            standbyCircleOverlay.Clear();
            standbyCircleOverlay.Polygons.Add(gpol);

        }
        private void InsertAllWaypoints()
        {
            for (int i=0;i<PlansData.GetWaypointsCount();i++)
            {
                string title = PlansData.GetWaypoint(i);
                SetFromMap(0, 0, 0, title.Substring(0,title.Length < 2 ? 1 : 2));
            }

            UpdateList();
        }
        private void UpdateMonitorMap()
        {
            gMapMonitor.Overlays.Remove(MMarkersOverlay);
            MMarkersOverlay.Clear();
            foreach (GMapMarker m in MarkersOverlay.Markers)
            {
                GMapMarker nm = new GMapStationMarker(new PointLatLng(m.Position.Lat, m.Position.Lng), (m as GMapStationMarker).name);
                MMarkersOverlay.Markers.Add(nm);
            }
            gMapMonitor.Overlays.Add(MMarkersOverlay);
            gMapMonitor.Invalidate();
            gMapMonitor.Refresh();
        }

        private void mainSplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ReorderGaugesByTabMonitor();
        }

        private void InitComm()
        {
            CommSerial = new DataStream(this);
            BaudrateList.Items.AddRange(new String[] { "9600", "19200", "38400", "57600", "115200" });
            CommPortList.Items.AddRange(CommSerial.PortsName.ToArray());
            if(CommSerial.PortsName.Count > 0)
                CommPortList.SelectedIndex = 0;
            BaudrateList.SelectedIndex = 0;
        }
        public static void PlaneMovingTimer(object sender, ElapsedEventArgs e, Main mainObj)
        {



        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (IsPlaying)
                return;
            if (IsConnected == false)
            {
                if (CommPortList.SelectedItem.ToString() == "")
                    return;
                if(CommSerial.Connect(CommPortList.SelectedItem.ToString(), int.Parse(BaudrateList.SelectedItem.ToString()))){
                    IsConnected = true;
                    button1.Text = "Disconnect";
                    ConnectionStatusBar.Text = "Connected";
                    ConnectionStatusBar.BackColor = Color.Green;
                    planeMovingTimer.Interval = 500;
                    planeMovingTimer.Elapsed += (_sender, evt) => PlaneMovingTimer(_sender,evt,this);
                    planeMovingTimer.Start();
                    recvBytesCnt = 0;
                }
            }
            else
            {
                planeMovingTimer.Stop();
                CommSerial.Disconnect();
                IsConnected = false;
                button1.Text = "Connect";
                ConnectionStatusBar.Text = "Not Connected";
                ConnectionStatusBar.BackColor = Color.Gray;
            }
        }

        private void UpdateList()
        {
            foreach (DataGridViewRow row in Commands.Rows)
            {
                DataGridViewTextBoxCell curentBlock = row.Cells[0] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell blockName = row.Cells[1] as DataGridViewTextBoxCell;
                DataGridViewTextBoxCell statgeStatus = row.Cells[2] as DataGridViewTextBoxCell;

                if (row.Index == Parameters.Navigation.cur_block)
                {
                    statgeStatus.Value = Parameters.Navigation.cur_stage + "/" + FP_MAX_STAGES[Parameters.Navigation.cur_block];
                    curentBlock.Value = ">";
                }else
                    curentBlock.Value = "";

            }
        }
        public void SetFromMap(double lat, double lng, int alt, string title)
        {

            GMapMarker CM = new GMapStationMarker(new PointLatLng(lat, lng), title);

            (CM as GMapStationMarker).Alt = alt;
            if (title == "ST")
                DrawCircleOnMap(CM.Position, Parameters.DlValue[(int)(Communication.Setting.PPrzSettingList._nav_radius)]);
            MarkersOverlay.Markers.Add(CM);
            gMapControl.Invalidate();
            gMapControl.Refresh();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            gMapControl.Zoom = trackBar1.Value + 2;
        }

        private void gMapControl_OnMapZoomChanged()
        {
            trackBar1.Value = (int)(gMapControl.Zoom) - 2 ;

        }

        private void gMapControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                NewMarkerPos.X = e.X;
                NewMarkerPos.Y = e.Y;
                modifyAltToolStripMenuItem.Visible = IsMouseOnMarker;
                clearFlightPathToolStripMenuItem.Visible = false;
                DragingMarker = null;
                IsDragingMarker = false;
            }
            else if (e.Button == MouseButtons.Left && IsMouseOnMarker)
            {
                IsDragingMarker = true;
                DragingMarker = SelectedMarker;
                StartDragingPos.X = (int)gMapControl.FromLatLngToLocal(SelectedMarker.Position).X;
                StartDragingPos.Y = (int)gMapControl.FromLatLngToLocal(SelectedMarker.Position).Y;
                StartDragingMouse.X = e.X;
                StartDragingMouse.Y = e.Y;

            }
            else
            {
                DragingMarker = null;
                IsDragingMarker = false;
            }
            if(e.Button == MouseButtons.Left)
            {
                NewMarkerPosDblClick.X = e.X;
                NewMarkerPosDblClick.Y = e.Y;
            }
            groupBoxMarkerInfo.Visible = IsMouseOnMarker;
        }

        private void gMapControl_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                MarkerInfoLat.Text = item.Position.Lat.ToString("0.00000000");
                MarkerInfoLng.Text = item.Position.Lng.ToString("0.00000000");
                try
                {
                    MarkerInfoAlt.Text = ((GMapStationMarker)(item)).Alt.ToString("0.00000000");
                }catch(InvalidCastException exc)
                {
                    //MarkerInfoAlt.Text = ((GMapPlane)(item)).Alt.ToString("0.00000000");
                }
            }
        }

        private void gMapControl_OnMarkerEnter(GMapMarker item)
        {
            if (item != MPlaneOverlay.Markers[0] && item != PlaneOverlay.Markers[0])
                IsMouseOnMarker = true;
            SelectedMarker = item;
        }

        private void gMapControl_OnMarkerLeave(GMapMarker item)
        {
            IsMouseOnMarker = false;
            SelectedMarker = null;
        }


        private void gMapControl_MouseMove(object sender, MouseEventArgs e)
        {
            modifyAltToolStripMenuItem.Visible = gMapControl.IsMouseOverMarker;
            if (IsDragingMarker)
            {
                DragingMarker.Position = gMapControl.FromLocalToLatLng(e.X - StartDragingMouse.X + StartDragingPos.X, e.Y - StartDragingMouse.Y + StartDragingPos.Y) ;
                if ((DragingMarker as GMapStationMarker).name == "ST")
                    DrawCircleOnMap(DragingMarker.Position, Parameters.DlValue[(int)(Communication.Setting.PPrzSettingList._nav_radius)]);
                gMapControl.UpdateMarkerLocalPosition(DragingMarker);
                gMapControl.Invalidate();
                gMapControl.Refresh();
            }
        }

        private void gMapControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && IsDragingMarker)
            {
                IsDragingMarker = false;
                WpMoved((byte)(MarkersOverlay.Markers.IndexOf((DragingMarker as GMapStationMarker))), DragingMarker.Position.Lat, DragingMarker.Position.Lng, (DragingMarker as GMapStationMarker).Alt);
                DragingMarker = null;
            }
        }

        private void modifyAltToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newAlt = "";
            if (SelectedMarker != null)
            {
                if (InputBox.Show("Altiude Value", "Enter new altiude value :", (SelectedMarker as GMapStationMarker).Alt.ToString(), ref newAlt) == DialogResult.OK)
                {
                    (SelectedMarker as GMapStationMarker).Alt = int.Parse(newAlt);
                    WpMoved((byte)(MarkersOverlay.Markers.IndexOf((SelectedMarker as GMapStationMarker))), (SelectedMarker as GMapStationMarker).Position.Lat, (SelectedMarker as GMapStationMarker).Position.Lng, (SelectedMarker as GMapStationMarker).Alt);
                }
            }
        }

        private void gMapMonitor_OnMapZoomChanged()
        {
            trackBar2.Value = (int)gMapMonitor.Zoom-2;
            gMapMonitor.Invalidate();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            gMapMonitor.Zoom = trackBar2.Value + 2;
            gMapMonitor.Invalidate();
        }
        private void ReorderGaugesByTabMonitor()
        {
            try
            {
                int GaugesSize = splitContainer1.Panel2.Width / 12;
                int Height = GaugesSize * 2;
                int Width = splitContainer1.Panel2.Width;
                splitContainer1.SplitterDistance = Size.Height - Height - 50;
                altimeterInstrumentControl.Top = 0;                
                status3D1.Top = GaugesSize;
            }
            catch (Exception exc)
            {

            }
        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            PingTransmitter.Stop();
            DownLinkRateTimer.Stop();
            uiUpdaterTimer.Stop();
            CommSerial.DisposeRecvThread();
            MessagesItems.killTuningThread();
            CommSerial.Disconnect();
        }

        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
            gMapControl.Zoom = trackBar1.Value + 2;
            gMapControl.Invalidate();
        }

        private void comboBoxMapProvider_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (comboBoxMapProvider.SelectedIndex == 0)
            {
                gMapMonitor.MapProvider = BingSatelliteMapProvider.Instance;
                gMapControl.MapProvider = BingSatelliteMapProvider.Instance;
            }
            else
            {
                gMapMonitor.MapProvider = BingMapProvider.Instance;
                gMapControl.MapProvider = BingMapProvider.Instance;
            }
        }

        private void buttonRec_Click(object sender, EventArgs e)
        {
            if (IsConnected && !IsPlaying)
            {
                if (IsRecording)
                {
                    buttonRec.Text = "Record";
                    RecorderStream.StopRecording();
                    IsRecording = false;
                }
                else
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "*.pld|*.pld";
                    sfd.ShowDialog(this);
                    if (sfd.FileName == "")
                        return;
                    RecordFileStream = new BinaryWriter(sfd.OpenFile());
                    buttonRec.Text = "Stop";
                    RecorderStream.StartRecording(RecordFileStream);
                    IsRecording = true;
                }
            }
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (!IsConnected && !IsRecording)
            {
                if (IsPlaying)
                {
                    buttonPlay.Text = "Playback";
                    RecorderStream.StopPlayBack();
                    IsPlaying = false;
                }
                else
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Filter = "*.pld|*.pld";
                    ofd.ShowDialog(this);
                    if (ofd.FileName == "")
                        return;
                    PlayBackFileStream = new BinaryReader(ofd.OpenFile());
                    buttonPlay.Text = "Stop";
                    RecorderStream.StartPlayBack(PlayBackFileStream, PlayBackMessageCB, PlayBackFinish);
                    IsPlaying = true;
                }
            }
        }

        public void PlayBackFinish()
        {
            buttonPlay.BeginInvoke((ThreadStart)delegate
            {
                buttonPlay.Text = "Playback";
            });
            IsPlaying = false;
            this.BeginInvoke((ThreadStart)delegate
            {
                MessageBox.Show(this, "End of playback", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        public void PlayBackMessageCB(byte cmd,byte [] data)
        {
            DataStream.MsgDownRecv(0, (DataStream.MessagesDownID)cmd, data);
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (mainTabController.SelectedIndex == 0)
                ReorderGaugesByTabMonitor();
        }

        public void Pong()
        {
            PingTime.Stop();
            
            //pingLabel.Text = (PingTime.ElapsedMilliseconds).ToString();
            cMain.BeginInvoke((ThreadStart)delegate
            {
                pingLabel.Text = (PingTime.ElapsedMilliseconds).ToString();
            });
        }

        private void clearFlightPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GMapRoute route = new GMapRoute(PlaneRoute, "Plane Path");
            PlaneRoute.Clear();
            route.Stroke = new Pen(Color.White, 1);
            MPlaneRoutesOverlay.Routes.Clear();
            MPlaneRoutesOverlay.Routes.Add(route);
        }

        private void Commands_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right && !KeyIsDown)
            {
                CommSerial.SetCurrBlock((byte)Commands.SelectedRows[0].Index);
                KeyIsDown = true;
            }
        }

        private void Commands_KeyUp(object sender, KeyEventArgs e)
        {
            KeyIsDown = false;
        }

        private void trackBarLog__MouseDown(object sender, MouseEventArgs e)
        {
            IsTrackBarMoving = true;
        }

        private void trackBarLog__MouseUp(object sender, MouseEventArgs e)
        {
            if (!IsPlaying)
            {
                IsTrackBarMoving = false;
                return;
            }
            if (IsTrackBarMoving)
                RecorderStream.SetSeekBar(PlayBackFileStream, (ulong)trackBarLog.Value * RecorderStream.LogLengthMs / 100);
            IsTrackBarMoving = false;
        }

        private void trackBarLog_ValueChanged(object sender, EventArgs e)
        {
            if (!IsTrackBarMoving)
                return;
            labelLogTimer.Text = ((trackBarLog.Value - currentLogTrackBarPos) < 0 ? "-" : "+") + TimeSpan.FromMilliseconds(Math.Abs(trackBarLog.Value - currentLogTrackBarPos) * (int)RecorderStream.LogLengthMs / 100).ToString(@"hh\:mm\:ss");
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            if (!IsConnected && !IsRecording && !IsPlaying)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "*.pld|*.pld";
                ofd.ShowDialog(this);
                if (ofd.FileName == "")
                    return;
                PlayBackFileStream = new BinaryReader(ofd.OpenFile());

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "*.csv|*.csv";
                sfd.ShowDialog(this);
                if (sfd.FileName == "")
                    return;
                StreamWriter CsvFile = new StreamWriter(sfd.OpenFile());

                RecorderStream.ConvertToCSV(PlayBackFileStream, CsvFile, MessagesItems);
            }
        }
        static double DegreeBearing(double lat1, double lon1,double lat2, double lon2)
        {
            var dLon = ToRad(lon2 - lon1);
            var dPhi = Math.Log(
                Math.Tan(ToRad(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRad(lat1) / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI)
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            return (Math.Atan2(dLon, dPhi));
        }

        static  double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        static  double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }
        
        static double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }
        static double bearing(double lat, double lon, double lat2, double lon2)
        {

            double teta1 = ToRad(lat);
            double teta2 = ToRad(lat2);
            double delta1 = ToRad(lat2 - lat);
            double delta2 = ToRad(lon2 - lon);

            //==================Heading Formula Calculation================//

            double y =Math.Sin(delta2) * Math.Cos(teta2);
            double x = Math.Cos(teta1) * Math.Sin(teta2) - Math.Sin(teta1) * Math.Cos(teta2) * Math.Cos(delta2);
            double brng = Math.Atan2(y, x);
            brng = ToDegrees(brng);// radians to degrees
            brng = (((int)brng + 360) % 360);
            return brng;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "UAV Log|*.*";
            ofd.ShowDialog(this);
            if (ofd.FileName == "")
                return;
            UAVLogAnalyse log = new UAVLogAnalyse(MessagesItems);
            log.LoadFile(ofd.FileName);


            LogParser logParser = new LogParser();

            logParser.SetUavLog(log);

            logParser.Show(this);
        }

        private void gMapMonitor_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                modifyAltToolStripMenuItem.Visible = false;
                clearFlightPathToolStripMenuItem.Visible = true;
            }
        }
    }


}
