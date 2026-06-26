<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmPreview
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
        PicPreview = New PictureBox()
        CType(PicPreview, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' PicPreview
        ' 
        PicPreview.Dock = DockStyle.Fill
        PicPreview.Location = New Point(0, 0)
        PicPreview.Name = "PicPreview"
        PicPreview.Size = New Size(640, 360)
        PicPreview.SizeMode = PictureBoxSizeMode.Zoom
        PicPreview.TabIndex = 0
        PicPreview.TabStop = False
        ' 
        ' FrmPreview
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(640, 360)
        Controls.Add(PicPreview)
        MinimizeBox = False
        MinimumSize = New Size(656, 399)
        Name = "FrmPreview"
        Text = "映像プレビュー"
        CType(PicPreview, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents PicPreview As PictureBox
End Class
