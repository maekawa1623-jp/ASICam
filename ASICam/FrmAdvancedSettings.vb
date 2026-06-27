Public Class FrmAdvancedSettings

    Private Sub FrmAdvancedSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 記憶されているエンコーダ設定（0〜4）を読み込んで選択状態にする
        Dim pref As Integer = My.Settings.EncoderPreference
        If pref >= 0 AndAlso pref < CmbEncoder.Items.Count Then
            CmbEncoder.SelectedIndex = pref
        Else
            CmbEncoder.SelectedIndex = 0
        End If

        ' 初期表示時にラベルの更新を呼び出す
        UpdateAutoResultLabel()
    End Sub

    ' 💡【追加】コンボボックスの選択が変更されたとき
    Private Sub CmbEncoder_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CmbEncoder.SelectedIndexChanged
        UpdateAutoResultLabel()
    End Sub

    ' 💡【改修】ラベルの表示とUIの整合性を合わせる処理
    Private Sub UpdateAutoResultLabel()
        ' まだチェックされていなければ調べる
        If Not BasFunction._isGpuChecked Then BasFunction.CheckGpuCapabilities()

        LblAutoResult.Visible = True ' 常に何かしら表示する

        Select Case CmbEncoder.SelectedIndex
            Case 0 ' 自動判定
                Dim detectedName As String = BasFunction.GetAutoDetectedEncoderName()
                LblAutoResult.Text = $"※現在の自動判定結果: {detectedName}"
                LblAutoResult.ForeColor = Color.DimGray

            Case 1 ' Intel QSV
                If BasFunction._hasIntel Then
                    LblAutoResult.Text = "✅ このPCで利用可能です"
                    LblAutoResult.ForeColor = Color.DarkGreen
                Else
                    LblAutoResult.Text = "⚠️ Intel GPU未検出。自動でソフトウェア処理に切り替わります。"
                    LblAutoResult.ForeColor = Color.Red
                End If

            Case 2 ' NVIDIA NVENC
                If BasFunction._hasNvidia Then
                    LblAutoResult.Text = "✅ このPCで利用可能です"
                    LblAutoResult.ForeColor = Color.DarkGreen
                Else
                    LblAutoResult.Text = "⚠️ NVIDIA GPU未検出。自動でソフトウェア処理に切り替わります。"
                    LblAutoResult.ForeColor = Color.Red
                End If

            Case 3 ' AMD AMF
                If BasFunction._hasAmd Then
                    LblAutoResult.Text = "✅ このPCで利用可能です"
                    LblAutoResult.ForeColor = Color.DarkGreen
                Else
                    LblAutoResult.Text = "⚠️ AMD GPU未検出。自動でソフトウェア処理に切り替わります。"
                    LblAutoResult.ForeColor = Color.Red
                End If

            Case 4 ' ソフトウェア
                LblAutoResult.Text = "※CPUに高い負荷がかかるため発熱にご注意ください。"
                LblAutoResult.ForeColor = Color.DimGray
        End Select
    End Sub

    Private Sub BtnSave_Click(sender As Object, e As EventArgs) Handles BtnSave.Click
        ' 選択された設定を記憶する
        My.Settings.EncoderPreference = CmbEncoder.SelectedIndex
        My.Settings.Save()

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As EventArgs) Handles BtnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class