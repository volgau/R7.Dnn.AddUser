//
// AddUserManager.cs
//
// Author:
//       Roman M. Yagodin <roman.yagodin@gmail.com>
//
// Copyright (c) 2018 Roman M. Yagodin
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using R7.Dnn.AddUser.Models;

namespace R7.Dnn.AddUser.Components
{
    class AddUserManager
    {
        public INameFormatter NameFormatter { get; set; }

        public IPasswordGenerator PasswordGenerator { get; set; }

        public AddUserManager (INameFormatter nameFormatter, IPasswordGenerator passwordGenerator)
        {
            NameFormatter = nameFormatter;
            PasswordGenerator = passwordGenerator;
        }

        public AddUserResult AddUser (HumanName name,
                                      AddUserSettings settings,
                                      string email, bool useEmailAsUserName, int portalId)
        {
            var userName = NameFormatter.FormatUserName (settings.UserNameFormat, name, email, useEmailAsUserName);
            var user = new UserInfo {
                FirstName = name.FirstName,
                LastName = name.LastName,
                DisplayName = NameFormatter.FormatDisplayName (GetDisplayNameFormat (settings, PortalSettings.Current), name, userName),
                Email = email,
                Username = userName,
                PortalID = portalId
            };

            var password = GeneratePassword (settings.DesiredPasswordLength,
                                             MembershipProviderConfig.MinNonAlphanumericCharacters,
                                             settings.AllowedSpecialChars);
            
            user.Membership.Password = password;
            
            UserCreateStatus userCreateStatus = UserController.CreateUser (ref user);

            if (userCreateStatus == UserCreateStatus.Success) {
                UpdateUserProfile (user, name, portalId);
            
                try {
                    AssignUserToRoles (user, settings.RoleIds, portalId);
                }
                catch (Exception ex) {
                    userCreateStatus = UserCreateStatus.UnexpectedError;
                    Exceptions.LogException (new SecurityException ("Cannot assign user to roles", ex));
                }
            }
            
            return new AddUserResult {
                UserCreateStatus = userCreateStatus,
                User = user,
                Password = password
            };
        }

        void UpdateUserProfile (UserInfo user, HumanName name, int portalId)
        {
            user.Profile.InitialiseProfile (portalId);
            user.Profile.FirstName = name.FirstName;
            user.Profile.LastName = name.LastName;
            UserController.UpdateUser (portalId, user);
        }

        string GetDisplayNameFormat (AddUserSettings settings, PortalSettings portalSettings)
        {
            return !string.IsNullOrEmpty (settings.DisplayNameFormat)
                          ? settings.DisplayNameFormat
                              : portalSettings.Registration.DisplayNameFormat;
        }

        string GeneratePassword (int? desiredPasswordLength, int? minSpecialChars, string allowedSpecialChars)
        {
            var password = PasswordGenerator.GeneratePassword (desiredPasswordLength);
            var passwordSimplifier = new PasswordSimplifier ();

            if (!string.IsNullOrEmpty (allowedSpecialChars)) {
                password = passwordSimplifier.ReduceVarietyOfSpecialChars (password, allowedSpecialChars);
            }

            if (minSpecialChars != null) {
                password = passwordSimplifier.MinifyNumberOfSpecialChars (password, minSpecialChars.Value);
            }

            return password;
        }

        void AssignUserToRoles (UserInfo user, IEnumerable<int> roleIds, int portalId)
        {
            foreach (var roleId in roleIds) {
                var role = RoleController.Instance.GetRoleById (portalId, roleId);
                if (role != null) {
                    RoleController.Instance.AddUserRole (portalId, user.UserID, role.RoleID,
                                                         RoleStatus.Approved, false,
                                                         DateTime.MinValue, DateTime.MinValue);
                }
            }
        }
    }
}
