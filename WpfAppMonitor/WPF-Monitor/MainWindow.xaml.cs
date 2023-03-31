using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using OpenHardwareMonitor.Hardware;


namespace WpfAppMonitor
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            string[] driveInfos;
            InitializeComponent();

            SystemInfo si = new SystemInfo();

            driveInfos = si.getDriveInfos(); 
            osName.Content = si.getOsInfos("os");
            osArch.Content = si.getOsInfos("arch");
            cpuName.Content = si.getCPUInfos();
            gpuName.Content = si.getGPUInfos();
            ListeDisques.ItemsSource = driveInfos;
            

            // Appel toute les secondes.
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            SystemInfo si = new SystemInfo();

            double ramTotal = si.getRamInfos();
            double ramFree = si.getRAMUsage();
            double ramUsed = ramTotal - ramFree;

            RamTotal.Content = "Total : " + ramTotal.ToString("0.00") + " GB";
            RamFree.Content = "Disponible : " + ramFree.ToString("0.00") + " GB";
            RamUsed.Content = "Utilisé : " + (ramTotal - ramFree).ToString("0.00") + " GB";

            ramBar.Value = ramUsed / ramTotal * 100;
            cpuUsage.Content = si.getCPUUsage().ToString("0.00") + "%";
        }

        private void infoMsg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://google.fr");
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }

    public class SystemInfo
    {
        protected PerformanceCounter cpuCounter = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
        protected PerformanceCounter ramCounter;

        public string getOsInfos(string param)
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

            foreach (ManagementObject mo in mos.Get())
            {
                switch (param)
                {
                    case "os":
                        return mo["Caption"].ToString();
                    case "arch":
                        return mo["OSArchitecture"].ToString();
                    case "osv":
                        return mo["CSDVersion"].ToString();
                }
            }

            return "";
        }

        public string getCPUInfos()
        {
            RegistryKey processor_name = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0", RegistryKeyPermissionCheck.ReadSubTree);
            if (processor_name != null)
            {
                return processor_name.GetValue("ProcessorNameString").ToString();
            }

            return "";
        }

        public string getGPUInfos()
        {
            string subkey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\WinSAT";

            RegistryKey localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            string value = localKey.OpenSubKey(subkey).GetValue("PrimaryAdapterString").ToString();

            return value;
        }

        public double getCPUUsage()
        {
            return cpuCounter.NextValue();
        }

        public double getRAMUsage()
        {
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            return ramCounter.NextValue() * 0.0009765625;
        }

        public double getRamInfos()
        {
            double output = 0;
            ManagementObjectSearcher Search = new ManagementObjectSearcher("Select * From Win32_ComputerSystem");

            foreach (ManagementObject Mobject in Search.Get())
            {
                double Ram_Bytes = (Convert.ToDouble(Mobject["TotalPhysicalMemory"]));
                output = (Ram_Bytes / 1073741824);
            }

            return output;
        }

        public string[] getDriveInfos()
        { 
            DriveInfo[] alldrives = DriveInfo.GetDrives();
            string[] output = new string[alldrives.Length];
            int cpt = 0;

            foreach(DriveInfo drive in alldrives)
            {
                output[cpt] = drive.VolumeLabel + " " + drive.Name + " " + drive.AvailableFreeSpace / 1073741824 + " Go libres / " + drive.TotalSize / 1073741824 + " Go";
                cpt++;
            }

            return output;
        }
    }

    
}
