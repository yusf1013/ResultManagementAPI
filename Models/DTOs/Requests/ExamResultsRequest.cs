namespace Models.DTOs.Requests
{
    public class ExamResultsRequest
    {
        public string StudentId { get; set; }
        public int ExamId { get; set; }

        public int ObtainedMark { get; set; }
    }
}