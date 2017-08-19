/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.MP4
{
    /// <summary>
    /// a.k.a. USMT
    /// see: http://rubenlaguna.com/wp/2007/02/25/how-to-read-title-in-sony-psp-mp4-files/
    /// </summary>
    public class UserSpecificMetaDataBox : UserBox
    {
        public static readonly Guid UserTypeGuid = new Guid("55534D54-21d2-4fce-bb88-695cfac9c740"); // the first 4 bytes equals to 'USMT'

        public UserSpecificMetaDataBox() : base(UserTypeGuid)
        {
        }

        public UserSpecificMetaDataBox(Stream stream) : base(stream)
        {
        }

        public override BoxContentType ContentType
        {
            get
            {
                return BoxContentType.DataAndChildren;
            }
        }
    }
}
