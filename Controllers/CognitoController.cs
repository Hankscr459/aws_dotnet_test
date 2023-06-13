using System.Net;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using Dtos.cognito;
using Microsoft.AspNetCore.Mvc;

namespace aws_test.Controllers;

[ApiController]
[Route("[controller]")]
public class CognitoController : ControllerBase
{
    private readonly ILogger<CognitoController> _logger;
    private readonly IAmazonCognitoIdentityProvider _cognitoService;
    private readonly CognitoUserPool _pool;

    public CognitoController(ILogger<CognitoController> logger)
    {
        _logger = logger;
        var credential = new AnonymousAWSCredentials();
        _cognitoService = new AmazonCognitoIdentityProviderClient(credential, RegionEndpoint.APSoutheast2);
        string ClientId = Environment.GetEnvironmentVariable("AWS_Cognito_ClientId");
        string poolId = Environment.GetEnvironmentVariable("AWS_Pool_Id");
        _pool = new CognitoUserPool(
            poolId,
            ClientId,
            _cognitoService
        );
    }

    [HttpPost("aws/signup")]
    public async Task<object> SignUpAsync(SignupInput signup)
    {
        var userAttrsList = new List<AttributeType>(){
            new AttributeType
            {
                Name = "email",
                Value = signup.Email,
            },
            new AttributeType
            {
                Name = "given_name",
                Value = signup.GivenName,
            },
            new AttributeType
            {
                Name = "family_name",
                Value = signup.FamilyName,
            },
            new AttributeType
            {
                Name = "address",
                Value = signup.Address,
            },
        };

        var signUpRequest = new SignUpRequest
        {
            UserAttributes = userAttrsList,
            Username = signup.Email,
            ClientId = Environment.GetEnvironmentVariable("AWS_Cognito_ClientId"),
            Password = signup.Password
        };

        var res = await _cognitoService.SignUpAsync(signUpRequest);
        return new { success = true };
    }

    [HttpPost("aws/signup/comfirm")]
    public async Task<Object> ConfirmCode(ConfirmCodeInput input)
    {
        string ClientId = Environment.GetEnvironmentVariable("AWS_Cognito_ClientId");
        var signUpRequest = new ConfirmSignUpRequest
        {
            ClientId = ClientId,
            ConfirmationCode = input.Code,
            Username = input.Email,
        };

        var response = await _cognitoService.ConfirmSignUpAsync(signUpRequest);
        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            Console.WriteLine($"{input.Email} was confirmed");
            return new { success = false };
        }
        return new { success = true };
    }

    [HttpPost("aws/signin")]
    public async Task<object> SignInAsync(SignInInput input)
    {
        string ClientId = Environment.GetEnvironmentVariable("AWS_Cognito_ClientId");
        CognitoUser user = new CognitoUser(
            input.Email,
            ClientId,
            _pool,
            _cognitoService
        );
        InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
        {
            Password = input.Password
        };

        AuthFlowResponse authResponse = await user.StartWithSrpAuthAsync(authRequest);
        var accessToken = authResponse.AuthenticationResult.AccessToken;
        return new { success = true, data = authResponse };
    }
}
