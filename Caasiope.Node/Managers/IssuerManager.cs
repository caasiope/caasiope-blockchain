using System;
using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Managers
{
    public class IssuerManager
    {
        private readonly Dictionary<Currency, Address> issuers = new Dictionary<Currency, Address>();

        public void Initialize(List<Issuer> list)
        {
            foreach (var issuer in list)
            {
                // if(issuer.Currency == Currency.CAS) throw new ArgumentException($"{nameof(Currency.CAS)} cannot have issuer");
                // CreateAccount(issuer.Address);
                issuers.Add(issuer.Currency, issuer.Address);
            }

            // maybe not good
            // issuers[Currency.CAS] = null;

            // TODO check that every currency has an issuer
            // TODO check CAS issuer, needs to be timelock 0
        }

        // create the account for the issuer in memory at statup
        private void CreateAccount(Address address)
        {
            // LiveService.AccountManager.GetOrCreateAccount(address, () => new ExtendedAccount());
            // throw new Exception($"Issuer account {address.Encoded} does not exist !");
        }

        public bool IsIssuer(Currency currency, Address address)
        {
            return issuers[currency] == address;
        }

        public IEnumerable<Issuer> GetIssuers()
        {
            foreach (var issuer in issuers)
                yield return new Issuer(issuer.Value, issuer.Key);
        }
    }
}