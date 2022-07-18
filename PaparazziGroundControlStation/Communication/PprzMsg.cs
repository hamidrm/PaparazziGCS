using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PaparazziGroundControlStation.Communication
{

    abstract class PprzDownMsg
    {
        public PprzDownMsg(Main _mainClass)
        {
            mainClass = _mainClass;
        }
        public abstract void DoCmd(byte acId,byte [] payload);

        public float GetFloat(byte[] rv)
        {
            float r = System.BitConverter.ToSingle(rv, offset);
            offset += 4;
            return r;
        }
        public int GetInt32(byte [] rv) {
            int r = System.BitConverter.ToInt32(rv, offset);
            offset += 4;
            return r;
        }
        public short GetInt16(byte[] rv)
        {
            short r = System.BitConverter.ToInt16(rv, offset);
            offset += 2;
            return r;
        }
        public uint GetUInt32(byte[] rv)
        {
            uint r = System.BitConverter.ToUInt32(rv, offset);
            offset += 4;
            return r;
        }
        public ushort GetUInt16(byte [] rv) {
            ushort r = System.BitConverter.ToUInt16(rv, offset);
            offset += 2;
            return r;
        }
        public int offset;
        public Main mainClass;
    }
    class MsgVersion : PprzDownMsg{

        public MsgVersion(Main _mainClass) : base(_mainClass){ }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.AutopilotVersion.version = GetUInt32(payload);
            byte patch = (byte)(Parameters.AutopilotVersion.version % 100);
            byte minor = (byte)((Parameters.AutopilotVersion.version / 100) % 100);
            byte major = (byte)((Parameters.AutopilotVersion.version / 10000) % 100);
            int len = payload[offset];
            offset++;
            Parameters.AutopilotVersion.desc = System.Text.Encoding.UTF8.GetString(payload.Skip(offset).Take(len).ToArray());
            Parameters.fwDesc = Parameters.AutopilotVersion.desc;
            Parameters.fwVersion = String.Format("{0}.{1}.{2}", major, minor, patch);
            Parameters.aircraftId = acId;
        }
    };

    class MsgAds1115 : PprzDownMsg{

        public MsgAds1115(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.Ads1115.ADC0 = GetFloat(payload);
            Parameters.Ads1115.ADC1 = GetFloat(payload);
            Parameters.Ads1115.ADC2 = GetFloat(payload);
            Parameters.Ads1115.ADC3 = GetFloat(payload);
            Parameters.aircraftId = acId;
        }
    };

    class MsgWpMoved : PprzDownMsg{

        public MsgWpMoved(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            double lat,lon;
            Util.UTMConvertor utmConv = new Util.UTMConvertor("WGS 84");
            offset = 0;
            int idx = payload[offset++];
            Parameters.WpMoved[idx].utm_east = GetFloat(payload);
            Parameters.WpMoved[idx].utm_north = GetFloat(payload);
            Parameters.WpMoved[idx].alt = GetFloat(payload);
            Parameters.WpMoved[idx].utm_zone = payload[offset++];
            Parameters.aircraftId = acId;
            Util.UTMConvertor.LatLng latLon = utmConv.convertUtmToLatLng((double)(Parameters.WpMoved[idx].utm_east), (double)(Parameters.WpMoved[idx].utm_north), Parameters.WpMoved[idx].utm_zone,"N");
            mainClass.SetWaypointsPos(latLon.Lat,latLon.Lng, Parameters.WpMoved[idx].alt, (byte)idx);
        }
    };

    class MsgGps : PprzDownMsg
    {

        public MsgGps(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId, byte[] payload)
        {
            offset = 0;
            Parameters.Gps.mode = payload[offset++];
            Parameters.Gps.utm_east = GetInt32(payload); 
            Parameters.Gps.utm_north = GetInt32(payload);
            Parameters.Gps.course = GetInt16(payload); 
            Parameters.Gps.alt = GetInt32(payload); ;
            Parameters.Gps.speed = GetInt16(payload); 
            Parameters.Gps.climb = GetInt16(payload); 
            Parameters.Gps.week = GetInt16(payload); 
            Parameters.Gps.itow = GetInt32(payload);             
            Parameters.Gps.utm_zone = payload[offset++];
            Parameters.Gps.gps_nb_err = payload[offset++];
            Parameters.aircraftId = acId;
            //gcsMgr->GetWindow()->UpdateGauges();
            //qDebug() << mode << "," << utm_east << "," << utm_north << "," << course << "," << alt << "," << speed << "," << climb << "," << week << "," << itow << "," << utm_zone << "," << gps_nb_err;
        }
    }
    class MsgLaserAlt : PprzDownMsg
    {

        public MsgLaserAlt(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId, byte[] payload)
        {
            offset = 0;
            Parameters.LaserAlt.alt = GetFloat(payload);
        }
    }

    class MsgCircle : PprzDownMsg
    {

        public MsgCircle(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId, byte[] payload)
        {
            offset = 0;
            Parameters.Circle.center_east = GetFloat(payload);
            Parameters.Circle.center_north = GetFloat(payload);
            Parameters.Circle.radius = GetFloat(payload);

        }
    }
    class MsgMotor : PprzDownMsg
    {

        public MsgMotor(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId, byte[] payload)
        {
            offset = 0;
            Parameters.Motor.rpm = GetUInt16(payload);
            Parameters.Motor.current = GetInt32(payload);
        }
    }
    class MsgParachute : PprzDownMsg
    {

        public MsgParachute(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId, byte[] payload)
        {
            offset = 0;
            Parameters.Parachute.trigger = payload[offset++];
        }
    }

    class MsgAirspeedMs45xx : PprzDownMsg
    {

        public MsgAirspeedMs45xx(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId, byte[] payload)
        {
            offset = 0;
            Parameters.AirspeedMs45xx.diffPress = GetFloat(payload);
            Parameters.AirspeedMs45xx.temperature = GetUInt16(payload);
            Parameters.AirspeedMs45xx.airspeed = GetFloat(payload);
        }
    }
    class MsgLoggerStatus : PprzDownMsg
    {

        public MsgLoggerStatus(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId, byte[] payload)
        {
            offset = 0;
            Parameters.LoggerStatus.status = payload[offset++];
            Parameters.LoggerStatus.errno = payload[offset++];
            Parameters.LoggerStatus.used = GetUInt32(payload);
        }
    }
    class MsgGpsSol : PprzDownMsg
    {

        public MsgGpsSol(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId, byte[] payload)
        {
            offset = 0;
            Parameters.GpsSol.Pacc = GetUInt32(payload);
            Parameters.GpsSol.Sacc = GetUInt32(payload);
            Parameters.GpsSol.PDOP = GetUInt16(payload);
            Parameters.GpsSol.numSV = payload[offset++];
        }
    }
    class MsgSvInfo : PprzDownMsg{

        public MsgSvInfo(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.SvInfo.chn = payload[offset++];
            Parameters.SvInfo.SVID = payload[offset++];
            Parameters.SvInfo.Flags = payload[offset++];
            Parameters.SvInfo.QI = payload[offset++];
            Parameters.SvInfo.CNO = payload[offset++];
            Parameters.SvInfo.Elev = payload[offset++];
            Parameters.SvInfo.Azim = GetInt16(payload);;
            Parameters.aircraftId = acId;
        }
    };

    class MsgDataLinkReport : PprzDownMsg{

        public MsgDataLinkReport(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.DataLinkReport.uplink_lost_time = GetUInt16(payload);;
            Parameters.DataLinkReport.uplink_nb_msgs = GetUInt16(payload);;
            Parameters.DataLinkReport.downlink_nb_msgs = GetUInt16(payload);;
            Parameters.DataLinkReport.downlink_rate = GetUInt16(payload);;
            Parameters.DataLinkReport.uplink_rate = GetUInt16(payload);;
            Parameters.DataLinkReport.downlink_ovrn = payload[offset++];
            Parameters.aircraftId = acId;
        }
    };

    class MsgEstimator : PprzDownMsg{

        public MsgEstimator(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.Estimator.z = GetFloat(payload);
            Parameters.Estimator.z_dot = GetFloat(payload);
            Parameters.aircraftId = acId;
        }
    };

    class MsgAirData : PprzDownMsg
    {

        public MsgAirData(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId, byte[] payload)
        {
            offset = 0;
            Parameters.AirData.pressure = GetFloat(payload);
            Parameters.AirData.diff_p = GetFloat(payload);
            Parameters.AirData.temp = GetFloat(payload);
            Parameters.AirData.qnh = GetFloat(payload);
            Parameters.AirData.amsl_baro = GetFloat(payload);
            Parameters.AirData.airspeed = GetFloat(payload);
            Parameters.AirData.tas = GetFloat(payload);
            Parameters.aircraftId = acId;
        }
    };
    class MsgAttitude : PprzDownMsg{

        public MsgAttitude(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.Attitude.phi = GetFloat(payload);
            Parameters.Attitude.psi = GetFloat(payload);
            Parameters.Attitude.theta = GetFloat(payload);
            Parameters.aircraftId = acId;
        }
    };

    class MsgFbwStatus : PprzDownMsg{

        public MsgFbwStatus(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.FbwStatus.rc_status = payload[offset++];
            Parameters.FbwStatus.frame_rate = payload[offset++];
            Parameters.FbwStatus.mode = payload[offset++];
            Parameters.FbwStatus.vsupply = GetUInt16(payload);;
            Parameters.FbwStatus.current = GetInt32(payload);;
            Parameters.aircraftId = acId;
        }
    };

    class MsgStateFilterStatus : PprzDownMsg{

        public MsgStateFilterStatus(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.StateFilterStatus.id = payload[offset++];
            Parameters.StateFilterStatus.state_filter_mode = payload[offset++];
            Parameters.StateFilterStatus.value = GetUInt16(payload);;
            Parameters.aircraftId = acId;
        }
    };

    class MsgImuGyro : PprzDownMsg{

        public MsgImuGyro(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.ImuGyro.gp = GetFloat(payload);
            Parameters.ImuGyro.gq = GetFloat(payload);
            Parameters.ImuGyro.gr = GetFloat(payload);
            Parameters.aircraftId = acId;
        }
    };

    class MsgAdcGeneric : PprzDownMsg{

        public MsgAdcGeneric(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.AdcGeneric.val1 = GetUInt16(payload);;
            Parameters.AdcGeneric.val2 = GetUInt16(payload);;
            Parameters.aircraftId = acId;
        }
    };

    class MsgBat : PprzDownMsg{

        public MsgBat(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.Bat.throttle = GetUInt16(payload);;
            Parameters.Bat.voltage = GetUInt16(payload);;
            Parameters.Bat.amps = GetInt16(payload);;
            Parameters.Bat.flight_time = GetUInt16(payload);;
            Parameters.Bat.kill_auto_throttle = payload[offset++];
            Parameters.Bat.block_time = GetUInt16(payload);;
            Parameters.Bat.stage_time = GetUInt16(payload);;
            Parameters.Bat.energy = GetInt16(payload);;
            Parameters.aircraftId = acId;
        }
    };

    class MsgPprzMode : PprzDownMsg{

        public MsgPprzMode(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.PprzMode.ap_mode = payload[offset++];
            Parameters.PprzMode.ap_gaz = payload[offset++];
            Parameters.PprzMode.ap_lateral = payload[offset++];
            Parameters.PprzMode.ap_horizontal = payload[offset++];
            Parameters.PprzMode.if_calib_mode = payload[offset++];
            Parameters.PprzMode.mcu1_status = payload[offset++];
            Parameters.aircraftId = acId;
        }
    };

    class MsgDesired : PprzDownMsg{

        public MsgDesired(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.Desired.roll = GetFloat(payload);
            Parameters.Desired.pitch = GetFloat(payload);
            Parameters.Desired.course = GetFloat(payload);
            Parameters.Desired.x = GetFloat(payload);
            Parameters.Desired.y = GetFloat(payload);
            Parameters.Desired.altitude = GetFloat(payload);
            Parameters.Desired.climb = GetFloat(payload);
            Parameters.Desired.airspeed = GetFloat(payload);
            Parameters.aircraftId = acId;
        }
    };

    class MsgPpm : PprzDownMsg{

        public MsgPpm(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.Ppm.ppm_rate = payload[offset++];
            Parameters.aircraftId = acId;
        }
    };

    class MsgNavigation : PprzDownMsg{

        public MsgNavigation(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.Navigation.cur_block = payload[offset++];
            Parameters.Navigation.cur_stage = payload[offset++];
            Parameters.Navigation.pos_x = GetFloat(payload);
            Parameters.Navigation.pos_y = GetFloat(payload);
            Parameters.Navigation.dist_wp = GetFloat(payload);
            Parameters.Navigation.dist_home = GetFloat(payload);
            Parameters.Navigation.circle_count = payload[offset++];
            Parameters.Navigation.oval_count = payload[offset++];
            Parameters.aircraftId = acId;

            //qDebug() << cur_block << "," << cur_stage << "," << pos_x << "," << pos_y << "," << dist_wp << "," << dist_home << "," << circle_count << "," << oval_count;
        }
    };

    class MsgPong : PprzDownMsg{

        public MsgPong(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.aircraftId = acId;
            mainClass.Pong();
        }
    };

    class MsgWpMovedLla : PprzDownMsg{

        public MsgWpMovedLla(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){}
    };

    class MsgNavigationRef : PprzDownMsg{

        public MsgNavigationRef(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.NavigationRef.utm_east = GetInt32(payload);;
            Parameters.NavigationRef.utm_north = GetInt32(payload);;
            Parameters.NavigationRef.utm_zone = payload[offset++];
            Parameters.NavigationRef.ground_alt = GetFloat(payload);
            Parameters.aircraftId = acId;
        }
    };

    class MsgEnergy : PprzDownMsg{

        public MsgEnergy(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.Energy.bat = GetFloat(payload);
            Parameters.Energy.amp = GetFloat(payload);
            Parameters.Energy.energy = GetUInt16(payload);;
            Parameters.Energy.power = GetFloat(payload);
            Parameters.aircraftId = acId;
        }
    };

    class MsgI2cErrors : PprzDownMsg{

        public MsgI2cErrors(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.I2cErrors.wd_reset_cnt = GetUInt16(payload);;
            Parameters.I2cErrors.queue_full_cnt = GetUInt16(payload);;
            Parameters.I2cErrors.acknowledge_failure_cnt = GetUInt16(payload);;
            Parameters.I2cErrors.misplaced_start_or_stop_cnt = GetUInt16(payload);;
            Parameters.I2cErrors.arbitration_lost_cnt = GetUInt16(payload);;
            Parameters.I2cErrors.overrun_or_underrun_cnt = GetUInt16(payload);;
            Parameters.I2cErrors.pec_error_in_reception_cnt = GetUInt16(payload);;
            Parameters.I2cErrors.timeout_or_tlow_error_cnt = GetUInt16(payload);;
            Parameters.I2cErrors.smbus_alert_cnt = GetUInt16(payload);;
            Parameters.I2cErrors.unexpected_event_cnt = GetUInt16(payload);;
            Parameters.I2cErrors.last_unexpected_event = GetUInt32(payload);;
            Parameters.I2cErrors.bus_number = payload[offset++];
            Parameters.aircraftId = acId;

        }
    };

    class MsgCalibration : PprzDownMsg{

        public MsgCalibration(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.Calibration.climb_sum_err = GetFloat(payload);
            Parameters.Calibration.climb_gaz_submode = payload[offset++];
            Parameters.aircraftId = acId;

        }
    };

    class MsgAirspeed : PprzDownMsg{

        public MsgAirspeed(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            Parameters.Airspeed.airspeed = GetFloat(payload);
            Parameters.Airspeed.airspeed_sp = GetFloat(payload);
            Parameters.Airspeed.airspeed_cnt = GetFloat(payload);
            Parameters.Airspeed.groundspeed_sp = GetFloat(payload);
            Parameters.aircraftId = acId;

            //gcsMgr->GetWindow()->UpdateGauges();
        }
    };

    class MsgDlValue : PprzDownMsg{

        public MsgDlValue(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            byte index = payload[offset++];
            Parameters.DlValue[index] = GetFloat(payload);
            Parameters.aircraftId = acId;
            mainClass.DlReceived(index, Parameters.DlValue[index]);
        }
    };

    class MsgCommands : PprzDownMsg{

        public MsgCommands(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            if (Parameters.Commands.ticks == null)
                Parameters.Commands.ticks = new List<ushort>();
            byte len1 = payload[offset++];
            Parameters.Commands.ticks.Clear();
            for(int i=0;i<len1;i++)
                Parameters.Commands.ticks.Add(GetUInt16(payload));
            Parameters.aircraftId = acId;
        }
    };

    class MsgAlive : PprzDownMsg{

        public MsgAlive(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            byte len1 = payload[offset++];
            Parameters.Alive.md5sum = BitConverter.ToString(payload.Skip(offset).Take(len1).ToArray());
            Parameters.aircraftId = acId;
        }
    };

    class MsgActuators : PprzDownMsg{

        public MsgActuators(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId,byte [] payload){
            offset = 0;
            byte len = payload[offset++];
            Parameters.aircraftId = acId;

            Parameters.Actuators.Clear();

            for(int i=0;i<len;i++){
                ushort val = GetUInt16(payload);
                Parameters.Actuators.Add(val);
            }
        }
    };

    class MsgHctl : PprzDownMsg
    {

        public MsgHctl(Main _mainClass) : base(_mainClass) { }
        public override void DoCmd(byte acId, byte[] payload)
        {
            offset = 0;
            Parameters.Hctl.roll_sum_err = GetFloat(payload);
            Parameters.Hctl.roll_sp = GetFloat(payload);
            Parameters.Hctl.roll_ref = GetFloat(payload);
            Parameters.Hctl.phi = GetUInt16(payload);
            Parameters.Hctl.pitch_sum_err = GetFloat(payload);
            Parameters.Hctl.pitch_sp = GetFloat(payload);
            Parameters.Hctl.pitch_ref = GetFloat(payload);
            Parameters.Hctl.theta = GetUInt16(payload);
            Parameters.aircraftId = acId;
        }
    };
}
