Imports OpenCvSharp

Public Interface ICameraProvider
    ' 映像の1フレームがカメラから届いた時に発生するイベント
    Event FrameArrived(ByVal frame As Mat)

    ' カメラの開始処理（引数 deviceIndex は選ばれたカメラの番号）
    Function StartCamera(ByVal deviceIndex As Integer) As Boolean

    ' カメラの停止処理
    Sub StopCamera()

    ' 現在カメラが動作中かどうかを返すプロパティ
    ReadOnly Property IsRunning As Boolean
End Interface
