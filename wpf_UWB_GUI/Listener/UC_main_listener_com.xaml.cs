﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using wpf_UWB_GUI.Listener;

namespace wpf_UWB_GUI
{
    /// <summary>
    /// UC_main_gateway_com.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UC_main_listener_com : Page
    {
        String TAG = "UC_main_gateway_com.xaml";

        public delegate void OnListenerParseHandler(List<class_listener_list> mlistListenerParse);
        public event OnListenerParseHandler listenerParse;

        public delegate void OnListenerConnectHandler(Boolean fState);
        public event OnListenerConnectHandler listenerConnect;

        public MainSetSerialConnectHandler setSerialHandler;

        public Main_Setting_Data mainSettingData = new Main_Setting_Data();
        private List<Device_Reference> realDeviceInfo = new List<Device_Reference>();

        DispatcherTimer timer10hz = new DispatcherTimer();

        public SerialConnect serialConnect;

        long prevMillis = 0;

        List<Control> listCursorControl = new List<Control>();


        public UC_main_listener_com()
        {
            Console.WriteLine("UC_main_listener_com()");

            InitializeComponent();

            listCursorControl.Add(cbx_serialPort);
            listCursorControl.Add(connected_btn);
            listCursorControl.Add(stop_btn);

            for(int i=0;i< listCursorControl.Count; i++)
            {
                listCursorControl[i].MouseEnter += new MouseEventHandler(mouseEnterHandler);
                listCursorControl[i].MouseLeave += new MouseEventHandler(mouseLeaveHandler);
            }

            var portnames = SerialPort.GetPortNames();
            for (int i = 0; i < portnames.Length; i++) cbx_serialPort.Items.Add(portnames[i]);
            serialPort_Status(false);

            timer10hz.Interval = TimeSpan.FromMilliseconds(10);
            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();

            serialConnect = new SerialConnect();
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
            if(listenerParse != null)
            {
                if(serialConnect.getList() != null)
                {
                    if (serialConnect.getList().Count != 0)
                    {
                        listenerParse(serialConnect.getList());
                        serialConnect.clearList();
                    }
                }

                if(serialConnect.getAnchorList() != null)
                {
                    if(serialConnect.getAnchorList().Count != 0)
                    {
                        listenerParse(serialConnect.getAnchorList());
                        serialConnect.clearAnchorList();
                    }
                }
            }

            if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevMillis > 1000 * 1)
            {
                prevMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;

                var portnames = SerialPort.GetPortNames();

                for (int i = 0; i < portnames.Length; i++)
                    if (cbx_serialPort.Items.IndexOf(portnames[i]) == -1)
                        cbx_serialPort.Items.Add(portnames[i]);
            }
        }

        private void serialPort_Status(bool fState)
        {
            if (fState)
            {
                if (listenerConnect != null)
                    listenerConnect(true);
                cbx_serialPort.IsEnabled = false;
                connected_btn.IsEnabled = false;
                stop_btn.Background = new SolidColorBrush(config.titleSelectColor);
                connected_btn.Background = new SolidColorBrush(config.titleUnSelectColor);
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_connected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "Connected";
                label_connect_status.Foreground = new SolidColorBrush(config.connectColor);
            }
            else
            {
                if (listenerConnect != null)
                    listenerConnect(false);
                cbx_serialPort.IsEnabled = true;
                connected_btn.IsEnabled = true;
                connected_btn.Background = new SolidColorBrush(config.titleSelectColor);
                stop_btn.Background = new SolidColorBrush(config.titleUnSelectColor);
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_disconnected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "DisConnected";
                label_connect_status.Foreground = new SolidColorBrush(config.disconnectColor);
            }
        }

        private void connected_btn_Click(object sender, RoutedEventArgs e)
        {
            //none select
            if (cbx_serialPort.SelectedIndex == -1) return;
            if (serialConnect.fConnectState()) return;

            serialConnect.init();
            serialConnect.setSerialPort(cbx_serialPort.SelectedItem.ToString(), 115200);

            try
            {
                serialConnect.sp_Connect();
                setSerialHandler(serialConnect);
                serialPort_Status(true);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Port is opened. Not Connect.");
                return;
            }
        }

        private void stop_btn_Click(object sender, RoutedEventArgs e)
        {
            serialConnect.sp_DisConnect();
            setSerialHandler(null);

            serialPort_Status(false);
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

        private void cbx_serialPort_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if(e.Delta > 0)
            {
                //UP
                if (cbx_serialPort.SelectedIndex > 0)
                {
                    cbx_serialPort.SelectedIndex -= 1;
                }
            }
            else if (e.Delta < 0)
            {
                //DOWN
                if (cbx_serialPort.SelectedIndex < cbx_serialPort.Items.Count)
                {
                    cbx_serialPort.SelectedIndex += 1;
                }
            }
        }
    }
}
