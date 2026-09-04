namespace QuizApp.Web.ViewModels;

public class StudentQuizListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CategoryName { get; set; }
    public int QuestionCount { get; set; }
    public int? LastScore { get; set; }
    public int? LastTotalQuestions { get; set; }
}

public class PlayQuestionViewModel
{
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionNumber { get; set; }
    public int TotalQuestions { get; set; }
    public List<PlayAnswerOption> Answers { get; set; } = new();
}

public class PlayAnswerOption
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class PlayResultViewModel
{
    public bool IsCorrect { get; set; }
    public string? CorrectAnswerText { get; set; }
    public int AttemptId { get; set; }
    public int QuestionNumber { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsLastQuestion { get; set; }
}

public class PlaySummaryViewModel
{
    public string QuizTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
}