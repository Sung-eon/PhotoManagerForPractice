namespace PhotoManager.Shared.Components;

public interface IPageable
{
    public void MoveNext();
    public void MovePrevious();
    public void MoveFirst();
}