using Microsoft.Extensions.Configuration;
namespace BrewNest.Modules.Returns.Application.Interfaces;
internal interface IReturnsDocumentNumbers{string Return();string Rma();string Credit();string Void();}
internal sealed class ReturnsDocumentNumbers(IConfiguration c):IReturnsDocumentNumbers{private long n=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();private string Next(string key,string fallback)=>$"{c[$"Returns:DocumentNumbers:{key}"]??fallback}-{Interlocked.Increment(ref n)}";public string Return()=>Next("ReturnPrefix","RET");public string Rma()=>Next("RmaPrefix","RMA");public string Credit()=>Next("CreditPrefix","CN");public string Void()=>Next("VoidPrefix","VOID");}
