namespace Melodee.Common.Models;

public enum OperationResponseType
{
    NotSet = 0,

    Unauthorized = 401,

    AccessDenied = 403,

    Forbidden = 403,

    Error = 500,

    NotFound = 404,

    Ok = 200,

    ValidationFailure = 400,

    BadRequest = 400,

    Conflict = 409,

    NotImplementedOrDisabled = 501
}
