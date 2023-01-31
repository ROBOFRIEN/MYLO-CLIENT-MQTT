﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MYLO_CLIENT_MQTT.Listener
{
    /// <summary>
    /// DeviceInformationWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DeviceEditWindow : Window
    {
        String TAG = "DeviceEditWindow";

        public delegate void OnDevEditHandler(class_listener_list cl_Addlistener);
        public event OnDevEditHandler devEditHandler;

        class_listener_list clList;

        List<Control> listCursorControl = new List<Control>();
        List<Control> listFieldCursorControl = new List<Control>();

        public DeviceEditWindow()
        {
            InitializeComponent();

            listFieldCursorControl.Add(textBox_devName);
            listFieldCursorControl.Add(textBox_positionX);
            listFieldCursorControl.Add(textBox_positionY);
            listFieldCursorControl.Add(textBox_positionZ);
            listCursorControl.Add(btn_save);
            listCursorControl.Add(btn_close);

            for (int i = 0; i < listFieldCursorControl.Count; i++)
            {
                listFieldCursorControl[i].MouseEnter += new MouseEventHandler(mouseFieldEnterHandler);
                listFieldCursorControl[i].MouseLeave += new MouseEventHandler(mouseLeaveHandler);
            }

            for (int i = 0; i < listCursorControl.Count; i++)
            {
                listCursorControl[i].MouseEnter += new MouseEventHandler(mouseEnterHandler);
                listCursorControl[i].MouseLeave += new MouseEventHandler(mouseLeaveHandler);
            }

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            textBox_devName.Focus();
        }

        private void mouseEnterHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void mouseFieldEnterHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.IBeam;
        }

        private void mouseLeaveHandler(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void DeviceEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            textBox_positionX.PreviewKeyDown += textbox_Position_KeyPress;
            textBox_positionY.PreviewKeyDown += textbox_Position_KeyPress;
            textBox_positionZ.PreviewKeyDown += textbox_Position_KeyPress;

            textBox_devName.GotFocus += textbox_GotFocus;
            textBox_positionX.GotFocus += textbox_GotFocus;
            textBox_positionY.GotFocus += textbox_GotFocus;
            textBox_positionZ.GotFocus += textbox_GotFocus;
        }

        private void textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tmpTextBox = (TextBox)sender;
            tmpTextBox.Select(tmpTextBox.Text.Length, 0);
        }

        public void set_clList(class_listener_list mClList)
        {
            clList = mClList;
        }

        public void UpdateUI()
        {

            label_devSN.Content = clList.devSN;
            label_devType.Content = clList.devType;
            textBox_devName.Text = clList.devTagName;

            //Type TAG
            if (clList.devType.Equals("Tag"))
            {
                panel_tag.Visibility = Visibility.Visible;
                panel_anchor.Visibility = Visibility.Hidden;

                label_devPositionRaw.Content = clList.devPosition;
                label_devPositionFilter.Content = clList.devFilter;
            }
            //Type Anchor
            else
            {
                panel_tag.Visibility = Visibility.Hidden;
                panel_anchor.Visibility = Visibility.Visible;

                textBox_positionX.Text = clList.tag_pos_x.ToString();
                textBox_positionY.Text = clList.tag_pos_y.ToString();
                textBox_positionZ.Text = clList.tag_pos_z.ToString();
            }



            //control_Panel_visible();
        }


        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (textBox_positionX.Text.Length == 0)
            {
                MessageBox.Show("Position X 의 값이 비었습니다.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox_positionX.Focus();
                return;
            }
            if (textBox_positionY.Text.Length == 0)
            {
                MessageBox.Show("Position Y 의 값이 비었습니다.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox_positionY.Focus();
                return;
            }
            if (textBox_positionZ.Text.Length == 0)
            {
                MessageBox.Show("Position Z 의 값이 비었습니다.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox_positionZ.Focus();
                return;
            }

            if (devEditHandler != null)
            {
                class_listener_list clList = new class_listener_list();

                if (label_devType.Content.ToString().Equals("Anchor"))
                {
                    clList.devSN = label_devSN.Content.ToString();
                    clList.devType = label_devType.Content.ToString();
                    if (textBox_devName.Text.Length == 0)
                        clList.devTagName = "Unknown";
                    else
                        clList.devTagName = textBox_devName.Text.ToString();
                    clList.tag_pos_x = Double.Parse(textBox_positionX.Text.ToString());
                    clList.tag_pos_y = Double.Parse(textBox_positionY.Text.ToString());
                    clList.tag_pos_z = Double.Parse(textBox_positionZ.Text.ToString());
                }
                else
                {
                    clList.devSN = label_devSN.Content.ToString();
                    clList.devType = label_devType.Content.ToString();
                    if (textBox_devName.Text.Length == 0)
                        clList.devTagName = "Unknown";
                    else
                        clList.devTagName = textBox_devName.Text.ToString();
                }

                devEditHandler(clList);
            }
            this.Close();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void control_Panel_visible()
        {
            panel_tag.Visibility = Visibility.Hidden;
            panel_anchor.Visibility = Visibility.Hidden;

            if (label_devType.Content.Equals("Tag"))
            {
                panel_tag.Visibility = Visibility.Visible;
            }
            else
            {
                panel_anchor.Visibility = Visibility.Visible;
            }
        }

        private void textbox_Position_KeyPress(object sender, KeyEventArgs e)
        {
            TextBox tmpTextBox = (TextBox)sender;
            if (!(((Key.D0 <= e.Key) && (e.Key <= Key.D9))
                 || ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9))
                 || e.Key == Key.Decimal
                 || e.Key == Key.OemPeriod
                 || e.Key == Key.Tab
                 || e.Key == Key.Left
                 || e.Key == Key.Right
                 || e.Key == Key.Up
                 || e.Key == Key.Down
                 || e.Key == Key.Back))
            {
                e.Handled = true;
            }
            if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
            {
                if (tmpTextBox.Text.ToString().Contains("."))
                {
                    e.Handled = true;
                }
            }
        }

    }
}
