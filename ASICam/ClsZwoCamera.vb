Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Threading.Tasks
Imports OpenCvSharp

Public Class ClsZwoCamera
    Implements ICameraProvider

    Public Event FrameArrived(ByVal frame As Mat) Implements ICameraProvider.FrameArrived

    ' 💡【追加】容量不足の緊急停止を通知するイベント
    Public Event DiskSpaceCritical()

    ' --- クラス内部の変数 ---
    Private _cameraId As Integer = -1
    Private _camWidth As Integer
    Private _camHeight As Integer
    Private _bufferSize As Integer
    Private _bufferPtr As IntPtr = IntPtr.Zero
    Private _isCapturing As Boolean = False
    Private _captureTask As Task = Nothing

    Private _currentFlipMode As Integer = 0
    Private _isColorCam As Boolean = False
    Private _bayerCode As ColorConversionCodes = ColorConversionCodes.BayerBG2BGR

    ' ★現在のビニング値を記憶する変数
    Private _currentBin As Integer = 1

    ' ★出力ループ制作用変数（配信・録画共通）
    Private _isOutputRunning As Boolean = False
    Private _outputTask As Task = Nothing
    Private _streamQueue As New System.Collections.Concurrent.ConcurrentQueue(Of Mat)()

    ' 💡 配信・録画の独立状態を記憶する変数
    Private _isStreamEnabled As Boolean = False
    Private _streamUrl As String = ""
    Private _isRecordEnabled As Boolean = False
    Private _recordSavePath As String = ""
    Private _retentionDays As Integer = 7 ' 💡【追加】保存日数を記憶

    ' ★カメラの実際のFPSをリアルタイムに追跡する変数
    Private _currentFps As Double = 30.0

    ' --- プロパティ ---
    Public ReadOnly Property Width As Integer
        Get
            Return _camWidth
        End Get
    End Property

    Public ReadOnly Property Height As Integer
        Get
            Return _camHeight
        End Get
    End Property

    Public ReadOnly Property IsRunning As Boolean
        Get
            Return _isCapturing
        End Get
    End Property

    Public ReadOnly Property IsColor As Boolean
        Get
            Return _isColorCam
        End Get
    End Property

    Public ReadOnly Property CurrentBin As Integer
        Get
            Return _currentBin
        End Get
    End Property

    Private ReadOnly Property ICameraProvider_IsRunning As Boolean Implements ICameraProvider.IsRunning
        Get
            Return IsRunning
        End Get
    End Property

    Public Property FlipModeSetting As Integer
        Get
            Return _currentFlipMode
        End Get
        Set(value As Integer)
            _currentFlipMode = value
        End Set
    End Property

    Public ReadOnly Property CurrentFps As Double
        Get
            Return _currentFps
        End Get
    End Property


    ' ==========================================================
    ' 接続と初期化
    ' ==========================================================
    Public Function Connect(cameraId As Integer) As Boolean
        If ASI_SDK.ASIOpenCamera(cameraId) <> 0 Then Return False
        If ASI_SDK.ASIInitCamera(cameraId) <> 0 Then Return False

        _cameraId = cameraId

        Dim info As New ASI_SDK.ASI_CAMERA_INFO()
        ASI_SDK.ASIGetCameraProperty(info, _cameraId)

        _camWidth = info.MaxWidth
        _camHeight = info.MaxHeight
        _isColorCam = (info.IsColorCam = 1)
        _currentBin = 1

        If _isColorCam Then
            Select Case info.BayerPattern
                Case 0 : _bayerCode = ColorConversionCodes.BayerBG2BGR
                Case 1 : _bayerCode = ColorConversionCodes.BayerRG2BGR
                Case 2 : _bayerCode = ColorConversionCodes.BayerGB2BGR
                Case 3 : _bayerCode = ColorConversionCodes.BayerGR2BGR
            End Select
        End If

        If ASI_SDK.ASISetROIFormat(_cameraId, _camWidth, _camHeight, _currentBin, ASI_SDK.ASI_IMG_TYPE.ASI_IMG_RAW8) <> 0 Then
            Return False
        End If

        AllocateBuffer(_camWidth, _camHeight)

        Return True
    End Function

    Public Function SetBinning(bin As Integer) As Boolean
        If _cameraId = -1 Then Return False
        Dim wasCapturing As Boolean = _isCapturing
        If wasCapturing Then StopCamera()

        Dim info As New ASI_SDK.ASI_CAMERA_INFO()
        ASI_SDK.ASIGetCameraProperty(info, _cameraId)
        Dim newWidth As Integer = info.MaxWidth \ bin
        Dim newHeight As Integer = info.MaxHeight \ bin

        ASI_SDK.ASISetStartPos(_cameraId, 0, 0)
        Dim res = ASI_SDK.ASISetROIFormat(_cameraId, newWidth, newHeight, bin, ASI_SDK.ASI_IMG_TYPE.ASI_IMG_RAW8)

        If res = 0 Then
            _currentBin = bin
            _camWidth = newWidth
            _camHeight = newHeight
            AllocateBuffer(_camWidth, _camHeight)
            If wasCapturing Then StartCamera(_cameraId)
            Return True
        End If

        If wasCapturing AndAlso Not _isCapturing Then StartCamera(_cameraId)
        Return False
    End Function

    Public Function SetROI(w As Integer, h As Integer) As Boolean
        If _cameraId = -1 Then Return False
        Dim wasCapturing As Boolean = _isCapturing
        If wasCapturing Then StopCamera()

        Dim info As New ASI_SDK.ASI_CAMERA_INFO()
        ASI_SDK.ASIGetCameraProperty(info, _cameraId)
        Dim currentMaxW As Integer = info.MaxWidth \ _currentBin
        Dim currentMaxH As Integer = info.MaxHeight \ _currentBin

        Dim startX As Integer = ((currentMaxW - w) \ 2 \ 4) * 4
        Dim startY As Integer = ((currentMaxH - h) \ 2 \ 2) * 2

        ASI_SDK.ASISetStartPos(_cameraId, 0, 0)
        Dim res = ASI_SDK.ASISetROIFormat(_cameraId, w, h, _currentBin, ASI_SDK.ASI_IMG_TYPE.ASI_IMG_RAW8)

        If res = 0 Then
            ASI_SDK.ASISetStartPos(_cameraId, startX, startY)
            _camWidth = w
            _camHeight = h
            AllocateBuffer(_camWidth, _camHeight)
            If wasCapturing Then StartCamera(_cameraId)
            Return True
        End If

        If wasCapturing AndAlso Not _isCapturing Then StartCamera(_cameraId)
        Return False
    End Function

    Public Function ApplyFormat(bin As Integer, w As Integer, h As Integer) As Boolean
        If _cameraId = -1 Then Return False
        Dim wasCapturing As Boolean = _isCapturing
        If wasCapturing Then StopCamera()

        Dim info As New ASI_SDK.ASI_CAMERA_INFO()
        ASI_SDK.ASIGetCameraProperty(info, _cameraId)
        Dim currentMaxW As Integer = info.MaxWidth \ bin
        Dim currentMaxH As Integer = info.MaxHeight \ bin

        Dim startX As Integer = ((currentMaxW - w) \ 2 \ 4) * 4
        Dim startY As Integer = ((currentMaxH - h) \ 2 \ 2) * 2

        ASI_SDK.ASISetStartPos(_cameraId, 0, 0)
        Dim res = ASI_SDK.ASISetROIFormat(_cameraId, w, h, bin, ASI_SDK.ASI_IMG_TYPE.ASI_IMG_RAW8)

        If res = 0 Then
            ASI_SDK.ASISetStartPos(_cameraId, startX, startY)
            _currentBin = bin
            _camWidth = w
            _camHeight = h
            AllocateBuffer(_camWidth, _camHeight)
            If wasCapturing Then StartCamera(_cameraId)
            Return True
        End If

        If wasCapturing AndAlso Not _isCapturing Then StartCamera(_cameraId)
        Return False
    End Function

    Private Sub AllocateBuffer(w As Integer, h As Integer)
        If _bufferPtr <> IntPtr.Zero Then
            Marshal.FreeHGlobal(_bufferPtr)
            _bufferPtr = IntPtr.Zero
        End If
        _bufferSize = w * h
        _bufferPtr = Marshal.AllocHGlobal(_bufferSize)
    End Sub

    ' ==========================================================
    ' FFmpeg 出力制御メソッド（RTSP配信 ＆ ローカル録画を統合制御）
    ' ==========================================================
    ' 💡【引数追加】splitMinutes（分割時間：分）を追加
    Public Sub UpdateOutputState(isStream As Boolean, url As String, isRecord As Boolean, savePath As String, retentionDays As Integer, splitMinutes As Integer)
        _isStreamEnabled = isStream
        _streamUrl = url
        _isRecordEnabled = isRecord
        _recordSavePath = savePath
        _retentionDays = retentionDays

        ' 💡 配信または録画の「どちらか片方でもON」なら、裏方のFFmpeg出力ループを動かす
        Dim shouldOutput As Boolean = _isStreamEnabled OrElse _isRecordEnabled

        If shouldOutput Then
            If Not _isOutputRunning Then
                _isOutputRunning = True
                ' キューの大掃除
                Dim dummy As Mat = Nothing
                While _streamQueue.TryDequeue(dummy)
                    If dummy IsNot Nothing Then dummy.Dispose()
                End While

                ' 💡【核心の修正】現在のクラスの最新URL状態（_streamUrl）を明示的に引き渡して起動！
                ' 💡【引数追加】splitMinutesをStreamLoopWorkerに渡す
                _outputTask = Task.Run(Sub() StreamLoopWorker(_streamUrl, splitMinutes))
            End If
        Else
            ' 両方OFFなら、裏方のFFmpegを完全に停止する
            StopStreaming()
        End If
    End Sub

    Public Sub StopStreaming()
        If Not _isOutputRunning Then Return
        _isOutputRunning = False

        If _outputTask IsNot Nothing Then
            _outputTask.Wait(5000)
            _outputTask = Nothing
        End If

        Dim dummy As Mat = Nothing
        While _streamQueue.TryDequeue(dummy)
            If dummy IsNot Nothing Then dummy.Dispose()
        End While
    End Sub

    ' ==========================================================
    ' 撮影の開始・停止
    ' ==========================================================
    Public Function StartCamera(deviceIndex As Integer) As Boolean Implements ICameraProvider.StartCamera
        If _cameraId < 0 Then Return False
        If ASI_SDK.ASIStartVideoCapture(_cameraId) = 0 Then
            _isCapturing = True
            _captureTask = Task.Run(AddressOf CaptureLoopWorker)
            Return True
        End If
        Return False
    End Function

    Public Sub StopCamera() Implements ICameraProvider.StopCamera
        If _cameraId >= 0 AndAlso _isCapturing Then
            _isCapturing = False
            If _captureTask IsNot Nothing Then
                _captureTask.Wait(12000)
                _captureTask = Nothing
            End If
            ASI_SDK.ASIStopVideoCapture(_cameraId)
        End If
    End Sub

    ' ==========================================================
    ' 映像取得ループ ＆ タイムスタンプ ＆ キュー投入
    ' ==========================================================
    Private Sub CaptureLoopWorker()
        Dim val As Integer = 0
        Dim isAuto As Integer = 0
        Dim waitMs As Integer = 1000

        Dim measuredFrameCount As Integer = 0
        Dim lastFpsTime As DateTime = DateTime.Now

        While _isCapturing
            If _bufferPtr = IntPtr.Zero Then
                Thread.Sleep(10)
                Continue While
            End If

            If ASI_SDK.ASIGetControlValue(_cameraId, ASI_SDK.ASI_CONTROL_TYPE.ASI_EXPOSURE, val, isAuto) = 0 Then
                waitMs = CInt((val / 1000) * 2 + 500)
            End If

            Dim status = ASI_SDK.ASIGetVideoData(_cameraId, _bufferPtr, _bufferSize, waitMs)

            If status = 0 Then
                If Not _isCapturing Then Exit While

                measuredFrameCount += 1
                Dim elapsed As Double = (DateTime.Now - lastFpsTime).TotalSeconds

                If elapsed >= 1.0 Then
                    Dim realFps As Double = measuredFrameCount / elapsed
                    If realFps > 30.0 Then realFps = 30.0
                    If realFps < 0.5 Then realFps = 0.5

                    _currentFps = realFps
                    measuredFrameCount = 0
                    lastFpsTime = DateTime.Now
                End If

                Using rawMat As Mat = Mat.FromPixelData(_camHeight, _camWidth, MatType.CV_8UC1, _bufferPtr)
                    Using outFrame As New Mat()

                        If _isColorCam Then
                            Cv2.CvtColor(rawMat, outFrame, _bayerCode)
                        Else
                            Cv2.CvtColor(rawMat, outFrame, ColorConversionCodes.GRAY2BGR)
                        End If

                        ' 反転処理
                        Dim flipVal As Integer = _currentFlipMode
                        If flipVal = 1 Then
                            Cv2.Flip(outFrame, outFrame, FlipMode.Y)
                        ElseIf flipVal = 2 Then
                            Cv2.Flip(outFrame, outFrame, FlipMode.X)
                        ElseIf flipVal = 3 Then
                            Cv2.Flip(outFrame, outFrame, FlipMode.XY)
                        End If

                        ' ==========================================================
                        ' 💡【自動リサイズ版】カメラの解像度に合わせて文字サイズを最適化
                        ' ==========================================================
                        Dim timeStr As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

                        ' 画面の高さ(Height)を基準に、文字の大きさと太さを全自動で計算します
                        ' （例：縦2800pxなら約4.0倍、縦960pxなら約1.3倍になります）
                        Dim fontScale As Double = Math.Max(0.5, outFrame.Height / 700.0)
                        Dim thickness As Integer = CInt(Math.Max(1, fontScale * 2.0))

                        ' 縁取りの黒枠の太さと、画面端からの余白も自動調整
                        Dim outlineThickness As Integer = thickness + CInt(Math.Max(2, fontScale * 1.5))
                        Dim margin As Integer = CInt(Math.Max(10, 12 * fontScale))

                        Dim baseLine As Integer = 0
                        Dim textSize As Size = Cv2.GetTextSize(timeStr, HersheyFonts.HersheySimplex, fontScale, thickness, baseLine)

                        Dim textX As Integer = outFrame.Width - textSize.Width - margin
                        Dim textY As Integer = outFrame.Height - margin
                        Dim textOrg As New Point(textX, textY)

                        ' 描画（黒い縁取りを先に描き、その上に白い文字を重ねる）
                        Cv2.PutText(outFrame, timeStr, textOrg, HersheyFonts.HersheySimplex, fontScale, Scalar.Black, outlineThickness, LineTypes.AntiAlias)
                        Cv2.PutText(outFrame, timeStr, textOrg, HersheyFonts.HersheySimplex, fontScale, Scalar.White, thickness, LineTypes.AntiAlias)
                        ' ==========================================================

                        ' 💡 配信か録画のどちらかが動いていればキューに投入
                        If _isOutputRunning Then
                            ' 💡【整合性修正】バッファ上限を「2」から「60（約2秒分）」に大幅拡張！
                            ' これにより、1分ごとのファイル切り替え負荷が発生しても一切コマ落ちしなくなります。
                            While _streamQueue.Count >= 60
                                Dim discarded As Mat = Nothing
                                If _streamQueue.TryDequeue(discarded) Then
                                    If discarded IsNot Nothing Then discarded.Dispose()
                                End If
                            End While
                            _streamQueue.Enqueue(outFrame.Clone())
                        End If

                        ' プレビュー用バッファは使わず、直接Cloneを渡す
                        RaiseEvent FrameArrived(outFrame.Clone())

                    End Using
                End Using
            Else
                Thread.Sleep(10)
            End If
        End While
    End Sub

    ' ==========================================================
    ' ★ 2プロセス並行ランナー方式：FFmpeg出力ループ（構文エラー完全排除版）
    ' ==========================================================
    ' 💡【引数追加】splitMinutesを追加
    Private Sub StreamLoopWorker(url As String, splitMinutes As Integer)
        ' 配信専用と録画専用のプロセスをそれぞれ独立して管理
        Dim ffmpegStreamProcess As System.Diagnostics.Process = Nothing
        Dim ffmpegRecordProcess As System.Diagnostics.Process = Nothing

        Dim lastW As Integer = 0
        Dim lastH As Integer = 0
        Dim lastFps As Double = 0.0

        ' 前回の配信・録画状態を記憶
        Dim lastStreamState As Boolean = False
        Dim lastRecordState As Boolean = False

        Dim videoBuffer() As Byte = Nothing

        ' ディスク容量チェック用のタイマー変数
        Dim lastDiskCheckTime As DateTime = DateTime.Now

        ' 自動お掃除（上書き）機能用の変数
        Dim lastCleanupTime As DateTime = DateTime.MinValue ' 初回はすぐに実行

        While _isOutputRunning

            ' ==========================================================
            ' 💡【究極の最適化】1時間に1回、自動お掃除 ＆ 最終容量チェックをまとめて実行
            ' ==========================================================
            If _isRecordEnabled AndAlso (DateTime.Now - lastCleanupTime).TotalHours >= 1.0 Then
                lastCleanupTime = DateTime.Now
                Try
                    ' --- 🧹 A. 自動お掃除（古いファイルの削除） ---
                    If System.IO.Directory.Exists(_recordSavePath) Then
                        Dim threshold As DateTime = DateTime.Now.AddDays(-_retentionDays)
                        Dim dirInfo As New System.IO.DirectoryInfo(_recordSavePath)

                        For Each fileInfo In dirInfo.GetFiles("*.mp4", System.IO.SearchOption.AllDirectories)
                            If fileInfo.CreationTime < threshold Then
                                Try
                                    fileInfo.Delete()
                                    System.Diagnostics.Debug.WriteLine($"古い録画を自動削除しました: {fileInfo.Name}")
                                Catch ex As Exception
                                    ' 使用中ファイル等は安全スルー
                                End Try
                            End If
                        Next
                    End If

                    ' --- 🚨 B. 最終防衛線（1時間に1回チェック） ---
                    Dim root As String = System.IO.Path.GetPathRoot(_recordSavePath)
                    Dim dInfo As New System.IO.DriveInfo(root)
                    If dInfo.IsReady Then
                        Dim freeGB As Double = dInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0)

                        ' 💡【修正】古いものを消した結果、それでも「10GB未満」なら緊急停止！
                        If freeGB < 10.0 Then
                            RaiseEvent DiskSpaceCritical()
                        End If
                    End If

                Catch ex As Exception
                End Try
            End If

            ' ==========================================================
            ' 💡 キューからのフレーム取り出しと各FFmpegへの配信
            ' ==========================================================
            Dim frame As Mat = Nothing
            If _streamQueue.TryDequeue(frame) Then
                Using frame
                    ' 1. カメラ解像度・FPS、またはユーザーによるUIのON/OFF切り替えが発生したか？
                    Dim baseChanged As Boolean = False
                    If lastW <> frame.Width OrElse lastH <> frame.Height OrElse
                       Math.Abs(lastFps - _currentFps) > 5.0 OrElse
                       lastStreamState <> _isStreamEnabled OrElse
                       lastRecordState <> _isRecordEnabled Then
                        baseChanged = True
                    End If

                    ' 根本的な変更があった時だけ、全プロセスを一旦綺麗にリセットする
                    If baseChanged Then
                        ' 配信プロセスのクリーンアップ
                        Try
                            If ffmpegStreamProcess IsNot Nothing AndAlso Not ffmpegStreamProcess.HasExited Then
                                ffmpegStreamProcess.StandardInput.Close()
                                ffmpegStreamProcess.WaitForExit(200)
                                ffmpegStreamProcess.Kill()
                            End If
                        Catch ex As Exception
                        End Try
                        ffmpegStreamProcess = Nothing

                        ' 録画プロセスのクリーンアップ
                        Try
                            If ffmpegRecordProcess IsNot Nothing AndAlso Not ffmpegRecordProcess.HasExited Then
                                ffmpegRecordProcess.StandardInput.Close()
                                ffmpegRecordProcess.WaitForExit(200)
                                ffmpegRecordProcess.Kill()
                            End If
                        Catch ex As Exception
                        End Try
                        ffmpegRecordProcess = Nothing

                        ' キューの大掃除
                        If lastW > 0 Then
                            System.Threading.Thread.Sleep(100)
                            Dim dummy As Mat = Nothing
                            While _streamQueue.TryDequeue(dummy)
                                If dummy IsNot Nothing Then dummy.Dispose()
                            End While
                        End If

                        ' 共用バッファの確保
                        Dim size As Integer = frame.Width * frame.Height * 3
                        ReDim videoBuffer(size - 1)

                        ' 状態の同期
                        lastW = frame.Width
                        lastH = frame.Height
                        lastFps = _currentFps
                        lastStreamState = _isStreamEnabled
                        lastRecordState = _isRecordEnabled
                    End If

                    ' 共通の実行ファイルパスと引数基礎値
                    Dim exePath As String = System.IO.Path.Combine(Application.StartupPath, "ffmpeg.exe")
                    Dim fpsStr As String = lastFps.ToString("F1")
                    Dim gopSize As Integer = CInt(Math.Max(5, lastFps))

                    ' ==========================================================
                    ' 2. 🚀【正式文法展開】RTSP配信プロセスの個別監視・個別起動
                    ' ==========================================================
                    If _isStreamEnabled Then
                        If ffmpegStreamProcess Is Nothing OrElse ffmpegStreamProcess.HasExited Then

                            ' 💡 配信側を元の「8M / CRF 20 / veryfast」の最高画質に戻します
                            Dim streamArgs As String = $"-y -use_wallclock_as_timestamps 1 -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {lastW}x{lastH} -i - " &
                           $"-c:v libx264 -preset veryfast -tune zerolatency -r {fpsStr} -g {gopSize} -crf 20 -b:v 8M -maxrate 8M -bufsize 4M " &
                           $"-rtsp_transport tcp -f rtsp {url}"

                            Dim startInfo As New System.Diagnostics.ProcessStartInfo()
                            startInfo.FileName = exePath
                            startInfo.Arguments = streamArgs
                            startInfo.UseShellExecute = False
                            startInfo.RedirectStandardInput = True
                            startInfo.CreateNoWindow = True
                            Try
                                ffmpegStreamProcess = System.Diagnostics.Process.Start(startInfo)
                            Catch ex As Exception
                                System.Diagnostics.Debug.WriteLine("配信FFmpeg起動失敗: " & ex.Message)
                            End Try
                        End If
                    Else
                        Try
                            If ffmpegStreamProcess IsNot Nothing AndAlso Not ffmpegStreamProcess.HasExited Then
                                ffmpegStreamProcess.StandardInput.Close()
                                ffmpegStreamProcess.Kill()
                            End If
                        Catch ex As Exception
                        End Try
                        ffmpegStreamProcess = Nothing
                    End If

                    ' ==========================================================
                    ' 3. 🚀【正式文法展開】ローカル録画プロセスの個別監視・個別起動（時計連動）
                    ' ==========================================================
                    If _isRecordEnabled Then
                        If ffmpegRecordProcess Is Nothing OrElse ffmpegRecordProcess.HasExited Then
                            Dim normalPath As String = _recordSavePath.Replace("\", "/")
                            If Not System.IO.Directory.Exists(_recordSavePath) Then System.IO.Directory.CreateDirectory(_recordSavePath)

                            ' 💡【修正】画面から受け取った「分」を「秒」に変換
                            Dim splitSeconds As Integer = splitMinutes * 60

                            ' 💡【黄金比アジャスト】固定の600ではなく {splitSeconds} を使って分割設定
                            Dim recordArgs As String = $"-y -use_wallclock_as_timestamps 1 -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s {lastW}x{lastH} -i - " &
                           $"-c:v libx264 -preset veryfast -tune zerolatency -r {fpsStr} -g {gopSize} -crf 20 -b:v 8M -maxrate 8M -bufsize 4M " &
                           $"-f segment -segment_time {splitSeconds} -segment_atclocktime 1 -reset_timestamps 1 -strftime 1 ""{normalPath}/ASICam_%Y%m%d_%H%M00.mp4"""

                            Dim startInfo As New System.Diagnostics.ProcessStartInfo()
                            startInfo.FileName = exePath
                            startInfo.Arguments = recordArgs
                            startInfo.UseShellExecute = False
                            startInfo.RedirectStandardInput = True
                            startInfo.CreateNoWindow = True
                            Try
                                ffmpegRecordProcess = System.Diagnostics.Process.Start(startInfo)
                            Catch ex As Exception
                                System.Diagnostics.Debug.WriteLine("録画FFmpeg起動失敗: " & ex.Message)
                            End Try
                        End If
                    Else
                        Try
                            If ffmpegRecordProcess IsNot Nothing AndAlso Not ffmpegRecordProcess.HasExited Then
                                ffmpegRecordProcess.StandardInput.Close()
                                ffmpegRecordProcess.Kill()
                            End If
                        Catch ex As Exception
                        End Try
                        ffmpegRecordProcess = Nothing
                    End If

                    ' ==========================================================
                    ' 4. 【データ個別送信】生画像データをそれぞれに安全に分配
                    ' ==========================================================
                    Try
                        Dim size As Integer = frame.Width * frame.Height * 3
                        System.Runtime.InteropServices.Marshal.Copy(frame.Data, videoBuffer, 0, size)

                        ' 配信側へ送信
                        If ffmpegStreamProcess IsNot Nothing AndAlso Not ffmpegStreamProcess.HasExited Then
                            ffmpegStreamProcess.StandardInput.BaseStream.Write(videoBuffer, 0, size)
                            ffmpegStreamProcess.StandardInput.BaseStream.Flush()
                        End If

                        ' 録画側へ送信
                        If ffmpegRecordProcess IsNot Nothing AndAlso Not ffmpegRecordProcess.HasExited Then
                            ffmpegRecordProcess.StandardInput.BaseStream.Write(videoBuffer, 0, size)
                            ffmpegRecordProcess.StandardInput.BaseStream.Flush()
                        End If

                    Catch ex As Exception
                        System.Diagnostics.Debug.WriteLine("FFmpeg Pipe Write Error: " & ex.Message)
                    End Try
                End Using
            Else
                System.Threading.Thread.Sleep(10)
            End If
        End While

        ' ==========================================================
        ' ループ終了時のお片付け処理（安全な複数行による解放）
        ' ==========================================================
        Try
            If ffmpegStreamProcess IsNot Nothing AndAlso Not ffmpegStreamProcess.HasExited Then
                ffmpegStreamProcess.StandardInput.Close()
                ffmpegStreamProcess.Kill()
                ffmpegStreamProcess.Dispose()
            End If
        Catch ex As Exception
        End Try

        Try
            If ffmpegRecordProcess IsNot Nothing AndAlso Not ffmpegRecordProcess.HasExited Then
                ffmpegRecordProcess.StandardInput.Close()
                ffmpegRecordProcess.Kill()
                ffmpegRecordProcess.Dispose()
            End If
        Catch ex As Exception
        End Try

        videoBuffer = Nothing
    End Sub

    Public Sub Disconnect()
        If _cameraId >= 0 Then
            StopStreaming()
            StopCamera()
            ASI_SDK.ASICloseCamera(_cameraId)
            _cameraId = -1

            If _bufferPtr <> IntPtr.Zero Then
                Marshal.FreeHGlobal(_bufferPtr)
                _bufferPtr = IntPtr.Zero
                _bufferSize = 0
            End If
        End If
    End Sub

    Public Function GetSupportedBins(targetID As Integer) As List(Of Integer)
        Dim binList As New List(Of Integer)
        Try
            Dim info As New ASI_SDK.ASI_CAMERA_INFO()
            ASI_SDK.ASIGetCameraProperty(info, targetID)
            For Each b As Integer In info.SupportedBins
                If b = 0 Then Exit For
                binList.Add(b)
            Next
        Catch ex As Exception
        End Try
        Return binList
    End Function

    Protected Overrides Sub Finalize()
        If _bufferPtr <> IntPtr.Zero Then
            Marshal.FreeHGlobal(_bufferPtr)
            _bufferPtr = IntPtr.Zero
        End If
        MyBase.Finalize()
    End Sub
End Class