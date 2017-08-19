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
using MediaFormatLibrary.Mpeg2;
using MediaFormatLibrary.AC3;
using Utilities;

namespace MP4Maker
{
    public class Mpeg2TansportStreamHelper
    {
        public static void PrintTrackInfo(Mpeg2TransportStream stream)
        {
            int packetLength = stream.IsBluRayTransportStream ? TransportPacket.PacketLength + 4 : TransportPacket.PacketLength;
            int maxBytesToRead = packetLength * 65536; // it seems SSIF uses 6MB chunks, so 12MB should do it
            MemoryStream shortStream = new MemoryStream();
            ByteUtils.CopyStream(stream.BaseStream, shortStream, maxBytesToRead);
            shortStream.Seek(0, SeekOrigin.Begin);
            
            ProgramAssociationSection pat = null;

            Mpeg2TransportStreamReader reader = new Mpeg2TransportStreamReader(new Mpeg2TransportStream(shortStream, stream.IsBluRayTransportStream));
            List<TransportPacket> packetSequence = reader.ReadPacketSequence();
            KeyValuePairList<ushort, ElementaryStreamEntry> streams = new KeyValuePairList<ushort, ElementaryStreamEntry>();

            while (packetSequence != null)
            {
                ushort pid = packetSequence[0].Header.PID;
                byte[] frameBytes = TransportPacketHelper.AssemblePayload(packetSequence);
                if (pid == Mpeg2TransportStream.ProgramAssociationTablePID)
                {
                    byte pointer = frameBytes[0];
                    pat = new ProgramAssociationSection(frameBytes, 1 + pointer);
                }
                else if (pat != null && pat.Programs.Values.Contains(pid))
                {
                    if (ProgramSpecificInformationSection.IsSectionComplete(frameBytes))
                    {
                        ProgramSpecificInformationSection section = ProgramSpecificInformationSection.ReadSection(frameBytes);
                        if (section is ProgramMapSection)
                        {
                            ProgramMapSection pmt = (ProgramMapSection)section;
                            foreach (ElementaryStreamEntry entry in pmt.StreamEntries)
                            {
                                if (!streams.ContainsKey(entry.PID))
                                {
                                    streams.Add(entry.PID, entry);
                                }
                            }
                        }
                    }
                }

                packetSequence = reader.ReadPacketSequence();
            }

            foreach (KeyValuePair<ushort, ElementaryStreamEntry> entry in streams)
            {
                Console.WriteLine("PID: 0x{0}, Type: {1}", entry.Key.ToString("X4"), entry.Value.StreamType);
            }
        }

