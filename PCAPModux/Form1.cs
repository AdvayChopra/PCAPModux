using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace PCAPModux
{
    public partial class Form1 : Form
    {
        private int packetCount = 0; // Counter for packets
        private Dictionary<string, string> requesters = new Dictionary<string, string>(); // MAC/IP addresses making requests
        private Dictionary<string, string> responders = new Dictionary<string, string>(); // MAC/IP addresses responding

        public Form1()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PCAP files (*.pcap)|*.pcap|All files (*.*)|*.*";
                openFileDialog.Title = "Select a PCAP file";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string pcapFilePath = openFileDialog.FileName;

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
                string senderIp = arpPacket.SenderProtocolAddress.ToString();
                string targetIp = arpPacket.TargetProtocolAddress.ToString();
                string senderVendor = MacAddressVendorLookup.GetVendorName(senderMac);
                string targetVendor = MacAddressVendorLookup.GetVendorName(targetMac);

                // Add to requesters or responders list
                if (arpPacket.Operation == ArpOperation.Request)
                {
                    if (!requesters.ContainsKey(senderMac))
                    {
                        requesters[senderMac] = senderIp;
                    }
                }
                else if (arpPacket.Operation == ArpOperation.InArpReply)
                {
                    if (!responders.ContainsKey(senderMac))
                    {
                        responders[senderMac] = senderIp;
                    }
                }

                var packetNode = new TreeNode($"ARP Packet {packetCount}");
                packetNode.Nodes.Add($"Timestamp: {rawPacket.Timeval.Date}");
                packetNode.Nodes.Add($"Packet Length: {rawPacket.Data.Length} bytes");
                packetNode.Nodes.Add($"Sender MAC: {senderMac} ({senderVendor})");
                packetNode.Nodes.Add($"Sender IP: {senderIp}");
                packetNode.Nodes.Add($"Target MAC: {targetMac} ({targetVendor})");
                packetNode.Nodes.Add($"Target IP: {targetIp}");
                packetNode.Nodes.Add($"Opcode: {arpPacket.Operation}");
                packetNode.Nodes.Add($"Hardware Type: {arpPacket.HardwareAddressType}");
                packetNode.Nodes.Add($"Protocol Type: {arpPacket.ProtocolAddressType}");
                packetNode.Nodes.Add($"Hardware Size: {arpPacket.HardwareAddressLength} bytes");
                packetNode.Nodes.Add($"Protocol Size: {arpPacket.ProtocolAddressLength} bytes");

                // Extract and format ARP request information
                if (arpPacket.Operation == ArpOperation.Request)
                {
                    string arpInfo = $"Info: Who has {targetIp}? Tell {senderIp}";
                    packetNode.Nodes.Add(arpInfo);
                }

                packetTreeView.Invoke(new Action(() =>
                {
                    packetTreeView.Nodes.Add(packetNode);
                }));
            }
        }

        private void showMetricsButton_Click(object sender, EventArgs e)
        {
            string metrics = "Requesters:\n";
            foreach (var requester in requesters)
            {
                metrics += $"MAC: {requester.Key}, IP: {requester.Value}\n";
            }

            metrics += "\nResponders:\n";
            if (responders.Count == 0)
            {
                metrics += "No responses\n";
            }
            else
            {
                foreach (var responder in responders)
                {
                    metrics += $"MAC: {responder.Key}, IP: {responder.Value}\n";
                }
            }

            MessageBox.Show(metrics, "Aggregated Metrics");
        }
    }
}