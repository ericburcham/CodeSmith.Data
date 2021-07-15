namespace CodeSmith.Data
{
    public interface ILinqEntity
    {
        void Detach();

        bool IsAttached();

        string ToEntityString(int indentLevel, string indentValue);
    }
}
