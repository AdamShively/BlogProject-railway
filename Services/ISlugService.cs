namespace BlogProject.Services
{
    public interface ISlugService
    {
        string UrlUsable(string title);
        bool slugExists(string slug);

    }
}
