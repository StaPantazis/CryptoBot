namespace Cryptobot.Tests.Builders.Bases;

public abstract class BaseBuilder<TObject, TBuilder> where TBuilder : BaseBuilder<TObject, TBuilder>
{
    protected TObject _object;

    public virtual TObject Build() => _object;

    public static implicit operator TObject(BaseBuilder<TObject, TBuilder> builder) => builder.Build();
}
