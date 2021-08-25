using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using _50P.Software.Settings.Dialogs;

namespace Filmoteka_WPF
{
    class Settings : _50P.Software.Settings.Dialogs.Settings
    {
        [UserScopedSetting()]
        [DefaultSettingValue(null)]
        public string Folder
        {
            get
            {
                return (string)this["Folder"];
            }
            set
            {
                this["Folder"] = (string)value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("false")]
        public bool AutoAdd
        {
            get
            {
                return (bool)this["AutoAdd"];
            }
            set
            {
                this["AutoAdd"] = (bool)value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("true")]
        public bool TryMode
        {
            get
            {
                return (bool)this["TryMode"];
            }
            set
            {
                this["TryMode"] = (bool)value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("false")]
        public bool AllowExport
        {
            get
            {
                return (bool)this["AllowExport"];
            }
            set
            {
                this["AllowExport"] = (bool)value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue(null)]
        public string ExportFilename
        {
            get
            {
                return (string)this["ExportFilename"];
            }
            set
            {
                this["ExportFilename"] = (string)value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("true")]
        public bool FirstTime
        {
            get
            {
                return (bool)this["FirstTime"];
            }
            set
            {
                this["FirstTime"] = (bool)value;
            }
        }
    }   

    class ForTheFirstTime : SettingsBase
    {
        [UserScopedSetting()]
        [DefaultSettingValue("true")]
        public bool FirstTime
        {
            get
            {
                return (bool)this["FirstTime"];
            }
            set
            {
                this["FirstTime"] = (bool)value;
            }
        }
    }
}
