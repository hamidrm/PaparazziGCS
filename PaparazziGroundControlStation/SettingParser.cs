using PaparazziGroundControlStation.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace PaparazziGroundControlStation
{
    class SettingParser
    {
        XmlDocument xmlDoc = new XmlDocument();
        List<SettingItem> SettingItemList;
        //public static string[] PprzSettingListString = new string[] {
        //    "telemetry_mode_Ap" , 
        //     "telemetry_mode_Fbw" , 
        //     "gps_ubx_ucenter_gps_ubx_ucenter_periodic_status" , 
        //     "gps_ublox_gps_ubx_periodic_check_status" , 
        //     "flight_recorder_flight_recorder_periodic_status" , 
        //     "flight_altitude" , 
        //     "nav_course" , 
        //     "nav_shift" , 
        //     "autopilot.flight_time" , 
        //     "nav_radius" , 
        //     "autopilot.mode" , 
        //     "autopilot.launch" , 
        //     "autopilot.kill_throttle" , 
        //     "h_ctl_yaw_dgain" , 
        //     "h_ctl_yaw_ny_igain" , 
        //     "ap_state->command_roll_trim" , 
        //     "ap_state->command_pitch_trim" , 
        //     "h_ctl_roll_attitude_gain" , 
        //     "h_ctl_roll_rate_gain" , 
        //     "h_ctl_roll_igain" , 
        //     "h_ctl_pitch_pgain" , 
        //     "h_ctl_pitch_dgain" , 
        //     "h_ctl_pitch_igain" , 
        //     "h_ctl_pitch_of_roll" , 
        //     "h_ctl_aileron_of_throttle" , 
        //     "h_ctl_roll_max_setpoint" , 
        //     "use_airspeed_ratio" , 
        //     "h_ctl_roll_Kffa" , 
        //     "h_ctl_roll_Kffd" , 
        //     "h_ctl_pitch_Kffa" , 
        //     "h_ctl_pitch_Kffd" , 
        //     "v_ctl_altitude_pgain" , 
        //     "v_ctl_speed_mode" , 
        //     "v_ctl_auto_throttle_min_cruise_throttle" , 
        //     "v_ctl_auto_throttle_max_cruise_throttle" , 
        //     "v_ctl_auto_throttle_cruise_throttle" , 
        //     "v_ctl_pitch_trim" , 
        //     "v_ctl_auto_pitch_pgain" , 
        //     "v_ctl_auto_pitch_igain" , 
        //     "v_ctl_auto_pitch_dgain" , 
        //     "v_ctl_auto_throttle_pgain" , 
        //     "v_ctl_auto_throttle_igain" , 
        //     "v_ctl_auto_throttle_dgain" , 
        //     "v_ctl_auto_throttle_climb_throttle_increment" , 
        //     "v_ctl_auto_throttle_pitch_of_vz_pgain" , 
        //     "v_ctl_auto_airspeed_setpoint" , 
        //     "v_ctl_auto_airspeed_throttle_pgain" , 
        //     "v_ctl_auto_airspeed_throttle_dgain" , 
        //     "v_ctl_auto_airspeed_throttle_igain" , 
        //     "v_ctl_auto_airspeed_pitch_pgain" , 
        //     "v_ctl_auto_airspeed_pitch_dgain" , 
        //     "v_ctl_auto_airspeed_pitch_igain" , 
        //     "v_ctl_auto_groundspeed_setpoint" , 
        //     "v_ctl_auto_groundspeed_pgain" , 
        //     "v_ctl_auto_groundspeed_igain" , 
        //     "h_ctl_course_pgain" , 
        //     "h_ctl_course_dgain" , 
        //     "h_ctl_course_pre_bank_correction" , 
        //     "nav_glide_pitch_trim" , 
        //     "h_ctl_roll_slew" , 
        //     "nav_radius" , 
        //     "nav_course" , 
        //     "nav_mode" , 
        //     "nav_climb" , 
        //     "fp_pitch" , 
        //     "fp_throttle" , 
        //     "fp_climb" , 
        //     "nav_shift" , 
        //     "nav_ground_speed_setpoint" , 
        //     "nav_ground_speed_pgain" , 
        //     "nav_survey_shift" , 
        //     "takeoff_armed" , 
        //     "advanced_to_rudder_p_gain" , 
        //     "advanced_to_rudder_d_gain" , 
        //     "advanced_to_rudder_max_setpoint" , 
        //     "trig_enable" , 
        //     "cutoff_enable" , 
        //     "ms45xx.sync_send" , 
        //     "ms45xx.pressure_scale" , 
        //     "ms45xx.pressure_offset" , 
        //     "ms45xx.airspeed_scale" , 
        //     "geo_mag.calc_once" , 
        //     "gps_ubx_ucenter.sw_ver_h" , 
        //     "gps_ubx_ucenter.sw_ver_l" , 
        //     "gps_ubx_ucenter.hw_ver_h" , 
        //     "gps_ubx_ucenter.hw_ver_l" , 
        //     "gps_ubx_ucenter.baud_init" , 
        //     "gps_ubx_ucenter.baud_run" , 
        //     "air_data.qnh" , 
        //     "air_data.tas_factor" , 
        //     "air_data.calc_qnh_once" , 
        //     "air_data.calc_airspeed" , 
        //     "air_data.calc_tas_factor" , 
        //     "air_data.calc_amsl_baro" , 
        //     "nav_radius" , 
        //     "nav_course" , 
        //     "nav_mode" , 
        //     "nav_climb" , 
        //     "fp_pitch" , 
        //     "fp_throttle" , 
        //     "fp_climb" , 
        //     "nav_shift" , 
        //     "nav_ground_speed_setpoint" , 
        //     "nav_ground_speed_pgain" , 
        //     "nav_survey_shift" , 
        //     "multi_gps_mode" , 
        //     "ahrs_icq.gravity_heuristic_factor" , 
        //     "ahrs_icq.accel_omega" , 
        //     "ahrs_icq.accel_zeta" , 
        //     "ahrs_icq.mag_omega" , 
        //     "ahrs_icq.mag_zeta" , 
        //     "imu.body_to_imu.eulers_f.phi" , 
        //     "imu.body_to_imu.eulers_f.theta" , 
        //     "imu.body_to_imu.eulers_f.psi" , 
        //     "imu.b2i_set_current" , 
        //     "v_ctl_altitude_pgain" , 
        //     "v_ctl_speed_mode" , 
        //     "v_ctl_auto_throttle_min_cruise_throttle" , 
        //     "v_ctl_auto_throttle_max_cruise_throttle" , 
        //     "v_ctl_auto_throttle_cruise_throttle" , 
        //     "v_ctl_pitch_trim" , 
        //     "v_ctl_auto_pitch_pgain" , 
        //     "v_ctl_auto_pitch_igain" , 
        //     "v_ctl_auto_pitch_dgain" , 
        //     "v_ctl_auto_throttle_pgain" , 
        //     "v_ctl_auto_throttle_igain" , 
        //     "v_ctl_auto_throttle_dgain" , 
        //     "v_ctl_auto_throttle_climb_throttle_increment" , 
        //     "v_ctl_auto_throttle_pitch_of_vz_pgain" , 
        //     "ap_state->command_roll_trim" , 
        //     "ap_state->command_pitch_trim" , 
        //     "ap_state->command_yaw_trim" , 
        //     "h_ctl_roll_attitude_gain" , 
        //     "h_ctl_roll_rate_gain" , 
        //     "h_ctl_roll_igain" , 
        //     "h_ctl_pitch_pgain" , 
        //     "h_ctl_pitch_dgain" , 
        //     "h_ctl_pitch_igain" , 
        //     "h_ctl_pitch_of_roll" , 
        //     "h_ctl_aileron_of_throttle" , 
        //     "h_ctl_roll_max_setpoint" , 
        //     "use_airspeed_ratio" , 
        //     "h_ctl_roll_Kffa" , 
        //     "h_ctl_roll_Kffd" , 
        //     "h_ctl_pitch_Kffa" , 
        //     "h_ctl_pitch_Kffd" , 
        //     "h_ctl_course_pgain" , 
        //     "h_ctl_course_dgain" , 
        //     "h_ctl_course_pre_bank_correction" , 
        //     "h_ctl_roll_slew"
        //};
        public void SettingsUpdate(byte index,float value){
            foreach(SettingItem i in SettingItemList)
                i.SetValue(index,value);
        }
        public SettingParser(Control parent,string xmlFile, Action<byte,float> saveClick, Action<byte> loadClick)
        {
            SettingItemList = new List<SettingItem>();

            if (!File.Exists(xmlFile))
            {
                MessageBox.Show("Settings.XML not found!");
                return;
            }
            xmlDoc.XmlResolver = null;
            xmlDoc.Load(xmlFile);
            
            //Create a tab control
            TabControl mainTab = new TabControl();
            

            mainTab.Parent = parent;
            mainTab.Dock = DockStyle.Fill;
            mainTab.Show();

            XmlNode settingsElem = xmlDoc.SelectSingleNode("settings/dl_settings");

            foreach (XmlNode dl_setting_lvl1 in settingsElem.ChildNodes)
            {
                if (dl_setting_lvl1.Name == "dl_settings")
                {
                    // Add new tab page
                    TabPage tabPage = new TabPage(dl_setting_lvl1.Attributes["name"].Value);
                    tabPage.Name = dl_setting_lvl1.Attributes["name"].Value;
                    mainTab.TabPages.Add(tabPage);

                    // Is there any dl_settings object?
                    if (dl_setting_lvl1.FirstChild.Name == "dl_settings")
                    {
                        //Create a sub tab control
                        TabControl lvl2Tab = new TabControl();
                        tabPage.Controls.Add(lvl2Tab);
                        lvl2Tab.Dock = DockStyle.Fill;
                        lvl2Tab.Show();
                        foreach (XmlNode dl_setting_lvl2 in dl_setting_lvl1.ChildNodes)
                        {

                            if (dl_setting_lvl2.Name == "dl_settings")
                            {
                                // Add new tab page
                                TabPage tabPageLvl2 = new TabPage(dl_setting_lvl2.Attributes["name"].Value);
                                tabPageLvl2.Name = dl_setting_lvl2.Attributes["name"].Value;
                                lvl2Tab.TabPages.Add(tabPageLvl2);
                                tabPageLvl2.Show();

                                //Create a sub FlowLayoutPanel
                                FlowLayoutPanel settingGroupLvl2 = new FlowLayoutPanel();
                                tabPageLvl2.Controls.Add(settingGroupLvl2);
                                settingGroupLvl2.Dock = DockStyle.Fill;
                                settingGroupLvl2.FlowDirection = FlowDirection.TopDown;
                                settingGroupLvl2.WrapContents = false;
                                settingGroupLvl2.AutoScroll = true;

                                foreach (XmlNode dl_setting_lvl3 in dl_setting_lvl2.ChildNodes)
                                {
                                    if (dl_setting_lvl3.Name == "dl_setting")
                                    {
                                        SettingItem setting = new SettingItem();
                                        settingGroupLvl2.Controls.Add(setting);
                                        setting.saveClick += saveClick;
                                        setting.loadClick += loadClick;

                                        SettingItemList.Add(setting);
                                        if (dl_setting_lvl3.Attributes["values"] == null)
                                        {
                                            setting.Steps = Convert.ToSingle(dl_setting_lvl3.Attributes["step"].Value);
                                            setting.MinValue = Convert.ToSingle(dl_setting_lvl3.Attributes["min"].Value);
                                            setting.MaxValue = Convert.ToSingle(dl_setting_lvl3.Attributes["max"].Value);
                                            setting.settingType = SettingItem.SettingType.SettingTrackBar;
                                        }
                                        else
                                        {
                                            string value = dl_setting_lvl3.Attributes["values"].Value;
                                            string [] values = value.Split('|');
                                            if (values.Count() == 2)
                                            {
                                                setting.RadioButton1 = values[0];
                                                setting.RadioButton2 = values[1];
                                                setting.RadioButton1V = Convert.ToSingle(dl_setting_lvl3.Attributes["min"].Value);
                                                setting.RadioButton2V = Convert.ToSingle(dl_setting_lvl3.Attributes["max"].Value);
                                                setting.settingType = SettingItem.SettingType.SettingRadioButton;
                                            }
                                            else
                                            {
                                                setting.ListItems.AddRange(values);
                                                setting.settingType = SettingItem.SettingType.SettingListBox;
                                            }
                                        }

                                        if (dl_setting_lvl3.Attributes["var"] != null)
                                        {
                                            setting.settingPprz = (byte)Communication.Setting.PprzSettingListString.ToList().IndexOf(dl_setting_lvl3.Attributes["var"].Value);
                                            setting.settingTitle = dl_setting_lvl3.Attributes["var"].Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (dl_setting_lvl1.FirstChild.Name == "dl_setting")
                    {

                        // Create a FlowLayoutPanel
                        FlowLayoutPanel settingGroupLvl1 = new FlowLayoutPanel();
                        tabPage.Controls.Add(settingGroupLvl1);
                        settingGroupLvl1.Dock = DockStyle.Fill;
                        settingGroupLvl1.WrapContents = false;
                        settingGroupLvl1.FlowDirection = FlowDirection.TopDown;
                        settingGroupLvl1.AutoScroll = true;
                        settingGroupLvl1.Show();

                        foreach (XmlNode dl_setting_lvl2 in dl_setting_lvl1.ChildNodes)
                        {
                            if (dl_setting_lvl2.Name == "dl_setting")
                            {
                                SettingItem setting = new SettingItem();
                                settingGroupLvl1.Controls.Add(setting);
                                setting.saveClick += saveClick;
                                setting.loadClick += loadClick;
                                SettingItemList.Add(setting);
                                if (dl_setting_lvl2.Attributes["values"] == null)
                                {
                                    setting.Steps = Convert.ToSingle(dl_setting_lvl2.Attributes["step"].Value);
                                    setting.MinValue = Convert.ToSingle(dl_setting_lvl2.Attributes["min"].Value);
                                    setting.MaxValue = Convert.ToSingle(dl_setting_lvl2.Attributes["max"].Value);
                                    setting.settingType = SettingItem.SettingType.SettingTrackBar;
                                }
                                else
                                {
                                    string value = dl_setting_lvl2.Attributes["values"].Value;
                                    string[] values = value.Split('|');
                                    if (values.Count() == 2)
                                    {
                                        setting.RadioButton1 = values[0];
                                        setting.RadioButton2 = values[1];
                                        setting.RadioButton1V = Convert.ToSingle(dl_setting_lvl2.Attributes["min"].Value);
                                        setting.RadioButton2V = Convert.ToSingle(dl_setting_lvl2.Attributes["max"].Value);
                                        setting.settingType = SettingItem.SettingType.SettingRadioButton;
                                    }
                                    else
                                    {
                                        setting.ListItems.AddRange(values);
                                        setting.settingType = SettingItem.SettingType.SettingListBox;
                                    }
                                }
                                if (dl_setting_lvl2.Attributes["var"] != null)
                                {
                                    setting.settingPprz = (byte)Communication.Setting.PprzSettingListString.ToList().IndexOf(dl_setting_lvl2.Attributes["var"].Value);
                                    setting.settingTitle = dl_setting_lvl2.Attributes["var"].Value;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
