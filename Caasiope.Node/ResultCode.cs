namespace Caasiope.Node
{
    public enum ResultCode : byte
    {
        Success = Helios.JSON.ResultCode.Success,
        Failed = Helios.JSON.ResultCode.Failed,
        UnknownMessage = Helios.JSON.ResultCode.UnknownMessage,

        InvalidInputParam = 16,
        UnknownAccount = 17,
        TransactionValidationFailed = 18,
        CannotReadSignedTransaction = 19,
        CannotReadSignedLedger = 20,
        LedgerDoesnotExist = 21,

        NotABase64 = 22,
        NotAHash256 = 23,
        ReadAddressFailure = 24,
    }
}