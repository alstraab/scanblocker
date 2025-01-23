namespace Alstra.ScanBlockPlugin.Config
{
    /// <summary>
    /// Default values for <see cref="ScanBlockFeatureConfig"/>"/>
    /// </summary>
    public static class ScanBlockFeatureConfigDefaults
    {
        /// <summary>
        /// Path to list host scores
        /// </summary>
        public static readonly string HostScoreListingPath = "/scanblock/hosts";

        /// <summary>
        /// Key to Items on request to prevent multiple scans on the same request
        /// </summary>
        public static readonly string RequestItemKey = "ScanBlockFeature";

        /// <summary>
        /// Hosts with score greater or equal to this threshold will be blocked
        /// </summary>
        public const ushort BlockScoreThreshold = 20;

        /// <summary>
        /// Score for matching full paths in <see cref="ForbiddenFullPaths"/>
        /// </summary>
        public const ushort ForbiddenFullPathsScore = 10;

        /// <summary>
        /// Score for matching partial paths in <see cref="ForbiddenPartialPaths"/>
        /// </summary>
        public const ushort ForbiddenPartialPathsScore = 5;

        /// <summary>
        /// Score for matching file endings in <see cref="BadFileEndings"/>
        /// </summary>
        public const ushort BadFileEndingsScore = 1;

        /// <summary>
        /// Score for matching partial paths in <see cref="BadPartialPaths"/>
        /// </summary>
        public const ushort BadPartialPathsScore = 1;

        /// <summary>
        /// Lowercase hosts that can never be banned
        /// </summary>
        public static readonly string[] PermanentlyAllowedHosts = new string[]
        {
            "localhost", "127.0.0.1", "::1"
        };

        /// <summary>
        /// Lowercase full paths that will severely increase the ban score of the request host
        /// </summary>
        public static readonly string[] ForbiddenFullPaths = new string[]
        {
            // Php
            "/index.php", "/wp-login.php", "/xmlrpc.php", "/wp-cron.php", "/wp-config.php",
            "/wp-admin/admin-ajax.php", "/wp-admin/admin-post.php", "/admin/post.php", "/admin/login.php",

            // Git
            "/.gitignore", "/.dockerignore", "/.gitattributes", "/.gitmodules",
            "/.git/head", "/.git/config", "/.git/index", "/.git/logs/head", "/.git/logs/refs/heads/master",

            // GitHub
            "/.github/workflows", "/.github/issue_template", "/.github/pull_request_template",

            // Env
            "/.env", "/.env.example", "/.env.local", "/.env.development", "/.env.test", "/.env.production",

            // Config
            "/.config/aspnetcore-runtime-config.json", "/.config/aspnetcore-runtime-config.xml",

            // VS
            "/.vs/vsworkspacestate.json",

            // Rider
            "/.idea/workspace.xml", "/.idea/misc.xml",

            // VSCode
            "/.vscode/settings.json", "/.vscode/launch.json", "/.vscode/tasks.json", "/.vscode/extensions.json",

            // Docker
            "/dockerfile", "/compose.yml", "/docker-compose.yml", "/docker-compose.ci-build.yml",

            // NPM
            "/package.json", "/package-lock.json",

            // Settings
            "/appsettings.json", "/appsettings.development.json", "/appsettings.production.json", "/web.config",

            // Logs
            "/log.txt", "/logs.txt", "/log.log", "/logs.log",

            // Backup
            "/backup.zip", "/backup.tar", "/backup.tar.gz", "/backup.tgz", "/backup.sql", "/db.sql", "/database.sql", "/dump.sql", "/old.sql",
            "/backup/database.sql", "/backup/db.sql", "/backup/dump.sql", "/backup/backup.sql", "/backup/old.sql",

            // Misc
            "/readme.md",
        };

        /// <summary>
        /// Lowercase partial paths that will severely increase the ban score of the request host.
        /// Must be url encoded.
        /// </summary>
        public static readonly string[] ForbiddenPartialPaths = new string[]
        {
            // SQL injection
            "select(", "select+", "when(", "+when+", "cast(", "concat(", "char(",
            "'(", "%22(", "('", "(%22", "((",
            "')", "%22)", ")'", ")%22", "))",
            "%22=%22", "'='",
            "%22or%22", "'or'", "+or+",
            "%22and%22", "'and'", "+and+",

            // Directory traversal
            "../", "..%2f", "..%5c", "..%c0%af", "..%c1%9c", "..%252f", "..%255c",
        };

        /// <summary>
        /// Lowercase file endings that will increase the ban score of the request host
        /// </summary>
        public static readonly string[] BadFileEndings = new string[]
        {
            // Web
            ".asp", ".aspx", ".php",

            // Shell & executable
            ".bat", ".sh", ".cmd", ".ps1", ".cgi", ".exe", ".dll", ".so",

            // Source code
            ".py", ".pl", ".rb", ".cs", ".go", ".cpp", ".c", ".java", ".sln", ".csproj",

            // Db
            ".sql", ".mdb", ".sqlite", ".sqlite3", ".db", ".db3", ".s3db", ".sl3",

            // Archive
            ".rpm", ".msi", ".dmg", ".pkg", ".app",

            // Config
            ".yml", ".env", ".config", ".owa", ".htaccess", ".htpasswd",

            // Log & Backup
            ".log", ".bak", ".backup", ".old",
        };

        /// <summary>
        /// Lowercase partial paths that will increase the ban score of the request host
        /// </summary>
        public static readonly string[] BadPartialPaths = new string[]
        {
            // Directories
            "/php/", "/wp/", "/wordpress/", "/wp-content/", "/wp-includes/", "/wp-admin/",
            "/magento/", "/magento_version/", "/magmi/", "/woocommerce/", "/shopify/", "/prestashop/", "/drupal/", "/sitecore/",
        };
    }
}
