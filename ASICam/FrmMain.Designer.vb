<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmMain
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows フォーム デザイナーで必要です。
    Private components As System.ComponentModel.IContainer

    'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
    'Windows フォーム デザイナーを使用して変更できます。  
    'コード エディターを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmMain))
        GroupBox1 = New GroupBox()
        LblFlip = New Label()
        CmbFlip = New ComboBox()
        LblROI = New Label()
        CmbRoi = New ComboBox()
        LblBinning = New Label()
        CmbBinning = New ComboBox()
        BtnConnect = New Button()
        BtnRefresh = New Button()
        CmbCameraList = New ComboBox()
        GroupBox2 = New GroupBox()
        Label4 = New Label()
        ChkHighSpeed = New CheckBox()
        TrcUsbBandwidth = New TrackBar()
        LblUsbBandwidth = New Label()
        GroupBox3 = New GroupBox()
        LblTargetTemp = New Label()
        TrcTargetTemp = New TrackBar()
        ChkCoolerOn = New CheckBox()
        GrpGain = New GroupBox()
        TrkbGain = New TrackBar()
        LblGain = New Label()
        ChkGainAuto = New CheckBox()
        GrpExp = New GroupBox()
        LblExp = New Label()
        TrkbExp = New TrackBar()
        ChkExpAuto = New CheckBox()
        GrpWB = New GroupBox()
        Lbl_B = New Label()
        Lbl_R = New Label()
        LblWb = New Label()
        TrkbWbB = New TrackBar()
        TrkbWbR = New TrackBar()
        ChkWBAuto = New CheckBox()
        StsMain = New StatusStrip()
        LblStreamStatus = New ToolStripStatusLabel()
        LblStatusResolution = New ToolStripStatusLabel()
        LblStatusFps = New ToolStripStatusLabel()
        LblSensorTemp = New ToolStripStatusLabel()
        LblCoolerPower = New ToolStripStatusLabel()
        LblRecordStatus = New ToolStripStatusLabel()
        ChkShowPreview = New CheckBox()
        GroupBox4 = New GroupBox()
        Label2 = New Label()
        LblGamma = New Label()
        TrkbGamma = New TrackBar()
        TmrUIUpdate = New Timer(components)
        GroupBox6 = New GroupBox()
        ChkEnableStream = New CheckBox()
        Label1 = New Label()
        TxtRtspUrl = New TextBox()
        ChkRecord = New CheckBox()
        BtnSetSavePath = New Button()
        BtnAdvancedSettings = New Button()
        GroupBox1.SuspendLayout()
        GroupBox2.SuspendLayout()
        CType(TrcUsbBandwidth, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox3.SuspendLayout()
        CType(TrcTargetTemp, ComponentModel.ISupportInitialize).BeginInit()
        GrpGain.SuspendLayout()
        CType(TrkbGain, ComponentModel.ISupportInitialize).BeginInit()
        GrpExp.SuspendLayout()
        CType(TrkbExp, ComponentModel.ISupportInitialize).BeginInit()
        GrpWB.SuspendLayout()
        CType(TrkbWbB, ComponentModel.ISupportInitialize).BeginInit()
        CType(TrkbWbR, ComponentModel.ISupportInitialize).BeginInit()
        StsMain.SuspendLayout()
        GroupBox4.SuspendLayout()
        CType(TrkbGamma, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox6.SuspendLayout()
        SuspendLayout()
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(LblFlip)
        GroupBox1.Controls.Add(CmbFlip)
        GroupBox1.Controls.Add(LblROI)
        GroupBox1.Controls.Add(CmbRoi)
        GroupBox1.Controls.Add(LblBinning)
        GroupBox1.Controls.Add(CmbBinning)
        GroupBox1.Controls.Add(BtnConnect)
        GroupBox1.Controls.Add(BtnRefresh)
        GroupBox1.Controls.Add(CmbCameraList)
        GroupBox1.Location = New Point(12, 12)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(230, 170)
        GroupBox1.TabIndex = 1
        GroupBox1.TabStop = False
        GroupBox1.Text = "接続設定"
        ' 
        ' LblFlip
        ' 
        LblFlip.AutoSize = True
        LblFlip.Location = New Point(6, 140)
        LblFlip.Name = "LblFlip"
        LblFlip.Size = New Size(62, 15)
        LblFlip.TabIndex = 8
        LblFlip.Text = "映像の向き"
        ' 
        ' CmbFlip
        ' 
        CmbFlip.FormattingEnabled = True
        CmbFlip.Items.AddRange(New Object() {"通常", "左右反転", "上下反転", "180度回転"})
        CmbFlip.Location = New Point(72, 137)
        CmbFlip.Name = "CmbFlip"
        CmbFlip.Size = New Size(150, 23)
        CmbFlip.TabIndex = 5
        ' 
        ' LblROI
        ' 
        LblROI.AutoSize = True
        LblROI.Location = New Point(6, 111)
        LblROI.Name = "LblROI"
        LblROI.Size = New Size(26, 15)
        LblROI.TabIndex = 6
        LblROI.Text = "ROI"
        ' 
        ' CmbRoi
        ' 
        CmbRoi.FormattingEnabled = True
        CmbRoi.Items.AddRange(New Object() {"0: 全画面 (100%)", "1: 中央 95% 切り出し", "2: 中央 90% 切り出し", "3: 中央 85% 切り出し", "4: 中央 80% 切り出し"})
        CmbRoi.Location = New Point(72, 108)
        CmbRoi.Name = "CmbRoi"
        CmbRoi.Size = New Size(150, 23)
        CmbRoi.TabIndex = 4
        ' 
        ' LblBinning
        ' 
        LblBinning.AutoSize = True
        LblBinning.Location = New Point(6, 82)
        LblBinning.Name = "LblBinning"
        LblBinning.Size = New Size(41, 15)
        LblBinning.TabIndex = 4
        LblBinning.Text = "ビニング"
        ' 
        ' CmbBinning
        ' 
        CmbBinning.FormattingEnabled = True
        CmbBinning.Location = New Point(72, 79)
        CmbBinning.Name = "CmbBinning"
        CmbBinning.Size = New Size(150, 23)
        CmbBinning.TabIndex = 3
        ' 
        ' BtnConnect
        ' 
        BtnConnect.Location = New Point(162, 50)
        BtnConnect.Name = "BtnConnect"
        BtnConnect.Size = New Size(60, 23)
        BtnConnect.TabIndex = 2
        BtnConnect.Text = "接続"
        BtnConnect.UseVisualStyleBackColor = True
        ' 
        ' BtnRefresh
        ' 
        BtnRefresh.Location = New Point(162, 22)
        BtnRefresh.Name = "BtnRefresh"
        BtnRefresh.Size = New Size(60, 23)
        BtnRefresh.TabIndex = 1
        BtnRefresh.Text = "更新"
        BtnRefresh.UseVisualStyleBackColor = True
        ' 
        ' CmbCameraList
        ' 
        CmbCameraList.DropDownStyle = ComboBoxStyle.DropDownList
        CmbCameraList.FormattingEnabled = True
        CmbCameraList.Location = New Point(6, 22)
        CmbCameraList.Name = "CmbCameraList"
        CmbCameraList.Size = New Size(150, 23)
        CmbCameraList.TabIndex = 0
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(Label4)
        GroupBox2.Controls.Add(ChkHighSpeed)
        GroupBox2.Controls.Add(TrcUsbBandwidth)
        GroupBox2.Controls.Add(LblUsbBandwidth)
        GroupBox2.Location = New Point(12, 188)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(230, 140)
        GroupBox2.TabIndex = 2
        GroupBox2.TabStop = False
        GroupBox2.Text = "USB設定"
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(6, 110)
        Label4.Name = "Label4"
        Label4.Size = New Size(159, 15)
        Label4.TabIndex = 3
        Label4.Text = "※ONにすると画質が低下します"
        ' 
        ' ChkHighSpeed
        ' 
        ChkHighSpeed.AutoSize = True
        ChkHighSpeed.Location = New Point(6, 88)
        ChkHighSpeed.Name = "ChkHighSpeed"
        ChkHighSpeed.Size = New Size(158, 19)
        ChkHighSpeed.TabIndex = 7
        ChkHighSpeed.Text = "High Speed Mode (10bit)"
        ChkHighSpeed.UseVisualStyleBackColor = True
        ' 
        ' TrcUsbBandwidth
        ' 
        TrcUsbBandwidth.Location = New Point(6, 37)
        TrcUsbBandwidth.Maximum = 100
        TrcUsbBandwidth.Minimum = 40
        TrcUsbBandwidth.Name = "TrcUsbBandwidth"
        TrcUsbBandwidth.Size = New Size(218, 45)
        TrcUsbBandwidth.TabIndex = 6
        TrcUsbBandwidth.TickFrequency = 5
        TrcUsbBandwidth.Value = 40
        ' 
        ' LblUsbBandwidth
        ' 
        LblUsbBandwidth.AutoSize = True
        LblUsbBandwidth.Location = New Point(6, 19)
        LblUsbBandwidth.Name = "LblUsbBandwidth"
        LblUsbBandwidth.Size = New Size(80, 15)
        LblUsbBandwidth.TabIndex = 0
        LblUsbBandwidth.Text = "USB帯域: 40%"
        ' 
        ' GroupBox3
        ' 
        GroupBox3.Controls.Add(LblTargetTemp)
        GroupBox3.Controls.Add(TrcTargetTemp)
        GroupBox3.Controls.Add(ChkCoolerOn)
        GroupBox3.Location = New Point(12, 334)
        GroupBox3.Name = "GroupBox3"
        GroupBox3.Size = New Size(230, 116)
        GroupBox3.TabIndex = 3
        GroupBox3.TabStop = False
        GroupBox3.Text = "冷却設定"
        ' 
        ' LblTargetTemp
        ' 
        LblTargetTemp.AutoSize = True
        LblTargetTemp.Location = New Point(6, 44)
        LblTargetTemp.Name = "LblTargetTemp"
        LblTargetTemp.Size = New Size(98, 15)
        LblTargetTemp.TabIndex = 2
        LblTargetTemp.Text = "ターゲット温度: 0℃"
        ' 
        ' TrcTargetTemp
        ' 
        TrcTargetTemp.Location = New Point(6, 62)
        TrcTargetTemp.Maximum = 30
        TrcTargetTemp.Minimum = -40
        TrcTargetTemp.Name = "TrcTargetTemp"
        TrcTargetTemp.Size = New Size(218, 45)
        TrcTargetTemp.TabIndex = 1
        ' 
        ' ChkCoolerOn
        ' 
        ChkCoolerOn.AutoSize = True
        ChkCoolerOn.Location = New Point(6, 22)
        ChkCoolerOn.Name = "ChkCoolerOn"
        ChkCoolerOn.Size = New Size(95, 19)
        ChkCoolerOn.TabIndex = 0
        ChkCoolerOn.Text = "冷却機能 ON"
        ChkCoolerOn.UseVisualStyleBackColor = True
        ' 
        ' GrpGain
        ' 
        GrpGain.Controls.Add(TrkbGain)
        GrpGain.Controls.Add(LblGain)
        GrpGain.Controls.Add(ChkGainAuto)
        GrpGain.Location = New Point(248, 12)
        GrpGain.Name = "GrpGain"
        GrpGain.Size = New Size(280, 100)
        GrpGain.TabIndex = 4
        GrpGain.TabStop = False
        GrpGain.Text = "Gain"
        ' 
        ' TrkbGain
        ' 
        TrkbGain.Location = New Point(6, 47)
        TrkbGain.Name = "TrkbGain"
        TrkbGain.Size = New Size(268, 45)
        TrkbGain.TabIndex = 2
        ' 
        ' LblGain
        ' 
        LblGain.AutoSize = True
        LblGain.Location = New Point(120, 22)
        LblGain.Name = "LblGain"
        LblGain.Size = New Size(31, 15)
        LblGain.TabIndex = 1
        LblGain.Text = "Gain"
        ' 
        ' ChkGainAuto
        ' 
        ChkGainAuto.AutoSize = True
        ChkGainAuto.Location = New Point(6, 22)
        ChkGainAuto.Name = "ChkGainAuto"
        ChkGainAuto.Size = New Size(51, 19)
        ChkGainAuto.TabIndex = 1
        ChkGainAuto.Text = "オート"
        ChkGainAuto.UseVisualStyleBackColor = True
        ' 
        ' GrpExp
        ' 
        GrpExp.Controls.Add(LblExp)
        GrpExp.Controls.Add(TrkbExp)
        GrpExp.Controls.Add(ChkExpAuto)
        GrpExp.Location = New Point(248, 118)
        GrpExp.Name = "GrpExp"
        GrpExp.Size = New Size(280, 100)
        GrpExp.TabIndex = 5
        GrpExp.TabStop = False
        GrpExp.Text = "露出"
        ' 
        ' LblExp
        ' 
        LblExp.AutoSize = True
        LblExp.Location = New Point(120, 22)
        LblExp.Name = "LblExp"
        LblExp.Size = New Size(26, 15)
        LblExp.TabIndex = 2
        LblExp.Text = "Exp"
        ' 
        ' TrkbExp
        ' 
        TrkbExp.Location = New Point(6, 47)
        TrkbExp.Name = "TrkbExp"
        TrkbExp.Size = New Size(268, 45)
        TrkbExp.TabIndex = 2
        ' 
        ' ChkExpAuto
        ' 
        ChkExpAuto.AutoSize = True
        ChkExpAuto.Location = New Point(6, 22)
        ChkExpAuto.Name = "ChkExpAuto"
        ChkExpAuto.Size = New Size(51, 19)
        ChkExpAuto.TabIndex = 1
        ChkExpAuto.Text = "オート"
        ChkExpAuto.UseVisualStyleBackColor = True
        ' 
        ' GrpWB
        ' 
        GrpWB.Controls.Add(Lbl_B)
        GrpWB.Controls.Add(Lbl_R)
        GrpWB.Controls.Add(LblWb)
        GrpWB.Controls.Add(TrkbWbB)
        GrpWB.Controls.Add(TrkbWbR)
        GrpWB.Controls.Add(ChkWBAuto)
        GrpWB.Location = New Point(248, 224)
        GrpWB.Name = "GrpWB"
        GrpWB.Size = New Size(280, 150)
        GrpWB.TabIndex = 6
        GrpWB.TabStop = False
        GrpWB.Text = "ホワイトバランス"
        ' 
        ' Lbl_B
        ' 
        Lbl_B.AutoSize = True
        Lbl_B.Location = New Point(6, 98)
        Lbl_B.Name = "Lbl_B"
        Lbl_B.Size = New Size(14, 15)
        Lbl_B.TabIndex = 5
        Lbl_B.Text = "B"
        ' 
        ' Lbl_R
        ' 
        Lbl_R.AutoSize = True
        Lbl_R.Location = New Point(6, 47)
        Lbl_R.Name = "Lbl_R"
        Lbl_R.Size = New Size(14, 15)
        Lbl_R.TabIndex = 4
        Lbl_R.Text = "R"
        ' 
        ' LblWb
        ' 
        LblWb.AutoSize = True
        LblWb.Location = New Point(100, 22)
        LblWb.Name = "LblWb"
        LblWb.Size = New Size(50, 15)
        LblWb.TabIndex = 3
        LblWb.Text = "WB(R:B)"
        ' 
        ' TrkbWbB
        ' 
        TrkbWbB.Location = New Point(26, 98)
        TrkbWbB.Name = "TrkbWbB"
        TrkbWbB.Size = New Size(248, 45)
        TrkbWbB.TabIndex = 3
        ' 
        ' TrkbWbR
        ' 
        TrkbWbR.Location = New Point(26, 47)
        TrkbWbR.Name = "TrkbWbR"
        TrkbWbR.Size = New Size(248, 45)
        TrkbWbR.TabIndex = 2
        ' 
        ' ChkWBAuto
        ' 
        ChkWBAuto.AutoSize = True
        ChkWBAuto.Location = New Point(6, 22)
        ChkWBAuto.Name = "ChkWBAuto"
        ChkWBAuto.Size = New Size(51, 19)
        ChkWBAuto.TabIndex = 1
        ChkWBAuto.Text = "オート"
        ChkWBAuto.UseVisualStyleBackColor = True
        ' 
        ' StsMain
        ' 
        StsMain.Items.AddRange(New ToolStripItem() {LblStreamStatus, LblStatusResolution, LblStatusFps, LblSensorTemp, LblCoolerPower, LblRecordStatus})
        StsMain.Location = New Point(0, 539)
        StsMain.Name = "StsMain"
        StsMain.Size = New Size(539, 22)
        StsMain.SizingGrip = False
        StsMain.TabIndex = 7
        StsMain.Text = "StatusStrip1"
        ' 
        ' LblStreamStatus
        ' 
        LblStreamStatus.Name = "LblStreamStatus"
        LblStreamStatus.Size = New Size(43, 17)
        LblStreamStatus.Text = "未配信"
        ' 
        ' LblStatusResolution
        ' 
        LblStatusResolution.Name = "LblStatusResolution"
        LblStatusResolution.Size = New Size(64, 17)
        LblStatusResolution.Text = "解像度: ---"
        ' 
        ' LblStatusFps
        ' 
        LblStatusFps.Name = "LblStatusFps"
        LblStatusFps.Size = New Size(47, 17)
        LblStatusFps.Text = "FPS: 0.0"
        ' 
        ' LblSensorTemp
        ' 
        LblSensorTemp.Name = "LblSensorTemp"
        LblSensorTemp.Size = New Size(52, 17)
        LblSensorTemp.Text = "温度: ---"
        ' 
        ' LblCoolerPower
        ' 
        LblCoolerPower.Name = "LblCoolerPower"
        LblCoolerPower.Size = New Size(84, 17)
        LblCoolerPower.Text = "冷却パワー: --%"
        ' 
        ' LblRecordStatus
        ' 
        LblRecordStatus.Name = "LblRecordStatus"
        LblRecordStatus.Size = New Size(58, 17)
        LblRecordStatus.Text = "録画: OFF"
        ' 
        ' ChkShowPreview
        ' 
        ChkShowPreview.AutoSize = True
        ChkShowPreview.Location = New Point(248, 484)
        ChkShowPreview.Name = "ChkShowPreview"
        ChkShowPreview.Size = New Size(143, 19)
        ChkShowPreview.TabIndex = 10
        ChkShowPreview.Text = "プレビュー画面を表示する"
        ChkShowPreview.UseVisualStyleBackColor = True
        ' 
        ' GroupBox4
        ' 
        GroupBox4.Controls.Add(Label2)
        GroupBox4.Controls.Add(LblGamma)
        GroupBox4.Controls.Add(TrkbGamma)
        GroupBox4.Location = New Point(248, 378)
        GroupBox4.Name = "GroupBox4"
        GroupBox4.Size = New Size(280, 100)
        GroupBox4.TabIndex = 13
        GroupBox4.TabStop = False
        GroupBox4.Text = "画像調整"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(6, 78)
        Label2.Name = "Label2"
        Label2.Size = New Size(245, 15)
        Label2.TabIndex = 19
        Label2.Text = "中間調の明るさを変え、夜空の黒さを引き締めます"
        ' 
        ' LblGamma
        ' 
        LblGamma.AutoSize = True
        LblGamma.Location = New Point(6, 19)
        LblGamma.Name = "LblGamma"
        LblGamma.Size = New Size(77, 15)
        LblGamma.TabIndex = 18
        LblGamma.Text = "ガンマ補正: 50"
        ' 
        ' TrkbGamma
        ' 
        TrkbGamma.Location = New Point(6, 37)
        TrkbGamma.Maximum = 100
        TrkbGamma.Name = "TrkbGamma"
        TrkbGamma.Size = New Size(268, 45)
        TrkbGamma.TabIndex = 1
        TrkbGamma.Value = 50
        ' 
        ' TmrUIUpdate
        ' 
        TmrUIUpdate.Interval = 1000
        ' 
        ' GroupBox6
        ' 
        GroupBox6.Controls.Add(ChkEnableStream)
        GroupBox6.Controls.Add(Label1)
        GroupBox6.Controls.Add(TxtRtspUrl)
        GroupBox6.Location = New Point(12, 456)
        GroupBox6.Name = "GroupBox6"
        GroupBox6.Size = New Size(230, 74)
        GroupBox6.TabIndex = 14
        GroupBox6.TabStop = False
        ' 
        ' ChkEnableStream
        ' 
        ChkEnableStream.AutoSize = True
        ChkEnableStream.Location = New Point(6, 0)
        ChkEnableStream.Name = "ChkEnableStream"
        ChkEnableStream.Size = New Size(100, 19)
        ChkEnableStream.TabIndex = 2
        ChkEnableStream.Text = "RTSP配信有効"
        ChkEnableStream.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(6, 22)
        Label1.Name = "Label1"
        Label1.Size = New Size(64, 15)
        Label1.TabIndex = 1
        Label1.Text = "配信先URL"
        ' 
        ' TxtRtspUrl
        ' 
        TxtRtspUrl.Location = New Point(6, 40)
        TxtRtspUrl.Name = "TxtRtspUrl"
        TxtRtspUrl.Size = New Size(218, 23)
        TxtRtspUrl.TabIndex = 0
        TxtRtspUrl.Text = "rtsp://127.0.0.1:8554/live"
        ' 
        ' ChkRecord
        ' 
        ChkRecord.AutoSize = True
        ChkRecord.Location = New Point(248, 509)
        ChkRecord.Name = "ChkRecord"
        ChkRecord.Size = New Size(95, 19)
        ChkRecord.TabIndex = 15
        ChkRecord.Text = "録画機能 ON"
        ChkRecord.UseVisualStyleBackColor = True
        ' 
        ' BtnSetSavePath
        ' 
        BtnSetSavePath.Location = New Point(349, 506)
        BtnSetSavePath.Name = "BtnSetSavePath"
        BtnSetSavePath.Size = New Size(80, 23)
        BtnSetSavePath.TabIndex = 16
        BtnSetSavePath.Text = "保存先設定"
        BtnSetSavePath.UseVisualStyleBackColor = True
        ' 
        ' BtnAdvancedSettings
        ' 
        BtnAdvancedSettings.Location = New Point(447, 506)
        BtnAdvancedSettings.Name = "BtnAdvancedSettings"
        BtnAdvancedSettings.Size = New Size(80, 23)
        BtnAdvancedSettings.TabIndex = 17
        BtnAdvancedSettings.Text = "詳細設定"
        BtnAdvancedSettings.UseVisualStyleBackColor = True
        ' 
        ' FrmMain
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(539, 561)
        Controls.Add(BtnAdvancedSettings)
        Controls.Add(BtnSetSavePath)
        Controls.Add(ChkRecord)
        Controls.Add(GroupBox6)
        Controls.Add(GroupBox4)
        Controls.Add(ChkShowPreview)
        Controls.Add(StsMain)
        Controls.Add(GrpWB)
        Controls.Add(GrpExp)
        Controls.Add(GrpGain)
        Controls.Add(GroupBox3)
        Controls.Add(GroupBox2)
        Controls.Add(GroupBox1)
        FormBorderStyle = FormBorderStyle.FixedSingle
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        Name = "FrmMain"
        Text = "ASICam"
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        CType(TrcUsbBandwidth, ComponentModel.ISupportInitialize).EndInit()
        GroupBox3.ResumeLayout(False)
        GroupBox3.PerformLayout()
        CType(TrcTargetTemp, ComponentModel.ISupportInitialize).EndInit()
        GrpGain.ResumeLayout(False)
        GrpGain.PerformLayout()
        CType(TrkbGain, ComponentModel.ISupportInitialize).EndInit()
        GrpExp.ResumeLayout(False)
        GrpExp.PerformLayout()
        CType(TrkbExp, ComponentModel.ISupportInitialize).EndInit()
        GrpWB.ResumeLayout(False)
        GrpWB.PerformLayout()
        CType(TrkbWbB, ComponentModel.ISupportInitialize).EndInit()
        CType(TrkbWbR, ComponentModel.ISupportInitialize).EndInit()
        StsMain.ResumeLayout(False)
        StsMain.PerformLayout()
        GroupBox4.ResumeLayout(False)
        GroupBox4.PerformLayout()
        CType(TrkbGamma, ComponentModel.ISupportInitialize).EndInit()
        GroupBox6.ResumeLayout(False)
        GroupBox6.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents LblROI As Label
    Friend WithEvents CmbRoi As ComboBox
    Friend WithEvents LblBinning As Label
    Friend WithEvents CmbBinning As ComboBox
    Friend WithEvents BtnConnect As Button
    Friend WithEvents BtnRefresh As Button
    Friend WithEvents CmbCameraList As ComboBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents GrpGain As GroupBox
    Friend WithEvents GrpExp As GroupBox
    Friend WithEvents GrpWB As GroupBox
    Friend WithEvents Label4 As Label
    Friend WithEvents ChkHighSpeed As CheckBox
    Friend WithEvents TrcUsbBandwidth As TrackBar
    Friend WithEvents LblUsbBandwidth As Label
    Friend WithEvents LblFlip As Label
    Friend WithEvents CmbFlip As ComboBox
    Friend WithEvents ChkCoolerOn As CheckBox
    Friend WithEvents TrcTargetTemp As TrackBar
    Friend WithEvents LblTargetTemp As Label
    Friend WithEvents StsMain As StatusStrip
    Friend WithEvents TrkbGain As TrackBar
    Friend WithEvents LblGain As Label
    Friend WithEvents ChkGainAuto As CheckBox
    Friend WithEvents ChkExpAuto As CheckBox
    Friend WithEvents LblExp As Label
    Friend WithEvents TrkbExp As TrackBar
    Friend WithEvents LblWb As Label
    Friend WithEvents TrkbWbB As TrackBar
    Friend WithEvents TrkbWbR As TrackBar
    Friend WithEvents ChkWBAuto As CheckBox
    Friend WithEvents ChkShowPreview As CheckBox
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents TrkbGamma As TrackBar
    Friend WithEvents LblGamma As Label
    Friend WithEvents LblStatusResolution As ToolStripStatusLabel
    Friend WithEvents LblStatusFps As ToolStripStatusLabel
    Friend WithEvents LblSensorTemp As ToolStripStatusLabel
    Friend WithEvents TmrUIUpdate As Timer
    Friend WithEvents Lbl_R As Label
    Friend WithEvents Lbl_B As Label
    Friend WithEvents GroupBox6 As GroupBox
    Friend WithEvents TxtRtspUrl As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents LblStreamStatus As ToolStripStatusLabel
    Friend WithEvents ChkEnableStream As CheckBox
    Friend WithEvents Label2 As Label
    Friend WithEvents LblCoolerPower As ToolStripStatusLabel
    Friend WithEvents ChkRecord As CheckBox
    Friend WithEvents BtnSetSavePath As Button
    Friend WithEvents LblRecordStatus As ToolStripStatusLabel
    Friend WithEvents BtnAdvancedSettings As Button
End Class
