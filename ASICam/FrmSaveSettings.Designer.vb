<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmSaveSettings
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
        TxtPath = New TextBox()
        BtnBrowse = New Button()
        BtnSave = New Button()
        BtnCancel = New Button()
        Label1 = New Label()
        NumRetentionDays = New NumericUpDown()
        Label2 = New Label()
        Label3 = New Label()
        CmbSplitTime = New ComboBox()
        Label4 = New Label()
        Label5 = New Label()
        CType(NumRetentionDays, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' TxtPath
        ' 
        TxtPath.Location = New Point(12, 27)
        TxtPath.Name = "TxtPath"
        TxtPath.Size = New Size(294, 23)
        TxtPath.TabIndex = 0
        ' 
        ' BtnBrowse
        ' 
        BtnBrowse.Location = New Point(312, 27)
        BtnBrowse.Name = "BtnBrowse"
        BtnBrowse.Size = New Size(40, 23)
        BtnBrowse.TabIndex = 1
        BtnBrowse.Text = "..."
        BtnBrowse.UseVisualStyleBackColor = True
        ' 
        ' BtnSave
        ' 
        BtnSave.Location = New Point(272, 122)
        BtnSave.Name = "BtnSave"
        BtnSave.Size = New Size(80, 30)
        BtnSave.TabIndex = 2
        BtnSave.Text = "保存"
        BtnSave.UseVisualStyleBackColor = True
        ' 
        ' BtnCancel
        ' 
        BtnCancel.Location = New Point(186, 122)
        BtnCancel.Name = "BtnCancel"
        BtnCancel.Size = New Size(80, 30)
        BtnCancel.TabIndex = 3
        BtnCancel.Text = "キャンセル"
        BtnCancel.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(12, 9)
        Label1.Name = "Label1"
        Label1.Size = New Size(43, 15)
        Label1.TabIndex = 4
        Label1.Text = "保存先"
        ' 
        ' NumRetentionDays
        ' 
        NumRetentionDays.Location = New Point(73, 56)
        NumRetentionDays.Name = "NumRetentionDays"
        NumRetentionDays.Size = New Size(53, 23)
        NumRetentionDays.TabIndex = 5
        NumRetentionDays.TextAlign = HorizontalAlignment.Center
        NumRetentionDays.Value = New Decimal(New Integer() {7, 0, 0, 0})
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(12, 58)
        Label2.Name = "Label2"
        Label2.Size = New Size(55, 15)
        Label2.TabIndex = 6
        Label2.Text = "保存日数"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(132, 58)
        Label3.Name = "Label3"
        Label3.Size = New Size(31, 15)
        Label3.TabIndex = 7
        Label3.Text = "日間"
        ' 
        ' CmbSplitTime
        ' 
        CmbSplitTime.DropDownStyle = ComboBoxStyle.DropDownList
        CmbSplitTime.FormattingEnabled = True
        CmbSplitTime.Items.AddRange(New Object() {"1", "5", "10", "20", "30", "40", "50", "60"})
        CmbSplitTime.Location = New Point(129, 85)
        CmbSplitTime.Name = "CmbSplitTime"
        CmbSplitTime.Size = New Size(53, 23)
        CmbSplitTime.TabIndex = 8
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(12, 88)
        Label4.Name = "Label4"
        Label4.Size = New Size(111, 15)
        Label4.TabIndex = 9
        Label4.Text = "１ファイルの録画時間"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(188, 88)
        Label5.Name = "Label5"
        Label5.Size = New Size(19, 15)
        Label5.TabIndex = 10
        Label5.Text = "分"
        ' 
        ' FrmSaveSettings
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(364, 161)
        Controls.Add(Label5)
        Controls.Add(Label4)
        Controls.Add(CmbSplitTime)
        Controls.Add(Label3)
        Controls.Add(Label2)
        Controls.Add(NumRetentionDays)
        Controls.Add(Label1)
        Controls.Add(BtnCancel)
        Controls.Add(BtnSave)
        Controls.Add(BtnBrowse)
        Controls.Add(TxtPath)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmSaveSettings"
        Text = "録画設定"
        CType(NumRetentionDays, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents TxtPath As TextBox
    Friend WithEvents BtnBrowse As Button
    Friend WithEvents BtnSave As Button
    Friend WithEvents BtnCancel As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents NumRetentionDays As NumericUpDown
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents CmbSplitTime As ComboBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
End Class
