using System;
using System.Collections.Generic;
using System.Text;

namespace MediaFormatLibrary.H264
{
    public enum NalUnitType : byte
    {
        Unspecified = 0,
        CodedSliceNonIDR = 1,
        CodedSliceDataPartitionA = 2,
        CodedSliceDataPartitionB = 3,
        CodedSliceDataPartitionC = 4,
        CodedSliceIDR = 5,
        SEI = 6,
        SequenceParameterSet = 7,
        PictureParameterSet = 8,
        AccessUnitDelimiter = 9,
        EndOfSequence = 10,
        EndOfStream = 11,
        FillerData = 12,
        SequenceParameterSetExtension = 13,
        PrefixNalUnit = 14,
        SubsetSequenceParameterSet = 15,
        CodedSliceExtension = 20,
        CodedSliceExtensionForDepthView = 21,

        /// <summary>
        /// Blu-ray specific NAL that is not part of the H.264 standard.
        /// MVC bitstream DRD is used to separate base view component and dependent
        /// view component within single access unit.
        /// Blu-ray spec requires every MVC access unit to start with AUD.
        /// Every MVC AU contains base view component and dependent view component.
        /// Coded representation of dependent view component within AU should be started with DRD.
        /// </summary>
        DependencyRepresentationDelimiter = 24,
    }
}
