using System;
using System.Collections.Generic;
using System.Text;
using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;

namespace PCAPModux
{
    public partial class Form1 : Form
    {
        private int packetCount = 0; // Counter for packets
        private Dictionary<string, List<string>> requesters = new Dictionary<string, List<string>>(); // MAC/IP addresses making requests
        private Dictionary<string, string> responders = new Dictionary<string, string>(); // MAC/IP addresses responding
        private Dictionary<string, List<string>> dnsRequesters = new Dictionary<string, List<string>>(); // IP addresses making DNS requests
        private Dictionary<string, List<string>> dnsResponders = new Dictionary<string, List<string>>(); // IP addresses responding to DNS requests

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
                        // Reset packet counter
                        packetCount = 0;

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

            // Check if the packet is an ARP packet
            var arpPacket = packet.Extract<ArpPacket>();
            if (arpPacket != null)
            {
                HandleArpPacket(packet, rawPacket);
                return;
            }

            var ipPacket = packet.Extract<IPPacket>();
            if (ipPacket == null) return;

            var udpPacket = packet.Extract<UdpPacket>();
            if (udpPacket == null) return;

            // Check if the packet is a DNS packet
            if (udpPacket.DestinationPort == 53 || udpPacket.SourcePort == 53)
            {
                HandleDnsPacket(udpPacket.PayloadData, rawPacket, ipPacket, udpPacket);
            }
        }

