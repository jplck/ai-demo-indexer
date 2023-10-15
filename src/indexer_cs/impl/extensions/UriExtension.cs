namespace Company.Function {
    public static class UriExtension {
        public static string GetFileEnding(this Uri uri) {
            var info = new FileInfo(uri.AbsolutePath);
            var ext = info.Extension;
            if (!string.IsNullOrWhiteSpace(ext) && ext.StartsWith(".")) {
                ext = ext[1..];
            }
            return ext;
        }
    }
}