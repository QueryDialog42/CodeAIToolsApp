namespace CodeAIToolsWPF
{
    public struct Configs
    {
        public const string ADMIN = "ADMIN";
        public const string WORKER = "WORKER";
        public const string LIBS_DIR = "libs";
        public const string COD_TO_PARSE = "Java";
        public const string FLOW_FILE = "Flow.txt";
        public const string CODE_FILE = "Code.txt";
        public const string ROOT_DIR = "CodeAI_Root";
        public const string PROJECT_NAME = "CodeAITools";
    }

    public struct Marks
    {
        public const string DIR_COLOR = "#fcba03";
        public const string TXT_PRIMA = "TextPrimary";
        public const string ROOT_DIR_COLOR = "#00b3ff";
        public const string TXT_SECA = "TextSecondary";
        public const string ADM_POP_TITLE_COL = "#1d4ed8";
        public const string WORK_POP_TITLE_COL = "#065f46";
    }

    public struct Icons
    {
        public const string ADM_ICON = "🛡️";
        public const string FILE_ICON = "📄";
        public const string WORK_ICON = "👷";
        public const string DIR_CLOSED_ICON = "📁";
        public const string DIR_OPENED_ICON = "📂";
    }

    public struct ApiEndpoints
    {
        public const string LOG_API = "http://localhost:8080/login";
        public const string REG_API = "http://localhost:8080/register";
        public const string GET_ALL_API = "http://localhost:8080/getAll";
        public const string ACTIV_USER_API = "http://localhost:8080/getUser";
        public const string AI_EXPL_API = "http://localhost:8080/AI/explain";
        public const string AI_TRANS_API = "http://localhost:8080/AI/transform";
        public const string DEL_PROJ_API = "http://localhost:8080/project/delete";
        public const string CRE_PROJ_API = "http://localhost:8080/project/create";
        public const string GET_PROJS_API = "http://localhost:8080/project/getAll";
        public const string ADD_COLL_API = "http://localhost:8080/teams/addCollaborator";
        public const string GET_COLLS_API = "http://localhost:8080/teams/getCollaborators";
        public const string REM_COLL_API = "http://localhost:8080/teams/removeCollaborator";
    }
}