        private void HandleArpPacket(Packet packet, RawCapture rawPacket)
        {
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
                        requesters[senderMac] = new List<string>();
                    }
                    if (!requesters[senderMac].Contains(senderIp))
                    {
                        requesters[senderMac].Add(senderIp);
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

        private void HandleDnsPacket(byte[] payloadData, RawCapture rawPacket, IPPacket ipPacket, UdpPacket udpPacket)
        {
            packetCount++; // Increment packet count

            var packetNode = new TreeNode($"DNS Packet {packetCount}");
            packetNode.Nodes.Add($"Timestamp: {rawPacket.Timeval.Date}");
            packetNode.Nodes.Add($"Packet Length: {rawPacket.Data.Length} bytes");
            packetNode.Nodes.Add($"Sender IP: {ipPacket.SourceAddress}");
            packetNode.Nodes.Add($"Destination IP: {ipPacket.DestinationAddress}");
            packetNode.Nodes.Add($"Source Port: {udpPacket.SourcePort}");
            packetNode.Nodes.Add($"Destination Port: {udpPacket.DestinationPort}");

            // Add to DNS requesters or responders list
            if (!dnsRequesters.ContainsKey(ipPacket.SourceAddress.ToString()))
            {
                dnsRequesters[ipPacket.SourceAddress.ToString()] = new List<string>();
            }
            if (!dnsRequesters[ipPacket.SourceAddress.ToString()].Contains(ipPacket.DestinationAddress.ToString()))
            {
                dnsRequesters[ipPacket.SourceAddress.ToString()].Add(ipPacket.DestinationAddress.ToString());
            }

            if (!dnsResponders.ContainsKey(ipPacket.DestinationAddress.ToString()))
            {
                dnsResponders[ipPacket.DestinationAddress.ToString()] = new List<string>();
            }
            if (!dnsResponders[ipPacket.DestinationAddress.ToString()].Contains(ipPacket.SourceAddress.ToString()))
            {
                dnsResponders[ipPacket.DestinationAddress.ToString()].Add(ipPacket.SourceAddress.ToString());
            }

            // Manually parse DNS packet
            int transactionId = (payloadData[0] << 8) | payloadData[1];
            int flags = (payloadData[2] << 8) | payloadData[3];
            int questionCount = (payloadData[4] << 8) | payloadData[5];
            int answerCount = (payloadData[6] << 8) | payloadData[7];
            int authorityCount = (payloadData[8] << 8) | payloadData[9];
            int additionalCount = (payloadData[10] << 8) | payloadData[11];

            packetNode.Nodes.Add($"Transaction ID: {transactionId}");
            packetNode.Nodes.Add($"Flags: {flags}");
            packetNode.Nodes.Add($"Questions: {questionCount}");
            packetNode.Nodes.Add($"Answers: {answerCount}");
            packetNode.Nodes.Add($"Authority RRs: {authorityCount}");
            packetNode.Nodes.Add($"Additional RRs: {additionalCount}");

            int offset = 12; // DNS header is 12 bytes

            // Parse Question Section
            for (int i = 0; i < questionCount; i++)
            {
                string question = ParseDnsName(payloadData, ref offset);
                int qtype = (payloadData[offset] << 8) | payloadData[offset + 1];
                int qclass = (payloadData[offset + 2] << 8) | payloadData[offset + 3];
                offset += 4;

                packetNode.Nodes.Add($"Question: {question}, Type: {qtype}, Class: {qclass}");
            }

            // Parse Answer Section
            for (int i = 0; i < answerCount; i++)
            {
                string answer = ParseDnsName(payloadData, ref offset);
                int atype = (payloadData[offset] << 8) | payloadData[offset + 1];
                int aclass = (payloadData[offset + 2] << 8) | payloadData[offset + 3];
                int ttl = (payloadData[offset + 4] << 24) | (payloadData[offset + 5] << 16) | (payloadData[offset + 6] << 8) | payloadData[offset + 7];
                int rdlength = (payloadData[offset + 8] << 8) | payloadData[offset + 9];
                offset += 10;
                string rdata = BitConverter.ToString(payloadData, offset, rdlength);
                offset += rdlength;

                packetNode.Nodes.Add($"Answer: {answer}, Type: {atype}, Class: {aclass}, TTL: {ttl}, RDATA: {rdata}");
            }

            // Parse Authority Section
            for (int i = 0; i < authorityCount; i++)
            {
                string authority = ParseDnsName(payloadData, ref offset);
                int atype = (payloadData[offset] << 8) | payloadData[offset + 1];
                int aclass = (payloadData[offset + 2] << 8) | payloadData[offset + 3];
                int ttl = (payloadData[offset + 4] << 24) | (payloadData[offset + 5] << 16) | (payloadData[offset + 6] << 8) | payloadData[offset + 7];
                int rdlength = (payloadData[offset + 8] << 8) | payloadData[offset + 9];
                offset += 10;
                string rdata = BitConverter.ToString(payloadData, offset, rdlength);
                offset += rdlength;

                packetNode.Nodes.Add($"Authority: {authority}, Type: {atype}, Class: {aclass}, TTL: {ttl}, RDATA: {rdata}");
            }

            // Parse Additional Section
            for (int i = 0; i < additionalCount; i++)
            {
                string additional = ParseDnsName(payloadData, ref offset);
                int atype = (payloadData[offset] << 8) | payloadData[offset + 1];
                int aclass = (payloadData[offset + 2] << 8) | payloadData[offset + 3];
                int ttl = (payloadData[offset + 4] << 24) | (payloadData[offset + 5] << 16) | (payloadData[offset + 6] << 8) | payloadData[offset + 7];
                int rdlength = (payloadData[offset + 8] << 8) | payloadData[offset + 9];
                offset += 10;
                string rdata = BitConverter.ToString(payloadData, offset, rdlength);
                offset += rdlength;

                packetNode.Nodes.Add($"Additional: {additional}, Type: {atype}, Class: {aclass}, TTL: {ttl}, RDATA: {rdata}");
            }

            packetTreeView.Invoke(new Action(() =>
            {
                packetTreeView.Nodes.Add(packetNode);
            }));
        }

        private string ParseDnsName(byte[] data, ref int offset)
        {
            StringBuilder name = new StringBuilder();
            int length = data[offset++];

            while (length != 0)
            {
                if ((length & 0xC0) == 0xC0)
                {
                    // Pointer to a previous name
                    int pointer = ((length & 0x3F) << 8) | data[offset++];
                    int savedOffset = offset;
                    offset = pointer;
                    name.Append(ParseDnsName(data, ref offset));
                    offset = savedOffset;
                    break;
                }
                else
                {
                    // Label
                    name.Append(Encoding.ASCII.GetString(data, offset, length));
                    offset += length;
                    length = data[offset++];
                    if (length != 0)
                    {
                        name.Append(".");
                    }
                }
            }

            return name.ToString();
        }

        private void showMetricsButton_Click(object sender, EventArgs e)
        {
            string metrics = "ARP Requesters:\n";
            if (requesters.Count == 0)
            {
                metrics += "No requests\n";
            }
            else
            {
                foreach (var requester in requesters)
                {
                    metrics += $"MAC: {requester.Key}, IPs: {string.Join(", ", requester.Value)}\n";
                }
            }

            metrics += "\nARP Responders:\n";
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

            metrics += "\nDNS Requesters:\n";
            if (dnsRequesters.Count == 0)
            {
                metrics += "No requests\n";
            }
            else
            {
                foreach (var requester in dnsRequesters)
                {
                    metrics += $"IP: {requester.Key}, Requested IPs: {string.Join(", ", requester.Value)}\n";
                }
            }

            metrics += "\nDNS Responders:\n";
            if (dnsResponders.Count == 0)
            {
                metrics += "No responses\n";
            }
            else
            {
                foreach (var responder in dnsResponders)
                {
                    metrics += $"IP: {responder.Key}, Responded to IPs: {string.Join(", ", responder.Value)}\n";
                }
            }

            MessageBox.Show(metrics, "Aggregated Metrics");
        }
    }
}