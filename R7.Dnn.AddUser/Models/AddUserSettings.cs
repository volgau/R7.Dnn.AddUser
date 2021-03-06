﻿//
// AddUserSettings.cs
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
using System.Linq;
using DotNetNuke.Entities.Modules.Settings;

namespace R7.Dnn.AddUser.Models
{
    /// <summary>
    /// Provides strong typed access to settings used by module
    /// </summary>
    public class AddUserSettings
    {
        [ModuleSetting (Prefix = "AddUser_")]
        public string DisplayNameFormat { get; set; } = "[FIRSTNAME] [O] [LASTNAME]";

        [ModuleSetting (Prefix = "AddUser_")]
        public string UserNameFormat { get; set; } = "[LASTNAME]_[F][O]";

        [ModuleSetting (Prefix = "AddUser_")]
        public int? DesiredPasswordLength { get; set; }

        [ModuleSetting (Prefix = "AddUser_")]
        public string AllowedSpecialChars { get; set; }

        [ModuleSetting (Prefix = "AddUser_")]
        public string DoneUrl { get; set; }

        [ModuleSetting (Prefix = "AddUser_")]
        public bool DoneUrlOpenInPopup { get; set; }

        [ModuleSetting (Prefix = "AddUser_")]
        public string Roles { get; set; }

        public IEnumerable<int> RoleIds =>
            (Roles ?? string.Empty)
                .Split (";".ToCharArray (), StringSplitOptions.RemoveEmptyEntries)
                .Select (strRoleId => int.Parse (strRoleId));
    }
}
