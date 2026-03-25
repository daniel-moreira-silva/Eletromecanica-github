namespace Business.Dtos;

public class SetDocumentoTagRequest
{
    public List<Guid> TagIds { get; set; } = default!;
}
