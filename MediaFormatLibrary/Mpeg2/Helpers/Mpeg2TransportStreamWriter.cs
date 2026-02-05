/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.Mpeg2
{
    public class Mpeg2TransportStreamWriter
    {
        /// <summary>
        /// PTS starts at an arbitrary value, it's not specified in the standard.
        /// Most programs/devices that produce transport streams start at 0 for the beginning of each stream.
        /// tsMuxeR uses 378000000.
        /// </summary>
        public const ulong ProgramClockReferenceBase = 0;
        public const uint PTSResolution = 90000; // time units per second, [ISO/IEC 13818-1] "Time stamps are generally in units of 90 KHz".
        // [2.B Audio Visual Application Format Specifications for BD-ROM]:
        // "The maximum multiplex rate of the BDAV MPEG-2 Transport Stream is 48Mbps"
        public const int BluRayMaximumMultiplexRate = 48000000;

        private Mpeg2TransportStream m_stream;
        // [ISO/IEC 13818-1] The continuity_counter is a 4-bit field incrementing with each Transport Stream packet with the same PID
        private Dictionary<ushort, int> m_continuityCounter = new Dictionary<ushort,int>();
        private double m_systemTimeClock; // In seconds (to avoid integer precision issues)

        public Mpeg2TransportStreamWriter(Mpeg2TransportStream stream)
        {
            m_stream = stream;
        }

        public void WritePCRPacket(ushort pid)
        {
            // In a packet that contains a PCR, the PCR will be a few ticks later than the arrival_time_stamp.
            // The exact difference between the arrival_time_stamp and the PCR (and the number of bits between them)
            // indicates the intended fixed bitrate of the variable rate Transport Stream.
            TransportPacket packet = new TransportPacket(m_stream.IsBluRayTransportStream);
            packet.Header.PID = pid;
            packet.Header.AdaptationFieldExist = true;
            packet.AdaptationField.FieldLength = TransportPacket.PacketLength - TransportPacketHeader.Length - 1;
            packet.AdaptationField.PCRFlag = true;
            // Note: The PCR represent the system clock, and thus must be equal to or greater than the value in the previous packet.
            packet.AdaptationField.ProgramClockReferenceBase = (ulong)(ProgramClockReferenceBase + m_systemTimeClock * PTSResolution);
            // [ISO/IEC 13818-1] The continuity_counter shall not be incremented when the adaptation_field_control of the packet equals '00' or '10'.
            WritePacketAndIncrementClock(packet, false);
        }

        public void WritePSISection(ushort pid, ProgramSpecificInformationSection section)
        {
            byte[] payload = section.GetBytes();
            List<TransportPacket> packets = Packetize(payload, true);
            foreach (TransportPacket packet in packets)
            {
                packet.Header.PID = pid;
                bool incrementContinuityCountr = false;
                if (section is SelectionInformationSection)
                {
                    incrementContinuityCountr = true;
                }
                WritePacketAndIncrementClock(packet, incrementContinuityCountr);
            }
        }

        public void WritePesPacket(ushort pid, PesPacket pesPacket)
        {
            byte[] payload = pesPacket.GetBytes();
            List<TransportPacket> packets = Packetize(payload, false);
            foreach(TransportPacket packet in packets)
            {
                packet.Header.PID = pid;
                packet.Header.TransportPriority = true;
                WritePacketAndIncrementClock(packet, true);
            }
        }

        public void FillAlignedUnit(int alignmentBoundary)
        {
            while (m_stream.Position % alignmentBoundary != 0)
            {
                TransportPacket nullPacket = new TransportPacket(m_stream.IsBluRayTransportStream);
                nullPacket.Header.PID = Mpeg2TransportStream.NullPacketPID;
                WritePacketAndIncrementClock(nullPacket, false);
            }
        }

        /// <param name="incrementContinuityCountr">Increment continuity counter after writing this packet</param>
        private void WritePacketAndIncrementClock(TransportPacket packet, bool incrementContinuityCountr)
        {
            if (m_stream.IsBluRayTransportStream)
            {
                // Note: both ArrivalTimeStamp and the PCR represent the system clock, and thus must be equal to or
                // greater than the value in the previous packet.
                // 27 MHz = 300 * 90 KHz
                packet.ExtraHeader.ArrivalTimeStamp = (uint)(ProgramClockReferenceBase + 300 * m_systemTimeClock * PTSResolution);
            }
            if (!m_continuityCounter.ContainsKey(packet.Header.PID))
            {
                m_continuityCounter[packet.Header.PID] = 0;
            }
            int continuityCounter = m_continuityCounter[packet.Header.PID];
            packet.Header.ContinuityCounter = (byte)(continuityCounter % 16);
            m_stream.WritePacket(packet);
            double packetTimeSpan = (double)packet.Length / BluRayMaximumMultiplexRate;
            m_systemTimeClock += packetTimeSpan;
            if (incrementContinuityCountr)
            {
                m_continuityCounter[packet.Header.PID]++;
            }
        }

        private List<TransportPacket> Packetize(byte[] payload, bool isStuffingAllowed)
        {
            List<TransportPacket> result = new List<TransportPacket>();
            int maxPayloadBytesPerPacket = TransportPacket.PacketLength - TransportPacketHeader.Length;
            int packetCount = (int)Math.Ceiling((double)payload.Length / maxPayloadBytesPerPacket);
            for (int index = 0; index < packetCount; index++)
            {
                TransportPacket packet = new TransportPacket(m_stream.IsBluRayTransportStream);
                packet.Header.PayloadUnitStartIndicator = (index == 0);
                packet.Header.PayloadExist = true;
                int bytesInPayload = Math.Min(payload.Length - index * maxPayloadBytesPerPacket, maxPayloadBytesPerPacket);
                if ((bytesInPayload < maxPayloadBytesPerPacket) && !isStuffingAllowed)
                {
                    // Packet stuffing bytes of value 0xFF may be found in the payload of Transport Stream packets carrying PSI and/or private_sections.
                    // i.e. for PES we must use adaptation field
                    packet.Header.AdaptationFieldExist = true;
                    packet.AdaptationField.FieldLength = (byte)(maxPayloadBytesPerPacket - bytesInPayload - 1);
                }
                packet.Payload = ByteReader.ReadBytes(payload, index * maxPayloadBytesPerPacket, bytesInPayload); ;
                result.Add(packet);
            }
            return result;
        }

        /// <param name="systemTimeClock">seconds since the first packet was written</param>
        public void SetSystemTimeClock(double systemTimeClock)
        {
            m_systemTimeClock = systemTimeClock;
        }
    }
}
