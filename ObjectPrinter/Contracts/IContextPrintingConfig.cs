using System;

namespace ObjectPrinter.Contracts
{
    public interface IContextPrintingConfig<TOwner, TContext>
    {
        public PrintingConfig<TOwner> Using(Func<TContext, string> print);
    }
}