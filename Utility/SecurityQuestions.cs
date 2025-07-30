// Utilities/SecurityQuestions.cs
namespace InventorySolution.Utility
{
    public static class SecurityQuestions
    {
        public static List<(int Id, string Question)> Questions = new()
        {
            (1, "What is your pet's name?"),
            (2, "What is your favorite book?")
        };

        public static string GetQuestionText(int id) =>
            Questions.FirstOrDefault(q => q.Id == id).Question;
    }
}