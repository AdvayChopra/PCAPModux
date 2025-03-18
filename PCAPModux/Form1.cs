using System;
using System.Windows.Forms;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace PCAPModux
{
    public partial class Form1 : Form
    {
        private int packetCount = 0; // Counter for packets

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

                    packetTreeView.Nodes.Clear();
                    packetTreeView.Nodes.Add("Reading packets from PCAP file...");

                    // Start capturing packets
                    captureDevice.Capture();

                    packetTreeView.Nodes.Add("PCAP file reading completed.");
                }
            }
            catch (Exception ex)
            {
                packetTreeView.Nodes.Add($"Error: {ex.Message}");
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
                packetCount++; // Increment packet count

                string senderMac = arpPacket.SenderHardwareAddress.ToString();
                string targetMac = arpPacket.TargetHardwareAddress.ToString();
                string senderVendor = MacAddressVendorLookup.GetVendorName(senderMac);
                string targetVendor = MacAddressVendorLookup.GetVendorName(targetMac);

                var packetNode = new TreeNode($"ARP Packet {packetCount}");
                packetNode.Nodes.Add($"Timestamp: {rawPacket.Timeval.Date}");
                packetNode.Nodes.Add($"Packet Length: {rawPacket.Data.Length} bytes");
                packetNode.Nodes.Add($"Sender MAC: {senderMac} ({senderVendor})");
                packetNode.Nodes.Add($"Sender IP: {arpPacket.SenderProtocolAddress}");
                packetNode.Nodes.Add($"Target MAC: {targetMac} ({targetVendor})");
                packetNode.Nodes.Add($"Target IP: {arpPacket.TargetProtocolAddress}");
                packetNode.Nodes.Add($"Opcode: {arpPacket.Operation}");
                packetNode.Nodes.Add($"Hardware Type: {arpPacket.HardwareAddressType}");
                packetNode.Nodes.Add($"Protocol Type: {arpPacket.ProtocolAddressType}");
                packetNode.Nodes.Add($"Hardware Size: {arpPacket.HardwareAddressLength} bytes");
                packetNode.Nodes.Add($"Protocol Size: {arpPacket.ProtocolAddressLength} bytes");

                // Extract and format ARP request information
                if (arpPacket.Operation == ArpOperation.Request)
                {
                    string arpInfo = $"Info: Who has {arpPacket.TargetProtocolAddress}? Tell {arpPacket.SenderProtocolAddress}";
                    packetNode.Nodes.Add(arpInfo);
                }

                packetTreeView.Invoke(new Action(() =>
                {
                    packetTreeView.Nodes.Add(packetNode);
                }));
            }
        }
    }
}