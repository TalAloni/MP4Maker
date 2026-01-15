/* Copyright (C) 2014-2026 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
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

namespace MediaFormatLibrary.MP4
{
    public class BoxHelper
    {
        public static Box ReadBox(Stream stream)
        {
            return ReadBox(stream, stream.Length);
        }

        public static Box ReadBox(Stream stream, long endOfContainer)
        {
            long position = stream.Position;
            if (position <= endOfContainer - 8)
            {
                ulong size = BigEndianReader.ReadUInt32(stream);
                BoxType type = (BoxType)BigEndianReader.ReadUInt32(stream);
                Guid userType = Guid.Empty;
                if (size == 0)
                {
                    throw new Exception("Invalid MP4 stream");
                }
                else if (size == 1)
                {
                    size = BigEndianReader.ReadUInt64(stream);
                }

                if (type == BoxType.UserBox)
                {
                    userType = BigEndianReader.ReadGuidBytes(stream);
                }
                stream.Seek(position, SeekOrigin.Begin);

                Box result = ReadBox(stream, type, userType);

                if (size > Int64.MaxValue)
                {
                    // It's unlikely someone is trying to use a 2^64 byte file, most likely an issue with the input
                    throw new Exception("File is invalid or corrupted");
                }
                stream.Seek(position + (long)size, SeekOrigin.Begin);
                return result;
            }
            return null;
        }

        private static Box ReadBox(Stream stream, BoxType type, Guid userType)
        {
            switch (type)
            {
                case BoxType.AudioProfileEntry:
                    return new AudioProfileEntry(stream);
                case BoxType.AC3AudioSampleEntry:
                    return new AC3AudioSampleEntry(stream);
                case BoxType.AC3SpecificBox:
                    return new AC3SpecificBox(stream);
                case BoxType.AVCVisualSampleEntry:
                    return new AVCVisualSampleEntry(stream);
                case BoxType.AVCDecoderConfigurationRecordBox:
                    return new AVCDecoderConfigurationRecordBox(stream);
                case BoxType.ChunkLargeOffsetBox:
                    return new ChunkLargeOffsetBox(stream);
                case BoxType.ChunkOffsetBox:
                    return new ChunkOffsetBox(stream);
                case BoxType.CompositionTimeToSampleBox:
                    return new CompositionOffsetBox(stream);
                case BoxType.DataEntryUrlBox:
                    return new DataEntryUrlBox(stream);
                case BoxType.DataInformationBox:
                    return new DataInformationBox(stream);
                case BoxType.DataReferenceBox:
                    return new DataReferenceBox(stream);
                case BoxType.SegmentIndexBox:
                    return new SegmentIndexBox(stream);
                case BoxType.DecodingTimeToSampleBox:
                    return new TimeToSampleBox(stream);
                case BoxType.EditBox:
                    return new EditBox(stream);
                case BoxType.EditListBox:
                    return new EditListBox(stream);
                case BoxType.ElementaryStreamDescriptorBox:
                    return new ElementaryStreamDescriptorBox(stream);
                case BoxType.EncoderBox:
                    return new EncoderBox(stream);
                case BoxType.FileGlobalProfileEntry:
                    return new FileGolbalProfileEntry(stream);
                case BoxType.FileTypeBox:
                    return new FileTypeBox(stream);
                case BoxType.HandlerReferenceBox:
                    return new HandlerBox(stream);
                case BoxType.ItemListBox:
                    return new ItemListBox(stream);
                case BoxType.MediaBox:
                    return new MediaBox(stream);
                case BoxType.MediaHeaderBox:
                    return new MediaHeaderBox(stream);
                case BoxType.MediaInformationBox:
                    return new MediaInformationBox(stream);
                case BoxType.MetaBox:
                    return new MetaBox(stream);
                case BoxType.MetaDataBox:
                    return new MetaDataBox(stream);
                case BoxType.MetaDataValueBox:
                    return new MetaDataValueBox(stream);
                case BoxType.MovieFragmentHeaderBox:
                    return new MovieFragmentHeaderBox(stream);
                case BoxType.MovieBox:
                    return new MovieBox(stream);
                case BoxType.MovieFragmentBox:
                    return new MovieFragmentBox(stream);
                case BoxType.MovieHeaderBox:
                    return new MovieHeaderBox(stream);
                case BoxType.MP4AudioSampleEntry:
                    return new MP4AudioSampleEntry(stream);
                case BoxType.SampleDescriptionBox:
                    return new SampleDescriptionBox(stream);
                case BoxType.SampleSizeBox:
                    return new SampleSizeBox(stream);
                case BoxType.SampleTableBox:
                    return new SampleTableBox(stream);
                case BoxType.SampleToChunkBox:
                    return new SampleToChunkBox(stream);
                case BoxType.SoundMediaHeaderBox:
                    return new SoundMediaHeaderBox(stream);
                case BoxType.SyncSampleBox:
                    return new SyncSampleBox(stream);
                case BoxType.TrackBox:
                    return new TrackBox(stream);
                case BoxType.TrackHeaderBox:
                    return new TrackHeaderBox(stream);
                case BoxType.UserDataBox:
                    return new UserDataBox(stream);
                case BoxType.VideoMediaHeaderBox:
                    return new VideoMediaHeaderBox(stream);
                case BoxType.VideoProfileEntry:
                    return new VideoProfileEntry(stream);
                case BoxType.UserBox:
                    return ReadUserBox(stream, userType);
                default:
                    return new Box(stream);
            }
        }

        private static Box ReadUserBox(Stream stream, Guid userType)
        {
            if (userType == _3DDescriptorBox.UserTypeGuid)
            {
                return new _3DDescriptorBox(stream);
            }
            else if (userType == ProfileBox.UserTypeGuid)
            {
                return new ProfileBox(stream);
            }
            else if (userType == UserSpecificMetaDataBox.UserTypeGuid)
            {
                return new UserSpecificMetaDataBox(stream);
            }
            else
            {
                return new UserBox(stream);
            }
        }

        public static void WriteBoxHeader(Stream stream, BoxType boxType, ulong boxSize)
        {
            if (boxSize > UInt32.MaxValue)
            {
                BigEndianWriter.WriteUInt32(stream, 1);
            }
            else
            {
                BigEndianWriter.WriteUInt32(stream, (uint)boxSize);
            }
            BigEndianWriter.WriteUInt32(stream, (uint)boxType);
            if (boxSize > UInt32.MaxValue)
            {
                BigEndianWriter.WriteUInt64(stream, boxSize);
            }
        }

        public static Box FindBox(List<Box> boxes, BoxType type)
        {
            int index = IndexOfBox(boxes, type);
            if (index >= 0)
            {
                return boxes[index];
            }
            return null;
        }

        public static int IndexOfBox(List<Box> boxes, BoxType type)
        {
            for(int index = 0; index < boxes.Count; index++)
            {
                if (boxes[index].Type == type)
                {
                    return index;
                }
            }
            return -1;
        }

        public static void RemoveAll(List<Box> boxes, BoxType type)
        {
            int index = IndexOfBox(boxes, type);
            while (index >= 0)
            {
                boxes.RemoveAt(index);
                index = IndexOfBox(boxes, type);
            }
        }

        public static void RemoveRecursively(List<Box> boxes, BoxType type)
        {
            RemoveAll(boxes, type);

            foreach (Box box in boxes)
            {
                if (box.ContentType != BoxContentType.Data)
                {
                    RemoveRecursively(box.Children, type);
                }
            }
        }

        public static Box FindUserBox(List<Box> boxes, Guid userType)
        {
            int index = IndexOfUserBox(boxes, userType);
            if (index >= 0)
            {
                return boxes[index];
            }
            return null;
        }

        public static int IndexOfUserBox(List<Box> boxes, Guid userType)
        {
            for (int index = 0; index < boxes.Count; index++)
            {
                if (boxes[index] is UserBox && ((UserBox)boxes[index]).UserType == userType)
                {
                    return index;
                }
            }
            return -1;
        }

        public static List<Box> FindBoxes(List<Box> boxes, BoxType type)
        {
            List<Box> result = new List<Box>();
            foreach (Box box in boxes)
            {
                if (box.Type == type)
                {
                    result.Add(box);
                }
            }
            return result;
        }

        public static Box FindBoxFromPath(List<Box> boxes, params BoxType[] typePath)
        {
            List<Box> current = boxes;
            for(int index = 0; index < typePath.Length; index++)
            {
                Box box = FindBox(current, typePath[index]);
                if (box != null)
                {
                    if (index < typePath.Length - 1)
                    {
                        current = box.Children;
                    }
                    else
                    {
                        return box;
                    }
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        public static void UpdateUserBox(Box parent, UserBox child)
        {
            int index = BoxHelper.IndexOfUserBox(parent.Children, child.UserType);
            if (index >= 0)
            {
                parent.Children.RemoveAt(index);
            }
            parent.Children.Add(child);
        }

        public static List<long> GetMediaDataOffsets(List<Box> rootBoxes)
        {
            List<long> mediaDataOffsets = new List<long>();
            long currentOffset = 0;
            foreach (Box box in rootBoxes)
            {
                if (box.Type == BoxType.MediaDataBox)
                {
                    mediaDataOffsets.Add(currentOffset);
                }
                currentOffset += (long)box.Size;
            }
            return mediaDataOffsets;
        }
    }
}