        public static void PrintPacketInfo(Mpeg2TransportStream stream)
        {
            ProgramAssociationSection pat = null;
            ProgramMapSection pmt = null;

            Mpeg2TransportStreamReader reader = new Mpeg2TransportStreamReader(stream);
            List<TransportPacket> packetSequence = reader.ReadPacketSequence();
            while (packetSequence != null)
            {
                ushort pid = packetSequence[0].Header.PID;
                foreach (TransportPacket packet in packetSequence)
                {
                    Console.Write("[{0}] ", packet.PacketIndex.ToString("00000000"));
                    if (packet.ExtraHeader != null)
                    {
                        Console.Write("[ExtraHeader: {0}, {1}] ", packet.ExtraHeader.CopyPermissionIndicator, packet.ExtraHeader.ArrivalTimeStamp);
                    }
                    Console.Write("PID: 0x{0}, ", packet.Header.PID.ToString("X4"));
                    Console.Write("Unit Start: {0}, ", packet.Header.PayloadUnitStartIndicator);
                    Console.Write("Priority: {0}, ", Convert.ToByte(packet.Header.TransportPriority));
                    Console.Write("Scrambling: {0}, ", packet.Header.TransportScramblingControl);
                    Console.Write("Continuity: {0}", packet.Header.ContinuityCounter);
                    if (packet.Header.AdaptationFieldExist)
                    {
                        Console.Write(" [Adaptation Field");
                        if (packet.AdaptationField.DiscontinuityIndicator)
                        {
                            Console.Write(" D");
                        }
                        if (packet.AdaptationField.RandomAccessIndicator)
                        {
                            Console.Write(" R");
                        }
                        Console.Write("]");
                    }
                    if (pid == Mpeg2TransportStream.NullPacketPID)
                    {
                        Console.Write(" [Null]");
                    }
                    Console.WriteLine();
                }

                byte[] frameBytes = TransportPacketHelper.AssemblePayload(packetSequence);

                if (pid == Mpeg2TransportStream.ProgramAssociationTablePID)
                {
                    Console.WriteLine("\t[Program Association Table]");
                    byte pointer = frameBytes[0];
                    pat = new ProgramAssociationSection(frameBytes, 1 + pointer);
                    foreach (KeyValuePair<ushort, ushort> entry in pat.Programs)
                    {
                        Console.WriteLine("\tNumber: {0}, PID: 0x{1}", entry.Key, entry.Value.ToString("X4"));
                    }
                }
                else if (pat != null && pat.Programs.Values.Contains(pid))
                {
                    if (ProgramSpecificInformationSection.IsSectionComplete(frameBytes))
                    {
                        ProgramSpecificInformationSection section = ProgramSpecificInformationSection.ReadSection(frameBytes);
                        if (section is ProgramMapSection)
                        {
                            pmt = (ProgramMapSection)section;
                            Console.WriteLine("\t[Program Map Table]");
                            foreach (ElementaryStreamEntry entry in pmt.StreamEntries)
                            {
                                Console.Write("\t PID: 0x{0}, Type: {1}", entry.PID.ToString("X4"), entry.StreamType);

                                foreach (Descriptor descriptor in entry.Descriptors)
                                {
                                    if (descriptor is RegistrationDescriptor)
                                    {
                                        Console.Write(" [Reg: {0}", ((RegistrationDescriptor)descriptor).FormatIdentifier);
                                        if (((RegistrationDescriptor)descriptor).AdditionalIdentificationInfo.Length == 4)
                                        {
                                            uint additionalID = BigEndianConverter.ToUInt32(((RegistrationDescriptor)descriptor).AdditionalIdentificationInfo, 0);
                                            Console.Write("(0x{0})", additionalID.ToString("X8"));
                                        }
                                        Console.Write("]");
                                    }
                                    else
                                    {
                                        Console.Write(" [Desc: {0}] ", descriptor.Tag);
                                    }
                                }
                                Console.WriteLine();
                            }
                            Console.WriteLine("\t PCR_PID: 0x{0}", pmt.PCRPID.ToString("X4"));
                        }
                        else if (section is SelectionInformationSection)
                        {
                            Console.WriteLine("\t[Selection Information Table]");
                        }
                    }
                }
                else if (pmt != null && pmt.PCRPID == pid)
                {
                    Console.WriteLine("\t[PCR, Base: {0}]", packetSequence[0].AdaptationField.ProgramClockReferenceBase);
                }
                else
                {
                    PesPacket pesPacket = PesPacket.ReadPacket(frameBytes);
                    if (pesPacket != null)
                    {
                        Console.Write("\t[PES StreamID: {0}", pesPacket.Header.StreamID);
                        if (pesPacket.OptionalHeader != null)
                        {
                            if (pesPacket.OptionalHeader.HasPTS)
                            {
                                Console.Write(", PTS: {0}", pesPacket.OptionalHeader.PTS);
                            }
                        }
                        Console.WriteLine("]");
                    }
                }

                packetSequence = reader.ReadPacketSequence();
            }
        }

        public static void DemuxTrack(Mpeg2TransportStream inputStream, ushort pid, Stream trackStream)
        {
            Mpeg2TransportStreamReader reader = new Mpeg2TransportStreamReader(inputStream);
            List<TransportPacket> packetSequence = reader.ReadPacketSequence();
            while (packetSequence != null)
            {
                if (packetSequence[0].Header.PID == pid)
                {
                    byte[] frameBytes = TransportPacketHelper.AssemblePayload(packetSequence);
                    if (PesPacket.IsPesPacket(frameBytes))
                    {
                        PesPacket pesPacket = new PesPacket(frameBytes);
                        ByteWriter.WriteBytes(trackStream, pesPacket.Data);
                    }
                    else
                    {
                        ByteWriter.WriteBytes(trackStream, frameBytes);
                    }
                }
                packetSequence = reader.ReadPacketSequence();
            }
        }
    }
}
