using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using wpf_UWB_GUI.Listener;
using static wpf_UWB_GUI.UC_Menu_listener;

namespace wpf_UWB_GUI
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>

    //Menu_gateway_page

    //Get Connect State
    public delegate bool MainGetConnectStateHandler();

    //Get Set Remove Parse Data
    public delegate List<class_listener_list> MainGetAnchorListHandler();
    public delegate void MainClickAutoAnchorHandler();
    public delegate void MainSendMqttHandler(String Topic, String Message, bool retain);
    public delegate void MainRemoveDataHandler(String mdevSN);
    public delegate List<class_listener_list> MainGetParseDataAllHandler();
    public delegate void MainSetFilterDataHandler(class_listener_list cl_Tmp);

    public delegate void MainSetSerialConnectHandler(SerialConnect serialConnect);
    public delegate void MainSetMqttConnectHandler(MqttConnect mqttConnect);

    public delegate void MainGetMqttHandler();


    public partial class MainWindow : Window
    {
        String TAG = "MainWindow";

        //타이틀 게이트웨이
        const int StateTitleGateway = 1;
        //타이틀 리스너
        const int StateTitleListener = 2;

        //통신 설정
        const int StateTab_Gat_com = 1;
        //필터 설정
        const int StateTab_Gat_fil = 2;
        //장비 관리
        const int StateTab_Gat_dev = 3;
        //맵
        const int StateTab_Gat_map = 4;
        //전체 설정
        const int StateTab_Gat_set = 5;
        //Mqtt 설정
        const int StateTab_Gat_mqtt = 6;
        //프로그램 정보
        const int StateTab_Gat_sys = 7;

        int nowTitleState = StateTitleListener;
        int prevTitleState = 0;
        int nowTabState = StateTab_Gat_com;
        int prevTabState = 0;
        int nowMainState = StateTab_Gat_com;
        int prevMainState = 0;

        public static Frame _frame_listener_com;
        public static Frame _frame_listener_filter;

        //MENU BAR
        UC_Menu_listener ucMenuListener = new UC_Menu_listener();

        //LISTENER MAIN PAGE
        UC_main_listener_com ucMainListenerCom = new UC_main_listener_com();
        UC_main_listener_filter ucMainListenerFilter = new UC_main_listener_filter();
        UC_main_listener_device ucMainListenerDevice = new UC_main_listener_device();
        UC_main_listener_map ucMainListenerMap = new UC_main_listener_map();
        UC_main_listener_setting ucMainListenerSetting = new UC_main_listener_setting();
        UC_main_listener_info ucMainListenerInfo = new UC_main_listener_info();
        UC_main_listener_mqtt ucMainMqttInfo = new UC_main_listener_mqtt();

        bool fListenerConnectState = false;

        List<class_listener_list> listListenerParse = new List<class_listener_list>();

        DispatcherTimer timer10hz = new DispatcherTimer();


        SerialConnect serialConnect;
        MqttConnect mqttConnect;


        public MainWindow()
        {
            InitializeComponent();

            //Properties.Settings.Default.Reset();
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("MainForm_Loaded()");

            prevTitleState = nowTitleState;
            prevTabState = nowTabState;
            prevMainState = nowMainState;

            timer10hz.Interval = TimeSpan.FromMilliseconds(0.01);
            timer10hz.Tick += new EventHandler(timer10hz_Tick);
            timer10hz.Start();

            ucMenuListener.setListenerMenuButton += new MainGetMenuListenerDataEventHandler(this.setListenerMenuButton);

            tabBarLayout.Children.Clear();
            tabBarLayout.Children.Add(ucMenuListener);

            //Connection State Hanlder
            ucMainListenerFilter.getMainConnectHandler += new MainGetConnectStateHandler(getMainConnectState);
            ucMainListenerDevice.getMainConnectHandler += new MainGetConnectStateHandler(getMainConnectState);
            ucMainListenerMap.getMainConnectHandler += new MainGetConnectStateHandler(getMainConnectState);

            //com -> Main ( UWB DATA )
            ucMainListenerCom.listenerParse += new UC_main_listener_com.OnListenerParseHandler(setListenerParse);
            ucMainListenerCom.listenerConnect += new UC_main_listener_com.OnListenerConnectHandler(setListenerState);
            ucMainListenerCom.setSerialHandler += new MainSetSerialConnectHandler(setSerialHandler);

            //Main -> Filter ( UWB DATA )
            ucMainListenerFilter.setFilterHandler += new MainSetFilterDataHandler(setFilterHandler);
            ucMainListenerFilter.getParseDataAll += new MainGetParseDataAllHandler(getListenerParseDataAll);

            //Main -> Device ( UWB DATA )
            ucMainListenerDevice.getParseDataAll += new MainGetParseDataAllHandler(getListenerParseDataAll);
            ucMainListenerDevice.removeParseData += new MainRemoveDataHandler(removeListenerParseData);
            ucMainListenerDevice.getAnchorList += new MainGetAnchorListHandler(getAnchorList);
            ucMainListenerDevice.clickAutoAnchor += new MainClickAutoAnchorHandler(clickAutoAnchor);
            ucMainListenerDevice.sendMqtt += new MainSendMqttHandler(sendMqttDataHandler);
            ucMainListenerDevice.getMqtt += new MainGetMqttHandler(getMqttDataHandler);

            //Main -> Map ( UWB DATA )
            ucMainListenerMap.getParseDataAll += new MainGetParseDataAllHandler(getListenerParseDataAll);

            //Main -> Mqtt ( UWB DATA )
            ucMainMqttInfo.setMqttHandler += new MainSetMqttConnectHandler(setMqttHandler);
            ucMainMqttInfo.listenerParse += new UC_main_listener_mqtt.OnListenerParseHandler(setMqttParse);

            mainPageLayout.Navigate(ucMainListenerCom);

        }

        private void getMqttDataHandler()
        {
            if (mqttConnect == null) return;
            if (mqttConnect.IsConnected()) mqttConnect.mqtt_AllSub();

        }

        private void sendMqttDataHandler(String Topic, String Message, bool retain)
        {
            if (mqttConnect == null) return;
            if (mqttConnect.IsConnected()) mqttConnect.mqtt_Pub(Topic, Message, retain);
        }

        private void setMqttHandler(MqttConnect mMqtt)
        {
            Console.WriteLine("setMqttHandler()");
            mqttConnect = mMqtt;
        }

        private void setSerialHandler(SerialConnect mSerial)
        {
            Console.WriteLine("setSerialHandler()");
            serialConnect = mSerial;

            if (serialConnect == null) return;
            if (serialConnect.fConnectState())
                serialConnect.sp_EnterTwice();
        }

        private void clickAutoAnchor()
        {
            if (serialConnect != null)
            {
                serialConnect.sp_AnchorList();
            }
        }

        private List<class_listener_list> getAnchorList()
        {
            if (serialConnect != null)
            {
                return serialConnect.getAnchorList();
            }
            return null;
        }

        private List<class_listener_list> getMqttList()
        {
            if (mqttConnect != null)
            {
                return mqttConnect.getMqttList();
            }
            return null;
        }


        private void setFilterHandler(class_listener_list cl_Tmp)
        {

            String topic = "";
            String message = "";

            topic = "/tag/" + cl_Tmp.devSN.ToLower() + "/filteredData/";
            message = config.mqttSendFilterTagMessage(cl_Tmp);

            sendMqttDataHandler(topic, message, true);
        }

        private bool getMainConnectState()
        {
            return fListenerConnectState;
        }

        private void setMqttParse(List<class_listener_list> mMqttList)
        {
            listListenerParse = config.DeepCopy(mMqttList);
        }


        private void setListenerParse(List<class_listener_list> mlistListenerParse)
        {
            for (int i = 0; i < mlistListenerParse.Count; i++)
            {
                if (mlistListenerParse[i].devType.Contains("TAG"))
                {
                    String topic = "";
                    String message = "";

                    topic = "/tag/" + mlistListenerParse[i].devSN.ToLower() + "/rawData/";
                    message = config.mqttSendTagMessage(mlistListenerParse[i]);

                    sendMqttDataHandler(topic, message, true);
                }
                if (mlistListenerParse[i].devType.Contains("ANCHOR"))
                {
                    String topic = "";
                    String message = "";

                    topic = "/anchor/" + mlistListenerParse[i].devSN.ToLower();
                    message = config.mqttSendAnchorMessage(mlistListenerParse[i]);

                    sendMqttDataHandler(topic, message, true);
                }
            }
        }

        private void removeListenerParseData(String mdevSN)
        {
            int resultNum = listListenerParse.FindIndex(x => x.devSN.Equals(mdevSN));
            if (resultNum != -1)
            {
                listListenerParse.RemoveAt(resultNum);
            }
        }

        private List<class_listener_list> getListenerParseDataAll()
        {
            if (listListenerParse.Count != 0)
            {
                return listListenerParse;
            }
            else
            {
                return null;
            }
        }

        private void removeListenerParseDataAll()
        {
            listListenerParse.Clear();
        }

        private class_listener_list getListenerParseDataOne()
        {
            if (listListenerParse.Count != 0)
                return listListenerParse[0];
            else
                return null;
        }

        private void removeListenerParseDataOne()
        {
            listListenerParse.RemoveAt(0);
        }

        private void setListenerState(bool fState)
        {
            fListenerConnectState = fState;
        }

        private void timer10hz_Tick(object sender, EventArgs e)
        {
            if (nowTitleState != prevTitleState)
            {
                prevTitleState = nowTitleState;
            }

            if (nowTabState != prevTabState)
            {
                prevTabState = nowTabState;
            }

            if (nowMainState != prevMainState)
            {
                prevMainState = nowMainState;
            }
        }

        private void btn_listener_click(object sender, RoutedEventArgs e)
        {
            nowTitleState = StateTitleListener;
            btn_Title_menu_click(nowTitleState);
        }

        private void btn_Title_menu_click(int state)
        {
            tabBarLayout.Children.Clear();
        }

        public void setListenerMenuButton(int btnNumber)
        {
            switch (btnNumber)
            {
                case StateTab_Gat_com:
                    lbl_TopTitle.Content = "통신 설정";
                    lbl_TopSubTitle.Content = "리스너 장비와 통신 연결을 관리합니다.";
                    mainPageLayout.Navigate(ucMainListenerCom);
                    break;
                case StateTab_Gat_fil:
                    lbl_TopTitle.Content = "필터 설정";
                    lbl_TopSubTitle.Content = "장치 필터의 설정값을 관리합니다.";
                    mainPageLayout.Navigate(ucMainListenerFilter);
                    break;
                case StateTab_Gat_dev:
                    lbl_TopTitle.Content = "장비 관리";
                    lbl_TopSubTitle.Content = "장비의 연결상태, 표시색상 등 상태를 관리합니다.";
                    mainPageLayout.Navigate(ucMainListenerDevice);
                    break;
                case StateTab_Gat_map:
                    lbl_TopTitle.Content = "맵";
                    lbl_TopSubTitle.Content = "장비의 현재상태, 위치를 나타냅니다.";
                    mainPageLayout.Navigate(ucMainListenerMap);
                    break;
                case StateTab_Gat_set:
                    lbl_TopTitle.Content = "설정";
                    lbl_TopSubTitle.Content = "각종 설정을 합니다.";
                    mainPageLayout.Navigate(ucMainListenerSetting);
                    break;
                case StateTab_Gat_mqtt:
                    lbl_TopTitle.Content = "Mqtt 설정";
                    lbl_TopSubTitle.Content = "Mqtt 서버의 IP와 포트를 입력합니다.";
                    mainPageLayout.Navigate(ucMainMqttInfo);
                    break;
                case StateTab_Gat_sys:
                    lbl_TopTitle.Content = "정보";
                    lbl_TopSubTitle.Content = "회사 정보, 프로그램 정보를 나타냅니다.";
                    mainPageLayout.Navigate(ucMainListenerInfo);
                    break;
            }
        }


        /********************************
         * CUSTOM TITLE BAR START
         ********************************/

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            if (App.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                App.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else if (App.Current.MainWindow.WindowState == WindowState.Normal)
            {
                App.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Close();
        }

        private void TitleDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (App.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                App.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else if (App.Current.MainWindow.WindowState == WindowState.Normal)
            {
                App.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        private void TitleMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                App.Current.MainWindow.DragMove();
            }
        }

        /********************************
         * CUSTOM TITLE BAR END
         ********************************/

        //Form Size Changed Event
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double thisWidth = this.Height / 0.75f;
            this.Width = thisWidth;
        }
    }
}
