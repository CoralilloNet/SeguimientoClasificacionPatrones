namespace SeguimientoTareas.Web.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Specialist
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TaskTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TaskStageTemplate> Stages { get; set; } = new List<TaskStageTemplate>();
    }

    public class TaskStageTemplate
    {
        public int Id { get; set; }
        public int TaskTemplateId { get; set; }
        public int Ordinal { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationDays { get; set; }
    }

    public class Assignment
    {
        public int Id { get; set; }
        public int TaskTemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int AssignedToUserId { get; set; }
        public int AssignedByUserId { get; set; }
        public int? SpecialistId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        
        // Navigation properties
        public User? AssignedToUser { get; set; }
        public User? AssignedByUser { get; set; }
        public Specialist? Specialist { get; set; }
        public TaskTemplate? TaskTemplate { get; set; }
        public List<AssignmentStage> Stages { get; set; } = new List<AssignmentStage>();
    }

    public class AssignmentStage
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int Ordinal { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationDays { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public int ProgressPercent { get; set; }
        public bool IsComplete { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public List<StageEvidence> Evidences { get; set; } = new List<StageEvidence>();
    }

    public class StageEvidence
    {
        public int Id { get; set; }
        public int AssignmentStageId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int UploadedByUserId { get; set; }
        public DateTime UploadedAt { get; set; }
        
        public User? UploadedByUser { get; set; }
    }

    // DTOs for API responses
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class ApiResponse : ApiResponse<object>
    {
    }

    public class DashboardStats
    {
        public int TotalTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public List<UserTaskSummary> UserSummaries { get; set; } = new List<UserTaskSummary>();
    }

    public class UserTaskSummary
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double AverageProgress { get; set; }
    }

    public class AssignmentDetails
    {
        public Assignment Assignment { get; set; } = new Assignment();
        public double OverallProgress { get; set; }
        public bool IsOverdue { get; set; }
    }
}