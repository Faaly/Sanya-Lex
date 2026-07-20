namespace SanyaLex.Database
{
    public class KnowledgeMatch{
        public string? FunctionName { get; set; }
        public string Content { get; set; } = null!;
        public string? ToolArgs { get; set; }
    }
}