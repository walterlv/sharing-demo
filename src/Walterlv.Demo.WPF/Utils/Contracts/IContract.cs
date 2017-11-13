namespace Walterlv.Demo.Contracts
{
    ///// <summary>Represents the base interface for all contracts that are used for communication between components that are updated independently.</summary>
    //public interface IContract
    //{
    //    /// <summary>Returns a contract that is implemented by this contract.</summary>
    //    /// <param name="contractIdentifier">A string that identifies the contract that is being requested.</param>
    //    /// <returns>An <see cref="T:System.AddIn.Contract.IContract" /> that represents a contract that a client is requesting from the current contract; <see langword="null" /> if the current contract does not support the contract that is requested.</returns>
    //    IContract QueryContract(string contractIdentifier);

    //    /// <summary>Returns a hash code for the <see cref="T:System.AddIn.Contract.IContract" />.</summary>
    //    /// <returns>A hash code for the <see cref="T:System.AddIn.Contract.IContract" />.</returns>
    //    int GetRemoteHashCode();

    //    /// <summary>Indicates whether the specified contract is equal to this <see cref="T:System.AddIn.Contract.IContract" />.</summary>
    //    /// <param name="contract">The contract to compare with this <see cref="T:System.AddIn.Contract.IContract" />.</param>
    //    /// <returns>
    //    /// <see langword="true" /> if <paramref name="contract" /> is equal to this <see cref="T:System.AddIn.Contract.IContract" />; otherwise, <see langword="false" />.</returns>
    //    bool RemoteEquals(IContract contract);

    //    /// <summary>Returns a string representation of the current <see cref="T:System.AddIn.Contract.IContract" />.</summary>
    //    /// <returns>A string representation of the current <see cref="T:System.AddIn.Contract.IContract" />.</returns>
    //    string RemoteToString();

    //    /// <summary>Specifies that the contract is accessible to a client until the client revokes the contract.</summary>
    //    /// <returns>A value, also known as a lifetime token, that identifies the client that has acquired the contract.</returns>
    //    int AcquireLifetimeToken();

    //    /// <summary>Specifies that the contract is no longer accessible to a client.</summary>
    //    /// <param name="token">A value, also known as a lifetime token, that identifies the client that is revoking the contract.</param>
    //    void RevokeLifetimeToken(int token);
    //}
}
