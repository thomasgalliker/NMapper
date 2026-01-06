namespace NMapper.Internals
{
    internal interface IFastInvoker
    {
        MappingResult Invoke(object? source, MappingContext context);
    }
}
