using System.Collections.Generic;
using System.IO;

namespace Filmoteka_WPF
{
    internal static class AutoAdd
    {
        public static Film[] GetFilms(string RootFolder)
        {
            string[] filenames = GetFilmsFilename(RootFolder);
            Film[] films = new Film[filenames.Length];
            for (int index = 0; index < filenames.Length; index++)
            {
                FileInfo info = new FileInfo(filenames[index]);
                films[index] = new Film(info.Name, filenames[index], new List<string>(), 0, string.Empty);
            }

            return films;
        }

        public static string[] GetFilmsFilename(string RootFolder)
        {
            DirectoryInfo root = new DirectoryInfo(RootFolder);
            DirectoryInfo[] subFolders = root.GetDirectories();
            List<string> files = new List<string>();
            foreach (FileInfo file in root.GetFiles())
            {
                files.Add(file.Name);
            }


            foreach (DirectoryInfo subdir in subFolders)
            {
                foreach (FileInfo info in subdir.GetFiles())
                {
                    string path = subdir.Name + @"\" + info.Name;
                    files.Add(path);
                }
            }

            return files.ToArray();
        }
    }
}
