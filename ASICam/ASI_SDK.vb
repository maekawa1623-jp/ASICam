Imports System.Runtime.InteropServices

Public Class ASI_SDK

    Const DLL_PATH As String = "ASICamera2.dll"

    ' --- 列挙型 (Enumerations) ---
    Public Enum ASI_IMG_TYPE
        ASI_IMG_RAW8 = 0   ' 1ピクセル1バイト (グレースケール/解析用) [cite: 40]
        ASI_IMG_RGB24 = 1  ' RGB各1バイト、計3バイト (カラー表示用) [cite: 41]
        ASI_IMG_RAW16 = 2  ' 1ピクセル2バイト (高精細) [cite: 42]
        ASI_IMG_Y8 = 3     ' カラーカメラ用モノクロモード [cite: 43]
    End Enum

    ' ZWO公式マニュアル (Revision 2.9) に完全準拠したコントロール番号と機能一覧
    Public Enum ASI_CONTROL_TYPE
        ASI_GAIN = 0              ' ゲイン（感度） [cite: 131]
        ASI_EXPOSURE = 1          ' 露出時間（単位：マイクロ秒） 
        ASI_GAMMA = 2             ' ガンマ値（1〜100の範囲、通常は50） 
        ASI_WB_R = 3              ' ホワイトバランス（赤成分） [cite: 133]
        ASI_WB_B = 4              ' ホワイトバランス（青成分） [cite: 134]
        ASI_BRIGHTNESS = 5        ' ピクセル値のオフセット（旧ASI_OFFSET / 明るさの底上げ） [cite: 135]
        ASI_BANDWIDTHOVERLOAD = 6 ' USB転送帯域の割り当て（％） [cite: 136]
        ASI_OVERCLOCK = 7         ' オーバークロック [cite: 137]
        ASI_TEMPERATURE = 8       ' センサー温度（実際の温度の10倍の値が返る） 
        ASI_FLIP = 9              ' 画像の反転（上下・左右） [cite: 139]
        ASI_AUTO_MAX_GAIN = 10    ' オート調整時の最大ゲイン値 [cite: 140]
        ASI_AUTO_MAX_EXP = 11     ' オート調整時の最大露出時間（単位：マイクロ秒） [cite: 141]
        ASI_AUTO_MAX_BRIGHTNESS = 12 ' オート調整時の目標の明るさ [cite: 142]
        ASI_HARDWARE_BIN = 13     ' ハードウェアビニング [cite: 143]
        ASI_HIGH_SPEED_MODE = 14  ' ハイスピードモード（転送速度優先） [cite: 144]
        ASI_COOLER_POWER = 15     ' 冷却パワー（％） ※冷却カメラのみ。公式名: ASI_COOLER_POWER_PERC [cite: 145]
        ASI_TARGET_TEMP = 16      ' センサーの目標温度 ※冷却カメラのみ。10倍しなくてよい 
        ASI_COOLER_ON = 17        ' 冷却スイッチ（ペルチェ素子）のON/OFF ※冷却カメラのみ [cite: 147]
        ASI_MONO_BIN = 18         ' ソフトウェアビニング（カラーカメラ用） [cite: 148]
        ASI_FAN_ON = 19           ' 空冷ファンのON/OFF ※冷却カメラのみ [cite: 149]
        ASI_PATTERN_ADJUST = 20   ' パターン調整（現在はASI1600モノクロカメラ等でサポート） [cite: 150]
        ASI_ANTI_DEW_HEATER = 21  ' 結露防止ヒーターのON/OFF [cite: 151]
    End Enum

    ' --- 構造体 (Structures) ---
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
    Public Structure ASI_CAMERA_INFO
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=64)> Public Name As String ' [cite: 106]
        Public CameraID As Integer ' [cite: 107]
        Public MaxHeight As Integer ' [cite: 108] (longはC++では32bitのためInteger)
        Public MaxWidth As Integer ' [cite: 109]
        Public IsColorCam As Integer ' [cite: 110] (ASI_BOOL)
        Public BayerPattern As Integer ' [cite: 111] (ASI_BAYER_PATTERN)

        ' 重要：固定長配列のサイズを正確に指定する
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=16)> Public SupportedBins As Integer() ' [cite: 112]
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)> Public SupportedVideoFormat As Integer() ' [cite: 114]

        Public PixelSize As Double ' [cite: 115]
        Public MechanicalShutter As Integer ' [cite: 116]
        Public ST4Port As Integer ' [cite: 117]
        Public IsCoolerCam As Integer ' [cite: 118]
        Public IsUSB3Host As Integer ' [cite: 119]
        Public IsUSB3Camera As Integer ' [cite: 120]
        Public ElecPerADU As Single ' [cite: 121]
        Public BitDepth As Integer ' [cite: 122]
        Public IsTriggerCam As Integer ' [cite: 123]

        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=16)> Public Unused As String ' [cite: 124]
    End Structure

    ' --- 関数宣言 (Function Declarations) ---
    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIGetNumOfConnectedCameras() As Integer ' [cite: 192]
    End Function

    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIOpenCamera(ByVal iCameraID As Integer) As Integer ' [cite: 209]
    End Function

    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIInitCamera(ByVal iCameraID As Integer) As Integer ' [cite: 213]
    End Function

    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASISetROIFormat(ByVal iCameraID As Integer, ByVal iWidth As Integer, ByVal iHeight As Integer, ByVal iBin As Integer, ByVal Img_type As ASI_IMG_TYPE) As Integer ' [cite: 248]
    End Function

    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASISetControlValue(ByVal iCameraID As Integer, ByVal ControlType As ASI_CONTROL_TYPE, ByVal lValue As Integer, ByVal bAuto As Integer) As Integer ' [cite: 240]
    End Function

    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASICloseCamera(ByVal iCameraID As Integer) As Integer ' [cite: 217]
    End Function

    ' --- ASI_SDK.vb への追加分 ---
    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIStartVideoCapture(ByVal iCameraID As Integer) As Integer ' [cite: 305]
    End Function

    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIStopVideoCapture(ByVal iCameraID As Integer) As Integer ' [cite: 308]
    End Function

    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIGetVideoData(ByVal iCameraID As Integer, ByVal pBuffer As IntPtr, ByVal lBuffSize As Integer, ByVal iWaitms As Integer) As Integer ' [cite: 311]
    End Function

    ' --- ASI_SDK.vb 内にこれがあるか確認 ---
    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIGetCameraProperty(ByRef pASICameraInfo As ASI_CAMERA_INFO, ByVal iCameraIndex As Integer) As Integer
    End Function

    ' --- ASI_SDK.vb への追加 ---
    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIGetControlValue(ByVal iCameraID As Integer, ByVal ControlType As ASI_CONTROL_TYPE, ByRef plValue As Integer, ByRef pbAuto As Integer) As Integer ' [cite: 233]
    End Function

    ' --- ASI_SDK.vb 内に構造体を追加 ---
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
    Public Structure ASI_CONTROL_CAPS
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=64)> Public Name As String ' [cite: 156]
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)> Public Description As String ' [cite: 157]
        Public MaxValue As Integer ' [cite: 158]
        Public MinValue As Integer ' [cite: 159]
        Public DefaultValue As Integer ' [cite: 160]
        Public IsAutoSupported As Integer ' [cite: 161]
        Public IsWritable As Integer ' [cite: 162]
        Public ControlType As ASI_CONTROL_TYPE ' [cite: 163]
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)> Public Unused As Byte() ' [cite: 164]
    End Structure

    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIGetNumOfControls(ByVal iCameraID As Integer, ByRef piNumberOfControls As Integer) As Integer ' [cite: 224]
    End Function

    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIGetControlCaps(ByVal iCameraID As Integer, ByVal iControlIndex As Integer, ByRef pControlCaps As ASI_CONTROL_CAPS) As Integer ' [cite: 226]
    End Function

    ' ASI_SDK.vb 内に追加
    <DllImport(DLL_PATH, CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function ASIGetROIFormat(ByVal IntCamID As Integer, ByRef pWidth As Integer, ByRef pHeight As Integer, ByRef pBin As Integer, ByRef pImg_type As ASI_IMG_TYPE) As Integer
    End Function

End Class
