using System.ComponentModel.DataAnnotations;

namespace QuizApp.Web.ViewModels;

public class CategoryListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int QuizCount { get; set; }
}

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Името е задължително")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}