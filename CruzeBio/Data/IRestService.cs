using System.Collections.Generic;
using System.Threading.Tasks;
using CruzeBio.Models;

namespace CruzeBio.Data
{
	public interface IRestService
	{
		Task<LoginResponse> LoginAsync();

		Task <IdentifyResponse> IdentifyAsync(IdentifyRequest request);

	}
}