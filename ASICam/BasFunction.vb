Imports OpenCvSharp
Imports OpenCvSharp.Extensions

Module BasFunction

    ' ==========================================================
    ' ★ .NETデザイナーのバグ(WFO1000)を完全にシャットアウトするため、
    '    カメラの状態変数とZWOCam本体をすべてモジュール側に集約します！
    '    モジュール内のPublic変数は、プロジェクト内のどこからでも直接変数名だけで呼び出せます。
    ' ==========================================================
    Public CamID As Integer = -1
    Public HasCooler As Boolean = False
    Public _isUpdatingUI As Boolean = False
    Public _isInitializing As Boolean = True
    Public ZWOCam As New ClsZwoCamera()

    ' ==========================================================
    ' ★【追加】画像調整用パラメータ
    ' ==========================================================
    ' 💡 コントラストと明るさ、明暗差補正(WDR)はOpenCV(ClsZwoCamera)側でソフトウェア処理します
    Public _contrastVal As Double = 1.0  ' α倍 (1.0 = 変化なし。範囲 0.0〜3.0程度)
    Public _brightnessVal As Integer = 0 ' β足す (0 = 変化なし。範囲 -100〜100程度)
    Public _wdrOn As Boolean = False     ' 明暗差補正(CLAHE) ON/OFF

    ' 👇 ★この1行を追加します（デフォルト値を3.0とします）
    Public _claheClipVal As Double = 3.0

    ' 💡 ガンマはZWO SDK側でハードウェア処理します
    Public _gammaVal As Integer = 50     ' 範囲 1〜100 (Default: 50)

    ' ★この1行を BasFunction.vb のPublic変数が並んでいる場所に追記してください
    Public _rtspServerProcess As System.Diagnostics.Process = Nothing

    ''' <summary>
    ''' OpenCVのMat形式（映像フレーム）をWindows FormsのPictureBoxで
    ''' 表示できるBitmap形式に変換します。
    ''' </summary>
    Public Function MatToBitmap(ByVal mat As Mat) As System.Drawing.Bitmap
        If mat IsNot Nothing AndAlso Not mat.Empty() Then
            Return BitmapConverter.ToBitmap(mat)
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' カメラから現在の設定値（SDKコントロール）を取得し、UIのラベルやスライダーを更新する
    ''' </summary>
    Public Sub UpdateUIFromCamera()
        If CamID = -1 Then Return

        _isUpdatingUI = True

        Dim val As Integer = 0
        Dim isAuto As Integer = 0

        ' ==========================================
        ' 1. ゲイン取得と反映
        ' ==========================================
        If ASI_SDK.ASIGetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_GAIN, val, isAuto) = 0 Then
            ' 範囲外エラー防止チェック
            If val >= FrmMain.TrkbGain.Minimum AndAlso val <= FrmMain.TrkbGain.Maximum Then
                FrmMain.TrkbGain.Value = CInt(val)
            End If
            FrmMain.LblGain.Text = $"Gain: {val} {(If(isAuto = 1, "(Auto)", ""))}"
        End If

        ' ==========================================
        ' 2. 露出取得と反映
        ' ==========================================
        If ASI_SDK.ASIGetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_EXPOSURE, val, isAuto) = 0 Then
            Dim valMs As Integer = CInt(val / 1000.0)
            ' 範囲外エラー防止チェック
            If valMs >= FrmMain.TrkbExp.Minimum AndAlso valMs <= FrmMain.TrkbExp.Maximum Then
                FrmMain.TrkbExp.Value = valMs
            End If
            FrmMain.LblExp.Text = $"Exp: {valMs} ms {(If(isAuto = 1, "(Auto)", ""))}"
        End If

        ' ==========================================
        ' 3. ホワイトバランス(赤・青)取得と反映
        ' ==========================================
        Dim rVal As Integer = 0, bVal As Integer = 0
        Dim rAuto As Integer = 0, bAuto As Integer = 0

        ASI_SDK.ASIGetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_WB_R, rVal, rAuto)
        ASI_SDK.ASIGetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_WB_B, bVal, bAuto)

        ' スライダーにも追随させる (範囲外チェック付き)
        If rVal >= FrmMain.TrkbWbR.Minimum AndAlso rVal <= FrmMain.TrkbWbR.Maximum Then
            FrmMain.TrkbWbR.Value = rVal
        End If
        If bVal >= FrmMain.TrkbWbB.Minimum AndAlso bVal <= FrmMain.TrkbWbB.Maximum Then
            FrmMain.TrkbWbB.Value = bVal
        End If

        FrmMain.LblWb.Text = $"WB (R/B): {rVal} / {bVal} {(If(rAuto = 1 OrElse bAuto = 1, "(Auto)", ""))}"

        ' ==========================================================
        ' 💡【問題解決】4. ガンマの定期取得・反映を完全にストップ
        ' ==========================================================
        ' ※ 手動操作中にタイマーが古い値を上書きする競合バグを避けるため、
        '    ここでの更新処理は完全に無効化（コメントアウト）しました。
        ' ==========================================================
        'If ASI_SDK.ASIGetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_GAMMA, val, isAuto) = 0 Then
        '    _gammaVal = val
        '    If val >= FrmMain.TrkbGamma.Minimum AndAlso val <= FrmMain.TrkbGamma.Maximum Then
        '        FrmMain.TrkbGamma.Value = val
        '    End If
        '    FrmMain.LblGamma.Text = $"Gamma: {val}"
        'End If

        ' ==========================================
        ' ★【追加】6. センサー温度の取得 (非冷却カメラでも取得可能)
        ' ==========================================
        If ASI_SDK.ASIGetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_TEMPERATURE, val, isAuto) = 0 Then
            Dim currentTemp As Double = val / 10.0
            FrmMain.LblSensorTemp.Text = $"温度: {currentTemp:F1}℃"
        End If

        ' ==========================================
        ' ★【追加】7. 冷却パワー（ペルチェ素子の稼働率）の取得
        ' ==========================================
        If HasCooler Then
            ' 💡 ASI_COOLER_POWER_PERC ではなく ASI_COOLER_POWER を指定します
            If ASI_SDK.ASIGetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_COOLER_POWER, val, isAuto) = 0 Then
                FrmMain.LblCoolerPower.Text = $"冷却パワー: {val}%"
            End If
        End If


        _isUpdatingUI = False
    End Sub

    ''' <summary>
    ''' カメラがサポートする制御項目の限界値を取得し、UIのTrackBarに反映します
    ''' </summary>
    Public Sub InitializeCameraTrackBars()
        Dim numControls As Integer = 0
        If ASI_SDK.ASIGetNumOfControls(CamID, numControls) <> 0 Then Return

        For i As Integer = 0 To numControls - 1
            Dim caps As New ASI_SDK.ASI_CONTROL_CAPS()
            If ASI_SDK.ASIGetControlCaps(CamID, i, caps) = 0 Then
                Select Case caps.ControlType
                    Case ASI_SDK.ASI_CONTROL_TYPE.ASI_GAIN
                        SetTrackBarRange(FrmMain.TrkbGain, caps)
                    Case ASI_SDK.ASI_CONTROL_TYPE.ASI_EXPOSURE
                        Dim minMs As Integer = CInt(caps.MinValue / 1000)
                        Dim maxMs As Integer = CInt(caps.MaxValue / 1000)
                        If maxMs > 5000 Then maxMs = 5000

                        FrmMain.TrkbExp.Minimum = minMs
                        FrmMain.TrkbExp.Maximum = maxMs

                        Dim defMs As Integer = CInt(caps.DefaultValue / 1000)
                        If defMs > maxMs Then defMs = maxMs
                        If defMs < minMs Then defMs = minMs
                        FrmMain.TrkbExp.Value = defMs
                    Case ASI_SDK.ASI_CONTROL_TYPE.ASI_WB_R
                        SetTrackBarRange(FrmMain.TrkbWbR, caps)
                    Case ASI_SDK.ASI_CONTROL_TYPE.ASI_WB_B
                        SetTrackBarRange(FrmMain.TrkbWbB, caps)
                    Case ASI_SDK.ASI_CONTROL_TYPE.ASI_GAMMA
                        SetTrackBarRange(FrmMain.TrkbGamma, caps)

                    ' 💡【設定漏れ修正】USB帯域の限界値をカメラから取得し、Tagと初期値を安全に自動同期！
                    Case ASI_SDK.ASI_CONTROL_TYPE.ASI_BANDWIDTHOVERLOAD
                        SetTrackBarRange(FrmMain.TrcUsbBandwidth, caps)
                        ' ラベルのTagにメーカー推奨デフォルト値をしっかり記憶させる
                        FrmMain.LblUsbBandwidth.Tag = caps.DefaultValue
                        FrmMain.TrcUsbBandwidth.Value = caps.DefaultValue
                        FrmMain.LblUsbBandwidth.Text = $"USB帯域 (Def: {caps.DefaultValue}%): {caps.DefaultValue}%"


                    ' 💡【ここを追加】目標温度の限界値（例: -40℃ 〜 30℃）をカメラから自動取得してスライダーに適用する
                    Case ASI_SDK.ASI_CONTROL_TYPE.ASI_TARGET_TEMP
                        SetTrackBarRange(FrmMain.TrcTargetTemp, caps)
                End Select
            End If
        Next
    End Sub

    Private Sub SetTrackBarRange(ByVal trk As TrackBar, caps As ASI_SDK.ASI_CONTROL_CAPS)
        trk.Minimum = caps.MinValue
        trk.Maximum = caps.MaxValue
        Dim val = caps.DefaultValue
        If val < trk.Minimum Then val = trk.Minimum
        If val > trk.Maximum Then val = trk.Maximum
        trk.Value = val
    End Sub

    ''' <summary>
    ''' 有線LANを最優先、次点でWi-Fiのアドレスを自動取得します。
    ''' </summary>
    Public Function GetLocalIPAddress() As String
        Dim wifiIP As String = ""

        Try
            ' PCに存在するすべてのネットワークカード（インターフェース）を走査
            Dim interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()

            For Each ni In interfaces
                ' 稼働状態（Up）かつ、ループバックや仮想回線（Tunnel等）を除外
                If ni.OperationalStatus = System.Net.NetworkInformation.OperationalStatus.Up AndAlso
               ni.NetworkInterfaceType <> System.Net.NetworkInformation.NetworkInterfaceType.Loopback AndAlso
               ni.NetworkInterfaceType <> System.Net.NetworkInformation.NetworkInterfaceType.Tunnel Then

                    ' そのネットワークカードに割り当てられているIPアドレス情報を取得
                    Dim ipProps = ni.GetIPProperties()
                    For Each unicast In ipProps.UnicastAddresses
                        ' IPv4アドレスだけを対象にする
                        If unicast.Address.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork Then
                            Dim targetIP As String = unicast.Address.ToString()

                            ' 💡【核心の優先判定】
                            ' 有線LAN（Ethernet）だった場合は、それが大正解なのでその場で即確定！
                            If ni.NetworkInterfaceType = System.Net.NetworkInformation.NetworkInterfaceType.Ethernet Then
                                Return targetIP
                            End If

                            ' Wi-Fi（Wireless80211）だった場合は、一旦キープしておく（有線が見つからなかった時の保険）
                            If ni.NetworkInterfaceType = System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211 Then
                                wifiIP = targetIP
                            End If
                        End If
                    Next
                End If
            Next
        Catch ex As Exception
            ' ネットワーク情報の取得エラー時はスルー
        End Try

        ' 有線LANが見つからず、Wi-Fiだけが繋がっていた場合はWi-Fiのアドレスを返す
        If Not String.IsNullOrEmpty(wifiIP) Then
            Return wifiIP
        End If

        ' どちらもダメだった場合の最終防衛線
        Return "127.0.0.1"
    End Function

    ' ==========================================================
    ' ★【追加】GPUの判定結果を記憶する変数（毎回調べると重いのでキャッシュします）
    ' ==========================================================
    Public _hasIntel As Boolean = False
    Public _hasNvidia As Boolean = False
    Public _hasAmd As Boolean = False
    Public _isGpuChecked As Boolean = False

    ''' <summary>
    ''' アプリ起動時に1回だけPCのGPUをチェックします
    ''' </summary>
    Public Sub CheckGpuCapabilities()
        If _isGpuChecked Then Return
        Try
            Dim searcher As New System.Management.ManagementObjectSearcher("SELECT Name FROM Win32_VideoController")
            For Each obj As System.Management.ManagementObject In searcher.Get()
                Dim gpuName As String = obj("Name").ToString().ToLower()
                If gpuName.Contains("nvidia") Then _hasNvidia = True
                If gpuName.Contains("amd") OrElse gpuName.Contains("radeon") Then _hasAmd = True
                If gpuName.Contains("intel") Then _hasIntel = True
            Next
        Catch ex As Exception
        End Try
        _isGpuChecked = True
    End Sub

  ''' <summary>
    ''' PCに搭載されているGPUを判定し、自動選択されるエンコーダの名前を返します
    ''' </summary>
    Public Function GetAutoDetectedEncoderName() As String
        If Not _isGpuChecked Then CheckGpuCapabilities()

        If _hasNvidia Then Return "NVIDIA NVENC"
        If _hasAmd Then Return "AMD AMF"
        If _hasIntel Then Return "Intel QSV"
        Return "ソフトウェア (libx264)"
    End Function

    ''' <summary>
    ''' PCの搭載GPUを判定し、最も安全なエンコード引数を生成します（キャッシュ利用で爆速化）
    ''' </summary>
    Public Function GetOptimalEncoderArgs(fpsStr As String, gopSize As Integer) As String
        If Not _isGpuChecked Then CheckGpuCapabilities()

        Dim pref As Integer = My.Settings.EncoderPreference
        Dim softwareArgs As String = $"-c:v libx264 -preset veryfast -tune zerolatency -crf 20 -b:v 8M -maxrate 8M -bufsize 4M -r {fpsStr} -g {gopSize} "

        Select Case pref
            Case 1 ' QSV
                If _hasIntel Then Return $"-c:v h264_qsv -preset fast -b:v 8M -maxrate 8M -bufsize 4M -r {fpsStr} -g {gopSize} " Else Return softwareArgs
            Case 2 ' NVENC
                If _hasNvidia Then Return $"-c:v h264_nvenc -preset fast -b:v 8M -maxrate 8M -bufsize 4M -r {fpsStr} -g {gopSize} " Else Return softwareArgs
            Case 3 ' AMF
                If _hasAmd Then Return $"-c:v h264_amf -quality speed -b:v 8M -maxrate 8M -bufsize 4M -r {fpsStr} -g {gopSize} " Else Return softwareArgs
            Case 4 ' ソフトウェア
                Return softwareArgs
            Case Else ' 0 (自動判定)
                If _hasNvidia Then Return $"-c:v h264_nvenc -preset fast -b:v 8M -maxrate 8M -bufsize 4M -r {fpsStr} -g {gopSize} "
                If _hasAmd Then Return $"-c:v h264_amf -quality speed -b:v 8M -maxrate 8M -bufsize 4M -r {fpsStr} -g {gopSize} "
                If _hasIntel Then Return $"-c:v h264_qsv -preset fast -b:v 8M -maxrate 8M -bufsize 4M -r {fpsStr} -g {gopSize} "
                Return softwareArgs
        End Select
    End Function

End Module