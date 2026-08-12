namespace Trading.DTO.Models
{
    public record ValidationResultDTO( 
        bool IsValid, 
        string? RejectionReason
    );
}
