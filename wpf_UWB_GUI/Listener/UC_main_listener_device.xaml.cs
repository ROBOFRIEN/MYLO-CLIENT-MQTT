﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class UC_main_listener_device : Page
    {

        private String TAG = "UC_main_listener_device.xaml";


        public MainGetConnectStateHandler getMainConnectHandler;

        public MainGetAnchorListHandler getAnchorList;
        public MainClickAutoAnchorHandler clickAutoAnchor;
        public MainGetParseDataAllHandler getParseDataAll;
        public MainRemoveDataHandler removeParseData;
        public MainSendMqttHandler sendMqtt;
        public MainGetMqttHandler getMqtt;

        DispatcherTimer timer10hz = new DispatcherTimer();

        DeviceAddWindow devAddWindow;

        List<UC_listener_device_item> uc_deviceList = new List<UC_listener_device_item>();

        List<Control> listCursorControl = new List<Control>();

        long prevAutoCheckMillis = 0;
        long prevAnchorMillis = 0;

        public UC_main_listener_device()
        {
            Console.WriteLine("UC_main_listener_device()");

            InitializeComponent();

            listCursorControl.Add(btn_autoAdd);
            //listCursorControl.Add(btn_add);

            btn_sync.MouseEnter += new MouseEventHandler(mouseEnterHandler);
            btn_sync.MouseLeave += new MouseEventHandler(mouseLeaveHandler);

            for (int i = 0; i < listCursorControl.Count; i++)
            {
                listCursorControl[i].MouseEnter += new MouseEventHandler(mouseEnterHandler);
                listCursorControl[i].MouseLeave += new MouseEventHandler(mouseLeaveHandler);
            }

            DeviceUpdate();

            timer10hz.Interval = TimeSpan.FromMilliseconds(100);
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

        private void UC_main_listener_device_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("UC_main_listener_device_Loaded()");
        }

        List<class_listener_list> prev_clListTmp = new List<class_listener_list>();

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            device_connect_state(getMainConnectHandler());

            List<class_listener_list> cl_ListTmp = getParseDataAll();
            List<class_listener_list> mcl_list = new List<class_listener_list>();

            if (cl_ListTmp != null)
            {
                for (int i = 0; i < cl_ListTmp.Count; i++)
                {
                    class_listener_list cl_Tmp = cl_ListTmp[i];

                    if (cl_Tmp != null)
                    {
                        if (cl_Tmp.devType.Equals("ANCHOR"))
                        {
                            int num_result_Anchor = mcl_list.FindIndex(x => x.devSN.Equals(cl_Tmp.devSN));
                            if (num_result_Anchor != -1)
                            {
                                cl_Tmp.tag_pos_x = mcl_list[num_result_Anchor].tag_pos_x;
                                cl_Tmp.tag_pos_y = mcl_list[num_result_Anchor].tag_pos_y;
                                cl_Tmp.tag_pos_z = mcl_list[num_result_Anchor].tag_pos_z;
                                cl_Tmp.devColor = mcl_list[num_result_Anchor].devColor;
                            }
                            else
                            {
                                DeviceWindow_DevAddHandler(cl_Tmp);
                            }
                        }

                        int num_result_File = mcl_list.FindIndex(x => x.devSN.Equals(cl_Tmp.devSN));
                        if (num_result_File != -1)
                        {
                            cl_Tmp.devTagName = mcl_list[num_result_File].devTagName;
                            cl_Tmp.devColor = mcl_list[num_result_File].devColor;
                        }

                        int num_result_Tag = uc_deviceList.FindIndex(x => x.get_clList().devSN.Equals(cl_Tmp.devSN));
                        if (num_result_Tag != -1)
                        {
                            UC_listener_device_item cl_item = uc_deviceList.Find(x => x.get_clList().devSN.Equals(cl_Tmp.devSN));

                            cl_item.set_clList(cl_Tmp);
                            cl_item.Update();

                            if (prev_clListTmp == null)
                            {
                                class_listener_list tmpConfig = config.DeepCopy(cl_Tmp);
                                tmpConfig.time = tmpConfig.time - 100;

                                prev_clListTmp.Add(tmpConfig);
                            }

                            int num_prev_Tag = prev_clListTmp.FindIndex(x => x.devSN.Equals(cl_Tmp.devSN));
                            if (num_prev_Tag == -1)
                            {
                                prev_clListTmp.Add(config.DeepCopy(cl_Tmp));
                            }
                            else
                            {
                                //Console.WriteLine("time :: " + cl_Tmp.time + " // " + prev_clListTmp[num_prev_Tag].time);
                                if (cl_Tmp.time != prev_clListTmp[num_prev_Tag].time)
                                {
                                    prev_clListTmp[num_prev_Tag] = config.DeepCopy(cl_Tmp);
                                }
                            }
                        }
                        else
                        {
                            if (cl_Tmp.devType.Equals("TAG"))
                            {
                                cl_Tmp.devTagName = "Unknown";
                                DeviceWindow_DevAddHandler(cl_Tmp);
                            }
                        }
                    }
                }
            }

            //AutoAnchor List Refersh Scan Anchor
            if (fAutoAnchor)
            {
                if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevAutoCheckMillis > 500)
                {
                    List<class_listener_list> anchorList = new List<class_listener_list>();
                    //Console.WriteLine("getAnchorList():");
                    anchorList = getAnchorList();
                    if (anchorList == null) return;
                    if (anchorList.Count == 0) return;

                    for (int i = 0; i < anchorList.Count; i++)
                    {
                        //Console.WriteLine(i + ":" + anchorList[i].devSN + ":" + anchorList[i].tag_pos_x + ":" + anchorList[i].tag_pos_y + ":" + anchorList[i].tag_pos_z);
                        DeviceWindow_DevAddHandler(anchorList[i]);
                    }
                    prevAutoCheckMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
                }
            }

            //AutoAnchor nSec Auto Close
            if (fAutoAnchor)
            {
                if ((long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds - prevAnchorMillis > 1000 * 5)
                {
                    fAutoAnchor = false;
                }
            }

        }


        private void device_connect_state(bool fState)
        {
            if (fState)
            {
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_connected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "Connected";
                label_connect_status.Foreground = new SolidColorBrush(config.connectColor);
            }
            else
            {
                img_connect_status.Source = new BitmapImage(new Uri("/Resources/img_disconnected.png", UriKind.RelativeOrAbsolute));
                label_connect_status.Content = "DisConnected";
                label_connect_status.Foreground = new SolidColorBrush(config.disconnectColor);
            }
        }

        //private void btn_add_Click(object sender, RoutedEventArgs e)
        //{
        //    bool fReslt = false;
        //    fReslt = config.openFormCheck("DeviceAdd");
        //    if (!fReslt)
        //    {
        //        devAddWindow = new DeviceAddWindow();
        //        devAddWindow.devAddHandler += new DeviceAddWindow.OnDevAddHandler(DeviceWindow_DevAddHandler);
        //        devAddWindow.Show();
        //    }
        //    else
        //    {
        //        config.closeFormCheck("DeviceAdd");
        //        devAddWindow = new DeviceAddWindow();
        //        devAddWindow.Show();
        //    }
        //}

        private void addPanelItem(class_listener_list mcl_List)
        {
            int resultIndex = uc_deviceList.FindIndex(x => x.get_clList().devSN.Equals(mcl_List.devSN));
            if (resultIndex == -1)
            {
                UC_listener_device_item tmpDeviceItem = new UC_listener_device_item();
                tmpDeviceItem.devRemoveItemHandler += new UC_listener_device_item.OnDevRemoveItemHandler(DeviceWindow_RemoveItemHandler);
                tmpDeviceItem.devEditItemHandler += new UC_listener_device_item.OnDevEditItemHandler(DeviceWindow_EditItemHandler);
                tmpDeviceItem.devColorPick += new UC_listener_device_item.OnDevColorPickHandler(DeviceWindow_ColorHandler);
                tmpDeviceItem.set_clList(mcl_List);
                tmpDeviceItem.Update();

                uc_deviceList.Add(tmpDeviceItem);
                listPanelItem.Children.Add(tmpDeviceItem);
            }
            else
            {
                class_listener_list tmpItem = uc_deviceList[resultIndex].get_clList();
                bool fTrue = true;
                if (tmpItem.tag_pos_x != mcl_List.tag_pos_x)
                {
                    fTrue = false;
                    tmpItem.tag_pos_x = mcl_List.tag_pos_x;
                }
                if (tmpItem.tag_pos_y != mcl_List.tag_pos_y)
                {
                    fTrue = false;
                    tmpItem.tag_pos_y = mcl_List.tag_pos_y;
                }
                if (tmpItem.tag_pos_z != mcl_List.tag_pos_z)
                {
                    fTrue = false;
                    tmpItem.tag_pos_z = mcl_List.tag_pos_z;
                }
                tmpItem.time = mcl_List.time;
                uc_deviceList[resultIndex].set_clList(tmpItem);
                if (!fTrue) uc_deviceList[resultIndex].Update();
            }

        }

        //Clear Panel ITEM && LIST
        private void removePanelItem()
        {
            listPanelItem.Children.Clear();

            uc_deviceList.Clear();
            //uwbList.Clear();
        }

        private void DeviceWindow_ColorHandler(string topic, string message)
        {

            sendMqtt(topic, message, true);

            //int resultNum = uc_deviceList.FindIndex(x => x.get_clList().devSN.Equals(mcl_List.devSN));
            //if (resultNum != -1)
            //{
            //    UC_listener_device_item mDeviceItem = uc_deviceList.Find(x => x.get_clList().devSN.Equals(mcl_List.devSN));
            //    mDeviceItem.set_clList(mcl_List);

            //    string json_data = JsonConvert.SerializeObject(getDeviceItemListToClass(uc_deviceList),
            //        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            //    config.fn_DeviceAdd(TAG, json_data);

            //    DeviceUpdate();
            //}
        }

        //DeviceAdd Window Callback Handler
        private void DeviceWindow_DevAddHandler(class_listener_list mcl_List)
        {
            //Console.WriteLine("DeviceWindow_DevAddHandler()");
            //Console.WriteLine("mcl_List.devSN : " + mcl_List.devSN);
            //Console.WriteLine("mcl_List.devColor : " + mcl_List.devColor);

            addPanelItem(mcl_List);
            //string json_data = JsonConvert.SerializeObject(getDeviceItemListToClass(uc_deviceList),
            //    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            //config.fn_DeviceAdd(TAG, json_data);
        }

        //DeviceEdit callback Method
        private void DeviceWindow_EditItemHandler(class_listener_list cl_List)
        {
            int resultNum = uc_deviceList.FindIndex(x => x.get_clList().devSN.Equals(cl_List.devSN));
            if (resultNum != -1)
            {
                class_listener_list clTmp = uc_deviceList[resultNum].get_clList();
                clTmp.devTagName = cl_List.devTagName;
                clTmp.tag_pos_x = cl_List.tag_pos_x;
                clTmp.tag_pos_y = cl_List.tag_pos_y;
                clTmp.tag_pos_z = cl_List.tag_pos_z;
                clTmp.time = cl_List.time;

                uc_deviceList[resultNum].set_clList(clTmp);

                string json_data = JsonConvert.SerializeObject(getDeviceItemListToClass(uc_deviceList),
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                config.fn_DeviceAdd(TAG, json_data);

                DeviceUpdate();
            }
        }

        //DeviceEdit callback Method
        private void DeviceWindow_RemoveItemHandler(class_listener_list cl_List)
        {
            Console.WriteLine("DeviceWindow_RemoveItemHandler()");

            int resultNum = uc_deviceList.FindIndex(x => x.get_clList().devSN.Equals(cl_List.devSN));

            uc_deviceList.RemoveAt(resultNum);
            removeParseData(cl_List.devSN);

            //string json_data = JsonConvert.SerializeObject(getDeviceItemListToClass(uc_deviceList),
            //    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            //config.fn_DeviceAdd(TAG, json_data);

            DeviceUpdate();
        }

        //Set File to UC_listener_device_tiem -> class_listsener_list
        private List<class_listener_list> getDeviceItemListToClass(List<UC_listener_device_item> muc_deviceListItem)
        {
            List<class_listener_list> cl_list = new List<class_listener_list>();
            for (int i = 0; i < muc_deviceListItem.Count; i++)
            {
                cl_list.Add(muc_deviceListItem[i].get_clList());
            }
            return cl_list;
        }

        private void DeviceUpdate()
        {
            removePanelItem();

            //string json_data = config.fn_DeviceAddRead(TAG);
            //var device_data = JsonConvert.DeserializeObject<List<class_listener_list>>(json_data);
            List<class_listener_list> mcl_list = new List<class_listener_list>();
            //if (device_data != null) mcl_list = device_data;

            List<class_listener_list> mcl_TagList = new List<class_listener_list>();
            List<class_listener_list> mcl_AnchorList = new List<class_listener_list>();

            int num_result_Tag = mcl_list.FindIndex(x => x.devType.Equals("Tag"));
            if (num_result_Tag != -1)
            {
                mcl_TagList = mcl_list.FindAll(x => x.devType.Equals("Tag"));
                mcl_TagList = mcl_TagList.OrderBy(x => x.devSN).ToList();
            }

            int num_result_Anchor = mcl_list.FindIndex(x => x.devType.Equals("Anchor"));
            if (num_result_Anchor != -1)
            {
                mcl_AnchorList = mcl_list.FindAll(x => x.devType.Equals("Anchor"));
                mcl_AnchorList = mcl_AnchorList.OrderBy(x => x.devSN).ToList();
            }

            for (int i = 0; i < mcl_TagList.Count; i++)
            {
                addPanelItem(mcl_TagList[i]);
            }

            for (int i = 0; i < mcl_AnchorList.Count; i++)
            {
                addPanelItem(mcl_AnchorList[i]);
            }
        }

        private void btn_sync_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //DeviceUpdate();

            removePanelItem();

            getMqtt();
        }

        bool fAutoAnchor = false;

        private void btn_autoAdd_Click(object sender, RoutedEventArgs e)
        {
            if (getMainConnectHandler())
            {
                clickAutoAnchor();
                fAutoAnchor = true;
                prevAnchorMillis = (long)(DateTime.UtcNow - config.Jan1st1970).TotalMilliseconds;
            }
        }
    }
}
