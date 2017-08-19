﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using Gallery.DataLayer.Base;
using Gallery.DataLayer.Entities;
using Gallery.DataLayer.Entities.Base;
using Microsoft.Owin.Security;

namespace Gallery.Managers
{
    public class AuthenticationManager
    {
        private readonly IContext _context;

        public AuthenticationManager(IContext context)
        {
            _context = context;
        }

        public const string APPLICATION_COKIE_AUTH = "ApplicationCookie";

        public bool Authenticate(User user, IAuthenticationManager manager, bool isPersistent)
        {
            var claims = new[]
                         {
                             new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                             new Claim(ClaimTypes.Email, user.Email),
                             new Claim(ClaimTypes.Name, user.FullName),
                             new Claim(ClaimTypes.Role, user.Role.ToString())
                         };

            var identity = new ClaimsIdentity(claims, APPLICATION_COKIE_AUTH);

            manager.SignIn(new AuthenticationProperties
                           {
                               IsPersistent = isPersistent
                           }, identity);

            return true;
        }

        public bool SingOut(IAuthenticationManager manager)
        {
            manager.SignOut(APPLICATION_COKIE_AUTH);
            return true;
        }

        public bool HasAccess<T>(int userId, T resource, Operation operation, object data = null) where T : IEntity
        {
            var user = _context.GetById<User>(userId);

            return resource.HasAccess(user, operation, data);
        }
    }

    public static class AuthenitcationManagerEx
    {
        public static int GetUserId(this HttpRequestMessage request)
        {
            var context = request.GetOwinContext();
            var user = context.Authentication.User;
            var claimIdentity = user.FindFirst(ClaimTypes.NameIdentifier);
            int userId;

            if (claimIdentity == null || !int.TryParse(claimIdentity.Value, out userId))
                return -1;

            return userId;
        }
    }
}