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

namespace MediaFormatLibrary.MP4
{
    public class TrackHelper
    {
        public static List<ulong> FindChunkOffsetList(SampleTableBox sampleTable)
        {
            List<ulong> result = new List<ulong>();
            ChunkLargeOffsetBox chunkLargeOffsetBox = (ChunkLargeOffsetBox)BoxHelper.FindBox(sampleTable.Children, BoxType.ChunkLargeOffsetBox);
            if (chunkLargeOffsetBox != null)
            {
                return chunkLargeOffsetBox.Entries;
            }
            else
            {
                ChunkOffsetBox chunkOffsetBox = (ChunkOffsetBox)BoxHelper.FindBox(sampleTable.Children, BoxType.ChunkOffsetBox);
                if (chunkOffsetBox != null)
                {
                    foreach (uint chunkOffset in chunkOffsetBox.Entries)
                    {
                        result.Add(chunkOffset);
                    }
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public static Box CreateChunkOffsetBox(List<ulong> chunkOffsetList)
        {
            bool largeOffset = false;
            foreach (ulong offset in chunkOffsetList)
            {
                if (offset > UInt32.MaxValue)
                {
                    largeOffset = true;
                    break;
                }
            }

            if (largeOffset)
            {
                ChunkLargeOffsetBox chunkLargeOffsetBox = new ChunkLargeOffsetBox();
                chunkLargeOffsetBox.Entries = chunkOffsetList;
                return chunkLargeOffsetBox;
            }
            else
            {
                ChunkOffsetBox chunkOffsetBox = new ChunkOffsetBox();
                foreach (ulong offset in chunkOffsetList)
                {
                    chunkOffsetBox.Entries.Add((uint)offset);
                }
                return chunkOffsetBox;
            }
        }

        /// <returns>Average bitrate in bps</returns>
        public static double CalculateTrackAverageBitrate(TrackBox track)
        {
            ulong size = CalculateTrackSize(track);
            MediaHeaderBox mediaHeaderBox = (MediaHeaderBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.MediaHeaderBox);
            double duration = (double)mediaHeaderBox.Duration / mediaHeaderBox.Timescale;
            return (size / duration) * 8;
        }

        public static ulong CalculateTrackSize(TrackBox track)
        {
            SampleTableBox sampleTable = (SampleTableBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox);
            SampleSizeBox sampleSizeBox = (SampleSizeBox)BoxHelper.FindBox(sampleTable.Children, BoxType.SampleSizeBox);
            SampleToChunkBox sampleToChunk = (SampleToChunkBox)BoxHelper.FindBox(sampleTable.Children, BoxType.SampleToChunkBox);
            List<ulong> chunkOffsetList = FindChunkOffsetList(sampleTable);
            List<long> chunkSizeList = GetChunkSizeList(sampleSizeBox, sampleToChunk, chunkOffsetList.Count);

            ulong result = 0;
            for (int chunkIndex = 0; chunkIndex < chunkSizeList.Count; chunkIndex++)
            {
                result += (ulong)chunkSizeList[chunkIndex];
            }
            return result;
        }

        /// <summary>
        /// Will take SampleToChunkBox and return an uncompressed SampleToChunkEntry List (one entry for each chunk)
        /// </summary>
        public static List<SampleToChunkEntry> UncompressSampleToChunkBox(SampleToChunkBox sampleToChunk, int chunkCount)
        {
            List<SampleToChunkEntry> result = new List<SampleToChunkEntry>();
            for (int sampleToChunkIndex = 0; sampleToChunkIndex < sampleToChunk.Entries.Count; sampleToChunkIndex++)
            {
                SampleToChunkEntry sampleToChunkEntry = sampleToChunk.Entries[sampleToChunkIndex];
                int numberOfChunksInEntry;
                uint firstChunkIndex = sampleToChunkEntry.FirstChunk - 1;
                if (sampleToChunkIndex < sampleToChunk.Entries.Count - 1)
                {
                    uint firstChunkIndexOfNextEntry = sampleToChunk.Entries[sampleToChunkIndex + 1].FirstChunk - 1;
                    numberOfChunksInEntry = (int)(firstChunkIndexOfNextEntry - firstChunkIndex);
                }
                else
                {
                    numberOfChunksInEntry = (int)(chunkCount - firstChunkIndex);
                }

                
                for (int index = 0; index < numberOfChunksInEntry; index++)
                {
                    int chunkIndex = result.Count + 1;
                    result.Add(new SampleToChunkEntry((uint)chunkIndex, sampleToChunkEntry.SamplesPerChunk, sampleToChunkEntry.SampleDescriptionIndex));
                }
            }
            return result;
        }

        public static List<long> GetChunkSizeList(SampleSizeBox sampleSizeBox, SampleToChunkBox sampleToChunk, int chunkCount)
        {
            List<long> chunkSizeList = new List<long>();

            uint sampleCount = sampleSizeBox.SampleCount;

            int sampleIndex = 0;
            List<SampleToChunkEntry> sampleToChunkList = UncompressSampleToChunkBox(sampleToChunk, chunkCount);
            foreach(SampleToChunkEntry sampleToChunkEntry in sampleToChunkList)
            {
                long chunkSize = CountSizeOfSamples(sampleSizeBox, sampleIndex, (int)sampleToChunkEntry.SamplesPerChunk);
                chunkSizeList.Add(chunkSize);
                sampleIndex += (int)sampleToChunkEntry.SamplesPerChunk;
            }
            return chunkSizeList;
        }

        private static long CountSizeOfSamples(SampleSizeBox sampleSizeBox, int sampleIndex, int sampleCount)
        {
            if (sampleSizeBox.SampleSize == 0)
            {
                long result = 0;
                for (int offset = 0; offset < sampleCount; offset++)
                {
                    result += sampleSizeBox.Entries[sampleIndex + offset];
                }
                return result;
            }
            else
            {
                return sampleSizeBox.SampleSize * sampleCount;
            }
        }
    }
}
