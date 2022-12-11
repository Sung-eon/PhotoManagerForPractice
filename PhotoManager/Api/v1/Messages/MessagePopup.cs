namespace PhotoManager.Api.v1.Messages;

public enum Severity
{
    zero,
    low, 
    normal, 
    high
}

public class MessagePopup
{
    public Severity _severity { get; set; }
    public string _message { get; set; } = String.Empty;

    public async void SetMessage(string msg, Severity severity = Severity.zero, int DelayTime = 5000)
    {
        this._message = msg;
        this._severity = severity;

        await Task.Delay(DelayTime);

        this.ClearMessage();
    }

    public void ClearMessage() {
        this._message = String.Empty;
        this._severity = Severity.zero;
    }

}