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
using MediaFormatLibrary;
using MediaFormatLibrary.H264;
using MediaFormatLibrary.MP4;
using MediaFormatLibrary.Mpeg2;

namespace MP4Maker
{
    public enum Operation
    {
        Convert,
        Demux,
        View,
        Mux,
        Rewrite,
    }

    class Program
    {
        static void Main(string[] args)
        {
            args = CommandLineParser.GetCommandLineArgsIgnoreEscape();
            if (args.Length < 1)
            {
                PrintHelp();
                return;
            }

            List<string> inputList = new List<string>();
            string outputPath = null;
            Operation operation = Operation.Mux;
            MultiplexerProfile multiplexerProfile = MultiplexerProfile.mp42;
            bool enableCTTSv1 = true;
            foreach (string arg in args)
            {
                string switchName = arg;
                string switchParameter = String.Empty;
                if (arg.StartsWith("/"))
                {
                    switchName = arg.Substring(1);
                    int index = switchName.IndexOfAny(new char[] { ':', '=' });
                    if (index >= 0)
                    {
                        switchParameter = switchName.Substring(index + 1);
                        switchParameter = switchParameter.Trim('"');
                        switchName = switchName.Substring(0, index);
                    }
                }

                switch (switchName.ToLower())
                {
                    case "cttsv0":
                        enableCTTSv1 = false;
                        break;
                    case "input":
                        inputList.Add(switchParameter);
                        break;
                    case "output":
                        outputPath = switchParameter;
                        break;
                    case "convert":
                        operation = Operation.Convert;
                        break;
                    case "demux":
                        operation = Operation.Demux;
                        break;
                    case "mux":
                        operation = Operation.Mux;
                        break;
                    case "profile":
                        if (switchParameter.ToUpper() == "MSNV")
                        {
                            multiplexerProfile = MultiplexerProfile.MSNV;
                        }
                        else if (switchParameter.ToUpper() == "MSNV3D")
                        {
                            multiplexerProfile = MultiplexerProfile.MSNV3D;
                        }
                        else if (switchParameter.ToUpper() == "MP42")
                        {
                            multiplexerProfile = MultiplexerProfile.mp42;
                        }
                        else
                        {
                            Console.WriteLine("Error: Invalid profile argument: " + switchParameter);
                            return;
                        }
                        break;
                    case "view":
                        operation = Operation.View;
                        break;
                    case "rewrite":
                        operation = Operation.Rewrite;
                        break;
                    default:
                        Console.WriteLine("Error: Invalid command-line switch: " + switchName);
                        return;
                }
            }

            if (operation == Operation.Convert && inputList.Count == 1 && outputPath != null)
            {
                Convert(inputList[0], outputPath);
            }
            else if (operation == Operation.View && inputList.Count == 1)
            {
                ViewFileInfo(inputList[0]);
            }
            else if (operation == Operation.Demux && inputList.Count == 1)
            {
                if (outputPath == null)
                {
                    outputPath = Path.GetDirectoryName(inputList[0]);
                }
                Demux(inputList[0], outputPath);
            }
            else if (operation == Operation.Mux && inputList.Count > 0 && outputPath != null)
            {
                string outputExtension = Path.GetExtension(outputPath).ToLower();
                if (outputExtension == ".ts" || outputExtension == ".m2ts")
                {
                    MuxMpeg2TransportStream(inputList, outputPath);
                }
                else if (outputExtension == ".ssif")
                {
                    MuxSSIF(inputList, outputPath);
                }
                else
                {
                    MuxMP4(inputList, outputPath, multiplexerProfile, enableCTTSv1);
                }
            }
            else if (operation == Operation.Rewrite && inputList.Count == 1 && outputPath != null)
            {
                Rewrite(inputList[0], outputPath);
            }
            else
            {
                Console.WriteLine("Error: Invalid arguments.");
                Console.WriteLine();
                PrintHelp();
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("MP4Maker v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("Author: Tal Aloni (tal.aloni.il@gmail.com)");
            Console.WriteLine();
            Console.WriteLine("About:");
            Console.WriteLine("This software is an audio/video multiplexer that can create:");
            Console.WriteLine(" * MP4 files conforming to the standard 'mp42' brand");
            Console.WriteLine(" * MP4 files conforming to the MSNV brand (IEC/TS 62592)");
            Console.WriteLine(" * PlayStation 3 compatible frame-sequential 3D MP4 files");
            Console.WriteLine(" * MPEG-2 Transport Stream files (.ts)");
            Console.WriteLine(" * BDAV MPEG-2 Transport Stream files (.m2ts)");
            Console.WriteLine(" * Strereoscopic Interleaved files (.ssif)");
            Console.WriteLine();
            Console.WriteLine("The following input formats are supported: H264, AAC, AC3.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("    MP4Maker /mux [/profile=<mp42/MSNV/MSNV3D>] [/CTTSv0]");
            Console.WriteLine("             /input=<path> [/input=<path>..] /output=<path>");
            Console.WriteLine("    MP4Maker /demux /input=<path> /output=<folder>");
            Console.WriteLine("    MP4Maker /view /input=<path>");
            Console.WriteLine();
            Console.WriteLine("    /CTTSv0     Convert sample start offsets to ensure backward compatibility");
            Console.WriteLine("                with very old MP4 players.");
            Console.WriteLine();
            Console.WriteLine("    /profile    The profile that will be used when creating MP4 files.");     
        }

        private static void Rewrite(string inputPath, string outputPath)
        {
            MP4Stream inputStream;
            try
            {
                inputStream = MP4Stream.Open(inputPath, FileMode.Open, FileAccess.Read);
            }
            catch (IOException)
            {
                Console.WriteLine("Cannot open " + inputPath);
                return;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Cannot open {0} - Access Denied", inputPath);
                return;
            }

            FileStream outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

            List<Box> rootBoxes = inputStream.ReadRootBoxes();
            List<long> mediaDataOffset = BoxHelper.GetMediaDataOffsets(rootBoxes);
            Rewrite(inputStream.BaseStream, mediaDataOffset, outputStream, rootBoxes);
            inputStream.BaseStream.Close();
            outputStream.Close();
        }

        private static void Rewrite(Stream inputStream, List<long> mediaDataOffsets, Stream outputStream, List<Box> rootBoxes)
        {
            int offsetIndex = 0;
            foreach (Box box in rootBoxes)
            {
                if (box.Type == BoxType.MediaDataBox)
                {
                    inputStream.Seek(mediaDataOffsets[offsetIndex] + 8, SeekOrigin.Begin);
                    BoxHelper.WriteBoxHeader(outputStream, BoxType.MediaDataBox, box.Size);
                    if (box.Size > Int64.MaxValue)
                    {
                        throw new NotImplementedException("Cannot write box larger than 2^63 bytes");
                    }
                    long dataSize = (long)box.Size - 8;
                    ByteUtils.CopyStream(inputStream, outputStream, dataSize);
                    offsetIndex++;
                }
                else
                {
                    box.WriteBytes(outputStream);
                }
            }
        }

        /// <summary>
        /// Will convert 'mp42' brand MP4 to 'MSNV' brand MP4.
        /// </summary>
        private static void Convert(string inputPath, string outputPath)
        {
            MP4Stream inputStream;
            try
            {
                inputStream = MP4Stream.Open(inputPath, FileMode.Open, FileAccess.Read);
            }
            catch (IOException)
            {
                Console.WriteLine("Cannot open " + inputPath);
                return;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Cannot open {0} - Access Denied", inputPath);
                return;
            }

            FileStream outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

            List<Box> rootBoxes = inputStream.ReadRootBoxes();
            List<long> inputMediaDataOffsets = BoxHelper.GetMediaDataOffsets(rootBoxes);

            MSNVConvertHelper.RemoveUnnecessaryBoxes(rootBoxes);
            MSNVConvertHelper.UpdateMSNVBoxes(rootBoxes);
            MSNV3DHelper.FlagProfileAndTrackAs3D(rootBoxes);
            // We use dummyStream to update the box size
            MemoryStream dummyStream = new MemoryStream();
            foreach (Box box in rootBoxes)
            {
                if (box.Type != BoxType.MediaDataBox)
                {
                    box.WriteBytes(dummyStream);
                }
                else
                {
                    break;
                }
            }
            long outputMediaDataOffset = BoxHelper.GetMediaDataOffsets(rootBoxes)[0];

            long mediaDataShift = outputMediaDataOffset - inputMediaDataOffsets[0];
            MSNVConvertHelper.UpdateChunkOffsets(rootBoxes, mediaDataShift);
            Rewrite(inputStream.BaseStream, inputMediaDataOffsets, outputStream, rootBoxes);
            inputStream.BaseStream.Close();
            outputStream.Close();
            MP4Helper.PrintTrackInfo(rootBoxes);
            MP4Helper.PrintMSNVTrackProfileInfo(rootBoxes);
        }

        private static void Demux(string inputPath, string outputPath)
        {
            MP4Stream inputStream;
            try
            {
                inputStream = MP4Stream.Open(inputPath, FileMode.Open, FileAccess.Read);
            }
            catch (IOException)
            {
                Console.WriteLine("Cannot open " + inputPath);
                return;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Cannot open {0} - Access Denied", inputPath);
                return;
            }

            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(inputPath);

            if (!outputPath.EndsWith(@"\"))
            {
                outputPath += @"\";
            }

            List<Box> rootBoxes = inputStream.ReadRootBoxes();
            Box movieBox = BoxHelper.FindBox(rootBoxes, BoxType.MovieBox);
            List<Box> tracks = BoxHelper.FindBoxes(movieBox.Children, BoxType.TrackBox);
            for(int trackIndex = 0; trackIndex < tracks.Count; trackIndex++)
            {
                TrackBox track = (TrackBox)tracks[trackIndex];
                string extenstion = DemuxHelper.GetTrackFileExtention(track);
                string trackFilename = String.Format("{0}-Track{1}.{2}", filenameWithoutExtension, trackIndex.ToString(), extenstion);
                FileStream trackStream = new FileStream(outputPath + trackFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                DemuxHelper.DemuxTrack(track, inputStream.BaseStream, trackStream);
                trackStream.Close();
            }
            inputStream.BaseStream.Close();
        }

        private static List<IMultiplexerInput> OpenTracks(List<string> inputList)
        {
            // Verify extension is supported
            foreach (string inputPath in inputList)
            {
                string extension = Path.GetExtension(inputPath).ToLower();
                if (extension == ".h264" || extension == ".264" || extension == ".avc" || extension == ".aac" || extension == ".ac3")
                {
                    continue;
                }
                Console.WriteLine("Error: Unsupported file extension, only .h264, .aac and .ac3 files are supported.");
                return null;
            }

            List<IMultiplexerInput> trackList = new List<IMultiplexerInput>();
            foreach (string inputPath in inputList)
            {
                FileStream inputStream;
                try
                {
                    inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
                }
                catch (IOException)
                {
                    Console.WriteLine("Cannot open " + inputPath);
                    foreach (IMultiplexerInput track in trackList)
                    {
                        track.BaseStream.Close();
                    }
                    return null;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Cannot open {0} - Access Denied", inputPath);
                    foreach (IMultiplexerInput track in trackList)
                    {
                        track.BaseStream.Close();
                    }
                    return null;
                }

                string extension = Path.GetExtension(inputPath).ToLower();
                if (extension == ".h264" || extension == ".264" || extension == ".avc")
                {
                    H264MultiplexerInput h264MultiplexerInput = new H264MultiplexerInput(inputStream);
                    try
                    {
                        h264MultiplexerInput.GetMP4SampleEntry();
                    }
                    catch (MissingParameterSetException)
                    {
                        Console.WriteLine("Error: H.264 is missing SPS/PPS.");
                        foreach (IMultiplexerInput track in trackList)
                        {
                            track.BaseStream.Close();
                        }
                        return null;
                    }
                    trackList.Add(h264MultiplexerInput);
                }
                else if (extension == ".aac")
                {
                    AACMultiplexerInput aacMultiplexerInput = new AACMultiplexerInput(inputStream);
                    try
                    {
                        aacMultiplexerInput.GetMP4SampleEntry();
                    }
                    catch (NotImplementedException)
                    {
                        Console.WriteLine("Error: AAC is incompatible or unsupported.");
                        foreach (IMultiplexerInput track2 in trackList)
                        {
                            track2.BaseStream.Close();
                        }
                        return null;
                    }
                    trackList.Add(aacMultiplexerInput);
                }
                else if (extension == ".ac3")
                {
                    AC3MultiplexerInput ac3MultiplexerInput = new AC3MultiplexerInput(inputStream);
                    trackList.Add(ac3MultiplexerInput);
                }
            }
            return trackList;
        }

        private static void MuxMpeg2TransportStream(List<string> inputList, string outputPath)
        {
            List<IMultiplexerInput> trackList = OpenTracks(inputList);
            if (trackList == null)
            {
                // Error message has already been printed
                return;
            }

            string extention = Path.GetExtension(outputPath).ToLower();
            Mpeg2TransportStream outputStream;
            if (extention == ".ts")
            {
                outputStream = Mpeg2TransportStream.OpenTS(outputPath, FileMode.Create, FileAccess.ReadWrite);
            }
            else
            {
                outputStream = Mpeg2TransportStream.OpenM2TS(outputPath, FileMode.Create, FileAccess.ReadWrite);
            }

            Mpeg2TransportStreamMultiplexer.Mux(trackList, outputStream);
            outputStream.BaseStream.Close();
            foreach (IMultiplexerInput track in trackList)
            {
                track.BaseStream.Close();
            }
        }

        private static void MuxSSIF(List<string> inputList, string outputPath)
        {
            List<IMultiplexerInput> trackList = OpenTracks(inputList);
            if (trackList == null)
            {
                // Error message has already been printed
                return;
            }

            int baseViewTrackIndex = -1;
            int dependentViewTrackIndex = -1;
            for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
            {
                if (trackList[trackIndex] is H264MultiplexerInput)
                {
                    if (((H264MultiplexerInput)trackList[trackIndex]).IsMvc)
                    {
                        dependentViewTrackIndex = trackIndex;
                    }
                    else
                    {
                        baseViewTrackIndex = trackIndex;
                    }
                }
            }

            if (baseViewTrackIndex >= 0 && dependentViewTrackIndex >= 0)
            {
                IMultiplexerInput dependentTrack = trackList[dependentViewTrackIndex];
                trackList.RemoveAt(dependentViewTrackIndex);
                Mpeg2TransportStream outputStream = Mpeg2TransportStream.OpenM2TS(outputPath, FileMode.Create, FileAccess.ReadWrite);
                Mpeg2TransportStreamMultiplexer.MuxStereoscopic(trackList, dependentTrack, outputStream);
            }
            else
            {
                Console.WriteLine("Error: Two H.264 streams are required for SSIF creation (AVC+MVC).");
                return;
            }
        }

        private static void MuxMP4(List<string> inputList, string outputPath, MultiplexerProfile multiplexerProfile, bool enableCTTSv1)
        {
            List<IMultiplexerInput> trackList = OpenTracks(inputList);
            if (trackList == null)
            {
                // Error message has already been printed
                return;
            }

            bool isMSNV = (multiplexerProfile == MultiplexerProfile.MSNV || multiplexerProfile == MultiplexerProfile.MSNV3D);
            if (isMSNV) // Print MSNV related warnings
            {
                foreach (IMultiplexerInput track in trackList)
                {
                    if (track is H264MultiplexerInput)
                    {
                        ((H264MultiplexerInput)track).IsOutOfBandParameterSetDelivery = true;
                        ((H264MultiplexerInput)track).AppendLengthFieldPrefix = true;
                        AVCVisualSampleEntry sampleEntry = (AVCVisualSampleEntry)((H264MultiplexerInput)track).GetMP4SampleEntry();
                        if (!MSNVHelper.IsValidResolution(sampleEntry.Width, sampleEntry.Height))
                        {
                            Console.WriteLine("Warning: Video resolution ({0}x{1}) is incompatible with MSNV specifications", sampleEntry.Width, sampleEntry.Height);
                        }
                    }
                    else if (track is AACMultiplexerInput)
                    {
                        ((AACMultiplexerInput)track).IsRawDataDelivery = true;
                        MP4AudioSampleEntry sampleEntry = (MP4AudioSampleEntry)((AACMultiplexerInput)track).GetMP4SampleEntry();
                        if (sampleEntry.SampleRate != 48000)
                        {
                            Console.WriteLine("Warning: Audio sample rate ({0}) is incompatible with MSNV specifications", sampleEntry.SampleRate);
                        }
                        if (sampleEntry.Children.Count > 0 && sampleEntry.Children[0] is ElementaryStreamDescriptorBox)
                        {
                            /// [IEC/TS 62592] Only MPEG-4 AAC LC is supported
                            ElementaryStreamDescriptorBox elementaryStreamDescriptor = (ElementaryStreamDescriptorBox)sampleEntry.Children[0];
                            DecoderConfigDescriptor decoderConfig = elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor;
                            if (decoderConfig.ObjectTypeIndication == ObjectTypeIndication.Mpeg4AAC)
                            {
                                DecoderSpecificInfo info = decoderConfig.DecSpecificInfo.Count > 0 ? decoderConfig.DecSpecificInfo[0] : null;
                                if (info is AudioSpecificConfig)
                                {
                                    AudioSpecificConfig audioSpecificConfig = (AudioSpecificConfig)info;
                                    if (audioSpecificConfig.AudioObjectType != AudioObjectType.AACLC)
                                    {
                                        Console.WriteLine("Warning: MPEG-4 AAC LC should be used to comply with MSNV specifications.");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Warning: MPEG-4 AAC should be used to comply with MSNV specifications.");
                            }
                        }
                    }
                    else if (track is AC3MultiplexerInput)
                    {
                        Console.WriteLine("Warning: Dolby Digital (AC-3) is incompatible with MSNV specifications");
                    }
                }
            }
            FileStream outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            MP4Multiplexer.Mux(trackList, outputStream, multiplexerProfile, enableCTTSv1);
            outputStream.Close();
            foreach (IMultiplexerInput track in trackList)
            {
                track.BaseStream.Close();
            }
        }

        private static void ViewFileInfo(string inputPath)
        {
            FileStream inputStream;
            try
            {
                inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, MP4Stream.StreamBufferSize);
            }
            catch (IOException)
            {
                Console.WriteLine("Cannot open " + inputPath);
                return;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Cannot open {0} - Access Denied", inputPath);
                return;
            }

            string extension = Path.GetExtension(inputPath).ToLower();
            if (extension == ".ts")
            {
                Mpeg2TransportStream mpeg2Stream = new Mpeg2TransportStream(inputStream, false);
                Mpeg2TansportStreamHelper.PrintTrackInfo(mpeg2Stream);
#if DEBUG
                mpeg2Stream.BaseStream.Seek(0, SeekOrigin.Begin);
                Console.WriteLine();
                Mpeg2TansportStreamHelper.PrintPacketInfo(mpeg2Stream);
#endif
                mpeg2Stream.BaseStream.Close();
            }
            else if (extension == ".m2ts" || extension == ".ssif")
            {
                Mpeg2TransportStream mpeg2Stream = new Mpeg2TransportStream(inputStream, true);
                Mpeg2TansportStreamHelper.PrintTrackInfo(mpeg2Stream);
#if DEBUG
                mpeg2Stream.BaseStream.Seek(0, SeekOrigin.Begin);
                Console.WriteLine();
                Mpeg2TansportStreamHelper.PrintPacketInfo(mpeg2Stream);
#endif
                mpeg2Stream.BaseStream.Close();
            }
            else if (extension == ".mp4" || extension == ".m4a" || extension == ".m4v" || extension == ".m4p")
            {
                MP4Stream mp4Stream = new MP4Stream(inputStream);
                List<Box> rootBoxes = mp4Stream.ReadRootBoxes();
                mp4Stream.BaseStream.Close();
                MP4Helper.PrintMP4Info(rootBoxes, 0);
                MP4Helper.PrintTrackInfo(rootBoxes);
                MP4Helper.PrintMSNVTrackProfileInfo(rootBoxes);
#if DEBUG
                MP4Helper.PrintTemporalOffsetInfo(rootBoxes);
#endif
                mp4Stream.BaseStream.Close();
            }
            else if (extension == ".h264" || extension == ".264" || extension == ".avc")
            {
                H264ElementaryStream h264Stream = new H264ElementaryStream(inputStream);
                H264ElementaryStreamReader reader = new H264ElementaryStreamReader(h264Stream);
                H264Helper.PrintH264Info(reader);
                h264Stream.BaseStream.Close();
            }
            else
            {
                Console.WriteLine("Error: The file name extension ({0}) is unsupported.", extension);
            }
        }
    }
}
