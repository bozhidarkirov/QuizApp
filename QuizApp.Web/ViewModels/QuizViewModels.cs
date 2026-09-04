using System.ComponentModel.DataAnnotations;

namespace QuizApp.Web.ViewModels;

public class QuizListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int QuestionCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
}

public class CreateQuizViewModel
{
    [Required(ErrorMessage = "Заглавието е задължително")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Избери категория")]
    public int CategoryId { get; set; }

    [Range(5, 120)]
    public int TimePerQuestion { get; set; } = 20;

    public List<CategoryOption> Categories { get; set; } = new();
}

public class CategoryOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateQuestionViewModel
{
    public int QuizId { get; set; }

    [Required(ErrorMessage = "Текстът на въпроса е задължителен")]
    [StringLength(500)]
    public string Text { get; set; } = string.Empty;

    [Range(1, 300)]
    public int? TimeLimitSeconds { get; set; }

    [Range(1, 100)]
    public int Points { get; set; } = 10;

    [Required(ErrorMessage = "Попълни отговор 1")]
    [StringLength(200)]
    public string Answer1 { get; set; } = string.Empty;

    [Required(ErrorMessage = "Попълни отговор 2")]
    [StringLength(200)]
    public string Answer2 { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Answer3 { get; set; }

    [StringLength(200)]
    public string? Answer4 { get; set; }

    [Required(ErrorMessage = "Избери верен отговор")]
    [Range(1, 4, ErrorMessage = "Избери кой отговор е верен")]
    public int CorrectAnswerIndex { get; set; }
}