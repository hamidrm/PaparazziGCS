using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaparazziGroundControlStation.Communication 
{
    class DataStream : Comm
    {
        public delegate void PacketReceived(byte AcId,MessagesDownID Mid,Byte[] Data,bool CheckSum);
        public bool TxIsBusyNow;
        static private Dictionary<MessagesDownID,PprzDownMsg> MsgDownLink = new Dictionary<MessagesDownID,PprzDownMsg>();
        static int recvState = 0;
        struct RecvPacketType
        {
            public byte len;
            public byte ac_id;
            public byte cmd;
            public List<byte> payload;
            public byte chk_a;
            public byte chk_b;
        }

        static RecvPacketType recvPacket;
        static int recvBytes = 0;
        static Main mainClass = null;
        public DataStream(Main _mainClass) : base(ReceivedData)
        {
            mainClass = _mainClass;
            // Register downlink messages
            MsgDownLink.Add(MessagesDownID.MSG_DL_AUTOPILOT_VERSION, (new MsgVersion(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_ADS1115, (new MsgAds1115(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_ADC_GENERIC, (new MsgAdcGeneric(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_ATTITUDE, (new MsgAttitude(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_BAT, (new MsgBat(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_DATALINK_REPORT, (new MsgDataLinkReport(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_DESIRED, (new MsgDesired(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_ESTIMATOR, (new MsgEstimator(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_FBW_STATUS, (new MsgFbwStatus(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_GPS, (new MsgGps(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_IMU_GYRO, (new MsgImuGyro(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_NAVIGATION, (new MsgNavigation(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_PPM, (new MsgPpm(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_PPRZ_MODE, (new MsgPprzMode(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_STATE_FILTER_STATUS, (new MsgStateFilterStatus(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_SVINFO, (new MsgSvInfo(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_WP_MOVED_LLA, (new MsgWpMovedLla(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_WP_MOVED, (new MsgWpMoved(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_NAVIGATION_REF, (new MsgNavigationRef(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_CALIBRATION, (new MsgCalibration(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_DL_VALUE, (new MsgDlValue(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_COMMANDS, (new MsgCommands(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_ALIVE, (new MsgAlive(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_I2C_ERRORS, (new MsgI2cErrors(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_AIRSPEED, (new MsgAirspeed(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_ENERGY, (new MsgEnergy(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_ACTUATORS, (new MsgActuators(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_LASER_ALT, (new MsgLaserAlt(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_GPS_SOL, (new MsgGpsSol(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_CIRCLE, (new MsgCircle(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_MOTOR, (new MsgMotor(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_PARACHUTE, (new MsgParachute(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_AIRSPEED_MS45XX, (new MsgAirspeedMs45xx(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_LOGGER_STATUS, (new MsgLoggerStatus(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_AIR_DATA, (new MsgAirData(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_PONG, (new MsgPong(_mainClass)));
            MsgDownLink.Add(MessagesDownID.MSG_DL_H_CTL_A, (new MsgHctl(_mainClass)));
            recvPacket.payload = new List<byte>();
            TxIsBusyNow = false;
        }
        public enum MessagesUpID
        {
            MSG_UL_ACINFO = 1,
            MSG_UL_MOVE_WP = 2,
            MSG_UL_WIND_INFO = 3,
            MSG_UL_SETTING = 4,
            MSG_UL_BLOCK = 5,
            MSG_UL_HITL_UBX = 6,
            MSG_UL_HITL_INFRARED = 7,
            MSG_UL_PING = 8,
            MSG_UL_FORMATION_SLOT = 9,
            MSG_UL_FORMATION_STATUS = 10,
            MSG_UL_JOYSTICK_RAW = 11,
            MSG_UL_COMMANDS_RAW = 12,
            MSG_UL_DGPS_RAW = 13,
            MSG_UL_GET_SETTING = 16,
            MSG_UL_TCAS_RESOLVE = 17,
            MSG_UL_MISSION_GOTO_WP = 20,
            MSG_UL_MISSION_GOTO_WP_LLA = 21,
            MSG_UL_MISSION_CIRCLE = 22,
            MSG_UL_MISSION_CIRCLE_LLA = 23,
            MSG_UL_MISSION_SEGMENT = 24,
            MSG_UL_MISSION_SEGMENT_LLA = 25,
            MSG_UL_MISSION_PATH = 26,
            MSG_UL_MISSION_PATH_LLA = 27,
            MSG_UL_MISSION_SURVEY = 28,
            MSG_UL_MISSION_SURVEY_LLA = 29,
            MSG_UL_GOTO_MISSION = 30,
            MSG_UL_NEXT_MISSION = 31,
            MSG_UL_END_MISSION = 32,
            MSG_UL_GUIDED_SETPOINT_NED = 40,
            MSG_UL_WINDTURBINE_STATUS = 50,
            MSG_UL_RC_3CH = 51,
            MSG_UL_RC_4CH = 52,
            MSG_UL_REMOTE_GPS_SMALL = 54,
            MSG_UL_REMOTE_GPS = 55,
            MSG_UL_KITE_COMMAND = 96,
            MSG_UL_PAYLOAD_COMMAND = 97,
            MSG_UL_SET_ACTUATOR = 100,
            MSG_UL_CSC_SERVO_CMD = 101,
            MSG_UL_BOOZ2_FMS_COMMAND = 149,
            MSG_UL_BOOZ_NAV_STICK = 150,
            MSG_UL_EXTERNAL_FILTER_SOLUTION = 151,
            MSG_UL_ROTORCRAFT_CAM_STICK = 152,
            MSG_UL_GPS_INJECT = 153,
            MSG_UL_VIDEO_ROI = 155,
            MSG_UL_CS = 159,
            MSG_UL_datalink_NB = 44
        }

        public enum MessagesDownID {
            MSG_DL_AUTOPILOT_VERSION = 1,
            MSG_DL_ALIVE = 2,
            MSG_DL_PONG = 3,
            MSG_DL_TAKEOFF = 4,
            MSG_DL_ARDRONE_NAVDATA = 5,
            MSG_DL_ATTITUDE = 6,
            MSG_DL_IR_SENSORS = 7,
            MSG_DL_GPS = 8,
            MSG_DL_NAVIGATION_REF = 9,
            MSG_DL_NAVIGATION = 10,
            MSG_DL_PPRZ_MODE = 11,
            MSG_DL_BAT = 12,
            MSG_DL_DEBUG_MCU_LINK = 13,
            MSG_DL_CALIBRATION = 14,
            MSG_DL_SETTINGS = 15,
            MSG_DL_DESIRED = 16,
            MSG_DL_GPS_SOL = 17,
            MSG_DL_ADC_GENERIC = 18,
            MSG_DL_TEST_FORMAT = 19,
            MSG_DL_CAM = 20,
            MSG_DL_CIRCLE = 21,
            MSG_DL_SEGMENT = 22,
            MSG_DL_VECTORNAV_INFO = 23,
            MSG_DL_ADS1115 = 24,
            MSG_DL_SVINFO = 25,
            MSG_DL_DEBUG = 26,
            MSG_DL_SURVEY = 27,
            MSG_DL_RSSI = 28,
            MSG_DL_RANGEFINDER = 29,
            MSG_DL_DATALINK_REPORT = 30,
            MSG_DL_DL_VALUE = 31,
            MSG_DL_MARK = 32,
            MSG_DL_SYS_MON = 33,
            MSG_DL_MOTOR = 34,
            MSG_DL_WP_MOVED = 35,
            MSG_DL_MKK = 36,
            MSG_DL_ENERGY = 37,
            MSG_DL_BARO_BMP85_CALIB = 38,
            MSG_DL_BARO_BMP85 = 39,
            MSG_DL_SPEED_LOOP = 40,
            MSG_DL_ALT_KALMAN = 41,
            MSG_DL_ESTIMATOR = 42,
            MSG_DL_TUNE_ROLL = 43,
            MSG_DL_BARO_MS5534A = 44,
            MSG_DL_PRESSURE = 45,
            MSG_DL_BARO_WORDS = 46,
            MSG_DL_WP_MOVED_LLA = 47,
            MSG_DL_CHRONO = 48,
            MSG_DL_WP_MOVED_ENU = 49,
            MSG_DL_WINDTURBINE_STATUS_ = 50,
            MSG_DL_RC_3CH_ = 51,
            MSG_DL_MPPT = 52,
            MSG_DL_DEBUG_IR_I2C = 53,
            MSG_DL_AIRSPEED = 54,
            MSG_DL_XSENS = 55,
            MSG_DL_BARO_ETS = 56,
            MSG_DL_AIRSPEED_ETS = 57,
            MSG_DL_PBN = 58,
            MSG_DL_GPS_LLA = 59,
            MSG_DL_H_CTL_A = 60,
            MSG_DL_TURB_PRESSURE_RAW = 61,
            MSG_DL_TURB_PRESSURE_VOLTAGE = 62,
            MSG_DL_CAM_POINT = 63,
            MSG_DL_DC_INFO = 64,
            MSG_DL_AMSYS_BARO = 65,
            MSG_DL_AMSYS_AIRSPEED = 66,
            MSG_DL_FLIGHT_BENCHMARK = 67,
            MSG_DL_MPL3115_BARO = 68,
            MSG_DL_AOA = 69,
            MSG_DL_XTEND_RSSI = 70,
            MSG_DL_SUPERBITRF = 72,
            MSG_DL_GX3_INFO = 73,
            MSG_DL_EXPLAIN = 74,
            MSG_DL_VIDEO_TELEMETRY = 75,
            MSG_DL_VF_UPDATE = 76,
            MSG_DL_VF_PREDICT = 77,
            MSG_DL_INV_FILTER = 78,
            MSG_DL_MISSION_STATUS = 79,
            MSG_DL_CROSS_TRACK_ERROR = 80,
            MSG_DL_GENERIC_COM = 81,
            MSG_DL_FORMATION_SLOT_TM = 82,
            MSG_DL_FORMATION_STATUS_TM = 83,
            MSG_DL_BMP_STATUS = 84,
            MSG_DL_MLX_STATUS = 85,
            MSG_DL_TMP_STATUS = 86,
            MSG_DL_WIND_INFO_RET = 87,
            MSG_DL_SCP_STATUS = 88,
            MSG_DL_SHT_STATUS = 89,
            MSG_DL_ENOSE_STATUS = 90,
            MSG_DL_DPICCO_STATUS = 91,
            MSG_DL_ANTENNA_DEBUG = 92,
            MSG_DL_ANTENNA_STATUS = 93,
            MSG_DL_MOTOR_BENCH_STATUS = 94,
            MSG_DL_MOTOR_BENCH_STATIC = 95,
            MSG_DL_HIH_STATUS = 96,
            MSG_DL_TEMT_STATUS = 97,
            MSG_DL_GP2Y_STATUS = 98,
            MSG_DL_SHT_I2C_SERIAL = 99,
            MSG_DL_PPM = 100,
            MSG_DL_RC = 101,
            MSG_DL_COMMANDS = 102,
            MSG_DL_FBW_STATUS = 103,
            MSG_DL_ADC = 104,
            MSG_DL_ACTUATORS = 105,
            MSG_DL_BLUEGIGA = 106,
            MSG_DL_PARACHUTE = 107,
            MSG_DL_PIKSI_HEARTBEAT = 108,
            MSG_DL_MULTIGAZE_METERS = 109,
            MSG_DL_DC_SHOT = 110,
            MSG_DL_TEST_BOARD_RESULTS = 111,
            MSG_DL_LOGGER_STATUS = 112,
            MSG_DL_MLX_SERIAL = 113,
            MSG_DL_PAYLOAD = 114,
            MSG_DL_HTM_STATUS = 115,
            MSG_DL_BARO_MS5611 = 116,
            MSG_DL_MS5611_COEFF = 117,
            MSG_DL_ATMOSPHERE_CHARGE = 118,
            MSG_DL_SOLAR_RADIATION = 119,
            MSG_DL_TCAS_TA = 120,
            MSG_DL_TCAS_RA = 121,
            MSG_DL_TCAS_RESOLVED = 122,
            MSG_DL_TCAS_DEBUG = 123,
            MSG_DL_POTENTIAL = 124,
            MSG_DL_VERTICAL_ENERGY = 125,
            MSG_DL_TEMP_TCOUPLE = 126,
            MSG_DL_SHT_I2C_STATUS = 127,
            MSG_DL_CAMERA_SNAPSHOT = 128,
            MSG_DL_TIMESTAMP = 129,
            MSG_DL_STAB_ATTITUDE_FLOAT = 130,
            MSG_DL_IMU_GYRO_SCALED = 131,
            MSG_DL_IMU_ACCEL_SCALED = 132,
            MSG_DL_IMU_MAG_SCALED = 133,
            MSG_DL_FILTER = 134,
            MSG_DL_FILTER2 = 135,
            MSG_DL_RATE_LOOP = 136,
            MSG_DL_FILTER_ALIGNER = 137,
            MSG_DL_AIRSPEED_MS45XX = 138,
            MSG_DL_FILTER_COR = 139,
            MSG_DL_STAB_ATTITUDE_INT = 140,
            MSG_DL_STAB_ATTITUDE_REF_INT = 141,
            MSG_DL_STAB_ATTITUDE_REF_FLOAT = 142,
            MSG_DL_ROTORCRAFT_CMD = 143,
            MSG_DL_GUIDANCE_H_INT = 144,
            MSG_DL_VERT_LOOP = 145,
            MSG_DL_HOVER_LOOP = 146,
            MSG_DL_ROTORCRAFT_FP = 147,
            MSG_DL_TEMP_ADC = 148,
            MSG_DL_GUIDANCE_H_REF_INT = 149,
            MSG_DL_ROTORCRAFT_TUNE_HOVER = 150,
            MSG_DL_INS_Z = 151,
            MSG_DL_PCAP01_STATUS = 152,
            MSG_DL_GEIGER_COUNTER = 153,
            MSG_DL_INS_REF = 154,
            MSG_DL_GPS_INT = 155,
            MSG_DL_AHRS_EULER_INT = 156,
            MSG_DL_AHRS_QUAT_INT = 157,
            MSG_DL_AHRS_RMAT_INT = 158,
            MSG_DL_ROTORCRAFT_NAV_STATUS = 159,
            MSG_DL_ROTORCRAFT_RADIO_CONTROL = 160,
            MSG_DL_VFF_EXTENDED = 161,
            MSG_DL_VFF = 162,
            MSG_DL_GEO_MAG = 163,
            MSG_DL_HFF = 164,
            MSG_DL_HFF_DBG = 165,
            MSG_DL_HFF_GPS = 166,
            MSG_DL_INS_SONAR = 167,
            MSG_DL_ROTORCRAFT_CAM = 168,
            MSG_DL_AHRS_REF_QUAT = 169,
            MSG_DL_EKF7_XHAT = 170,
            MSG_DL_EKF7_Y = 171,
            MSG_DL_EKF7_P_DIAG = 172,
            MSG_DL_AHRS_EULER = 173,
            MSG_DL_AHRS_MEASUREMENT_EULER = 174,
            MSG_DL_WT = 175,
            MSG_DL_CSC_CAN_DEBUG = 176,
            MSG_DL_CSC_CAN_MSG = 177,
            MSG_DL_AHRS_GYRO_BIAS_INT = 178,
            MSG_DL_AEROPROBE = 179,
            MSG_DL_FMS_TIME = 180,
            MSG_DL_LOADCELL = 181,
            MSG_DL_FLA_DEBUG = 182,
            MSG_DL_BLMC_FAULT_STATUS = 183,
            MSG_DL_BLMC_SPEEDS = 184,
            MSG_DL_AHRS_DEBUG_QUAT = 185,
            MSG_DL_BLMC_BUSVOLTS = 186,
            MSG_DL_SYSTEM_STATUS = 187,
            MSG_DL_DYNAMIXEL = 188,
            MSG_DL_RMAT_DEBUG = 189,
            MSG_DL_SIMPLE_COMMANDS = 190,
            MSG_DL_VANE_SENSOR = 191,
            MSG_DL_CONTROLLER_GAINS = 192,
            MSG_DL_AHRS_LKF = 193,
            MSG_DL_AHRS_LKF_DEBUG = 194,
            MSG_DL_AHRS_LKF_ACC_DBG = 195,
            MSG_DL_AHRS_LKF_MAG_DBG = 196,
            MSG_DL_NPS_SENSORS_SCALED = 197,
            MSG_DL_INS = 198,
            MSG_DL_GPS_ERROR = 199,
            MSG_DL_IMU_GYRO = 200,
            MSG_DL_IMU_MAG = 201,
            MSG_DL_IMU_ACCEL = 202,
            MSG_DL_IMU_GYRO_RAW = 203,
            MSG_DL_IMU_ACCEL_RAW = 204,
            MSG_DL_IMU_MAG_RAW = 205,
            MSG_DL_IMU_MAG_SETTINGS = 206,
            MSG_DL_IMU_MAG_CURRENT_CALIBRATION = 207,
            MSG_DL_UART_ERRORS = 208,
            MSG_DL_IMU_GYRO_LP = 209,
            MSG_DL_IMU_PRESSURE = 210,
            MSG_DL_IMU_HS_GYRO = 211,
            MSG_DL_TEST_PASSTHROUGH_STATUS = 212,
            MSG_DL_TUNE_VERT = 213,
            MSG_DL_MF_DAQ_STATE = 214,
            MSG_DL_INFO_MSG = 215,
            MSG_DL_STAB_ATTITUDE_INDI = 216,
            MSG_DL_GPS_RTK = 217,
            MSG_DL_BEBOP_ACTUATORS = 218,
            MSG_DL_WEATHER = 219,
            MSG_DL_IMU_TURNTABLE = 220,
            MSG_DL_BARO_RAW = 221,
            MSG_DL_AIR_DATA = 222,
            MSG_DL_AMSL = 223,
            MSG_DL_DIVERGENCE = 224,
            MSG_DL_VIDEO_SYNC = 225,
            MSG_DL_PERIODIC_TELEMETRY_ERR = 226,
            MSG_DL_TIME = 227,
            MSG_DL_OPTIC_FLOW_EST = 228,
            MSG_DL_STEREO_IMG = 229,
            MSG_DL_GPS_RXMRTCM = 230,
            MSG_DL_ROTORCRAFT_STATUS = 231,
            MSG_DL_STATE_FILTER_STATUS = 232,
            MSG_DL_PX4FLOW = 233,
            MSG_DL_OPTICFLOW = 234,
            MSG_DL_VISUALTARGET = 235,
            MSG_DL_SONAR = 236,
            MSG_DL_PAYLOAD_FLOAT = 237,
            MSG_DL_NPS_POS_LLH = 238,
            MSG_DL_NPS_RPMS = 239,
            MSG_DL_NPS_SPEED_POS = 240,
            MSG_DL_NPS_RATE_ATTITUDE = 241,
            MSG_DL_NPS_GYRO_BIAS = 242,
            MSG_DL_NPS_RANGE_METER = 243,
            MSG_DL_NPS_WIND = 244,
            MSG_DL_ESC = 245,
            MSG_DL_RTOS_MON = 246,
            MSG_DL_PPRZ_DEBUG = 247,
            MSG_DL_NPS_ACCEL_LTP = 248,
            MSG_DL_LOOSE_INS_GPS = 249,
            MSG_DL_AFL_COEFFS = 250,
            MSG_DL_PWM_INPUT = 251,
            MSG_DL_LASER_ALT = 252,
            MSG_DL_I2C_ERRORS = 253,
            MSG_DL_RDYB_TRAJECTORY = 254,
            MSG_DL_HENRY_GNSS = 255
        }

        public enum ApModeTypes
        {
            MANUAL, AUTO1, AUTO2, HOME, NOGPS, FAILSAFE
        };

        String[] ApModeTypesString  = {"MANUAL","AUTO1", "AUTO2", "HOME", "NOGPS", "FAILSAFE"};
        String[] messagesString = {  "AUTOPILOT_VERSION", // = 1
                                     "ALIVE", // = 2
                                     "PONG", // = 3
                                     "TAKEOFF", // = 4
                                     "ARDRONE_NAVDATA", // = 5
                                     "ATTITUDE", // = 6
                                     "IR_SENSORS", // = 7
                                     "GPS", // = 8
                                     "NAVIGATION_REF", // = 9
                                     "NAVIGATION", // = 10
                                     "PPRZ_MODE", // = 11
                                     "BAT", // = 12
                                     "DEBUG_MCU_LINK", // = 13
                                     "CALIBRATION", // = 14
                                     "SETTINGS", // = 15
                                     "DESIRED", // = 16
                                     "GPS_SOL", // = 17
                                     "ADC_GENERIC", // = 18
                                     "TEST_FORMAT", // = 19
                                     "CAM", // = 20
                                     "CIRCLE", // = 21
                                     "SEGMENT", // = 22
                                     "VECTORNAV_INFO", // = 23
                                     "ADS1115", // = 24
                                     "SVINFO", // = 25
                                     "DEBUG", // = 26
                                     "SURVEY", // = 27
                                     "RSSI", // = 28
                                     "RANGEFINDER", // = 29
                                     "DATALINK_REPORT", // = 30
                                     "DL_VALUE", // = 31
                                     "MARK", // = 32
                                     "SYS_MON", // = 33
                                     "MOTOR", // = 34
                                     "WP_MOVED", // = 35
                                     "MKK", // = 36
                                     "ENERGY", // = 37
                                     "BARO_BMP85_CALIB", // = 38
                                     "BARO_BMP85", // = 39
                                     "SPEED_LOOP", // = 40
                                     "ALT_KALMAN", // = 41
                                     "ESTIMATOR", // = 42
                                     "TUNE_ROLL", // = 43
                                     "BARO_MS5534A", // = 44
                                     "PRESSURE", // = 45
                                     "BARO_WORDS", // = 46
                                     "WP_MOVED_LLA", // = 47
                                     "CHRONO", // = 48
                                     "WP_MOVED_ENU", // = 49
                                     "WINDTURBINE_STATUS_", // = 50
                                     "RC_3CH_", // = 51
                                     "MPPT", // = 52
                                     "DEBUG_IR_I2C", // = 53
                                     "AIRSPEED", // = 54
                                     "XSENS", // = 55
                                     "BARO_ETS", // = 56
                                     "AIRSPEED_ETS", // = 57
                                     "PBN", // = 58
                                     "GPS_LLA", // = 59
                                     "H_CTL_A", // = 60
                                     "TURB_PRESSURE_RAW", // = 61
                                     "TURB_PRESSURE_VOLTAGE", // = 62
                                     "CAM_POINT", // = 63
                                     "DC_INFO", // = 64
                                     "AMSYS_BARO", // = 65
                                     "AMSYS_AIRSPEED", // = 66
                                     "FLIGHT_BENCHMARK", // = 67
                                     "MPL3115_BARO", // = 68
                                     "AOA", // = 69
                                     "XTEND_RSSI", // = 70
                                     "", // = 71
                                     "SUPERBITRF", // = 72
                                     "GX3_INFO", // = 73
                                     "EXPLAIN", // = 74
                                     "VIDEO_TELEMETRY", // = 75
                                     "VF_UPDATE", // = 76
                                     "VF_PREDICT", // = 77
                                     "INV_FILTER", // = 78
                                     "MISSION_STATUS", // = 79
                                     "CROSS_TRACK_ERROR", // = 80
                                     "GENERIC_COM", // = 81
                                     "FORMATION_SLOT_TM", // = 82
                                     "FORMATION_STATUS_TM", // = 83
                                     "BMP_STATUS", // = 84
                                     "MLX_STATUS", // = 85
                                     "TMP_STATUS", // = 86
                                     "WIND_INFO_RET", // = 87
                                     "SCP_STATUS", // = 88
                                     "SHT_STATUS", // = 89
                                     "ENOSE_STATUS", // = 90
                                     "DPICCO_STATUS", // = 91
                                     "ANTENNA_DEBUG", // = 92
                                     "ANTENNA_STATUS", // = 93
                                     "MOTOR_BENCH_STATUS", // = 94
                                     "MOTOR_BENCH_STATIC", // = 95
                                     "HIH_STATUS", // = 96
                                     "TEMT_STATUS", // = 97
                                     "GP2Y_STATUS", // = 98
                                     "SHT_I2C_SERIAL", // = 99
                                     "PPM", // = 100
                                     "RC", // = 101
                                     "COMMANDS", // = 102
                                     "FBW_STATUS", // = 103
                                     "ADC", // = 104
                                     "ACTUATORS", // = 105
                                     "BLUEGIGA", // = 106
                                     "", // = 107
                                     "PIKSI_HEARTBEAT", // = 108
                                     "MULTIGAZE_METERS", // = 109
                                     "DC_SHOT", // = 110
                                     "TEST_BOARD_RESULTS", // = 111
                                     "", // = 112
                                     "MLX_SERIAL", // = 113
                                     "PAYLOAD", // = 114
                                     "HTM_STATUS", // = 115
                                     "BARO_MS5611", // = 116
                                     "MS5611_COEFF", // = 117
                                     "ATMOSPHERE_CHARGE", // = 118
                                     "SOLAR_RADIATION", // = 119
                                     "TCAS_TA", // = 120
                                     "TCAS_RA", // = 121
                                     "TCAS_RESOLVED", // = 122
                                     "TCAS_DEBUG", // = 123
                                     "POTENTIAL", // = 124
                                     "VERTICAL_ENERGY", // = 125
                                     "TEMP_TCOUPLE", // = 126
                                     "SHT_I2C_STATUS", // = 127
                                     "CAMERA_SNAPSHOT", // = 128
                                     "TIMESTAMP", // = 129
                                     "STAB_ATTITUDE_FLOAT", // = 130
                                     "IMU_GYRO_SCALED", // = 131
                                     "IMU_ACCEL_SCALED", // = 132
                                     "IMU_MAG_SCALED", // = 133
                                     "FILTER", // = 134
                                     "FILTER2", // = 135
                                     "RATE_LOOP", // = 136
                                     "FILTER_ALIGNER", // = 137
                                     "AIRSPEED_MS45XX", // = 138
                                     "FILTER_COR", // = 139
                                     "STAB_ATTITUDE_INT", // = 140
                                     "STAB_ATTITUDE_REF_INT", // = 141
                                     "STAB_ATTITUDE_REF_FLOAT", // = 142
                                     "ROTORCRAFT_CMD", // = 143
                                     "GUIDANCE_H_INT", // = 144
                                     "VERT_LOOP", // = 145
                                     "HOVER_LOOP", // = 146
                                     "ROTORCRAFT_FP", // = 147
                                     "TEMP_ADC", // = 148
                                     "GUIDANCE_H_REF_INT", // = 149
                                     "ROTORCRAFT_TUNE_HOVER", // = 150
                                     "INS_Z", // = 151
                                     "PCAP01_STATUS", // = 152
                                     "GEIGER_COUNTER", // = 153
                                     "INS_REF", // = 154
                                     "GPS_INT", // = 155
                                     "AHRS_EULER_INT", // = 156
                                     "AHRS_QUAT_INT", // = 157
                                     "AHRS_RMAT_INT", // = 158
                                     "ROTORCRAFT_NAV_STATUS", // = 159
                                     "ROTORCRAFT_RADIO_CONTROL", // = 160
                                     "VFF_EXTENDED", // = 161
                                     "VFF", // = 162
                                     "GEO_MAG", // = 163
                                     "HFF", // = 164
                                     "HFF_DBG", // = 165
                                     "HFF_GPS", // = 166
                                     "INS_SONAR", // = 167
                                     "ROTORCRAFT_CAM", // = 168
                                     "AHRS_REF_QUAT", // = 169
                                     "EKF7_XHAT", // = 170
                                     "EKF7_Y", // = 171
                                     "EKF7_P_DIAG", // = 172
                                     "AHRS_EULER", // = 173
                                     "AHRS_MEASUREMENT_EULER", // = 174
                                     "WT", // = 175
                                     "CSC_CAN_DEBUG", // = 176
                                     "CSC_CAN_MSG", // = 177
                                     "AHRS_GYRO_BIAS_INT", // = 178
                                     "AEROPROBE", // = 179
                                     "FMS_TIME", // = 180
                                     "LOADCELL", // = 181
                                     "FLA_DEBUG", // = 182
                                     "BLMC_FAULT_STATUS", // = 183
                                     "BLMC_SPEEDS", // = 184
                                     "AHRS_DEBUG_QUAT", // = 185
                                     "BLMC_BUSVOLTS", // = 186
                                     "SYSTEM_STATUS", // = 187
                                     "DYNAMIXEL", // = 188
                                     "RMAT_DEBUG", // = 189
                                     "SIMPLE_COMMANDS", // = 190
                                     "VANE_SENSOR", // = 191
                                     "CONTROLLER_GAINS", // = 192
                                     "AHRS_LKF", // = 193
                                     "AHRS_LKF_DEBUG", // = 194
                                     "AHRS_LKF_ACC_DBG", // = 195
                                     "AHRS_LKF_MAG_DBG", // = 196
                                     "NPS_SENSORS_SCALED", // = 197
                                     "INS", // = 198
                                     "GPS_ERROR", // = 199
                                     "IMU_GYRO", // = 200
                                     "IMU_MAG", // = 201
                                     "IMU_ACCEL", // = 202
                                     "IMU_GYRO_RAW", // = 203
                                     "IMU_ACCEL_RAW", // = 204
                                     "IMU_MAG_RAW", // = 205
                                     "IMU_MAG_SETTINGS", // = 206
                                     "IMU_MAG_CURRENT_CALIBRATION", // = 207
                                     "UART_ERRORS", // = 208
                                     "IMU_GYRO_LP", // = 209
                                     "IMU_PRESSURE", // = 210
                                     "IMU_HS_GYRO", // = 211
                                     "TEST_PASSTHROUGH_STATUS", // = 212
                                     "TUNE_VERT", // = 213
                                     "MF_DAQ_STATE", // = 214
                                     "INFO_MSG", // = 215
                                     "STAB_ATTITUDE_INDI", // = 216
                                     "", // = 217
                                     "BEBOP_ACTUATORS", // = 218
                                     "WEATHER", // = 219
                                     "IMU_TURNTABLE", // = 220
                                     "BARO_RAW", // = 221
                                     "AIR_DATA", // = 222
                                     "AMSL", // = 223
                                     "DIVERGENCE", // = 224
                                     "VIDEO_SYNC", // = 225
                                     "PERIODIC_TELEMETRY_ERR", // = 226
                                     "TIME", // = 227
                                     "OPTIC_FLOW_EST", // = 228
                                     "STEREO_IMG", // = 229
                                     "", // = 230
                                     "ROTORCRAFT_STATUS", // = 231
                                     "STATE_FILTER_STATUS", // = 232
                                     "PX4FLOW", // = 233
                                     "OPTICFLOW", // = 234
                                     "VISUALTARGET", // = 235
                                     "SONAR", // = 236
                                     "PAYLOAD_FLOAT", // = 237
                                     "NPS_POS_LLH", // = 238
                                     "NPS_RPMS", // = 239
                                     "NPS_SPEED_POS", // = 240
                                     "NPS_RATE_ATTITUDE", // = 241
                                     "NPS_GYRO_BIAS", // = 242
                                     "NPS_RANGE_METER", // = 243
                                     "NPS_WIND", // = 244
                                     "ESC", // = 245
                                     "RTOS_MON", // = 246
                                     "PPRZ_DEBUG", // = 247
                                     "NPS_ACCEL_LTP", // = 248
                                     "LOOSE_INS_GPS", // = 249
                                     "AFL_COEFFS", // = 250
                                     "PWM_INPUT", // = 251
                                     "LASER_ALT", // = 252
                                     "I2C_ERRORS", // = 253
                                     "RDYB_TRAJECTORY", // = 254
                                     "HENRY_GNSS" // = 255
                                  };

        byte[] FP_MAX_STAGES = {
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
        static public void CalculateCheckSum(byte[] buffer, ref byte chk_a, ref byte chk_b)
        {
            chk_a = 0;
            chk_b = 0;
            for(int i=0;i<buffer.Length;i++){
                chk_a += buffer[i];
                chk_b += chk_a;
            }
        }

        public void MoveWp(byte index,double lat,double lon,double alt)
        {
            if (TxIsBusyNow)
                return;
            TxIsBusyNow = true;
            List<byte> data = new List<byte>();
            int _lat = Convert.ToInt32(lat.ToString("0.0000000000").Remove(lat.ToString().IndexOf('.') + 8).Replace(".", ""));
            int _lon = Convert.ToInt32(lon.ToString("0.0000000000").Remove(lon.ToString().IndexOf('.') + 8).Replace(".", ""));
            int _alt = Convert.ToInt32(alt);

            data.Add(index);
            data.Add(Parameters.aircraftId);
            data.AddRange(System.BitConverter.GetBytes(_lat));
            data.AddRange(System.BitConverter.GetBytes(_lon));
            data.AddRange(System.BitConverter.GetBytes(_alt));

            SendCommand(DataStream.MessagesUpID.MSG_UL_MOVE_WP, data.ToArray());
            TxIsBusyNow = false;
        }
        public void SetSetting(Setting.PPrzSettingList setting, float value)
        {
            if (TxIsBusyNow)
                return;
            TxIsBusyNow = true;
            List<byte> data = new List<byte>();
            data.Add((byte)(setting));
            data.Add(Parameters.aircraftId);
            data.AddRange(System.BitConverter.GetBytes(value));
            SendCommand(DataStream.MessagesUpID.MSG_UL_SETTING, data.ToArray());
            TxIsBusyNow = false;
        }
        public void GetSetting(Setting.PPrzSettingList setting)
        {
            if (TxIsBusyNow)
                return;
            TxIsBusyNow = true;
            List<byte> data = new List<byte>();
            data.Add((byte)(setting));
            data.Add(Parameters.aircraftId);
            SendCommand(DataStream.MessagesUpID.MSG_UL_GET_SETTING, data.ToArray());
            TxIsBusyNow = false;
        }
        private void Send(byte[] data)
        {

            byte chk_a=0, chk_b=0;
            List<byte> buffer_list = new List<byte>();
            byte StartSign = 0x99;

            buffer_list.Add(StartSign);
            buffer_list.Add((byte)(data.Length + 4));
            buffer_list.AddRange(data);
            CalculateCheckSum(buffer_list.ToArray().Skip(1).ToArray(), ref chk_a, ref chk_b);
            buffer_list.Add(chk_a);
            buffer_list.Add(chk_b);
            SendByte(buffer_list.ToArray());
        }
        public bool SendPing()
        {
            if (TxIsBusyNow)
                return false;
            TxIsBusyNow = true;
            SendCommand(DataStream.MessagesUpID.MSG_UL_PING, null);
            TxIsBusyNow = false;
            return true;
        }
        public void SendCommand(MessagesUpID cmd,byte [] payload){
            byte gcs_ac_id = 0;
            if(!IsOpen()){
                MessageBox.Show("You are not connected to any A/C.","Error");
                return;
            }
            List<byte> data = new List<byte>();
            data.Add(gcs_ac_id);
            data.Add((byte)cmd);
            if (payload != null)
                data.AddRange(payload);
            Send(data.ToArray());
        }
        public void SetCurrBlock(byte blockId)
        {
            //if (TxIsBusyNow)
            //    return;
            TxIsBusyNow = true;
            SendCommand(MessagesUpID.MSG_UL_BLOCK, new byte[] { blockId, Parameters.aircraftId });
            TxIsBusyNow = false;
        }
        static public void MsgDownRecv(byte acId,MessagesDownID cmd,byte [] payload){
            try
            {
                if (MsgDownLink.ContainsKey(cmd))
                {
                    Parameters.uiUpdater.WaitOne();
                    MsgDownLink[cmd].DoCmd(acId, payload);
                    Parameters.uiUpdater.Release();
                }
                else
                {

                    //Error! Undefined command!
                }
                mainClass.BeginInvoke((ThreadStart)delegate()
                {
                    mainClass.MessageReceived((byte)cmd, payload);
                });
            }
            catch (Exception exc)
            {

            }
            finally
            {
                
            }
        }
        public void Dispose()
        {
            Disconnect();
        }
        private static void ReceivedData(byte data)
        {
                if (recvState == 0 && data == 0x99)
                    recvState = 1;
                else
                    switch (recvState)
                    {
                        case 1:
                            recvPacket.len = data;
                            recvState = 2;
                            break;
                        case 2:
                            recvPacket.ac_id = data;
                            recvState = 3;
                            break;
                        case 3:
                            recvPacket.cmd = data;
                            if (recvPacket.len == 6)
                                recvState = 5;
                            else
                                recvState = 4;
                            recvBytes = 0;
                            break;
                        case 4:
                            recvPacket.payload.Add(data);
                            recvBytes++;
                            if (recvBytes >= recvPacket.len - 6)
                                recvState = 5;
                            break;
                        case 5:
                            recvPacket.chk_a = data;
                            recvState = 6;
                            break;
                        case 6:
                            {
                                recvPacket.chk_b = data;
                                byte c_chk_a=0, c_chk_b=0;
                                List<byte> buffer=new List<byte>();
                                buffer.Add(recvPacket.len);
                                buffer.Add(recvPacket.ac_id);
                                buffer.Add(recvPacket.cmd);
                                buffer.AddRange(recvPacket.payload);
                                CalculateCheckSum(buffer.ToArray(), ref c_chk_a, ref c_chk_b);
                                if (recvPacket.chk_a == c_chk_a && recvPacket.chk_b == c_chk_b)
                                    MsgDownRecv(recvPacket.ac_id, (MessagesDownID)(recvPacket.cmd), recvPacket.payload.ToArray());
                                recvPacket.payload.Clear();
                                recvState = 0;
                            }
                            break;
                    }
            }
    }
}
