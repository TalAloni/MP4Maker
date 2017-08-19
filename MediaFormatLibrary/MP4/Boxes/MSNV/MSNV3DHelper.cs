/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaFormatLibrary.MP4
{
    public class MSNV3DHelper
    {
        public static void FlagProfileAndTrackAs3D(List<Box> rootBoxes)
        {
            ProfileBox profileBox = (ProfileBox)BoxHelper.FindUserBox(rootBoxes, ProfileBox.UserTypeGuid);
            MovieBox movieBox = (MovieBox)BoxHelper.FindBox(rootBoxes, BoxType.MovieBox);
            foreach (Box profile in profileBox.Children)
            {
                if (profile is VideoProfileEntry)
                {
                    FlagVideoProfileAs3D((VideoProfileEntry)profile);
                }
            }

            List<Box> tracks = BoxHelper.FindBoxes(movieBox.Children, BoxType.TrackBox);
            foreach (Box track in tracks)
            {
                FlagVideoTrackAs3D((TrackBox)track);
            }
        }

        public static void FlagVideoProfileAs3D(VideoProfileEntry videoProfile)
        {
            videoProfile.VideoAttributeFlags = 0x000B0002; // This is the value used by the two Sony 720p23.97 3D demo clips
        }

        public static void FlagVideoTrackAs3D(TrackBox track)
        {
            SampleTableBox sampleTableBox = (SampleTableBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox);
            AVCVisualSampleEntry avcBox = (AVCVisualSampleEntry)BoxHelper.FindBoxFromPath(sampleTableBox.Children, BoxType.SampleDescriptionBox, BoxType.AVCVisualSampleEntry);

            if (avcBox != null)
            {
                _3DDescriptorBox _3dDescription = new _3DDescriptorBox();
                // This is the value used by the two Sony 720p23.97 3D demo clips
                _3dDescription.Flags = 0x82811002;
                BoxHelper.UpdateUserBox(avcBox, _3dDescription);
            }
        }
    }
}
