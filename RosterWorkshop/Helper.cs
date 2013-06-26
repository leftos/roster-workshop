#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

#region Using Directives

#endregion

namespace RosterWorkshop
{
    #region Using Directives

    using System;
    using System.Windows;

    using Microsoft.Win32;

    #endregion

    public static class Helper
    {
        public static void SetRegistrySetting<T>(string setting, T value)
        {
            var rk = Registry.CurrentUser;
            try
            {
                try
                {
                    rk = rk.OpenSubKey(App.AppRegistryKey, true);
                    if (rk == null)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(App.AppRegistryKey);
                    rk = rk.OpenSubKey(App.AppRegistryKey, true);
                    if (rk == null)
                    {
                        throw new Exception();
                    }
                }

                rk.SetValue(setting, value);
            }
            catch
            {
                MessageBox.Show("Couldn't save changed setting.");
            }
        }

        public static T GetRegistrySetting<T>(string setting, T defaultValue)
        {
            var rk = Registry.CurrentUser;
            var settingValue = defaultValue;
            try
            {
                if (rk == null)
                {
                    throw new Exception();
                }

                rk = rk.OpenSubKey(App.AppRegistryKey);
                if (rk != null)
                {
                    settingValue = (T) Convert.ChangeType(rk.GetValue(setting, defaultValue), typeof(T));
                }
            }
            catch
            {
                settingValue = defaultValue;
            }

            return settingValue;
        }
    }
}