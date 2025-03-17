using System;
using System.Windows.Forms;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace PCAPModux
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            // Path to your PCAP file (Change this accordingly)
            string pcapFilePath = @"C:\Users\chopr\Downloads\arp-storm.pcap";

            try
            {
                // Open the PCAP file
                using (var captureDevice = new CaptureFileReaderDevice(pcapFilePath))
                {
                    captureDevice.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival);
                    captureDevice.Open();

                    outputTextBox.AppendText("Reading packets from PCAP file..." + Environment.NewLine);

                    // Start capturing packets
                    captureDevice.Capture();

                    outputTextBox.AppendText("PCAP file reading completed." + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                outputTextBox.AppendText($"Error: {ex.Message}" + Environment.NewLine);
            }
        }

        private void OnPacketArrival(object sender, PacketCapture e)
        {
            var rawPacket = e.GetPacket();
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

            var ethernetPacket = packet.Extract<EthernetPacket>();
            if (ethernetPacket == null) return;

            var arpPacket = packet.Extract<ArpPacket>();
            if (arpPacket != null)
            {
                outputTextBox.Invoke(new Action(() =>
                {
                    outputTextBox.AppendText("---- ARP Packet ----" + Environment.NewLine);
                    outputTextBox.AppendText($"Timestamp: {rawPacket.Timeval.Date}" + Environment.NewLine);
                    outputTextBox.AppendText($"Sender MAC: {arpPacket.SenderHardwareAddress}" + Environment.NewLine);
                    outputTextBox.AppendText($"Sender IP: {arpPacket.SenderProtocolAddress}" + Environment.NewLine);
                    outputTextBox.AppendText($"Target MAC: {arpPacket.TargetHardwareAddress}" + Environment.NewLine);
                    outputTextBox.AppendText($"Target IP: {arpPacket.TargetProtocolAddress}" + Environment.NewLine);
                    outputTextBox.AppendText("---------------------" + Environment.NewLine + Environment.NewLine);
                }));
            }
        }
    }
}