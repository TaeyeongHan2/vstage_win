using UnityEngine;
using VIVE.OpenXR.FacialTracking;
using System.Collections.Generic;

/// <summary>
/// VIVE의 37개 립 표현식과 Shinano의 블렌드셰이프 간의 참조 매핑
/// 페이셜 트래킹 설정을 위한 종합 가이드
/// Based on actual available XrLipExpressionHTC enums in VIVE OpenXR
/// </summary>
public static class VIVEToShinanoMappingReference
{
    // 표현식 유사성에 기반한 매핑 제안
    public static Dictionary<XrLipExpressionHTC, List<string>> GetComprehensiveMappings()
    {
        return new Dictionary<XrLipExpressionHTC, List<string>>
        {
            // ===== 턱 & 입 열기 =====
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC, new List<string> { "mouth_a1", "mouth_a2", "mouthparts_open" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_LEFT_HTC, new List<string> { "mouth_grin1_L", "mouth_H_L" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_RIGHT_HTC, new List<string> { "mouth_grin1_R", "mouth_H_R" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_FORWARD_HTC, new List<string> { "other_pout", "mouth_corner_thick" } },
            
            // ===== 미소 & 웃음 =====
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_LEFT_HTC, new List<string> { "mouth_smile", "mouth_smile_L" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_RIGHT_HTC, new List<string> { "mouth_smile", "mouth_smile_R" } },
            
            // ===== 입 벌리기 & 늘이기 =====
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_LEFT_HTC, new List<string> { "mouth_wide", "mouth_wide_L", "mouth_i1" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_RIGHT_HTC, new List<string> { "mouth_wide", "mouth_wide_R", "mouth_i2" } },
            
            // ===== 입술 위치 조정 =====
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_LEFT_HTC, new List<string> { "mouth_e1", "mouth_V" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_RIGHT_HTC, new List<string> { "mouth_e2", "mouth_V" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_LEFT_HTC, new List<string> { "mouth_center_down" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_RIGHT_HTC, new List<string> { "mouth_center_down" } },
            
            // ===== 입술 상하 움직임 =====
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC, new List<string> { "mouth_e1", "mouth_upper_R" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPLEFT_HTC, new List<string> { "mouth_e2", "mouth_upper_L" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNRIGHT_HTC, new List<string> { "mouth_sad_R", "mouth_center_down" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNLEFT_HTC, new List<string> { "mouth_sad_L", "mouth_center_down" } },
            
            // ===== 특수 입 모양 =====
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC, new List<string> { "mouth_ω", "mouth_○_big", "mouth_wa", "mouth_u1", "mouth_o1" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_APE_SHAPE_HTC, new List<string> { "mouth_□", "mouth_corner_up" } },
            
            // ===== 입술 뒤집기 =====
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_OVERTURN_HTC, new List<string> { "mouth_turnover" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_OVERTURN_HTC, new List<string> { "mouth_turnover" } },
            
            // ===== 입술 내부 =====
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_INSIDE_HTC, new List<string> { "mouth_narrow" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_INSIDE_HTC, new List<string> { "mouth_narrow" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_OVERLAY_HTC, new List<string> { "mouth_center_down" } },
            
            // ===== 볼 =====
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_PUFF_LEFT_HTC, new List<string> { "other_cheek_2", "other_cheek_3" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_PUFF_RIGHT_HTC, new List<string> { "other_cheek_2", "other_cheek_3" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_SUCK_HTC, new List<string> { "other_pout" } },
            
            // ===== 혀 =====
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_LONGSTEP1_HTC, new List<string> { "tongue_1", "tongue_pero" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_LONGSTEP2_HTC, new List<string> { "tongue_2", "tongue_long" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_DOWN_HTC, new List<string> { "tongue_position_down", "tongue_angle_down" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_UP_HTC, new List<string> { "tongue_position_up", "tongue_angle_up" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_RIGHT_HTC, new List<string> { "tongue_pero_R" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_LEFT_HTC, new List<string> { "tongue_pero_L" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_ROLL_HTC, new List<string> { "tongue_round" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_UPRIGHT_MORPH_HTC, new List<string> { "tongue_sharp", "tongue_pero_R" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_UPLEFT_MORPH_HTC, new List<string> { "tongue_sharp", "tongue_pero_L" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_DOWNRIGHT_MORPH_HTC, new List<string> { "tongue_wide", "tongue_pero_R" } },
            { XrLipExpressionHTC.XR_LIP_EXPRESSION_TONGUE_DOWNLEFT_MORPH_HTC, new List<string> { "tongue_wide", "tongue_pero_L" } }
        };
    }
    
    // 자연스러운 표정 조합 예시
    public static Dictionary<string, Dictionary<XrLipExpressionHTC, float>> GetNaturalExpressions()
    {
        return new Dictionary<string, Dictionary<XrLipExpressionHTC, float>>
        {
            // 기쁨
            ["행복한 미소"] = new Dictionary<XrLipExpressionHTC, float>
            {
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_LEFT_HTC, 0.8f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_RIGHT_HTC, 0.8f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_LEFT_HTC, 0.3f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_RIGHT_HTC, 0.3f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_PUFF_LEFT_HTC, 0.2f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_CHEEK_PUFF_RIGHT_HTC, 0.2f }
            },
            
            // 놀람
            ["놀란 표정"] = new Dictionary<XrLipExpressionHTC, float>
            {
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC, 0.6f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC, 0.5f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPLEFT_HTC, 0.3f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC, 0.3f }
            },
            
            // 슬픔
            ["슬픈 표정"] = new Dictionary<XrLipExpressionHTC, float>
            {
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNLEFT_HTC, 0.7f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_DOWNRIGHT_HTC, 0.7f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_LEFT_HTC, 0.4f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_LOWER_RIGHT_HTC, 0.4f }
            },
            
            // 발음 - 아
            ["아 (A)"] = new Dictionary<XrLipExpressionHTC, float>
            {
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC, 0.8f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_LEFT_HTC, 0.2f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_RIGHT_HTC, 0.2f }
            },
            
            // 발음 - 이
            ["이 (I)"] = new Dictionary<XrLipExpressionHTC, float>
            {
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_LEFT_HTC, 0.7f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_STRETCHER_RIGHT_HTC, 0.7f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC, 0.1f }
            },
            
            // 발음 - 우
            ["우 (U)"] = new Dictionary<XrLipExpressionHTC, float>
            {
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC, 0.8f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC, 0.2f }
            },
            
            // 발음 - 에
            ["에 (E)"] = new Dictionary<XrLipExpressionHTC, float>
            {
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC, 0.3f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPLEFT_HTC, 0.4f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_UPPER_UPRIGHT_HTC, 0.4f }
            },
            
            // 발음 - 오
            ["오 (O)"] = new Dictionary<XrLipExpressionHTC, float>
            {
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC, 0.5f },
                { XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_POUT_HTC, 0.6f }
            }
        };
    }
    
    // 특수 표정 조합 (이모티콘 스타일)
    public static Dictionary<string, string[]> GetEmoticonMappings()
    {
        return new Dictionary<string, string[]>
        {
            ["^_^"] = new[] { "eye_joy", "mouth_smile" },
            [">_<"] = new[] { "eye_><", "mouth_△_small" },
            ["o_o"] = new[] { "eye_○", "mouth_○_small" },
            ["T_T"] = new[] { "eye_T", "mouth_sad" },
            ["@_@"] = new[] { "eye_@", "mouth_grin2" },
            ["♥_♥"] = new[] { "eye_♥", "mouth_wa" },
            ["ω"] = new[] { "mouth_ω", "other_cheek_1" },
            [":3"] = new[] { "mouth_cat", "mouth_smile" },
            [":P"] = new[] { "tongue_pero", "mouth_smile", "eye_wink_L" },
            ["D:"] = new[] { "mouth_sad", "eye_sad", "eyebrow_sad1" }
        };
    }
    
    // 디버그용 - 실제 사용 가능한 enum 이름 목록
    public static string[] GetAllAvailableExpressionNames()
    {
        return new string[]
        {
            // Jaw movements
            "JAW_RIGHT", "JAW_LEFT", "JAW_FORWARD", "JAW_OPEN",
            // Mouth shapes
            "MOUTH_APE_SHAPE", "MOUTH_UPPER_RIGHT", "MOUTH_UPPER_LEFT",
            "MOUTH_LOWER_RIGHT", "MOUTH_LOWER_LEFT", "MOUTH_UPPER_OVERTURN",
            "MOUTH_LOWER_OVERTURN", "MOUTH_POUT", 
            // Smile/Stretch
            "MOUTH_RAISER_RIGHT", "MOUTH_RAISER_LEFT", 
            "MOUTH_STRETCHER_RIGHT", "MOUTH_STRETCHER_LEFT",
            // Upper/Lower movements
            "MOUTH_UPPER_UPRIGHT", "MOUTH_UPPER_UPLEFT", 
            "MOUTH_LOWER_DOWNRIGHT", "MOUTH_LOWER_DOWNLEFT",
            // Mouth inside
            "MOUTH_UPPER_INSIDE", "MOUTH_LOWER_INSIDE", "MOUTH_LOWER_OVERLAY",
            // Tongue
            "TONGUE_LONGSTEP1", "TONGUE_LONGSTEP2", "TONGUE_DOWN", "TONGUE_UP",
            "TONGUE_RIGHT", "TONGUE_LEFT", "TONGUE_ROLL", 
            "TONGUE_UPRIGHT_MORPH", "TONGUE_UPLEFT_MORPH", 
            "TONGUE_DOWNRIGHT_MORPH", "TONGUE_DOWNLEFT_MORPH",
            // Cheek
            "CHEEK_PUFF_RIGHT", "CHEEK_PUFF_LEFT", "CHEEK_SUCK"
        };
    }
    
    // 使用可能なマッピングの推奨設定
    public static List<string> GetRecommendedMappings()
    {
        return new List<string>
        {
            "JAW_OPEN → mouth_a1 (基本的な口の開き)",
            "MOUTH_RAISER_LEFT/RIGHT → mouth_smile (笑顔)",
            "MOUTH_STRETCHER_LEFT/RIGHT → mouth_wide, mouth_i1 (横に広げる/い音)",
            "MOUTH_POUT → mouth_o1, mouth_u1 (お/う音)",
            "MOUTH_LOWER_DOWNLEFT/RIGHT → mouth_sad (悲しい表情)",
            "CHEEK_PUFF_LEFT/RIGHT → other_cheek_2 (頬を膨らませる)",
            "CHEEK_SUCK → other_pout (頬をすぼめる)",
            "TONGUE_LONGSTEP1 → tongue_pero (舌を出す)"
        };
    }
} 