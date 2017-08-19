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
    public class Mpeg2TransportStreamReader
    {
        private Mpeg2TransportStream m_stream;
        private TransportPacket m_pendingPacket;

        private List<TransportPacket> m_psiBuffer;
        private int m_pcrPID = -1;
        private List<ushort> m_psiTableIDs = new List<ushort>();
        private Dictionary<ushort, int> m_pesExpectedLength = new Dictionary<ushort, int>();
        private KeyValuePairList<ushort, List<TransportPacket>> m_buffer = new KeyValuePairList<ushort, List<TransportPacket>>();
        
        public Mpeg2TransportStreamReader(Mpeg2TransportStream stream)
        {
            m_stream = stream;
        }

        /// <summary>
        /// Return null if EOF, empty list if packet does not end sequence
        /// </summary>
        public List<TransportPacket> ReadPacketSequence()
        {
            List<TransportPacket> result;
            do
            {
                result = ProcessPendingPacket();
            }
            while (result != null && result.Count == 0);
            return result;
        }

        /// <summary>
        /// Return null if EOF, empty list if packet does not end sequence
        /// </summary>
        private List<TransportPacket> ProcessPendingPacket()
        {
            TransportPacket packet;
            if (m_pendingPacket == null)
            {
                packet = m_stream.ReadPacket();
            }
            else
            {
                packet = m_pendingPacket;
                m_pendingPacket = null;
            }

            if (packet == null)
            {
                if (m_buffer.Count == 0)
                {
                    return null;
                }
                List<TransportPacket> result = m_buffer[0].Value;
                m_buffer.RemoveAt(0);
                return result;
            }

            bool isPSI = packet.Header.PID == Mpeg2TransportStream.ProgramAssociationTablePID || m_psiTableIDs.Contains(packet.Header.PID);
            bool isPCR = (packet.Header.PID == m_pcrPID);

            if (isPSI)
            {
                if (packet.Header.PayloadUnitStartIndicator)
                {
                    m_psiBuffer = new List<TransportPacket>();
                }
                else if (m_psiBuffer == null)
                {
                    // Incomplete PSI section or SSIF
                    List<TransportPacket> result = new List<TransportPacket>();
                    result.Add(packet);
                    return result;
                }
                m_psiBuffer.Add(packet);

                byte[] psiBuffer = TransportPacketHelper.AssemblePayload(m_psiBuffer);
                if (ProgramSpecificInformationSection.IsSectionComplete(psiBuffer))
                {
                    List<TransportPacket> result = new List<TransportPacket>();
                    result.AddRange(m_psiBuffer);
                    m_psiBuffer = null;
                    ProgramSpecificInformationSection section = ProgramSpecificInformationSection.ReadSection(psiBuffer);
                    if (section is ProgramAssociationSection)
                    {
                        m_psiTableIDs = new List<ushort>();
                        m_psiTableIDs.AddRange(((ProgramAssociationSection)section).Programs.Values);
                    }
                    else if (section is ProgramMapSection)
                    {
                        m_pcrPID = ((ProgramMapSection)section).PCRPID;
                    }
                    return result;
                }
                else
                {
                    return new List<TransportPacket>();
                }
            }
            else if (isPCR)
            {
                List<TransportPacket> result = new List<TransportPacket>();
                result.Add(packet);
                return result;
            }
            else
            {
                ushort pid = packet.Header.PID;
                if (pid == Mpeg2TransportStream.NullPacketPID)
                {
                    List<TransportPacket> result = new List<TransportPacket>();
                    result.Add(packet);
                    return result;
                }
                else if (packet.Header.PayloadUnitStartIndicator)
                {
                    if (packet.Header.PayloadExist)
                    {
                        int index = m_buffer.IndexOf(pid);
                        if (index >= 0)
                        {
                            List<TransportPacket> result = m_buffer[index].Value;
                            m_buffer.RemoveAt(index);
                            m_pendingPacket = packet;
                            return result;
                        }
                        else
                        {
                            AddToBuffer(pid, packet);
                            return new List<TransportPacket>();
                        }
                    }
                    else
                    {
                        List<TransportPacket> result = new List<TransportPacket>();
                        result.Add(packet);
                        return result;
                    }
                }
                else // PayloadUnitStartIndicator == false
                {
                    int index = m_buffer.IndexOf(pid);
                    if (index >= 0)
                    {
                        m_buffer[index].Value.Add(packet);
                        return new List<TransportPacket>();
                    }
                    else
                    {
                        List<TransportPacket> result = new List<TransportPacket>();
                        result.Add(packet);
                        return result;
                    }
                }
            }
        }

        public void AddToBuffer(ushort pid, TransportPacket packet)
        {
            List<TransportPacket> packets = new List<TransportPacket>();
            packets.Add(packet);
            m_buffer.Add(pid, packets);
        }
    }
}
