using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;



namespace onGuardManager.WebAPI.Common
{
	public static class AwsProperties
	{
		private const string secretName = "rds!db-4182f910-a337-4f22-a356-d04f43aa9623";
		private const string region = "us-east-1";


		public static async Task GetSecret()
		{
			IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));


			GetSecretValueRequest request = new GetSecretValueRequest
			{
				SecretId = secretName,
				VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
			};

			GetSecretValueResponse response;

			try
			{
				response = await client.GetSecretValueAsync(request);
			}
			catch (Exception e)
			{
				// For a list of the exceptions thrown, see
				// https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
				throw e;
			}

			string secret = response.SecretString;

			// Your code goes here
		}
	}
}
