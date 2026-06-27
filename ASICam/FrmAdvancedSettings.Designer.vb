<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmAdvancedSettings
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
        Label1 = New Label()
        CmbEncoder = New ComboBox()
        BtnSave = New Button()
        BtnCancel = New Button()
        LblAutoResult = New Label()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(12, 15)
        Label1.Name = "Label1"
        Label1.Size = New Size(136, 15)
        Label1.TabIndex = 0
        Label1.Text = "エンコーダ (動画圧縮方式):"
        ' 
        ' CmbEncoder
        ' 
        CmbEncoder.DropDownStyle = ComboBoxStyle.DropDownList
        CmbEncoder.FormattingEnabled = True
        CmbEncoder.Items.AddRange(New Object() {"0: 自動判定 (推奨)", "1: Intel QSV (省電力)", "2: NVIDIA NVENC (高画質)", "3: AMD AMF", "4: ソフトウェア (CPU高負荷)"})
        CmbEncoder.Location = New Point(154, 12)
        CmbEncoder.Name = "CmbEncoder"
        CmbEncoder.Size = New Size(198, 23)
        CmbEncoder.TabIndex = 1
        ' 
        ' BtnSave
        ' 
        BtnSave.Location = New Point(272, 119)
        BtnSave.Name = "BtnSave"
        BtnSave.Size = New Size(80, 30)
        BtnSave.TabIndex = 2
        BtnSave.Text = "保存"
        BtnSave.UseVisualStyleBackColor = True
        ' 
        ' BtnCancel
        ' 
        BtnCancel.Location = New Point(186, 119)
        BtnCancel.Name = "BtnCancel"
        BtnCancel.Size = New Size(80, 30)
        BtnCancel.TabIndex = 3
        BtnCancel.Text = "キャンセル"
        BtnCancel.UseVisualStyleBackColor = True
        ' 
        ' LblAutoResult
        ' 
        LblAutoResult.AutoSize = True
        LblAutoResult.ForeColor = Color.DimGray
        LblAutoResult.Location = New Point(20, 49)
        LblAutoResult.Name = "LblAutoResult"
        LblAutoResult.Size = New Size(128, 15)
        LblAutoResult.TabIndex = 4
        LblAutoResult.Text = "※現在の自動判定結果:"
        ' 
        ' FrmAdvancedSettings
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(364, 161)
        Controls.Add(LblAutoResult)
        Controls.Add(BtnCancel)
        Controls.Add(BtnSave)
        Controls.Add(CmbEncoder)
        Controls.Add(Label1)
        ForeColor = SystemColors.ControlText
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "FrmAdvancedSettings"
        Text = "詳細設定"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents CmbEncoder As ComboBox
    Friend WithEvents BtnSave As Button
    Friend WithEvents BtnCancel As Button
    Friend WithEvents LblAutoResult As Label
End Class
