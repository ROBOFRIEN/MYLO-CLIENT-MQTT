using MQTTnet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace MYLO_CLIENT_MQTT.Listener
{
    class MqttParsing
    {
        bool DEBUG = Properties.Settings.Default.DEBUG;

        private String TAG = "MqttParsing.cs";

        public List<class_listener_list> mqtt_DevList = new List<class_listener_list>();
        public List<class_listener_list> mqtt_SendList = new List<class_listener_list>();

        public List<Device_Reference> raw_received_device_info_buf = new List<Device_Reference>();
        public Main_Setting_Data Main_Setting_Data = new Main_Setting_Data();

        private readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public void initialize_timer()
        {
            try
            {
                Timer timer = new System.Timers.Timer();
                timer.Interval = 100;
                timer.Elapsed += new ElapsedEventHandler(timer_for_cal_hz);
                timer.Start();
            }
            catch (Exception e)
            {
                fn_ErrorFileWrite("MqttParsing Error : " + e.ToString());
            }
        }

        private void timer_for_cal_hz(object sender, ElapsedEventArgs e)
        {

        }

        public void parsing_MQTT_data(MqttApplicationMessageReceivedEventArgs e)
        {
            if (e == null) return;
            //Console.WriteLine("parsing_MQTT_data()");
            //print_raw_device_info_buf();

            String Topic_name = e.ApplicationMessage.Topic;
            String Topic_Message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            Console.WriteLine("topic : " + Topic_name + " // message:" + Topic_Message);

            JObject obj_T = JObject.Parse(Topic_Message);

            ///tag/dw0a84/rawData/
            ///tag/dw0a84/filteredData/
            ///anchor/dw2b22

            try
            {
                if (Topic_name.Contains("/tag/"))
                {
                    if (Topic_name.Contains("/rawData/"))
                    {
                        parsing_Tag_Raw(obj_T);
                    }
                    if (Topic_name.Contains("/filteredData/"))
                    {
                        parsing_Tag_Filter(obj_T);
                    }
                }
                else if (Topic_name.Contains("/anchor/"))
                {
                    parsing_Anchor(obj_T);
                }
                else if (Topic_name.Contains("/config"))
                {
                    parsing_Config(obj_T);
                }
            }
            catch (Exception e3)
            {
                Console.WriteLine(TAG + " // " + e3.ToString());
            }
        }

        private void parsing_Tag_Raw(JObject Topic_message)
        {
            if (mqtt_DevList != null)
            {
                int num_Tag = mqtt_DevList.FindIndex(x => x.devSN.Equals(Topic_message["name"].ToString()));
                if (num_Tag == -1)
                {
                    class_listener_list clTmp = new class_listener_list();

                    clTmp.devSN = Topic_message["name"].ToString();
                    clTmp.devType = "TAG";
                    clTmp.tag_pos_x = float.Parse(Topic_message["x"].ToString());
                    clTmp.tag_pos_y = float.Parse(Topic_message["y"].ToString());
                    clTmp.tag_pos_z = float.Parse(Topic_message["z"].ToString());
                    clTmp.time = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                    mqtt_DevList.Add(clTmp);

                }
                else
                {
                    mqtt_DevList[num_Tag].tag_pos_x = float.Parse(Topic_message["x"].ToString());
                    mqtt_DevList[num_Tag].tag_pos_y = float.Parse(Topic_message["y"].ToString());
                    mqtt_DevList[num_Tag].tag_pos_z = float.Parse(Topic_message["z"].ToString());
                    mqtt_DevList[num_Tag].time = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
                }
            }
            else
            {
                class_listener_list clTmp = new class_listener_list();

                clTmp.devSN = Topic_message["name"].ToString();
                clTmp.devType = "TAG";
                clTmp.tag_pos_x = float.Parse(Topic_message["x"].ToString());
                clTmp.tag_pos_y = float.Parse(Topic_message["y"].ToString());
                clTmp.tag_pos_z = float.Parse(Topic_message["z"].ToString());
                clTmp.time = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                mqtt_DevList.Add(clTmp);
            }
        }

        private void parsing_Tag_Filter(JObject Topic_message)
        {
            if (mqtt_DevList != null)
            {
                int num_Tag = mqtt_DevList.FindIndex(x => x.devSN.Equals(Topic_message["name"].ToString()));
                if (num_Tag != -1)
                {
                    mqtt_DevList[num_Tag].tag_lpf_x = float.Parse(Topic_message["x"].ToString());
                    mqtt_DevList[num_Tag].tag_lpf_y = float.Parse(Topic_message["y"].ToString());
                    mqtt_DevList[num_Tag].tag_lpf_z = float.Parse(Topic_message["z"].ToString());
                }
            }
        }

        private void parsing_Anchor(JObject Topic_message)
        {
            if (mqtt_DevList != null)
            {
                int num_Tag = mqtt_DevList.FindIndex(x => x.devSN.Equals(Topic_message["name"].ToString()));
                if (num_Tag == -1)
                {
                    class_listener_list clTmp = new class_listener_list();

                    clTmp.devSN = Topic_message["name"].ToString();
                    clTmp.devType = "ANCHOR";
                    clTmp.tag_pos_x = float.Parse(Topic_message["x"].ToString());
                    clTmp.tag_pos_y = float.Parse(Topic_message["y"].ToString());
                    clTmp.tag_pos_z = float.Parse(Topic_message["z"].ToString());
                    clTmp.time = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                    mqtt_DevList.Add(clTmp);
                }
                else
                {
                    mqtt_DevList[num_Tag].tag_pos_x = float.Parse(Topic_message["x"].ToString());
                    mqtt_DevList[num_Tag].tag_pos_y = float.Parse(Topic_message["y"].ToString());
                    mqtt_DevList[num_Tag].tag_pos_z = float.Parse(Topic_message["z"].ToString());
                    mqtt_DevList[num_Tag].time = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
                }
            }
            else
            {
                class_listener_list clTmp = new class_listener_list();

                clTmp.devSN = Topic_message["name"].ToString();
                clTmp.devType = "ANCHOR";
                clTmp.tag_pos_x = float.Parse(Topic_message["x"].ToString());
                clTmp.tag_pos_y = float.Parse(Topic_message["y"].ToString());
                clTmp.tag_pos_z = float.Parse(Topic_message["z"].ToString());
                clTmp.time = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                mqtt_DevList.Add(clTmp);
            }
        }

        private void parsing_Config(JObject Topic_message)
        {
            if (mqtt_DevList != null)
            {
                int num_Tag = mqtt_DevList.FindIndex(x => x.devSN.Equals(Topic_message["name"].ToString()));
                if (num_Tag != -1)
                {
                    mqtt_DevList[num_Tag].devColor = Topic_message["color"].ToString();
                }
            }
        }


        //LOG
        private void fn_LogWrite(string str)
        {
            //2021_09_16_13~14_receive_data.txt

            //string DirPath = System.Environment.CurrentDirectory;
            string DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";

            string strFormat = "yyyy_MM_dd_HH~";
            String textFileName = DateTime.Now.ToString(strFormat) + (int.Parse(DateTime.Now.ToString("HH")) + 1) + "_receive_data.txt";
            //String textFileName = DateTime.Now.ToString(strFormat) + "_receive_data.txt";

            string FilePath = DirPath + "\\" + textFileName;
            string temp;

            DirectoryInfo di = new DirectoryInfo(DirPath);
            FileInfo fi = new FileInfo(FilePath);

            try
            {
                if (!di.Exists) Directory.CreateDirectory(DirPath);
                if (!fi.Exists)
                {
                    using (StreamWriter sw = new StreamWriter(FilePath))
                    {
                        //temp = string.Format("{0}", str);
                        temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        //temp = string.Format("{0}", str);
                        temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                fn_ErrorFileWrite("fn_LogWrite Error : " + e.ToString());
            }
        }

        //Error file write
        private void fn_ErrorFileWrite(string str)
        {
            string DirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\UWB\\";

            string strFormat = "yyyy_MM";
            String textFileName = DateTime.Now.ToString(strFormat) + "_ERROR_LOG.txt";

            string FilePath = DirPath + "\\" + textFileName;
            string temp;

            DirectoryInfo di = new DirectoryInfo(DirPath);
            FileInfo fi = new FileInfo(FilePath);

            try
            {
                if (!di.Exists) Directory.CreateDirectory(DirPath);
                if (!fi.Exists)
                {
                    using (StreamWriter sw = new StreamWriter(FilePath))
                    {
                        temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        temp = string.Format("[{0}] {1}", DateTime.Now, str);
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("fn_ErrorFileWrite : " + e.ToString());
            }
        }

        internal List<class_listener_list> getMqttList()
        {
            List<class_listener_list> tagList = mqtt_DevList.FindAll(x => x.devType.Equals("TAG"));
            List<class_listener_list> anchorList = mqtt_DevList.FindAll(x => x.devType.Equals("ANCHOR"));

            tagList = tagList.OrderBy(x => x.devSN).ToList();
            anchorList = anchorList.OrderBy(x => x.devSN).ToList();

            mqtt_SendList.Clear();
            for (int i = 0; i < tagList.Count; i++)
                mqtt_SendList.Add(tagList[i]);
            for (int i = 0; i < anchorList.Count; i++)
                mqtt_SendList.Add(anchorList[i]);

            return mqtt_SendList;
        }

        public void clearList()
        {
            mqtt_SendList.Clear();
        }
    }
}
