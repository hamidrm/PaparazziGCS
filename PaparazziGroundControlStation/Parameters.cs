using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaparazziGroundControlStation
{
    public class Parameters
    {

        public static Semaphore uiUpdater=new Semaphore(1,1000);
        public struct AirDataType
        {
            public float pressure;
            public float diff_p;
            public float temp;
            public float qnh;
            public float amsl_baro;
            public float airspeed;
            public float tas;
        };
        public struct AutopilotVersionType{
            public uint version;
            public string desc;
        };
        public struct WpMovedType{
            public byte utm_zone;
            public float utm_east,utm_north,alt;
        };
        public struct Ads1115Type{
            public float ADC0,ADC1,ADC2,ADC3;
        };
        public struct GpsType{
            public byte mode, utm_zone, gps_nb_err;
            public int utm_east, utm_north, alt, itow;
            public short course, speed, climb, week;
        };
        public struct SvInfoType{
            public byte chn, SVID, Flags, QI, CNO, Elev;
            public short Azim;
        };
        public struct DataLinkReportType{
            public byte downlink_ovrn;
            public ushort uplink_lost_time, uplink_nb_msgs, downlink_nb_msgs, downlink_rate, uplink_rate;
        };
        public struct EstimatorType{
            public float z,z_dot;
        };
        public struct AttitudeType{
            public float phi,psi,theta;
        };
        public struct FbwStatusType{
            public byte rc_status; //OK, LOST, REALLY_LOST
            public byte mode; //MANUAL, AUTO, FAILSAFE
            public byte frame_rate;
            public ushort vsupply; //decivolt
            public int current;
        };
        public struct StateFilterStatusType{
            public byte id;
            public byte state_filter_mode; //UNKNOWN, INIT, ALIGN, OK, GPS_LOST, IMU_LOST, COV_ERR, IR_CONTRAST, ERROR
            public ushort value;
        };
        public struct ImuGyroType{
            public float gp,gq,gr;
        };
        public struct AdcGenericType{
            public ushort val1, val2;
        };
        public struct BatType{
            public ushort throttle, voltage, flight_time, block_time, stage_time;
            public short amps, energy;
            public byte kill_auto_throttle;
        };
        public struct PprzModeType{
            public byte ap_mode; //MANUAL, AUTO1, AUTO2, HOME, NOGPS, FAILSAFE
            public byte ap_gaz;  //MANUAL, AUTO_THROTTLE, AUTO_CLIMB, AUTO_ALT
            public byte ap_lateral;  //MANUAL, ROLL_RATE, ROLL, COURSE
            public byte ap_horizontal;   //WAYPOINT, ROUTE, CIRCLE
            public byte if_calib_mode;   //NONE, DOWN, UP
            public byte mcu1_status; //LOST, OK, REALLY_LOST
        };
        public struct DesiredType{
            public float roll,pitch,course,x,y,altitude,climb,airspeed;
        };
        public struct PpmType{
            public byte ppm_rate;
        };
        public struct NavigationType{
            public float pos_x,pos_y,dist_wp,dist_home;
            public byte cur_block, cur_stage, circle_count, oval_count;
        };
        public struct WpMovedLlaType{
            public int lat, lon, alt;
        };
        public struct NavigationRefType{
            public int utm_east, utm_north; //m
            public byte utm_zone;
            public float ground_alt;
        };
        public struct EnergyType{
            public float bat,amp,power;
            public ushort energy;
        };
        public struct I2cErrorsType{
            public ushort wd_reset_cnt, queue_full_cnt, acknowledge_failure_cnt, misplaced_start_or_stop_cnt, arbitration_lost_cnt, overrun_or_underrun_cnt, pec_error_in_reception_cnt, timeout_or_tlow_error_cnt, smbus_alert_cnt, unexpected_event_cnt;
            public uint last_unexpected_event;
            public byte bus_number;
        };
        public struct CalibrationType{
            public float climb_sum_err;
            public byte climb_gaz_submode;
        };
        public struct AirspeedType{
            public float airspeed,airspeed_sp,airspeed_cnt,groundspeed_sp;
        };
        public struct DlValueType{
            public float val;
        };
        public struct CommandsType{
            public List<ushort> ticks;
        };
        public struct AliveType{
            public string md5sum;
        };
        public struct GpsSolType
        {
            public uint Pacc;
            public uint Sacc;
            public ushort PDOP;
            public byte numSV;
        };
        public struct LaserAltType
        {
            public float alt;
        };
        public struct CircleType
        {
            public float center_east;
            public float center_north;
            public float radius;
        };
        public struct MotorType
        {
            public ushort rpm;
            public int current;
        };
        public struct ParachuteType
        {
            public byte trigger;
        };
        public struct AirspeedMs45xxType
        {
            public float diffPress;
            public ushort temperature;
            public float airspeed;
        };
        public struct LoggerStatusType
        {
            public byte status;
            public byte errno;
            public uint used;
        };
        public struct HCTLType
        {
            public float roll_sum_err;
            public float roll_sp;
            public float roll_ref;
            public float phi;
            public ushort aileron_sp;
            public float pitch_sum_err;
            public float pitch_sp;
            public float pitch_ref;
            public float theta;
            public ushort elevator_sp;
        };

        public static byte aircraftId;
        public static AutopilotVersionType AutopilotVersion;
        public static string fwVersion;
        public static string fwDesc;
        public static LoggerStatusType LoggerStatus;
        public static AirspeedMs45xxType AirspeedMs45xx;
        public static ParachuteType Parachute;
        public static CircleType Circle;
        public static MotorType Motor;
        public static LaserAltType LaserAlt;
        public static Ads1115Type Ads1115;
        public static WpMovedType[] WpMoved = new WpMovedType[20];
        public static GpsType Gps;
        public static GpsSolType GpsSol;
        public static SvInfoType SvInfo;
        public static DataLinkReportType DataLinkReport;
        public static EstimatorType Estimator;
        public static AttitudeType Attitude;
        public static FbwStatusType FbwStatus;
        public static StateFilterStatusType StateFilterStatus;
        public static ImuGyroType ImuGyro;
        public static AdcGenericType AdcGeneric;
        public static BatType Bat;
        public static PprzModeType PprzMode;
        public static DesiredType Desired;
        public static PpmType Ppm;
        public static NavigationType Navigation;
        public static WpMovedLlaType[] WpMovedLla = new WpMovedLlaType[12];
        public static NavigationRefType NavigationRef;
        public static EnergyType Energy;
        public static I2cErrorsType I2cErrors;
        public static CalibrationType Calibration;
        public static AirspeedType Airspeed;
        public static float[] DlValue = new float[256];
        public static CommandsType Commands;
        public static AliveType Alive;
        public static List<ushort> Actuators = new List<ushort>();
        public static float linkQuality;
        public static float DownLinkRate;
        public static AirDataType AirData;
        public static HCTLType Hctl;        
        public static float AGL;
        public static double Pbearing;
    }
}
