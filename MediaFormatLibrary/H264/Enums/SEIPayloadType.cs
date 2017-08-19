using System;
using System.Collections.Generic;
using System.Text;

namespace MediaFormatLibrary.H264
{
    public enum SEIPayloadType : byte
    {
        BufferingPeriod = 0, // buffering_period
        PicTiming = 1, // pic_timing
        PanScanRect = 2, // pan_scan_rect
        FillerPayload = 3, // filler_payload
        UserDataRegistered = 4, // user_data_registered_itu_t_t35
        UserDataUnregistered = 5, // user_data_unregistered
        RecoveryPoint = 6, // recovery_point
        DecRefPicMarkingRepetition = 7, // dec_ref_pic_marking_repetition
        SparePic = 8, // spare_pic
        SceneInfo = 9, // scene_info
        SubSeqInfo = 10, // sub_seq_info
        SubSeqLayerCharacteristics = 11, // sub_seq_layer_characteristics
        SubSeqCharacteristics = 12, // sub_seq_characteristics
        FullFrameFreeze = 13, // FullFrameFreeze
        FullFrameFreezeRelease = 14, // full_frame_freeze_release
        FullFrameSnapshot = 15, // full_frame_snapshot
        ProgressiveRefinementSegmentStart = 16, // progressive_refinement_segment_start
        ProgressiveRefinementSegmentEnd = 17, // progressive_refinement_segment_end
        MotionConstrainedSliceGroupSet = 18, // motion_constrained_slice_group_set
        FilmGrainCharacteristics = 19, // film_grain_characteristics
        DeblockingFilterDisplayPreference = 20, // deblocking_filter_display_preference
        StereoVideoInfo = 21, // stereo_video_info
        PostFilterHint = 22, // post_filter_hint
        ToneMappingInfo = 23, // tone_mapping_info
        ScalabilityInfo = 24, // scalability_info
        SubPicScalableLayer = 25, // sub_pic_scalable_layer
        NonRequiredLayerRep = 26, // non_required_layer_rep
        PriorityLayerInfo = 27, // priority_layer_info
        LayersNotPresent = 28, // layers_not_present
        LayerDependencyChange = 29, // layer_dependency_change
        ScalableNesting = 30, // scalable_nesting
        BaseLayerTemporalHrd = 31, // base_layer_temporal_hrd
        QualityLayerIntegrityCheck = 32, // quality_layer_integrity_check
        RedundantPicProperty = 33, // redundant_pic_property
        Tl0DepRepIndex = 34, // tl0_dep_rep_index
        TlSwitchingPoint = 35, // tl_switching_point
        ParallelDecodingInfo = 36, // parallel_decoding_info
        MvcScalableNesting = 37, // mvc_scalable_nesting
        ViewScalabilityInfo = 38, // view_scalability_info
        MultiviewSceneInfo = 39, // multiview_scene_info
        MultiviewAcquisitionInfo = 40, // multiview_acquisition_info
        NonRequiredViewComponent = 41, // non_required_view_component
        ViewDependencyChange = 42, // view_dependency_change
        OperationPointsNotPresent = 43, // operation_points_not_present
        BaseViewTemporalHrd = 44, // base_view_temporal_hrd
        FramePackingArrangement = 45, // frame_packing_arrangement
        MultiviewViewPosition = 46, // multiview_view_position
        DisplayOrientation = 47, // display_orientation
        MvcdScalableNesting = 48, // mvcd_scalable_nesting
        MvcdViewScalabilityInfo = 49, // mvcd_view_scalability_info
        DepthRepresentationInfo = 50, // depth_representation_info
        ThreeDimensionalReferenceDisplaysInfo = 51, // three_dimensional_reference_displays_info
        DepthTiming = 52, // depth_timing
        DepthSamplingInfo = 53, // depth_sampling_info
    }
}
