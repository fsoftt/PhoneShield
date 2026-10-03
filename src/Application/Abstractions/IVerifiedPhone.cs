namespace Tranqui.Application.Abstractions;

/// <summary>The phone number Firebase verified by SMS for the current request (appeals only). Never client-supplied.</summary>
public interface IVerifiedPhone
{
    string E164 { get; }
}
