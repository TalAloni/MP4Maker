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
    public class Mpeg2TransportStreamMultiplexer
    {
        public const ushort ProgramMapTablePID = 0x0100;
        public const ushort SelectionInformationTablePID = 0x001F;
        public const ushort PCRPID = 0x1001;

        // [ISO/IEC 13818-1] coded audio and video that represent sound and pictures that are to be presented simultaneously may be
        // separated in time within the coded bit stream by as much as one second, which is the maximum decoder buffer delay that
        // is allowed in the STD model

        // [ISO/IEC 13818-1] a PCR packet should occur at intervals up to 100 ms in MPEG-2 transport streams.
        public const double TargetChunkDuration = 0.1;
        /// <summary>
        /// The time to delay the presentation (in seconds) in order for the system time clock (read: arrival of the packet) to be less than the PTS.
        /// The STC will be less than the PTS, because the PTS equals the coding instant plus the total buffer delay (encoder + decoder).
        /// When the STC equals the PTS, the whole access unit is extracted, decoded, and displayed.
        /// (In case of reordering, the access unit is decoded when the STC equals the DTS,  but it is not displayed until the STC equals the PTS).
        /// See: http://www.bretl.com/mpeghtml/timemdl.HTM
        /// </summary>
        public const double PresentationDelay = 1;

        public static ProgramAssociationSection GetProgramAssociationTable()
        {
            ProgramAssociationSection pat = new ProgramAssociationSection();
            pat.TransportStreamID = 1;
            pat.CurrentNextIndicator = true;
            pat.Programs.Add(0, SelectionInformationTablePID);
            pat.Programs.Add(1, ProgramMapTablePID);
            return pat;
        }

        public static ProgramMapSection GetProgramMapSection(List<IMultiplexerInput> trackList)
        {
            ProgramMapSection pmt = new ProgramMapSection();
            pmt.ProgramNumber = 1;
            pmt.CurrentNextIndicator = true;
            pmt.PCRPID = PCRPID;
            ushort nextVideoPID = 0x1011;
            ushort nextAudioPID = 0x1100;
            ushort nextOtherPID = 0x1200;
            RegistrationDescriptor programRegistration = new RegistrationDescriptor();
            programRegistration.FormatIdentifier = FormatIdentifier.HDMV;
            pmt.ProgramDescriptors.Add(programRegistration);

            DigitalCopyProtectionDescriptor dtcpDescriptor = new DigitalCopyProtectionDescriptor();
            dtcpDescriptor.CASystemID = 0x0FFF;
            dtcpDescriptor.RetentionMoveMode = true;
            dtcpDescriptor.RetentionState = 0x07;
            dtcpDescriptor.EPN = true;
            dtcpDescriptor.ImageConstraintToken = true;
            pmt.ProgramDescriptors.Add(dtcpDescriptor);

            for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
            {
                IMultiplexerInput track = trackList[trackIndex];
                ElementaryStreamEntry entry = new ElementaryStreamEntry();
                entry.StreamType = track.Mpeg2StreamType;
                if (track.ContentType == ContentType.Video)
                {
                    entry.PID = nextVideoPID;
                    nextVideoPID++;
                }
                else if (track.ContentType == ContentType.Audio)
                {
                    entry.PID = nextAudioPID;
                    nextAudioPID++;
                }
                else
                {
                    entry.PID = nextOtherPID;
                    nextOtherPID++;
                }
                entry.Descriptors.AddRange(track.GetMpeg2Descriptors());
                pmt.StreamEntries.Add(entry);
            }
            return pmt;
        }

        public static SelectionInformationSection GetSelectionInformationSection()
        {
            SelectionInformationSection sit = new SelectionInformationSection();
            sit.CurrentNextIndicator = true;
            ServiceEntry entry = new ServiceEntry();
            entry.ServiceID = 1;
            entry.RunningStatus = 8;
            sit.ServiceEntries.Add(entry);
            Descriptor sitDescriptor = new Descriptor();
            
            PartialTransportStreamDescriptor tsDescriptor = new PartialTransportStreamDescriptor();
            tsDescriptor.PeakRate = 88750; // FIXME
            tsDescriptor.MinimumOverallSmoothingRate = 0x3FFFFF;
            tsDescriptor.MaximumOverallSmoothingBuffer = 0x3FFF;
            sit.Descriptors.Add(tsDescriptor);

            return sit;
        }

        public static void Mux(List<IMultiplexerInput> trackList, Mpeg2TransportStream outputStream)
        {
            Mpeg2TransportStreamWriter writer = new Mpeg2TransportStreamWriter(outputStream);
            ProgramAssociationSection pat = GetProgramAssociationTable();
            ProgramMapSection pmt = GetProgramMapSection(trackList);
            SelectionInformationSection sit = GetSelectionInformationSection();

            SampleData[] pendingSample = new SampleData[trackList.Count];
            bool hasTrackWithSamples = true;
            double[] trackDurationInSeconds = new double[trackList.Count];
            int referenceTrackIndex = 0;

            while (hasTrackWithSamples)
            {
                // The PAT / PMT should be transmitted every 500 ms
                writer.WritePSISection(Mpeg2TransportStream.ProgramAssociationTablePID, pat);
                writer.WritePSISection(ProgramMapTablePID, pmt);
                writer.WritePSISection(SelectionInformationTablePID, sit);
                // [ISO/IEC 13818-1] a PCR packet should occur at intervals up to 100 ms in MPEG-2 transport streams.
                writer.WritePCRPacket(PCRPID);

                for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
                {
                    ushort pid = pmt.StreamEntries[trackIndex].PID;
                    IMultiplexerInput track = trackList[trackIndex];
                    WriteChunk(track, trackIndex, pid, referenceTrackIndex, pendingSample, trackDurationInSeconds, writer);
                }

                // We shouldn't send packets from "the future" compared to the system clock sent after them.
                writer.SetSystemTimeClock(trackDurationInSeconds[referenceTrackIndex]);

                hasTrackWithSamples = false;
                for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
                {
                    if (pendingSample[trackIndex] != null)
                    {
                        hasTrackWithSamples = true;
                        break;
                    }
                }
            }
        }

        public static void MuxStereoscopic(List<IMultiplexerInput> trackList, IMultiplexerInput dependentTrack, Mpeg2TransportStream outputStream)
        {
            if (!outputStream.IsBluRayTransportStream)
            {
                throw new ArgumentException("SSIF must include TP_extra_header");
            }
            // We have two writers taking turns writing to the same SSIF stream, each have its own continuity counter and system time clock.
            Mpeg2TransportStreamWriter dependentStreamWriter = new Mpeg2TransportStreamWriter(outputStream);
            Mpeg2TransportStreamWriter baseStreamWriter = new Mpeg2TransportStreamWriter(outputStream);
            ProgramAssociationSection pat = GetProgramAssociationTable();
            ProgramMapSection basePMT = GetProgramMapSection(trackList);
            List<IMultiplexerInput> dependentTracks = new List<IMultiplexerInput>();
            dependentTracks.Add(dependentTrack);
            ProgramMapSection dependentPMT = GetProgramMapSection(dependentTracks);
            dependentPMT.StreamEntries[0].PID = 0x1012;
            
            SelectionInformationSection sit = GetSelectionInformationSection();

            SampleData[] pendingSample = new SampleData[trackList.Count + 1];
            bool hasTrackWithSamples = true;
            double[] trackDurationInSeconds = new double[trackList.Count + 1];
            int dependentTrackIndex = trackList.Count;

            while (hasTrackWithSamples)
            {
                // Write dependent stream interleave unit:
                for (int index = 0; index < 20; index++)
                {
                    // The PAT / PMT should be transmitted every 500 ms
                    dependentStreamWriter.WritePSISection(Mpeg2TransportStream.ProgramAssociationTablePID, pat);
                    dependentStreamWriter.WritePSISection(ProgramMapTablePID, dependentPMT);
                    dependentStreamWriter.WritePSISection(SelectionInformationTablePID, sit);
                    ushort dependentPID = dependentPMT.StreamEntries[0].PID;
                    
                    // [ISO/IEC 13818-1] a PCR packet should occur at intervals up to 100 ms in MPEG-2 transport streams.
                    dependentStreamWriter.WritePCRPacket(PCRPID);

                    int referenceTrackIndex = dependentTrackIndex;
                    WriteChunk(dependentTrack, dependentTrackIndex, dependentPID, referenceTrackIndex, pendingSample, trackDurationInSeconds, dependentStreamWriter);

                    dependentStreamWriter.SetSystemTimeClock(trackDurationInSeconds[referenceTrackIndex]);
                }
                // One Aligned unit consists of 32 source packets (192 * 32 = 6144 bytes)
                dependentStreamWriter.FillAlignedUnit(6144);

                // Write base stream interleave unit:
                for (int index = 0; index < 20; index++)
                {
                    // The PAT / PMT should be transmitted every 500 ms
                    baseStreamWriter.WritePSISection(Mpeg2TransportStream.ProgramAssociationTablePID, pat);
                    baseStreamWriter.WritePSISection(ProgramMapTablePID, basePMT);
                    baseStreamWriter.WritePSISection(SelectionInformationTablePID, sit);

                    // [ISO/IEC 13818-1] a PCR packet should occur at intervals up to 100 ms in MPEG-2 transport streams.
                    baseStreamWriter.WritePCRPacket(PCRPID);

                    for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
                    {
                        ushort pid = basePMT.StreamEntries[trackIndex].PID;
                        IMultiplexerInput track = trackList[trackIndex];
                        int referenceTrackIndex;
                        if (trackIndex == 0)
                        {
                            referenceTrackIndex = dependentTrackIndex;
                        }
                        else
                        {
                            referenceTrackIndex = 0;
                        }
                        WriteChunk(track, trackIndex, pid, referenceTrackIndex, pendingSample, trackDurationInSeconds, baseStreamWriter);
                    }

                    baseStreamWriter.SetSystemTimeClock(trackDurationInSeconds[0]);
                }

                // One Aligned unit consists of 32 source packets (192 * 32 = 6144 bytes)
                baseStreamWriter.FillAlignedUnit(6144);
            
                hasTrackWithSamples = false;
                for (int trackIndex = 0; trackIndex < trackList.Count + 1; trackIndex++)
                {
                    if (pendingSample[trackIndex] != null)
                    {
                        hasTrackWithSamples = true;
                        break;
                    }
                }
            }
        }

        private static void WriteChunk(IMultiplexerInput track, int trackIndex, ushort pid, int referenceTrackIndex, SampleData[] pendingSample, double[] trackDurationInSeconds, Mpeg2TransportStreamWriter writer)
        {
            double chunkDurationInSeconds = 0;
            SampleData sampleData;
            if (pendingSample[trackIndex] != null)
            {
                sampleData = pendingSample[trackIndex];
                pendingSample[trackIndex] = null;
            }
            else
            {
                sampleData = track.ReadSample();
            }

            while (sampleData != null)
            {
                bool isEndOfChunk = false;
                // The temporal offset between the first sample of each track in an interleave unit should be kept to a minimum
                // (to prevent excessive seeking)
                double temporalOffset = 0; // in seconds, positive if ahead
                if (trackIndex != referenceTrackIndex)
                {
                    temporalOffset = trackDurationInSeconds[trackIndex] - trackDurationInSeconds[referenceTrackIndex];
                }

                if (chunkDurationInSeconds > 0)
                {
                    if (chunkDurationInSeconds >= TargetChunkDuration)
                    {
                        isEndOfChunk = true;
                    }
                    else if (chunkDurationInSeconds + sampleData.DurationInSeconds > TargetChunkDuration)
                    {
                        // If we can add one last sample to the chunk and still have a non-positive temporal offset, then we should add it
                        // This will minimize the negative temporal offset
                        if (!(trackIndex != referenceTrackIndex && (temporalOffset + sampleData.DurationInSeconds <= 0)))
                        {
                            isEndOfChunk = true;
                        }
                    }
                    else if (trackIndex != referenceTrackIndex && pendingSample[referenceTrackIndex] != null && (temporalOffset + sampleData.DurationInSeconds >= 0))
                    {
                        isEndOfChunk = true;
                    }
                }

                if (!isEndOfChunk)
                {
                    PesPacket pesPacket = new PesPacket();
                    pesPacket.Header.StreamID = track.Mpeg2StreamID;
                    pesPacket.OptionalHeader.DataAlignmentIndicator = true;
                    pesPacket.OptionalHeader.HasPTS = true;
                    double packetPTSInSeconds = PresentationDelay + trackDurationInSeconds[trackIndex] + sampleData.SampleDelayInSeconds;
                    ulong packetPTSInProgramTimeUnits = (ulong)(packetPTSInSeconds * Mpeg2TransportStreamWriter.PTSResolution);
                    pesPacket.OptionalHeader.PTS = Mpeg2TransportStreamWriter.ProgramClockReferenceBase + packetPTSInProgramTimeUnits;
                    if (track.Mpeg2StreamIDExtention.HasValue)
                    {
                        pesPacket.OptionalHeader.ExtensionFlag = true;
                        pesPacket.OptionalHeader.ExtensionField.ExtensionFlag2 = true;
                        pesPacket.OptionalHeader.ExtensionField.ExtensionReserved = new byte[] { track.Mpeg2StreamIDExtention.Value };
                    }

                    // ATSC guidelines: "each PES packet contains no more than one coded video frame".
                    pesPacket.Data = sampleData.RawSampleData;
                    pesPacket.Header.PacketLength = (ushort)(pesPacket.Length - PesPacketHeader.Length);
                    writer.WritePesPacket(pid, pesPacket);

                    chunkDurationInSeconds += sampleData.DurationInSeconds;
                    //trackDurationInTimeUnits[trackIndex] += sampleData.DurationInTimeUnits;
                    trackDurationInSeconds[trackIndex] += sampleData.DurationInSeconds;
                }
                else
                {
                    pendingSample[trackIndex] = sampleData;
                    break;
                }

                sampleData = track.ReadSample();
            }
        }
    }
}
