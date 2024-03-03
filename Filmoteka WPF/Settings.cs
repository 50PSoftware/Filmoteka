using System.Configuration;

namespace Filmoteka_WPF
{
    internal class Settings : _50P.Software.Settings.Dialogs.Settings
    {
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
        [DefaultSettingValue(null)]
        public string ExportFileExtension
        {
            get
            {
                return (string)this["ExportFileExtension"];
            }
            set
            {
                this["ExportFileExtension"] = (string)value;
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
    }
}
