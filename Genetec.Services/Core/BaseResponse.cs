namespace Genetec.Services.Core;

public record BaseResponse<T>
{
    public T Result { get; set; } = default!;
}