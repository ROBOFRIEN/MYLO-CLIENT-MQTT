using MQTTnet;
using MQTTnet.Client;
using MQTTnet.ManagedClient;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MYLO_CLIENT_MQTT.Listener;

namespace MYLO_CLIENT_MQTT
{
    /// <summary>
    /// UC_main_gateway_com.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_main_listener_mqtt
    {
        String TAG = "UC_main_listener_mqtt.xaml";

        public delegate void OnListenerParseHandler(List<class_listener_list> mlistListenerParse);
        public event OnListenerParseHandler listenerParse;

        public delegate void OnListenerConnectHandler(Boolean fState);
        public event OnListenerConnectHandler listenerConnect;

        //public MainSetMqttConnectHandler setSerialHandler;

        public MainSetMqttConnectHandler setMqttHandler;

        public Main_Setting_Data mainSettingData = new Main_Setting_Data();
        private List<Device_Reference> realDeviceInfo = new List<Device_Reference>();

        DispatcherTimer timer10hz = new DispatcherTimer();

        long prevMillis = 0;

        long prevGetTopicMillis = 0;
        const long checkGetTopicTime = 1000 * 10;

        List<Control> listCursorControl = new List<Control>();

        MqttConnect mqttConnect;



        public UC_main_listener_mqtt()
        {
            Console.WriteLine("UC_main_listener_mqtt()");

            InitializeComponent();

            mqttConnect = new MqttConnect();
            mqttConnect.init();

            mqttConnect.MQTT_IP = mainSettingData.MQTT_IP;
            mqttConnect.MQTT_PORT = mainSettingData.MQTT_port;
            mqttConnect.fReconnect = false;

            //mqtt_Status(false);
            mqttConnect_Status("DISCONNECTED");

            listCursorControl.Add(server_IP1);
            listCursorControl.Add(server_IP2);
            listCursorControl.Add(server_IP3);
            listCursorControl.Add(server_IP4);
            listCursorControl.Add(server_Port);
            listCursorControl.Add(connected_btn);
            listCursorControl.Add(stop_btn);

            for(int i=0;i< listCursorControl.Count; i++)
            {
                listCursorControl[i].MouseEnter += new MouseEventHandler(mouseEnterHandler);
                listCursorControl[i].MouseLeave += new MouseEventHandler(mouseLeaveHandler);
            }

            timer10hz.Interval = TimeSpan.FromMilliseconds(10);
            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();
        }

        private void mouseEnterHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void mouseLeaveHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void UC_main_gateway_com_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("UC_main_gateway_com_Loaded()");

        }

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            if (listenerParse != null)
            {
                if (mqttConnect.getMqttList() != null)
                {
                    List<class_listener_list> tmpList = mqttConnect.getMqttList();
                    if (tmpList.Count != 0)
                    {
                        listenerParse(mqttConnect.getMqttList());
                        //mqttConnect.clearList();
                    }
                }
            }

            if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevMillis > 1000 * 1)
            {
                prevMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
                mqttConnect_Status(mqttConnect.connectedStatus);
            }

            if(mqttConnect.connectedStatus.Contains("CONNECTED"))
            {
                if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevGetTopicMillis > checkGetTopicTime)
                {
                    prevGetTopicMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
                    mqttConnect.mqtt_AllSub();
                }
            }
        }

        private void mqttConnect_Status(String connectedState)
        {
            if (connectedState.Contains("DISCONNECTED"))
            {
                if (listenerConnect != null)
                    listenerConnect(false);
                connected_btn.IsEnabled = true;
                connected_btn.Background = new SolidColorBrush(config.titleSelectColor);
                stop_btn.Background = new SolidColorBrush(config.titleUnSelectColor);
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_disconnected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = connectedState;
                label_connect_status.Foreground = new SolidColorBrush(config.disconnectColor);
            }
            else if (connectedState.Contains("CONNECTING"))
            {
                connected_btn.IsEnabled = false;
                stop_btn.Background = new SolidColorBrush(config.titleSelectColor);
                connected_btn.Background = new SolidColorBrush(config.titleUnSelectColor);
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_connecting.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = connectedState;
                label_connect_status.Foreground = new SolidColorBrush(config.connectingColor);
            }
            else if (connectedState.Contains("CONNECTED"))
            {
                if (listenerConnect != null)
                    listenerConnect(true);
                connected_btn.IsEnabled = false;
                stop_btn.Background = new SolidColorBrush(config.titleSelectColor);
                connected_btn.Background = new SolidColorBrush(config.titleUnSelectColor);
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_connected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = connectedState;
                label_connect_status.Foreground = new SolidColorBrush(config.connectColor);
            }
        }

        //Mqtt Connect Button
        private void connected_btn_Click(object sender, RoutedEventArgs e)
        {

            mqttConnect.clearList();

            try
            {
                mainSettingData.MQTT_IP = server_IP1.Text + "." + server_IP2.Text + "." + server_IP3.Text + "." + server_IP4.Text;
                mainSettingData.MQTT_port = server_Port.Text;
                mqttConnect.MQTT_IP = mainSettingData.MQTT_IP;
                mqttConnect.MQTT_PORT = mainSettingData.MQTT_port;
                mqttConnect.startClicked = true;

                json_generate();
                //setSerialHandler(sp_mqttConnect);
                //mqtt_Status(true);
                setMqttHandler(mqttConnect);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Port is opened. Not Connect.");
                return;
            }
        }

        //Mqtt disConnect Button
        private void stop_btn_Click(object sender, RoutedEventArgs e)
        {
            //sp_mqttConnect.sp_DisConnect();
            //setSerialHandler(null);
            mqttConnect.MQTT_IP = mainSettingData.MQTT_IP;
            mqttConnect.MQTT_PORT = mainSettingData.MQTT_port;
            mqttConnect.stopClicked = true;
            json_generate();

            //mqtt_Status(false);
            setMqttHandler(null);
        }

        //JSON SAVE Setting DATA
        private void json_generate()
        {
            string json = JsonConvert.SerializeObject(mainSettingData);
            Properties.Settings.Default.JSON_Setting_Info = json;
            Properties.Settings.Default.Save();
        }

        //JSON LOAD Setting DATA
        private void request_json()
        {
            string json_setting_string = Properties.Settings.Default.JSON_Setting_Info;
            var setting_data = JsonConvert.DeserializeObject<Main_Setting_Data>(json_setting_string);
            mainSettingData = setting_data;
        }
    }
}
