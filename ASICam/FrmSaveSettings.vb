Public Class FrmSaveSettings

    ' 画面が開いたときに、記憶されている値を読み込む
    Private Sub FrmSaveSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim savedPath As String = My.Settings.RecordSavePath

        ' 初回起動時などで何も記憶されていない場合は、ビデオフォルダ内の「ASI_Record」をセット
        If String.IsNullOrEmpty(savedPath) Then
            Dim videoPath As String = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
            savedPath = System.IO.Path.Combine(videoPath, "ASI_Record")
        End If

        TxtPath.Text = savedPath

        ' 入力できる範囲を「1〜7」に完全にロックする
        NumRetentionDays.Minimum = 1
        NumRetentionDays.Maximum = 7

        ' 保存日数の読み込みと安全ガード
        Dim days As Integer = My.Settings.RetentionDays
        If days < 1 OrElse days > 7 Then days = 7
        NumRetentionDays.Value = days

        ' ==========================================================
        ' 💡【追加】分割時間の選択肢をセットして、記憶を復元する
        ' ==========================================================
        CmbSplitTime.Items.Clear()
        Dim timeList As Integer() = {1, 5, 10, 20, 30, 40, 50, 60}
        For Each t In timeList
            CmbSplitTime.Items.Add(t.ToString())
        Next

        ' 記憶されている値をセット（見つからなければデフォルトの10分）
        Dim splitTime As String = My.Settings.RecordSplitTime.ToString()
        If CmbSplitTime.Items.Contains(splitTime) Then
            CmbSplitTime.SelectedItem = splitTime
        Else
            CmbSplitTime.SelectedItem = "10"
        End If
    End Sub

    Private Sub BtnBrowse_Click(sender As Object, e As EventArgs) Handles BtnBrowse.Click
        Using fbd As New FolderBrowserDialog()
            fbd.Description = "録画ファイルの保存先フォルダを選択してください。"
            fbd.SelectedPath = TxtPath.Text

            If fbd.ShowDialog() = DialogResult.OK Then
                TxtPath.Text = fbd.SelectedPath
            End If
        End Using
    End Sub

    Private Sub BtnSave_Click(sender As Object, e As EventArgs) Handles BtnSave.Click
        ' 💡 ここで全部まとめて My.Settings に書き込んで、PCに記憶させる！
        My.Settings.RecordSavePath = TxtPath.Text
        My.Settings.RetentionDays = CInt(NumRetentionDays.Value)

        ' 分割時間も記憶させる
        If CmbSplitTime.SelectedIndex <> -1 Then
            My.Settings.RecordSplitTime = Integer.Parse(CmbSplitTime.SelectedItem.ToString())
        End If

        My.Settings.Save()

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As EventArgs) Handles BtnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class