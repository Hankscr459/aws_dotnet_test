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

    public CognitoController(ILogger<CognitoController> logger)
    {
        _logger = logger;
        var credential = new AnonymousAWSCredentials();
        _cognitoService = new AmazonCognitoIdentityProviderClient(credential, RegionEndpoint.APSoutheast2);
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

    [HttpPost("aws/signin")]
    public async Task<object> SignInAsync(SignInInput input)
    {
        string ClientId = Environment.GetEnvironmentVariable("AWS_Cognito_ClientId");
        string poolId = Environment.GetEnvironmentVariable("AWS_Pool_Id");
        CognitoUserPool pool = new CognitoUserPool(
            poolId,
            ClientId,
            _cognitoService
        );
        CognitoUser user = new CognitoUser(
            input.Email,
            ClientId,
            pool,
            _cognitoService
        );
        InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
        {
            Password = input.Password
        };

        AuthFlowResponse authResponse = await user.StartWithSrpAuthAsync(authRequest);

        return new { success = true, data = authResponse };
    }
}
