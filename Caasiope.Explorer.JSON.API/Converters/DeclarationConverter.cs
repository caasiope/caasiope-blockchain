using System;
using Caasiope.Protocol.Types;
using Newtonsoft.Json.Linq;
using TxDeclaration = Caasiope.Explorer.JSON.API.Internals.TxDeclaration;

namespace Caasiope.Explorer.JSON.API.Converters
{
    public class DeclarationConverter : JsonCreationConverter<TxDeclaration>
    {
        protected override TxDeclaration Create(Type objectType, JObject jsonObject)
        {
            var typeName = (DeclarationType)Enum.Parse(typeof(DeclarationType), jsonObject["Type"].ToString());
            switch (typeName)
            {
                case DeclarationType.MultiSignature:
                    return new Internals.MultiSignature();
                case DeclarationType.HashLock:
                    return new Internals.HashLock();
                case DeclarationType.Secret:
                    return new Internals.SecretRevelation();
                case DeclarationType.TimeLock:
                    return new Internals.TimeLock();
                case DeclarationType.VendingMachine:
                    return new Internals.VendingMachine();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}