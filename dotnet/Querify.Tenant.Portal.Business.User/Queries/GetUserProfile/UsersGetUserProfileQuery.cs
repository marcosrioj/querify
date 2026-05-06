using Querify.Models.User.Dtos.User;
using MediatR;

namespace Querify.Tenant.Portal.Business.User.Queries.GetUserProfile;

public sealed class UsersGetUserProfileQuery : IRequest<UserProfileDto?>;