namespace Melodee.Blazor.Services;

public interface IAppVersionProvider
{
    string GetSemVerForDisplay();
}
