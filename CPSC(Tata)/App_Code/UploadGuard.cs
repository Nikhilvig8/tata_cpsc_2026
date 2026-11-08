using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace InputOutput
{
    // Checkmarx "Dangerous_File_Extension" (CWE-434) remediation.
    //
    // Uploaded files used to be written to disk with whatever content the client sent. These helpers
    // enforce, before anything touches the file system:
    //  - an allow-listed extension taken from the ORIGINAL upload name (not a caller-chosen suffix),
    //  - a matching file signature ("magic bytes") so e.g. an .aspx renamed to .jpg is rejected,
    //  - a size cap,
    //  - a server-generated file name (only [A-Za-z0-9_-]) inside a fixed target folder, so the
    //    request can never influence the directory or the final extension.
    public static class UploadGuard
    {
        private const int MaxImageBytes = 2 * 1024 * 1024;
        private const int MaxExcelBytes = 50 * 1024 * 1024;

        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] ExcelExtensions = { ".xlsx", ".xls" };

        public static bool IsAllowedImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0 || file.ContentLength > MaxImageBytes) return false;
            string ext = (Path.GetExtension(file.FileName) ?? string.Empty).ToLowerInvariant();
            if (!ImageExtensions.Contains(ext)) return false;

            byte[] head = ReadHead(file, 8);
            bool jpg = head.Length >= 3 && head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF;
            bool png = head.Length >= 8 && head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47
                       && head[4] == 0x0D && head[5] == 0x0A && head[6] == 0x1A && head[7] == 0x0A;
            return jpg || png;
        }

        public static bool IsAllowedExcel(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0 || file.ContentLength > MaxExcelBytes) return false;
            string ext = (Path.GetExtension(file.FileName) ?? string.Empty).ToLowerInvariant();
            if (!ExcelExtensions.Contains(ext)) return false;

            byte[] head = ReadHead(file, 8);
            // .xlsx = ZIP container ("PK\x03\x04"); legacy .xls = OLE2 compound file (D0 CF 11 E0 A1 B1 1A E1).
            bool zip = head.Length >= 4 && head[0] == 0x50 && head[1] == 0x4B && head[2] == 0x03 && head[3] == 0x04;
            bool ole = head.Length >= 8 && head[0] == 0xD0 && head[1] == 0xCF && head[2] == 0x11 && head[3] == 0xE0
                       && head[4] == 0xA1 && head[5] == 0xB1 && head[6] == 0x1A && head[7] == 0xE1;
            return (ext == ".xlsx" && zip) || (ext == ".xls" && ole);
        }

        // Saves an already-validated profile image as <safeName>.jpg in ~/assets/profile.
        public static bool SaveProfileImage(HttpPostedFileBase file, string baseName)
        {
            if (!IsAllowedImage(file)) return false;
            string safeName = SafeBaseName(baseName);
            if (safeName.Length == 0) return false;

            string folder = HttpContext.Current.Server.MapPath("~/assets/profile");
            Directory.CreateDirectory(folder);
            string target = Path.Combine(folder, safeName + ".jpg");
            if (!IsInside(folder, target)) return false;

            file.InputStream.Position = 0;
            using (var output = File.Create(target))
            {
                file.InputStream.CopyTo(output);
            }
            return true;
        }

        // Saves an already-validated Excel upload under ~/App_Data/Uploads with a random name and
        // returns the full path (caller deletes it when done). App_Data is never served by IIS.
        public static string SaveExcelToTemp(HttpPostedFileBase file)
        {
            if (!IsAllowedExcel(file)) return null;
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant() == ".xls" ? ".xls" : ".xlsx";

            string folder = HttpContext.Current.Server.MapPath("~/App_Data/Uploads");
            Directory.CreateDirectory(folder);
            string target = Path.Combine(folder, Guid.NewGuid().ToString("N") + ext);
            if (!IsInside(folder, target)) return null;

            file.InputStream.Position = 0;
            using (var output = File.Create(target))
            {
                file.InputStream.CopyTo(output);
            }
            return target;
        }

        public static string SafeBaseName(string value)
        {
            return Regex.Replace(value ?? string.Empty, @"[^A-Za-z0-9_\-]", string.Empty);
        }

        private static bool IsInside(string folder, string candidate)
        {
            string root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return Path.GetFullPath(candidate).StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        private static byte[] ReadHead(HttpPostedFileBase file, int count)
        {
            var buffer = new byte[count];
            Stream s = file.InputStream;
            s.Position = 0;
            int read = s.Read(buffer, 0, count);
            s.Position = 0;
            if (read < count) Array.Resize(ref buffer, read);
            return buffer;
        }
    }
}
