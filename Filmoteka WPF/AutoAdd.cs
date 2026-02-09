using System.Collections.Generic;
using System.IO;

namespace Filmoteka_WPF
{
    internal static class AutoAdd
    {
        public static Film[] GetFilms(string RootFolder)
        {
            var filenames = GetFilmsFilename(RootFolder);
            var films = new Film[filenames.Length];
            for (var index = 0; index < filenames.Length; index++)
            {
                var info = new FileInfo(filenames[index]);
                films[index] = new Film(info.Name, filenames[index], new List<string>(), 0, string.Empty);
            }

            return films;
        }

        public static string[] GetFilmsFilename(string RootFolder)
        {
            var root = new DirectoryInfo(RootFolder);
            var subFolders = root.GetDirectories();
            var files = new List<string>();
            foreach (var file in root.GetFiles())
            {
                files.Add(file.Name);
            }

            foreach (var subdir in subFolders)
            {
                foreach (var info in subdir.GetFiles())
                {
                    files.Add(Path.Combine(subdir.Name, info.Name));
                }
            }

            return files.ToArray();
        }
    }
}
