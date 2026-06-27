Imports OpenCvSharp

Public Class FrmMain
    ' プレビュー用フォームのインスタンスを保持
    ' 💡 最初は空っぽにしておき、必要な時だけ生成します
    Private previewForm As FrmPreview = Nothing

    ' ★ デザイナーバグの元凶だったカメラ関連 of 変数宣言はすべて BasFunction へ引っ越し済みです。

    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' 起動時にカメラリストを自動取得
        RefreshCameraList()

        ' ==========================================================
        ' 💡【追加】起動時の画面セット（HighSpeed=OFF、帯域=100%）
        ' ==========================================================
        _isUpdatingUI = True

        ChkHighSpeed.Checked = False
        TrcUsbBandwidth.Value = 100
        ' ラベルの表示も合わせて更新しておく
        LblUsbBandwidth.Text = $"USB帯域: {TrcUsbBandwidth.Value}%"

        ' 💡【冷却用を追加】
        ChkCoolerOn.Checked = False
        TrcTargetTemp.Value = 0
        LblTargetTemp.Text = "目標温度: 0℃"

        _isUpdatingUI = False

        ' 未接続時はUSBと冷却設定を無効化
        TrcUsbBandwidth.Enabled = False
        ChkHighSpeed.Enabled = False
        ChkCoolerOn.Enabled = False
        TrcTargetTemp.Enabled = False

        ' 初期状態では「選択無し（-1）」かつ「Disable（False）」
        _isUpdatingUI = True
        CmbBinning.SelectedIndex = -1
        CmbRoi.SelectedIndex = -1
        CmbFlip.SelectedIndex = -1
        _isUpdatingUI = False

        CmbBinning.Enabled = False
        CmbRoi.Enabled = False
        CmbFlip.Enabled = False

        ' 💡【追記】起動時にトラックバーの限界値と値を綺麗に初期化＆グレーアウト
        ResetCameraTrackBars()

        ' ==========================================================
        ' 💡【追加】RTSPのURLにこのPCのIPアドレス（有線優先）を自動セット
        ' ==========================================================
        Dim myIP As String = GetLocalIPAddress()
        Dim currentUrl As String = TxtRtspUrl.Text.Trim()

        If Not String.IsNullOrEmpty(currentUrl) Then
            ' 💡 デザイナーや前回の設定でデフォルトURLが入っている場合、IPアドレス部分だけを綺麗に置換
            Try
                ' rtsp:// のままだとUriクラスが解析できないためのハック（一時的にhttpに置換してHostを抽出）
                Dim uri As New Uri(currentUrl.Replace("rtsp://", "http://"))
                Dim oldHost As String = uri.Host
                TxtRtspUrl.Text = currentUrl.Replace(oldHost, myIP)
            Catch
                ' 万が一解析に失敗した場合は安全なデフォルト構造で上書き
                TxtRtspUrl.Text = $"rtsp://{myIP}:8554/live"
            End Try
        Else
            ' 💡 テキストボックスが完全に空っぽだった場合の初期生成
            TxtRtspUrl.Text = $"rtsp://{myIP}:8554/live"
        End If

        ' ★重要：初期化がすべて完了したのでバリアを解除
        _isInitializing = False
    End Sub

    Private Sub ChkShowPreview_CheckedChanged(sender As Object, e As EventArgs) Handles ChkShowPreview.CheckedChanged
        If _isUpdatingUI Then Return

        If ChkShowPreview.Checked Then
            ' 💡 チェックされたら、その瞬間に新しくフォームを生成
            previewForm = New FrmPreview()

            ' 💡 プレビュー画面の「×ボタン」が押された時、勝手にクラッシュしないようイベントを監視
            AddHandler previewForm.FormClosing, AddressOf PreviewForm_FormClosing

            previewForm.Show()
        Else
            ' 💡 チェックが外れたら、メモリとリソースを100%完全に解体・抹消する
            If previewForm IsNot Nothing Then
                RemoveHandler previewForm.FormClosing, AddressOf PreviewForm_FormClosing
                previewForm.Dispose()
                previewForm = Nothing
            End If
        End If
    End Sub


    Private Sub FrmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' メイン画面が閉じられる時は、プレビュー画面も解放する
        If previewForm IsNot Nothing Then
            previewForm.Dispose()
        End If

        ' 安全のため、アプリ終了時に映像イベントハンドラを解除
        RemoveHandler ZWOCam.FrameArrived, AddressOf ZWOCam_FrameArrived

        ' ★ FrmMain_FormClosing の中に追記
        ' アプリ終了時にRTSPサーバーが残っていたら確実に息の根を止める安全弁
        If BasFunction._rtspServerProcess IsNot Nothing Then
            Try : BasFunction._rtspServerProcess.Kill() : Catch : End Try
        End If
    End Sub

    ' ==========================================================
    ' USB機器の抜き差しを自動検知するWindowsメッセージ処理
    ' ==========================================================
    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_DEVICECHANGE As Integer = &H219

        ' デバイスの構成に変更があった場合（USBを挿した、抜いた等）
        If m.Msg = WM_DEVICECHANGE Then
            If ZWOCam Is Nothing OrElse Not ZWOCam.IsRunning Then
                RefreshCameraList()
            End If
        End If

        ' 最後に必ず基底クラスのWndProcを呼び出して標準のメッセージ処理を継続させる
        MyBase.WndProc(m)
    End Sub


    ' --------------------------------------------------------
    ' カメラリストの更新処理
    ' --------------------------------------------------------
    Private Sub RefreshCameraList()
        CmbCameraList.Items.Clear()
        BtnConnect.Enabled = False

        ' 1. 接続されているZWOカメラの台数を取得
        Dim numCameras As Integer = ASI_SDK.ASIGetNumOfConnectedCameras()

        If numCameras = 0 Then
            CmbCameraList.Items.Add("カメラが接続されていません")
            CmbCameraList.SelectedIndex = 0
            Return
        End If

        ' 2. 見つかったカメラの情報をループで取得してコンボボックスに追加
        For i As Integer = 0 To numCameras - 1
            Dim camInfo As New ASI_SDK.ASI_CAMERA_INFO()
            Dim status As Integer = ASI_SDK.ASIGetCameraProperty(camInfo, i)

            If status = 0 Then ' 0 = 成功 (ASI_SUCCESS)
                CmbCameraList.Items.Add(camInfo.Name)
            End If
        Next

        ' 3. カメラが1台以上追加されていれば、先頭を選択して接続ボタンを有効化
        If CmbCameraList.Items.Count > 0 Then
            CmbCameraList.SelectedIndex = 0
            BtnConnect.Enabled = True
        End If
    End Sub

    Private Sub BtnRefresh_Click(sender As Object, e As EventArgs) Handles BtnRefresh.Click
        ' USBケーブルを挿し直した時などに手動でリストを再取得する
        RefreshCameraList()
    End Sub


    ' ==========================================================
    ' 接続 / 切断ボタン (BtnConnect) の処理
    ' ==========================================================
    Private Sub BtnConnect_Click(sender As Object, e As EventArgs) Handles BtnConnect.Click
        ' --- A. 切断処理（すでに繋がっている場合） ---
        If CamID <> -1 Then
            Try
                ' ★切断時は即座にタイマーを止める
                TmrUIUpdate.Enabled = False

                ' ==========================================================
                ' 💡【追加】切断時に配信・録画のチェックとUIを完全な初期状態に戻す
                ' ==========================================================
                _isUpdatingUI = True
                ChkEnableStream.Checked = False
                ChkRecord.Checked = False

                ' ステータスラベルの文字と色をリセット
                LblStreamStatus.Text = "未配信 (OFF)"
                LblStreamStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText)
                LblRecordStatus.Text = "録画: OFF"
                LblRecordStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText)

                ' 入力欄やボタンのロックを解除
                TxtRtspUrl.Enabled = True
                BtnSetSavePath.Enabled = True
                _isUpdatingUI = False
                ' ==========================================================

                ' ★ 安全に映像イベントの紐付けを解除
                RemoveHandler ZWOCam.FrameArrived, AddressOf ZWOCam_FrameArrived

                ' 💡【追加】
                RemoveHandler ZWOCam.DiskSpaceCritical, AddressOf ZWOCam_DiskSpaceCritical

                ' ZWOCamクラスに切断とメモリ解放をすべて任せる
                ZWOCam.Disconnect()
                CamID = -1

                ' UIを初期状態に戻す
                BtnConnect.Text = "接続"
                BtnConnect.BackColor = Color.FromKnownColor(KnownColor.Control)
                BtnRefresh.Enabled = True
                CmbCameraList.Enabled = True
                CmbBinning.Enabled = False
                CmbRoi.Enabled = False
                CmbFlip.Enabled = False

                CmbBinning.SelectedIndex = -1
                CmbRoi.SelectedIndex = -1
                CmbFlip.SelectedIndex = -1

                ' 💡【追記】切断時にすべてのトラックバーを瞬時にリセット＆グレーアウト
                ResetCameraTrackBars()

                ' 切断時にUSBと冷却設定を無効化
                TrcUsbBandwidth.Enabled = False
                ChkHighSpeed.Enabled = False
                ChkCoolerOn.Enabled = False
                TrcTargetTemp.Enabled = False

            Catch ex As Exception
                MessageBox.Show("切断処理中にエラーが発生しました: " & ex.Message)
            End Try
            Return
        End If

        ' --- B. 接続処理（まだ繋がっていない場合） ---
        Try
            ' 1. バリデーション：カメラが選択されているか
            Dim camIndex As Integer = CmbCameraList.SelectedIndex
            If camIndex = -1 Then
                MessageBox.Show("カメラを選択してください。")
                Return
            End If

            ' 接続する前に「Index」を使って情報を取得する
            Dim info As New ASI_SDK.ASI_CAMERA_INFO()
            If ASI_SDK.ASIGetCameraProperty(info, camIndex) <> 0 Then
                Throw New Exception("カメラ情報の取得に失敗しました。")
            End If

            ' SDKから返ってきた「本当の操作用ID」と「冷却の有無」を記憶
            Dim realCamID As Integer = info.CameraID
            HasCooler = (info.IsCoolerCam = 1)
            CamID = realCamID

            ' 2. ZWOCamクラスを通じて接続実行（本当のIDを渡す）
            If Not ZWOCam.Connect(CamID) Then
                Throw New Exception("カメラのオープンまたは初期化に失敗しました。")
            End If

            ' ★ 接続成功した瞬間に、映像配信イベントを動的に結合する（Handlesの代わり）
            AddHandler ZWOCam.FrameArrived, AddressOf ZWOCam_FrameArrived

            ' 💡【追加】
            AddHandler ZWOCam.DiskSpaceCritical, AddressOf ZWOCam_DiskSpaceCritical

            ' ★ 結合直後にカメラの撮影（キャプチャループ）をスタートさせる！
            ZWOCam.StartCamera(CamID)

            ' 3. ビニングリストの構築 (カメラの能力に合わせてCmbBinningを更新)
            CmbBinning.Items.Clear()
            Dim bins = ZWOCam.GetSupportedBins(CamID)
            For Each b In bins
                CmbBinning.Items.Add($"{b} (Bin{b}x{b})")
            Next

            ' 初期選択をセット
            _isUpdatingUI = True
            If CmbBinning.Items.Count > 0 Then CmbBinning.SelectedIndex = 0
            If CmbRoi.Items.Count > 0 Then CmbRoi.SelectedIndex = 0
            If CmbFlip.Items.Count > 0 Then CmbFlip.SelectedIndex = 0
            _isUpdatingUI = False

            ' カメラのスペックに合わせてトラックバーの範囲を自動設定
            BasFunction.InitializeCameraTrackBars()

            ' ==========================================================
            ' 4. 接続時の初期パラメータ送信 (露出 60ms, Gain Auto, WB Auto)
            ' ==========================================================
            _isUpdatingUI = True

            ' 画面のチェックボックスを希望の状態にセット
            Me.ChkExpAuto.Checked = False
            Me.ChkGainAuto.Checked = True
            Me.ChkWBAuto.Checked = True

            ' 💡【ここを追加】手動/自動の初期状態に合わせて、トラックバーのロック状態を明示的に決定する
            Me.TrkbExp.Enabled = True    ' 露出は手動スタートなので、触れるようにロック解除！
            Me.TrkbGain.Enabled = False  ' ゲインはAutoスタートなのでロック
            Me.TrkbWbR.Enabled = False   ' WBはAutoスタートなのでロック
            Me.TrkbWbB.Enabled = False

            ' トラックバーの露出を60msにセット
            If 33 >= Me.TrkbExp.Minimum AndAlso 60 <= Me.TrkbExp.Maximum Then
                Me.TrkbExp.Value = 60
            End If

            ' トラックバーのゲインを 0 にセット
            If 0 >= Me.TrkbGain.Minimum AndAlso 0 <= Me.TrkbGain.Maximum Then
                Me.TrkbGain.Value = 0
            End If

            ' トラックバーのホワイトバランス(R/B)を 50 にセット
            If 50 >= Me.TrkbWbR.Minimum AndAlso 50 <= Me.TrkbWbR.Maximum Then Me.TrkbWbR.Value = 50
            If 50 >= Me.TrkbWbB.Minimum AndAlso 50 <= Me.TrkbWbB.Maximum Then Me.TrkbWbB.Value = 50

            ' カメラ本体へ設定値を直接送信 (露出: 60ms = 60000μs, オートOFF=0)
            Dim ret = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_EXPOSURE, 60000, 0)
            ret = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_GAIN, 0, 1)
            ret = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_WB_R, 50, 1)
            ret = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_WB_B, 50, 1)

            ' 💡【ここを修正】ガンマだけは50を強制送信せず、カメラの現在の値を安全に読み出してUIに反映する
            Dim gammaVal As Integer = 0
            Dim gammaAuto As Integer = 0
            If ASI_SDK.ASIGetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_GAMMA, gammaVal, gammaAuto) = 0 Then
                BasFunction._gammaVal = gammaVal
                Me.TrkbGamma.Enabled = True

                ' SDKがInitializeCameraTrackBarsで自動設定したミニマム・マックスの範囲内か安全チェックしてセット
                If gammaVal >= Me.TrkbGamma.Minimum AndAlso gammaVal <= Me.TrkbGamma.Maximum Then
                    Me.TrkbGamma.Value = gammaVal
                End If
                Me.LblGamma.Text = $"Gamma: {gammaVal}"
            End If

            _isUpdatingUI = False

            ' ==========================================================
            ' 💡【設定漏れ修正】5. USB速度と帯域の反映（カメラの推奨安全値をそのまま送信）
            ' ==========================================================
            _isUpdatingUI = True
            Me.ChkHighSpeed.Checked = False

            ' 💡【修正】100への強制上書きを完全削除！
            ' InitializeCameraTrackBarsでセットされたカメラ固有の推奨値をそのまま活かします。

            ' ラベルの表示も、初期化で読み込まれたTag（メーカー推奨値）を使ってきれいに組み立てます
            Dim defBandwidth As Integer = CInt(If(Me.LblUsbBandwidth.Tag, Me.TrcUsbBandwidth.Value))
            Me.LblUsbBandwidth.Text = $"USB帯域 (Def: {defBandwidth}%): {Me.TrcUsbBandwidth.Value}%"
            _isUpdatingUI = False

            ' カメラ本体へ安全な初期パラメータを送信
            Dim hsVal As Integer = 0 ' HighSpeedMode = OFF
            Dim ret_hs = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_HIGH_SPEED_MODE, hsVal, 0)

            ' これにより、カメラが本来要求する最も安定した帯域制限（40%や80%など）が100%確実に適用されます
            Dim ret_bw = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_BANDWIDTHOVERLOAD, Me.TrcUsbBandwidth.Value, 0)

            ' ==========================================================
            ' 💡 6. 冷却機能の初期化（冷却カメラのみ）
            ' ==========================================================
            If HasCooler Then
                ' 画面のUIを強制的にOFFと0度にセット
                _isUpdatingUI = True
                Me.ChkCoolerOn.Checked = False
                Me.TrcTargetTemp.Value = 0
                Me.LblTargetTemp.Text = "目標温度: 0℃"
                _isUpdatingUI = False

                ' カメラ本体へ安全な初期値（Cooler=OFF, TargetTemp=0℃）を直接送信
                ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_COOLER_ON, 0, 0)
                ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_TARGET_TEMP, 0, 0)
            End If

            ' 7. UIスライダーやラベルを現在のカメラ状態に同期
            BasFunction.UpdateUIFromCamera()


            ' 8. UI表示の最終更新
            BtnConnect.Text = "切断"
            BtnConnect.BackColor = Color.MistyRose
            BtnRefresh.Enabled = False
            CmbCameraList.Enabled = False
            CmbBinning.Enabled = True
            CmbRoi.Enabled = True
            CmbFlip.Enabled = True

            TrcUsbBandwidth.Enabled = True
            ChkHighSpeed.Enabled = True

            If HasCooler Then
                ChkCoolerOn.Enabled = True
                TrcTargetTemp.Enabled = True
            Else
                ChkCoolerOn.Enabled = False
                TrcTargetTemp.Enabled = False
            End If

            ' ★タイマーを起動してUI監視をスタート
            TmrUIUpdate.Enabled = True

        Catch ex As Exception
            ZWOCam.Disconnect()
            CamID = -1
            MessageBox.Show("接続に失敗しました: " & ex.Message)
        End Try
    End Sub


    ' ==========================================================
    ' 各種スライダーやチェックボックスの連動処理
    ' ==========================================================

    ' --- ゲイン変更時 ---
    Private Sub TrkbGain_Scroll(sender As Object, e As EventArgs) Handles TrkbGain.Scroll
        If _isInitializing OrElse _isUpdatingUI Then Return
        If CamID <> -1 Then
            Dim bAuto As Integer = If(ChkGainAuto.Checked, 1, 0)
            ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_GAIN, TrkbGain.Value, bAuto)
            LblGain.Text = $"Gain: {TrkbGain.Value} {(If(bAuto = 1, "(Auto)", ""))}"
        End If
    End Sub

    Private Sub ChkGainAuto_CheckedChanged(sender As Object, e As EventArgs) Handles ChkGainAuto.CheckedChanged
        If _isInitializing OrElse _isUpdatingUI Then Return
        If CamID <> -1 Then
            Dim bAuto As Integer = If(ChkGainAuto.Checked, 1, 0)
            ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_GAIN, TrkbGain.Value, bAuto)
            TrkbGain.Enabled = (bAuto = 0)
        End If
    End Sub

    ' --- 露出変更時 ---
    Private Sub TrkbExp_Scroll(sender As Object, e As EventArgs) Handles TrkbExp.Scroll
        If _isInitializing OrElse _isUpdatingUI Then Return
        If CamID <> -1 Then
            Dim bAuto As Integer = If(ChkExpAuto.Checked, 1, 0)
            ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_EXPOSURE, TrkbExp.Value * 1000, bAuto)
            LblExp.Text = $"Exp: {TrkbExp.Value} ms {(If(bAuto = 1, "(Auto)", ""))}"
        End If
    End Sub

    Private Sub ChkExpAuto_CheckedChanged(sender As Object, e As EventArgs) Handles ChkExpAuto.CheckedChanged
        If _isInitializing OrElse _isUpdatingUI Then Return
        If CamID <> -1 Then
            Dim bAuto As Integer = If(ChkExpAuto.Checked, 1, 0)
            ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_EXPOSURE, TrkbExp.Value * 1000, bAuto)
            TrkbExp.Enabled = (bAuto = 0)
        End If
    End Sub

    ' --- ホワイトバランス変更時 ---
    Private Sub TrkbWbR_Scroll(sender As Object, e As EventArgs) Handles TrkbWbR.Scroll
        If _isInitializing OrElse _isUpdatingUI Then Return
        If CamID <> -1 Then
            Dim bAuto As Integer = If(ChkWBAuto.Checked, 1, 0)
            ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_WB_R, TrkbWbR.Value, bAuto)
            LblWb.Text = $"WB (R/B): {TrkbWbR.Value} / {TrkbWbB.Value} {(If(bAuto = 1, "(Auto)", ""))}"
        End If
    End Sub

    Private Sub TrkbWbB_Scroll(sender As Object, e As EventArgs) Handles TrkbWbB.Scroll
        If _isInitializing OrElse _isUpdatingUI Then Return
        If CamID <> -1 Then
            Dim bAuto As Integer = If(ChkWBAuto.Checked, 1, 0)
            ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_WB_B, TrkbWbB.Value, bAuto)
            LblWb.Text = $"WB (R/B): {TrkbWbR.Value} / {TrkbWbB.Value} {(If(bAuto = 1, "(Auto)", ""))}"
        End If
    End Sub

    Private Sub ChkWBAuto_CheckedChanged(sender As Object, e As EventArgs) Handles ChkWBAuto.CheckedChanged
        If _isInitializing OrElse _isUpdatingUI Then Return
        If CamID <> -1 Then
            Dim bAuto As Integer = If(ChkWBAuto.Checked, 1, 0)
            ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_WB_R, TrkbWbR.Value, bAuto)
            ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_WB_B, TrkbWbB.Value, bAuto)
            TrkbWbR.Enabled = (bAuto = 0)
            TrkbWbB.Enabled = (bAuto = 0)
        End If
    End Sub

    '' --- ビニング変更時 ---
    'Private Sub CmbBinning_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CmbBinning.SelectedIndexChanged
    '    If _isInitializing OrElse _isUpdatingUI Then Return
    '    If CamID <> -1 AndAlso CmbBinning.SelectedIndex <> -1 Then
    '        Dim bins = ZWOCam.GetSupportedBins(CamID)
    '        If CmbBinning.SelectedIndex < bins.Count Then
    '            Dim selectedBin As Integer = bins(CmbBinning.SelectedIndex)
    '            ZWOCam.SetBinning(selectedBin)
    '        End If
    '    End If
    'End Sub

    '' --- ★修正：ROI（パーセンテージ固定値）変更時 ---
    'Private Sub CmbRoi_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CmbRoi.SelectedIndexChanged
    '    If _isInitializing OrElse _isUpdatingUI Then Return
    '    If CamID <> -1 AndAlso CmbRoi.SelectedIndex <> -1 Then
    '        Dim info As New ASI_SDK.ASI_CAMERA_INFO()
    '        Dim camIndex As Integer = CmbCameraList.SelectedIndex

    '        If camIndex <> -1 AndAlso ASI_SDK.ASIGetCameraProperty(info, camIndex) = 0 Then
    '            ' 💡 カメラの「現在のビニング状態」を取得
    '            Dim currentBin As Integer = BasFunction.ZWOCam.CurrentBin
    '            If currentBin <= 0 Then currentBin = 1

    '            ' 💡 ビニング適用後の「画面全体の最大解像度」を算出
    '            Dim maxW As Integer = info.MaxWidth \ currentBin
    '            Dim maxH As Integer = info.MaxHeight \ currentBin

    '            Dim ratio As Double = 1.0
    '            Dim selectedText As String = CmbRoi.SelectedItem.ToString()

    '            If selectedText.Contains("100") Then
    '                ratio = 1.0
    '            ElseIf selectedText.Contains("95") Then
    '                ratio = 0.95
    '            ElseIf selectedText.Contains("90") Then
    '                ratio = 0.9
    '            ElseIf selectedText.Contains("85") Then
    '                ratio = 0.85
    '            ElseIf selectedText.Contains("80") Then
    '                ratio = 0.8
    '            Else
    '                Select Case CmbRoi.SelectedIndex
    '                    Case 0 : ratio = 1.0
    '                    Case 1 : ratio = 0.95
    '                    Case 2 : ratio = 0.9
    '                    Case 3 : ratio = 0.85
    '                    Case 4 : ratio = 0.8
    '                End Select
    '            End If

    '            Dim targetW As Integer = CInt(maxW * ratio)
    '            Dim targetH As Integer = CInt(maxH * ratio)

    '            targetW = (targetW \ 8) * 8
    '            targetH = (targetH \ 2) * 2

    '            ' 💡 カメラに真ん中からの切り出しROIを指示。ビニングコンボボックスのリセットは行いません。
    '            ZWOCam.SetROI(targetW, targetH)
    '        End If
    '    End If
    'End Sub

    ' ==========================================================
    ' ★ 追加：BINとROIの変更を統合して処理するメソッド
    ' ==========================================================
    Private Sub UpdateCameraFormat()
        If CamID = -1 Then Return

        Dim info As New ASI_SDK.ASI_CAMERA_INFO()
        Dim camIndex As Integer = CmbCameraList.SelectedIndex
        If camIndex = -1 OrElse ASI_SDK.ASIGetCameraProperty(info, camIndex) <> 0 Then Return

        ' 1. 画面で現在選択されているBINの数値を取得
        Dim currentBin As Integer = 1
        If CmbBinning.SelectedIndex <> -1 Then
            Dim bins = BasFunction.ZWOCam.GetSupportedBins(CamID)
            If CmbBinning.SelectedIndex < bins.Count Then
                currentBin = bins(CmbBinning.SelectedIndex)
            End If
        End If

        ' 2. 画面で現在選択されているROIの割合(％)を取得
        Dim ratio As Double = 1.0
        If CmbRoi.SelectedIndex <> -1 Then
            Dim selectedText As String = CmbRoi.SelectedItem.ToString()
            If selectedText.Contains("100") Then
                ratio = 1.0
            ElseIf selectedText.Contains("95") Then
                ratio = 0.95
            ElseIf selectedText.Contains("90") Then
                ratio = 0.9
            ElseIf selectedText.Contains("85") Then
                ratio = 0.85
            ElseIf selectedText.Contains("80") Then
                ratio = 0.8
            End If
        End If

        ' 3. 新しいBIN適用後の最大解像度を割り出し、そこにROIの％を掛ける
        Dim maxW As Integer = info.MaxWidth \ currentBin
        Dim maxH As Integer = info.MaxHeight \ currentBin

        Dim targetW As Integer = CInt(maxW * ratio)
        Dim targetH As Integer = CInt(maxH * ratio)

        ' ZWOのルール（幅は8の倍数、高さは2の倍数）にアジャスト
        targetW = (targetW \ 8) * 8
        targetH = (targetH \ 2) * 2

        ' 4. カメラ本体に「新しいBIN」と「新しいROI」を同時に適用する
        BasFunction.ZWOCam.ApplyFormat(currentBin, targetW, targetH)
    End Sub

    ' --- ビニング変更時 ---
    Private Sub CmbBinning_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CmbBinning.SelectedIndexChanged
        If _isInitializing OrElse _isUpdatingUI Then Return
        ' 💡 ビニングを変えたときも、画面のROI(％)はリセットせずに現在の組み合わせで計算させる
        UpdateCameraFormat()
    End Sub

    ' --- ROI変更時 ---
    Private Sub CmbRoi_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CmbRoi.SelectedIndexChanged
        If _isInitializing OrElse _isUpdatingUI Then Return
        ' 💡 ROIを変えたときも、現在のビニング状態を維持した組み合わせで計算させる
        UpdateCameraFormat()
    End Sub

    ' --- 画像反転変更時 ---
    Private Sub CmbFlip_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CmbFlip.SelectedIndexChanged
        If _isInitializing OrElse _isUpdatingUI Then Return
        If CamID <> -1 Then
            ZWOCam.FlipModeSetting = CmbFlip.SelectedIndex
        End If
    End Sub

    ' ==========================================================
    ' ★【追加】画像調整コントロールの連動処理
    ' ==========================================================

    ' --- ガンマ変更時 (SDK側ハードウェア処理) ---
    Private Sub TrkbGamma_Scroll(sender As Object, e As EventArgs) Handles TrkbGamma.Scroll
        If _isInitializing OrElse _isUpdatingUI Then Return
        If CamID <> -1 Then
            ' カメラ本体へ設定を送信
            ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_GAMMA, TrkbGamma.Value, 0)
            BasFunction._gammaVal = TrkbGamma.Value
            LblGamma.Text = $"Gamma: {TrkbGamma.Value}"
        End If
    End Sub

    ' ==========================================================
    ' タイマーイベント（カメラの最新値をUIに定期反映）
    ' ==========================================================
    Private Sub TmrUIUpdate_Tick(sender As Object, e As EventArgs) Handles TmrUIUpdate.Tick
        BasFunction.UpdateUIFromCamera()
    End Sub

    ' ==========================================================
    ' 既存の周辺デバイス制御
    ' ==========================================================
    Private Sub TrcUsbBandwidth_Scroll(sender As Object, e As EventArgs) Handles TrcUsbBandwidth.Scroll
        If _isInitializing Then Return

        LblUsbBandwidth.Text = $"USB帯域 (Def: {LblUsbBandwidth.Tag}%): {TrcUsbBandwidth.Value}%"

        If CamID <> -1 Then
            Dim ret = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_BANDWIDTHOVERLOAD, TrcUsbBandwidth.Value, 0)
        End If
    End Sub

    Private Sub ChkHighSpeed_CheckedChanged(sender As Object, e As EventArgs) Handles ChkHighSpeed.CheckedChanged
        If _isInitializing Then Return

        If CamID <> -1 Then
            Dim val As Integer = If(ChkHighSpeed.Checked, 1, 0)
            Dim ret = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_HIGH_SPEED_MODE, val, 0)
        End If
    End Sub

    Private Sub ChkCoolerOn_CheckedChanged(sender As Object, e As EventArgs) Handles ChkCoolerOn.CheckedChanged
        ' 💡 _isUpdatingUI のバリアを追加！（プログラムからの変更時は無視する）
        If _isInitializing OrElse _isUpdatingUI Then Return

        If CamID <> -1 Then
            Dim isOn As Integer = If(ChkCoolerOn.Checked, 1, 0)
            Dim ret = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_COOLER_ON, isOn, 0)

            If isOn = 1 Then
                ret = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_TARGET_TEMP, TrcTargetTemp.Value, 0)
            End If
        End If
    End Sub

    Private Sub TrcTargetTemp_Scroll(sender As Object, e As EventArgs) Handles TrcTargetTemp.Scroll
        ' 💡 _isUpdatingUI のバリアを追加！
        If _isInitializing OrElse _isUpdatingUI Then Return

        LblTargetTemp.Text = $"目標温度: {TrcTargetTemp.Value}℃"

        If CamID <> -1 Then
            Dim ret = ASI_SDK.ASISetControlValue(CamID, ASI_SDK.ASI_CONTROL_TYPE.ASI_TARGET_TEMP, TrcTargetTemp.Value, 0)
        End If
    End Sub


    ' ==========================================================
    ' カメラから映像フレーム（Mat）が届いた時の処理（毎コマ必ず呼ばれる）
    ' ==========================================================
    Private Sub ZWOCam_FrameArrived(ByVal frame As Mat)
        If frame Is Nothing OrElse frame.Empty() Then Return

        ' 💡 【重要1】あとでアクセス違反が起きないよう、今のうちに数値だけを取り出しておく！
        ' （こうすればUIスレッドにframe本体のメモリを見に行かせる必要がなくなります）
        Dim w As Integer = frame.Width
        Dim h As Integer = frame.Height

        ' 💡【常時表示化】プレビューのON/OFFに関係なく、メイン画面のステータスバーを常に更新！
        If Me.InvokeRequired Then
            ' frame本体ではなく、ただの数値(wとh)を渡すので100%安全です
            Me.BeginInvoke(New Action(Sub() UpdateStatusFields(w, h)))
        Else
            UpdateStatusFields(w, h)
        End If

        ' 1. 【チェック時のみ実行】プレビューフォームへの描画
        If ChkShowPreview.Checked AndAlso previewForm IsNot Nothing Then
            If previewForm.PicPreview.InvokeRequired Then
                previewForm.PicPreview.BeginInvoke(New Action(Sub() UpdatePreview(frame)))
            Else
                UpdatePreview(frame)
            End If
        Else
            ' 💡 【重要2】プレビューを表示しない場合、ここで絶対にframeを破棄する！
            ' （これがないと「プレビューOFF」の時にクローンされたメモリが無限に増殖します）
            frame.Dispose()
        End If
    End Sub

    ' --------------------------------------------------------
    ' 💡【新設】プレビューの有無に関わらず、メイン画面のステータスバーを更新する処理
    ' --------------------------------------------------------
    Private Sub UpdateStatusFields(ByVal width As Integer, ByVal height As Integer)
        Try
            ' 💡 画面側での独自計算をやめ、カメラクラスから直接「超正確な実測FPS」をもらってくる
            Dim realFps As Double = BasFunction.ZWOCam.CurrentFps

            ' メイン画面のデザイン画面にある実際のステータスバーラベルへ反映
            LblStatusResolution.Text = $"解像度: {width} x {height}"
            LblStatusFps.Text = $"FPS: {realFps:F1}"
        Catch ex As Exception
            ' 安全スルー
        End Try
    End Sub

    ' --------------------------------------------------------
    ' プレビュー画面のPictureBoxを更新する安全な処理（スリム化完了！）
    ' --------------------------------------------------------
    Private Sub UpdatePreview(ByVal frame As Mat)
        Try
            ' 💡 計算処理を上の UpdateStatusFields へ引っ越したため、
            '    ここは純粋な「描画とお片付け（Dispose）」だけに特化させ、最軽量化しました！
            Dim oldImage = previewForm.PicPreview.Image

            previewForm.PicPreview.Image = BasFunction.MatToBitmap(frame)

            If oldImage IsNot Nothing Then
                oldImage.Dispose()
            End If
        Catch ex As Exception
            ' 軽微な描画例外は安全にスルー
        Finally
            ' 💡 変換が終わったら、受け取った分身のMatを確実に破棄する！
            If frame IsNot Nothing AndAlso Not frame.IsDisposed Then
                frame.Dispose()
            End If
        End Try

    End Sub

    ' 💡 プレビュー画面がユーザーの手で閉じられた時の安全な解体処理
    Private Sub PreviewForm_FormClosing(sender As Object, e As FormClosingEventArgs)
        ' メイン画面のチェックボックスをOFFにする（これにより上のCheckedChangedが走り、安全にDisposeされます）
        _isUpdatingUI = True
        ChkShowPreview.Checked = False
        _isUpdatingUI = False

        ' 自身を解放
        Dim frm = CType(sender, FrmPreview)
        RemoveHandler frm.FormClosing, AddressOf PreviewForm_FormClosing

        ' FormClosingイベント内なので、Disposeは.NETシステムに任せてNothingクリアだけ行う
        previewForm = Nothing
    End Sub


    ''' <summary>
    ''' カメラ用トラックバーとラベルをすべて未接続状態（グレーアウト・初期値）にリセットします
    ''' </summary>
    Private Sub ResetCameraTrackBars()
        _isUpdatingUI = True

        ' --- ゲイン ---
        TrkbGain.Minimum = 0
        TrkbGain.Maximum = 100
        TrkbGain.Value = 0
        TrkbGain.Enabled = False
        LblGain.Text = "Gain: --"

        ' --- 露出 ---
        TrkbExp.Minimum = 0
        TrkbExp.Maximum = 100
        TrkbExp.Value = 0
        TrkbExp.Enabled = False
        LblExp.Text = "Exp: --"

        ' --- ホワイトバランス (R/B) ---
        TrkbWbR.Minimum = 0
        TrkbWbR.Maximum = 100
        TrkbWbR.Value = 50
        TrkbWbR.Enabled = False

        TrkbWbB.Minimum = 0
        TrkbWbB.Maximum = 100
        TrkbWbB.Value = 50
        TrkbWbB.Enabled = False
        LblWb.Text = "WB (R/B): -- / --"

        ' --- ガンマ ---
        TrkbGamma.Minimum = 0
        TrkbGamma.Maximum = 100
        TrkbGamma.Value = 50
        TrkbGamma.Enabled = False
        LblGamma.Text = "Gamma: --"

        _isUpdatingUI = False
    End Sub

    Private Sub BtnSetSavePath_Click(sender As Object, e As EventArgs) Handles BtnSetSavePath.Click
        ' 新しく作った設定フォームを立ち上げる
        Using frm As New FrmSaveSettings()
            ' 💡 設定画面で「保存(OK)」が押されて戻ってきたら、新しい設定を即座に反映する
            If frm.ShowDialog(Me) = DialogResult.OK Then
                ApplyOutputSettings()
            End If
        End Using
    End Sub

    ' ==========================================================
    ' ★【排他制御アジャスト】RTSP配信チェック変更時
    ' ==========================================================
    Private Sub ChkEnableStream_CheckedChanged(sender As Object, e As EventArgs) Handles ChkEnableStream.CheckedChanged
        If _isUpdatingUI Then Return

        ' 💡【排他制御】配信がONになったら、録画を強制的にOFFにする
        If ChkEnableStream.Checked Then
            _isUpdatingUI = True
            ChkRecord.Checked = False
            _isUpdatingUI = False

            ' 手動操作時は録画側のエラー色などもリセットしておく
            LblRecordStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText)
        End If

        ' 手動操作時はエラー色をリセット
        If Not _isUpdatingUI Then LblStreamStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText)
        ApplyOutputSettings()
    End Sub


    ' ==========================================================
    ' ★【排他制御アジャスト】ローカル録画チェック変更時
    ' ==========================================================
    Private Sub ChkRecord_CheckedChanged(sender As Object, e As EventArgs) Handles ChkRecord.CheckedChanged
        If _isUpdatingUI Then Return

        ' 💡【排他制御】録画がONになったら、配信を強制的にOFFにする
        If ChkRecord.Checked Then
            _isUpdatingUI = True
            ChkEnableStream.Checked = False
            _isUpdatingUI = False

            ' 手動操作時は配信側のエラー色などもリセットしておく
            LblStreamStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText)
        End If

        ' 手動操作時はエラー色をリセット
        If Not _isUpdatingUI Then LblRecordStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText)
        ApplyOutputSettings()
    End Sub

    ''' <summary>
    ''' 配信と録画のON/OFF状態を組み合わせ、最適な設定をカメラへ送る共通メソッド
    ''' </summary>
    Private Sub ApplyOutputSettings()
        If _isInitializing OrElse _isUpdatingUI Then Return

        ' 1. カメラ未接続時の安全ガード
        If CamID = -1 Then
            _isUpdatingUI = True
            ChkEnableStream.Checked = False
            ChkRecord.Checked = False
            _isUpdatingUI = False
            Return
        End If

        Dim isStream As Boolean = ChkEnableStream.Checked
        Dim isRecord As Boolean = ChkRecord.Checked
        Dim url As String = TxtRtspUrl.Text.Trim()

        ' 2. 記憶されている保存先パスを引っ張り出す
        Dim currentSavePath As String = My.Settings.RecordSavePath
        If String.IsNullOrEmpty(currentSavePath) Then
            Dim videoPath As String = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
            currentSavePath = System.IO.Path.Combine(videoPath, "ASI_Record")
        End If

        ' 💡【修正】設定画面から保存された日数を引っ張り出す（1〜7の範囲外なら7日に矯正）
        Dim retentionDays As Integer = My.Settings.RetentionDays
        If retentionDays < 1 OrElse retentionDays > 7 Then
            retentionDays = 7
        End If

        ' 3. 💡【ダイアログ排除】録画開始前の空き容量チェック
        If isRecord Then
            Try
                Dim root As String = System.IO.Path.GetPathRoot(currentSavePath)
                Dim dInfo As New System.IO.DriveInfo(root)
                If dInfo.IsReady Then
                    Dim freeGB As Double = dInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0)

                    ' 💡【修正】5.0 から 10.0 に変更
                    If freeGB < 10.0 Then
                        ' 録画フラグを強制的にOFFに戻す
                        _isUpdatingUI = True
                        ChkRecord.Checked = False
                        _isUpdatingUI = False
                        isRecord = False

                        ' ステータスバーだけを赤くして警告
                        LblRecordStatus.Text = $"録画不可 (空き {freeGB:F1}GB)"
                        LblRecordStatus.ForeColor = Color.Red
                    End If
                End If
            Catch ex As Exception
            End Try
        End If

        ' 4. バリデーション：配信ONの時だけURLを必須とする
        If isStream AndAlso String.IsNullOrEmpty(url) Then
            _isUpdatingUI = True : ChkEnableStream.Checked = False : _isUpdatingUI = False
            isStream = False
            LblStreamStatus.Text = "URL未入力エラー"
            LblStreamStatus.ForeColor = Color.Red
        End If

        ' 5. UIロックの最適化
        Dim isAnyActive As Boolean = isStream OrElse isRecord
        CmbBinning.Enabled = Not isAnyActive
        CmbFlip.Enabled = Not isAnyActive
        CmbRoi.Enabled = Not isAnyActive

        TxtRtspUrl.Enabled = Not isStream
        BtnSetSavePath.Enabled = Not isRecord

        ' 6. 💡【独立表示】ステータスラベルの更新
        ' --- 配信ステータス ---
        If isStream Then
            LblStreamStatus.Text = "配信中 (ON)"
            LblStreamStatus.ForeColor = Color.Red
        Else
            ' エラー赤文字が出ている場合を除き、通常OFFに戻す
            If LblStreamStatus.ForeColor <> Color.Red Then
                LblStreamStatus.Text = "未配信 (OFF)"
                LblStreamStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText)
            End If
        End If

        ' --- 録画ステータス ---
        If isRecord Then
            LblRecordStatus.Text = "録画中 (ON)"
            LblRecordStatus.ForeColor = Color.DarkGreen ' 録画は緑色で安心感を
        Else
            ' 容量不足で赤文字の警告が出ている場合は上書きせずに残す
            If LblRecordStatus.ForeColor <> Color.Red Then
                LblRecordStatus.Text = "録画: OFF"
                LblRecordStatus.ForeColor = Color.FromKnownColor(KnownColor.ControlText)
            End If
        End If

        ' 7. RTSPサーバー(mediamtx.exe)の自動起動・終了管理
        If isStream Then
            Try
                Dim existingServers = System.Diagnostics.Process.GetProcessesByName("mediamtx")
                If existingServers.Length > 0 Then
                    BasFunction._rtspServerProcess = existingServers(0)
                Else
                    Dim serverPath As String = System.IO.Path.Combine(Application.StartupPath, "mediamtx.exe")
                    If System.IO.File.Exists(serverPath) Then
                        ' 💡【修正】ここに System.Diagnostics. を付け足します
                        Dim startInfo As New System.Diagnostics.ProcessStartInfo()
                        startInfo.FileName = serverPath
                        startInfo.CreateNoWindow = True
                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                        startInfo.UseShellExecute = False
                        BasFunction._rtspServerProcess = System.Diagnostics.Process.Start(startInfo)

                        System.Threading.Thread.Sleep(500)
                    End If
                End If
            Catch ex As Exception
            End Try
        Else
            Try
                If BasFunction._rtspServerProcess IsNot Nothing AndAlso Not BasFunction._rtspServerProcess.HasExited Then
                    BasFunction._rtspServerProcess.Kill()
                    BasFunction._rtspServerProcess.Dispose()
                End If
            Catch ex As Exception
            Finally
                BasFunction._rtspServerProcess = Nothing
            End Try
        End If

        ' 8. カメラクラスへ設定を流し込む
        ' 💡【修正】読み出した「retentionDays（保存日数）」と「splitMinutes（分割分数）」を一緒に送る
        Dim splitMin As Integer = My.Settings.RecordSplitTime
        BasFunction.ZWOCam.UpdateOutputState(isStream, url, isRecord, currentSavePath, retentionDays, splitMin)
    End Sub

    ' ==========================================================
    ' 💡【改修】録画中に容量が枯渇した時の緊急停止処理（ダイアログ排除版）
    ' ==========================================================
    Private Sub ZWOCam_DiskSpaceCritical()
        ' 別スレッドから呼ばれるため、UIスレッドへ安全に引き継ぐ
        If Me.InvokeRequired Then
            Me.BeginInvoke(New Action(AddressOf ZWOCam_DiskSpaceCritical))
            Return
        End If

        ' まだ録画ONになっていたら、強制的にチェックを外して止める
        If ChkRecord.Checked Then
            _isUpdatingUI = True
            ChkRecord.Checked = False ' チェックを外す
            _isUpdatingUI = False

            ' 💡 ダイアログを出さず、ステータスバーに「緊急停止」を赤文字で刻む
            LblRecordStatus.Text = "緊急停止 (容量枯渇)"
            LblRecordStatus.ForeColor = Color.Red

            ' UIと裏方の状態を同期させる（FFmpegが瞬時に配信のみに切り替わる）
            ApplyOutputSettings()
        End If
    End Sub

    ' ==========================================================
    ' ★ 詳細設定画面を開く処理
    ' ==========================================================
    Private Sub BtnAdvancedSettings_Click(sender As Object, e As EventArgs) Handles BtnAdvancedSettings.Click
        Using frm As New FrmAdvancedSettings()
            ' 💡 詳細設定ダイアログを表示し、「保存(OK)」が押されて戻ってきたら新しい設定を即座に反映する
            If frm.ShowDialog(Me) = DialogResult.OK Then
                ApplyOutputSettings()
            End If
        End Using
    End Sub
End Class